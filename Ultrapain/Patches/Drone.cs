using HarmonyLib;
using System;
using System.Reflection;
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

        static bool Prefix(Drone __instance, EnemyIdentifier ___eid, AudioClip __0)
        {
            if (___eid.enemyType != EnemyType.Drone)
                return true;

            if(__0 == __instance.windUpSound)
            {
                DroneFlag flag = __instance.GetComponent<DroneFlag>();
                if (flag == null)
                    return true;

                flag.currentMode = DroneFlag.GetNextFireMode(flag.currentMode);
                while (true)
                {
                    if (flag.currentMode == DroneFlag.Firemode.Explosive && !ConfigManager.droneExplosionBeamToggle.value)
                    {
                        flag.currentMode = DroneFlag.GetNextFireMode(flag.currentMode);
                        continue;
                    }
                    if (flag.currentMode == DroneFlag.Firemode.TurretBeam && !ConfigManager.droneSentryBeamToggle.value)
                    {
                        flag.currentMode = DroneFlag.GetNextFireMode(flag.currentMode);
                        continue;
                    }

                    break;
                }

                if (flag.currentMode == DroneFlag.Firemode.Projectile)
                    return true;

                if (flag.currentMode == DroneFlag.Firemode.Explosive)
                {
                    GameObject chargeEffect = GameObject.Instantiate(Plugin.chargeEffect, __instance.transform);
                    chargeEffect.transform.localPosition = new Vector3(0, 0, 0.8f);
                    chargeEffect.transform.localScale = Vector3.zero;

                    float duration = 0.75f / ___eid.totalSpeedModifier;
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

                if (flag.currentMode == DroneFlag.Firemode.TurretBeam)
                {
                    if(flag.particleSystem == null)
                    {
                        if (antennaFlash == null)
                            antennaFlash = (ParticleSystem)antennaFlashField.GetValue(Plugin.turret);
                        flag.particleSystem = GameObject.Instantiate(antennaFlash, __instance.transform);
                        flag.particleSystem.transform.localPosition = new Vector3(0, 0, 2);
                    }

                    flag.particleSystem.Play();
                    GameObject flash = GameObject.Instantiate(Plugin.turretFinalFlash, __instance.transform);
                    GameObject.Destroy(flash.transform.Find("MuzzleFlash/muzzleflash").gameObject);
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
            if(flag == null)
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

                return false;
            }

            Debug.LogError($"Drone fire mode in impossible state. Current value: {mode} : {(int)mode}");
            return true;
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
        public Firemode currentMode = Firemode.Projectile;
        private static Firemode[] allModes = Enum.GetValues(typeof(Firemode)) as Firemode[];

        public static Firemode GetNextFireMode(Firemode mode)
        {
            return (Enum.IsDefined(typeof(Firemode) ,(int)mode + 1)) ? (Firemode)((int)mode + 1) : (Firemode)(0);

            /*int nextMode = Array.IndexOf(allModes, mode) + 1;
            if (nextMode >= allModes.Length)
                nextMode = 0;

            return allModes[nextMode];*/
        }
    }
}
