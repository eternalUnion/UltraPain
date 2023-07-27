using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using UnityEngine;

namespace Ultrapain.Patches
{
    class Drone_Start_Patch
    {
        static void Postfix(Drone __instance, ref EnemyIdentifier ___eid)
        {
            if (___eid.enemyType != EnemyType.Drone)
                return;

            __instance.gameObject.AddComponent<DroneFlag>();
        }
    }

    class Drone_PlaySound_Patch
    {
        static FieldInfo antennaFlashField = typeof(Turret).GetField("antennaFlash", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        static ParticleSystem antennaFlash;
        public static Color defaultLineColor = new Color(1f, 0.44f, 0.74f);

        static bool Prefix(Drone __instance, EnemyIdentifier ___eid, AudioClip __0)
        {
            if (___eid.enemyType != EnemyType.Drone)
                return true;

            if(__0 == __instance.windUpSound)
            {
                DroneFlag flag = __instance.GetComponent<DroneFlag>();
                if (flag == null)
                    return true;

                List<Tuple<DroneFlag.Firemode, float>> chances = new List<Tuple<DroneFlag.Firemode, float>>();
                if (ConfigManager.droneProjectileToggle.value)
                    chances.Add(new Tuple<DroneFlag.Firemode, float>(DroneFlag.Firemode.Projectile, ConfigManager.droneProjectileChance.value));
                if (ConfigManager.droneExplosionBeamToggle.value)
                    chances.Add(new Tuple<DroneFlag.Firemode, float>(DroneFlag.Firemode.Explosive, ConfigManager.droneExplosionBeamChance.value));
                if (ConfigManager.droneSentryBeamToggle.value)
                    chances.Add(new Tuple<DroneFlag.Firemode, float>(DroneFlag.Firemode.TurretBeam, ConfigManager.droneSentryBeamChance.value));

                if (chances.Count == 0 || chances.Sum(item => item.Item2) <= 0)
                    flag.currentMode = DroneFlag.Firemode.Projectile;
                else
                    flag.currentMode = UnityUtils.GetRandomFloatWeightedItem(chances, item => item.Item2).Item1;

                if (flag.currentMode == DroneFlag.Firemode.Projectile)
                {
                    flag.attackDelay = ConfigManager.droneProjectileDelay.value;
                    return true;
                }
                else if (flag.currentMode == DroneFlag.Firemode.Explosive)
                {
                    flag.attackDelay = ConfigManager.droneExplosionBeamDelay.value;

                    GameObject chargeEffect = GameObject.Instantiate(Plugin.chargeEffect, __instance.transform);
                    chargeEffect.transform.localPosition = new Vector3(0, 0, 0.8f);
                    chargeEffect.transform.localScale = Vector3.zero;

                    float duration = ConfigManager.droneExplosionBeamDelay.value / ___eid.totalSpeedModifier;
                    RemoveOnTime remover = chargeEffect.AddComponent<RemoveOnTime>();
                    remover.time = duration;
                    CommonLinearScaler scaler = chargeEffect.AddComponent<CommonLinearScaler>();
                    scaler.targetTransform = scaler.transform;
                    scaler.scaleSpeed = 1f / duration;
                    CommonAudioPitchScaler pitchScaler = chargeEffect.AddComponent<CommonAudioPitchScaler>();
                    pitchScaler.targetAud = chargeEffect.GetComponent<AudioSource>();
                    pitchScaler.scaleSpeed = 1f / duration;

                    return false;
                }
                else if (flag.currentMode == DroneFlag.Firemode.TurretBeam)
                {
                    flag.attackDelay = ConfigManager.droneSentryBeamDelay.value;
                    if(ConfigManager.droneDrawSentryBeamLine.value)
                    {
                        flag.lr.enabled = true;
                        flag.SetLineColor(ConfigManager.droneSentryBeamLineNormalColor.value);
                        flag.Invoke("LineRendererColorToWarning", Mathf.Max(0.01f, (flag.attackDelay / ___eid.totalSpeedModifier) - ConfigManager.droneSentryBeamLineIndicatorDelay.value));
                    }

                    if (flag.particleSystem == null)
                    {
                        if (antennaFlash == null)
                            antennaFlash = (ParticleSystem)antennaFlashField.GetValue(Plugin.turret);
                        flag.particleSystem = GameObject.Instantiate(antennaFlash, __instance.transform);
                        flag.particleSystem.transform.localPosition = new Vector3(0, 0, 2);
                    }

                    flag.particleSystem.Play();
                    GameObject flash = GameObject.Instantiate(Plugin.turretFinalFlash, __instance.transform);
                    GameObject.Destroy(flash.transform.Find("MuzzleFlash/muzzleflash").gameObject);
                    return false;
                }
            }

            return true;
        }
    }

    class Drone_Shoot_Patch
    {
        static bool Prefix(Drone __instance, ref EnemyIdentifier ___eid)
        {
            DroneFlag flag = __instance.GetComponent<DroneFlag>();
            if(flag == null || __instance.crashing)
                return true;

            DroneFlag.Firemode mode = flag.currentMode;

            if (mode == DroneFlag.Firemode.Projectile)
                return true;
            if (mode == DroneFlag.Firemode.Explosive)
            {
                GameObject beam = GameObject.Instantiate(Plugin.beam.gameObject, __instance.transform.position + __instance.transform.forward, __instance.transform.rotation);

                RevolverBeam revBeam = beam.GetComponent<RevolverBeam>();
                revBeam.hitParticle = Plugin.shotgunGrenade.gameObject.GetComponent<Grenade>().explosion;
                revBeam.damage /= 2;
                revBeam.damage *= ___eid.totalDamageModifier;

                return false;
            }
            if(mode == DroneFlag.Firemode.TurretBeam)
            {
                GameObject turretBeam = GameObject.Instantiate(Plugin.turretBeam.gameObject, __instance.transform.position + __instance.transform.forward * 2f, __instance.transform.rotation);
                if (turretBeam.TryGetComponent<RevolverBeam>(out RevolverBeam revBeam))
                {
                    revBeam.damage = ConfigManager.droneSentryBeamDamage.value;
                    revBeam.damage *= ___eid.totalDamageModifier;
                    revBeam.alternateStartPoint = __instance.transform.position + __instance.transform.forward;
                    revBeam.ignoreEnemyType = EnemyType.Drone;
                }

                flag.lr.enabled = false;

                return false;
            }

            Debug.LogError($"Drone fire mode in impossible state. Current value: {mode} : {(int)mode}");
            return true;
        }
    }

    class Drone_Update
    {
        static void Postfix(Drone __instance, EnemyIdentifier ___eid, ref float ___attackCooldown, int ___difficulty)
        {
            if (___eid.enemyType != EnemyType.Drone)
                return;

            DroneFlag flag = __instance.GetComponent<DroneFlag>();
            if (flag == null || flag.attackDelay < 0)
                return;

            float attackSpeedDecay = (float)(___difficulty / 2);
            if (___difficulty == 1)
            {
                attackSpeedDecay = 0.75f;
            }
            else if (___difficulty == 0)
            {
                attackSpeedDecay = 0.5f;
            }
            attackSpeedDecay *= ___eid.totalSpeedModifier;

            float delay = flag.attackDelay / ___eid.totalSpeedModifier;
            __instance.CancelInvoke("Shoot");
            __instance.Invoke("Shoot", delay);
            ___attackCooldown = UnityEngine.Random.Range(2f, 4f) + (flag.attackDelay - 0.75f) * attackSpeedDecay;
            flag.attackDelay = -1;
        }
    }

    class DroneFlag : MonoBehaviour
    {
        public enum Firemode : int
        {
            Projectile = 0,
            Explosive,
            TurretBeam
        }

        public ParticleSystem particleSystem;
        public LineRenderer lr;
        public Firemode currentMode = Firemode.Projectile;
        private static Firemode[] allModes = Enum.GetValues(typeof(Firemode)) as Firemode[];

        static FieldInfo turretAimLine = typeof(Turret).GetField("aimLine", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        static Material whiteMat;
        public void Awake()
        {
            lr = gameObject.AddComponent<LineRenderer>();
            lr.enabled = false;
            lr.receiveShadows = false;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.startWidth = lr.endWidth = lr.widthMultiplier = 0.025f;

            if (whiteMat == null)
                whiteMat = ((LineRenderer)turretAimLine.GetValue(Plugin.turret)).material;

            lr.material = whiteMat;
        }

        public void SetLineColor(Color c)
        {
            Gradient gradient = new Gradient();
            GradientColorKey[] array = new GradientColorKey[1];
            array[0].color = c;
            GradientAlphaKey[] array2 = new GradientAlphaKey[1];
            array2[0].alpha = 1f;
            gradient.SetKeys(array, array2);
            lr.colorGradient = gradient;
        }

        public void LineRendererColorToWarning()
        {
            SetLineColor(ConfigManager.droneSentryBeamLineWarningColor.value);
        }

        public float attackDelay = -1;
        public bool homingTowardsPlayer = false;

        Transform target;
        Rigidbody rb;

        private void Update()
        {
            if(homingTowardsPlayer)
            {
                if(target == null)
                    target = PlayerTracker.Instance.GetTarget();
                if (rb == null)
                    rb = GetComponent<Rigidbody>();

                Quaternion to = Quaternion.LookRotation(target.position/* + MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity()*/ - transform.position);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, to, Time.deltaTime * ConfigManager.droneHomeTurnSpeed.value);
                rb.velocity = transform.forward * rb.velocity.magnitude;
            }
            
            if(lr.enabled)
            {
                lr.SetPosition(0, transform.position);
                lr.SetPosition(1, transform.position + transform.forward * 1000);
            }
        }
    }

    class Drone_Death_Patch
    {
        static bool Prefix(Drone __instance, EnemyIdentifier ___eid)
        {
            if (___eid.enemyType != EnemyType.Drone || __instance.crashing)
                return true;

            DroneFlag flag = __instance.GetComponent<DroneFlag>();
            if (flag == null)
                return true;

            if (___eid.hitter == "heavypunch" || ___eid.hitter == "punch")
                return true;

            flag.homingTowardsPlayer = true;
            return true;
        }
    }

    class Drone_GetHurt_Patch
    {
        static bool Prefix(Drone __instance, EnemyIdentifier ___eid, bool ___parried)
        {
            if((___eid.hitter == "shotgunzone" || ___eid.hitter == "punch") && !___parried)
            {
                DroneFlag flag = __instance.GetComponent<DroneFlag>();
                if (flag == null)
                    return true;
                flag.homingTowardsPlayer = false;
            }

            return true;
        }
    }
}
