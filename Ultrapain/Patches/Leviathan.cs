using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using ULTRAKILL.Cheats;
using UnityEngine;

namespace Ultrapain.Patches
{
    class Leviathan_Flag : MonoBehaviour
    {
        private LeviathanHead comp;
        private Animator anim;
        //private Collider col;
        private LayerMask envMask = new LayerMask() { value = 1 << 8 | 1 << 24 };

        public float playerRocketRideTracker = 0;

        private GameObject currentProjectileEffect;
        private AudioSource currentProjectileAud;
        private Transform shootPoint;
        public float currentProjectileSize = 0;
        public float beamChargeRate = 12f / 1f;

        public int beamRemaining = 0;

        public int projectilesRemaining = 0;
        public float projectileDelayRemaining = 0f;

        private static FieldInfo ___inAction = typeof(LeviathanHead).GetField("inAction", BindingFlags.NonPublic | BindingFlags.Instance);

        private void Awake()
        {
            comp = GetComponent<LeviathanHead>();
            anim = GetComponent<Animator>();
            //col = GetComponent<Collider>();
        }

        public bool beamAttack = false;
        public bool projectileAttack = false;

        public bool charging = false;
        private void Update()
        {
            if (currentProjectileEffect != null && (charging || currentProjectileSize < 11.9f))
            {
                currentProjectileSize += beamChargeRate * Time.deltaTime;
                currentProjectileEffect.transform.localScale = Vector3.one * currentProjectileSize;
                currentProjectileAud.pitch = currentProjectileSize / 2;
            }
        }

        public void ChargeBeam(Transform shootPoint)
        {
            if (currentProjectileEffect != null)
                return;
            this.shootPoint = shootPoint;

            charging = true;
            currentProjectileSize = 0;
            currentProjectileEffect = GameObject.Instantiate(Plugin.chargeEffect, shootPoint);
            currentProjectileAud = currentProjectileEffect.GetComponent<AudioSource>();
            currentProjectileEffect.transform.localPosition = new Vector3(0, 0, 6);
            currentProjectileEffect.transform.localScale = Vector3.zero;

            beamRemaining = ConfigManager.leviathanChargeCount.value;
            beamChargeRate = 11.9f / ConfigManager.leviathanChargeDelay.value;
            Invoke("PrepareForFire", ConfigManager.leviathanChargeDelay.value / comp.lcon.eid.totalSpeedModifier);
        }

        private Grenade FindTargetGrenade()
        {
            List<Grenade> list = GrenadeList.Instance.grenadeList;
            Grenade targetGrenade = null;
            Vector3 playerPos = PlayerTracker.Instance.GetTarget().position;
            foreach (Grenade grn in list)
            {
                if (Vector3.Distance(grn.transform.position, playerPos) <= 10f)
                {
                    targetGrenade = grn;
                    break;
                }
            }

            return targetGrenade;
        }

        private Grenade targetGrenade = null;
        public void PrepareForFire()
        {
            charging = false;

            // OLD PREDICTION
            //targetShootPoint = NewMovement.Instance.playerCollider.bounds.center;
            //if (Physics.Raycast(NewMovement.Instance.transform.position, Vector3.down, out RaycastHit hit, float.MaxValue, envMask))
            //    targetShootPoint = hit.point;

            // Malicious face beam prediction
            GameObject player = MonoSingleton<PlayerTracker>.Instance.GetPlayer().gameObject;
            Vector3 a = new Vector3(MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().x, MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().y / (float)(MonoSingleton<NewMovement>.Instance.ridingRocket ? 1 : 2), MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().z);
            targetShootPoint = (MonoSingleton<NewMovement>.Instance.ridingRocket ? MonoSingleton<NewMovement>.Instance.ridingRocket.transform.position : player.transform.position) + a / (1f / ConfigManager.leviathanChargeDelay.value) / comp.lcon.eid.totalSpeedModifier;
            RaycastHit raycastHit;
            // I guess this was in case player is approaching the malface, but it is very unlikely with leviathan
            /*if (MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().magnitude > 0f && col.Raycast(new Ray(player.transform.position, MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity()), out raycastHit, MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().magnitude * 0.5f / comp.lcon.eid.totalSpeedModifier))
            {
                targetShootPoint = player.transform.position;
            }
            else */if (Physics.Raycast(player.transform.position, targetShootPoint - player.transform.position, out raycastHit, Vector3.Distance(targetShootPoint, player.transform.position), LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies), QueryTriggerInteraction.Collide))
            {
                targetShootPoint = raycastHit.point;
            }

            Invoke("Shoot", ConfigManager.leviathanChargeDelay.value / comp.lcon.eid.totalSpeedModifier);
        }

        private Vector3 RandomVector(float min, float max)
        {
            return new Vector3(UnityEngine.Random.Range(min, max), UnityEngine.Random.Range(min, max), UnityEngine.Random.Range(min, max));
        }

        public Vector3 targetShootPoint;
        private void Shoot()
        {
            Debug.Log("Attempting to shoot projectile for leviathan");
            GameObject proj = GameObject.Instantiate(Plugin.maliciousFaceProjectile, shootPoint.transform.position, Quaternion.identity);

            if (targetGrenade == null)
                targetGrenade = FindTargetGrenade();

            if (targetGrenade != null)
            {
                //NewMovement.Instance.playerCollider.bounds.center
                //proj.transform.rotation = Quaternion.LookRotation(targetGrenade.transform.position - shootPoint.transform.position);
                proj.transform.rotation = Quaternion.LookRotation(targetGrenade.transform.position - shootPoint.transform.position);
            }
            else
            {
                proj.transform.rotation = Quaternion.LookRotation(targetShootPoint - proj.transform.position);
                //proj.transform.rotation = Quaternion.LookRotation(NewMovement.Instance.playerCollider.bounds.center - proj.transform.position);
                //proj.transform.eulerAngles += RandomVector(-5f, 5f);
            }
            proj.transform.localScale = new Vector3(2f, 1f, 2f);

            if (proj.TryGetComponent(out RevolverBeam projComp))
            {
                GameObject expClone = GameObject.Instantiate(projComp.hitParticle, new Vector3(1000000, 1000000, 1000000), Quaternion.identity);

                foreach (Explosion exp in expClone.GetComponentsInChildren<Explosion>())
                {
                    exp.maxSize *= ConfigManager.leviathanChargeSizeMulti.value;
                    exp.speed *= ConfigManager.leviathanChargeSizeMulti.value;
                    exp.damage = (int)(exp.damage * ConfigManager.leviathanChargeDamageMulti.value * comp.lcon.eid.totalDamageModifier);
                    exp.toIgnore.Add(EnemyType.Leviathan);
                }

                projComp.damage *= 2 * comp.lcon.eid.totalDamageModifier;
                projComp.hitParticle = expClone;
            }

            beamRemaining -= 1;
            if (beamRemaining <= 0)
            {
                Destroy(currentProjectileEffect);
                currentProjectileSize = 0;
                beamAttack = false;

                if (projectilesRemaining <= 0)
                {
                    comp.lookAtPlayer = false;
                    anim.SetBool("ProjectileBurst", false);
                    ___inAction.SetValue(comp, false);
                    targetGrenade = null;
                }
                else
                {
                    comp.lookAtPlayer = true;
                    projectileAttack = true;
                }
            }
            else
            {
                targetShootPoint = NewMovement.Instance.playerCollider.bounds.center;
                if (Physics.Raycast(NewMovement.Instance.transform.position, Vector3.down, out RaycastHit hit, float.MaxValue, envMask))
                    targetShootPoint = hit.point;

                comp.lookAtPlayer = true;
                Invoke("PrepareForFire", ConfigManager.leviathanChargeDelay.value / comp.lcon.eid.totalSpeedModifier);
            }
        }

        private void SwitchToSecondPhase()
        {
            comp.lcon.phaseChangeHealth = comp.lcon.stat.health;
        }
    }

    class LeviathanTail_Flag : MonoBehaviour
    {
        public int swingCount = 0;

        private Animator ___anim;
        private void Awake()
        {
            ___anim = GetComponent<Animator>();
        }

        public static float crossfadeDuration = 0.05f;
        private void SwingAgain()
        {
            ___anim.CrossFade("TailWhip", crossfadeDuration, 0, LeviathanTail_SwingEnd.targetStartNormalized);
        }
    }


    class Leviathan_Start
    {
        static void Postfix(LeviathanHead __instance)
        {
            Leviathan_Flag flag = __instance.gameObject.AddComponent<Leviathan_Flag>();
            if(ConfigManager.leviathanSecondPhaseBegin.value)
                flag.Invoke("SwitchToSecondPhase", 2f / __instance.lcon.eid.totalSpeedModifier);
        }
    }

    class Leviathan_FixedUpdate
    {
        public static float projectileForward = 10f;

        static bool Roll(float chancePercent)
        {
            return UnityEngine.Random.Range(0, 99.9f) <= chancePercent;
        }

        static bool Prefix(LeviathanHead __instance, LeviathanController ___lcon, ref bool ___projectileBursting, float ___projectileBurstCooldown,
            Transform ___shootPoint, ref bool ___trackerIgnoreLimits, Animator ___anim, ref int ___previousAttack)
        {
            if (!__instance.active)
            {
                return false;
            }

            Leviathan_Flag flag = __instance.GetComponent<Leviathan_Flag>();
            if (flag == null)
                return true;

            if (___projectileBursting && flag.projectileAttack)
            {
                if (flag.projectileDelayRemaining > 0f)
                {
                    flag.projectileDelayRemaining = Mathf.MoveTowards(flag.projectileDelayRemaining, 0f, Time.deltaTime * __instance.lcon.eid.totalSpeedModifier);
                }
                else
                {
                    flag.projectilesRemaining -= 1;
                    flag.projectileDelayRemaining = (1f / ConfigManager.leviathanProjectileDensity.value) / __instance.lcon.eid.totalSpeedModifier;

                    GameObject proj = null;
                    Projectile comp = null;
                    if (Roll(ConfigManager.leviathanProjectileYellowChance.value) && ConfigManager.leviathanProjectileMixToggle.value)
                    {
                        proj = GameObject.Instantiate(Plugin.hideousMassProjectile, ___shootPoint.position, ___shootPoint.rotation);
                        comp = proj.GetComponent<Projectile>();
                        comp.target = MonoSingleton<PlayerTracker>.Instance.GetTarget();
                        comp.safeEnemyType = EnemyType.Leviathan;

                        // values from p2 flesh prison
                        comp.turnSpeed *= 4f;
                        comp.turningSpeedMultiplier *= 4f;
                        comp.predictiveHomingMultiplier = 1.25f;
                    }
                    else if (Roll(ConfigManager.leviathanProjectileBlueChance.value) && ConfigManager.leviathanProjectileMixToggle.value)
                    {
                        proj = GameObject.Instantiate(Plugin.homingProjectile, ___shootPoint.position, ___shootPoint.rotation);
                        comp = proj.GetComponent<Projectile>();
                        comp.target = MonoSingleton<PlayerTracker>.Instance.GetTarget();
                        comp.safeEnemyType = EnemyType.Leviathan;

                        // values from mindflayer
                        comp.turningSpeedMultiplier = 0.5f;
                        comp.speed = 20f;
                        comp.speed *= ___lcon.eid.totalSpeedModifier;
                        comp.damage *= ___lcon.eid.totalDamageModifier;
                    }
                    else
                    {
                        proj = GameObject.Instantiate<GameObject>(MonoSingleton<DefaultReferenceManager>.Instance.projectile, ___shootPoint.position, ___shootPoint.rotation);
                        comp = proj.GetComponent<Projectile>();
                        comp.safeEnemyType = EnemyType.Leviathan;

                        comp.speed *= 2f;
                        comp.enemyDamageMultiplier = 0.5f;
                    }

                    comp.speed *= __instance.lcon.eid.totalSpeedModifier;
                    comp.damage *= __instance.lcon.eid.totalDamageModifier;
                    comp.safeEnemyType = EnemyType.Leviathan;
                    comp.enemyDamageMultiplier = ConfigManager.leviathanProjectileFriendlyFireDamageMultiplier.normalizedValue;
                    proj.transform.localScale *= 2f;
                    proj.transform.position += proj.transform.forward * projectileForward;
                }
            }

            if (___projectileBursting)
            {
                if (flag.projectilesRemaining <= 0 || BlindEnemies.Blind)
                {
                    flag.projectileAttack = false;

                    if (!flag.beamAttack)
                    {
                        ___projectileBursting = false;
                        ___trackerIgnoreLimits = false;
                        ___anim.SetBool("ProjectileBurst", false);
                    }
                }
                else
                {
                    if (NewMovement.Instance.ridingRocket != null && ConfigManager.leviathanChargeAttack.value && ConfigManager.leviathanChargeHauntRocketRiding.value)
                    {
                        flag.playerRocketRideTracker += Time.deltaTime;
                        if (flag.playerRocketRideTracker >= 1 && !flag.beamAttack)
                        {
                            flag.projectileAttack = false;
                            flag.beamAttack = true;
                            __instance.lookAtPlayer = true;
                            flag.ChargeBeam(___shootPoint);
                            flag.beamRemaining = 1;

                            return false;
                        }
                    }
                    else
                    {
                        flag.playerRocketRideTracker = 0;
                    }
                }
            }

            return false;
        }
    }

    class Leviathan_ProjectileBurst
    {
        static bool Prefix(LeviathanHead __instance, Animator ___anim,
            ref int ___projectilesLeftInBurst, ref float ___projectileBurstCooldown, ref bool ___inAction)
        {
            if (!__instance.active)
            {
                return false;
            }

            Leviathan_Flag flag = __instance.GetComponent<Leviathan_Flag>();
            if (flag == null)
                return true;
            if (flag.beamAttack || flag.projectileAttack)
                return false;

            flag.beamAttack = false;
            if (ConfigManager.leviathanChargeAttack.value)
            {
                if (NewMovement.Instance.ridingRocket != null && ConfigManager.leviathanChargeHauntRocketRiding.value)
                {
                    flag.beamAttack = true;
                }
                else if (UnityEngine.Random.Range(0, 99.9f) <= ConfigManager.leviathanChargeChance.value && ConfigManager.leviathanChargeAttack.value)
                {
                    flag.beamAttack = true;
                }
            }

            flag.projectileAttack = true;
            return true;

            /*if (!beamAttack)
            {
                flag.projectileAttack = true;
                return true;
            }*/

            /*if(flag.beamAttack)
            {
                Debug.Log("Attempting to prepare beam for leviathan");
                ___anim.SetBool("ProjectileBurst", true);

                //___projectilesLeftInBurst = 1;
                //___projectileBurstCooldown = 100f;
                ___inAction = true;

                return true;
            }*/
        }
    }

    class Leviathan_ProjectileBurstStart
    {
        static bool Prefix(LeviathanHead __instance, Transform ___shootPoint)
        {
            Leviathan_Flag flag = __instance.GetComponent<Leviathan_Flag>();
            if (flag == null)
                return true;
            if (flag.projectileAttack)
            {
                if(flag.projectilesRemaining <= 0)
                {
                    flag.projectilesRemaining = ConfigManager.leviathanProjectileCount.value;
                    flag.projectileDelayRemaining = 0;
                }
            }

            if (flag.beamAttack)
            {
                if (!flag.charging)
                {
                    Debug.Log("Attempting to charge beam for leviathan");
                    __instance.lookAtPlayer = true;
                    flag.ChargeBeam(___shootPoint);
                }
            }
            return true;
        }
    }


    class LeviathanTail_Start
    {
        static void Postfix(LeviathanTail __instance)
        {
            __instance.gameObject.AddComponent<LeviathanTail_Flag>();
        }
    }

    // This is the tail attack animation begin
    // fires at n=3.138
    // l=5.3333
    // 0.336
    // 0.88
    class LeviathanTail_BigSplash
    {
        static bool Prefix(LeviathanTail __instance)
        {
            LeviathanTail_Flag flag = __instance.gameObject.GetComponent<LeviathanTail_Flag>();
            if (flag == null)
                return true;

            flag.swingCount = ConfigManager.leviathanTailComboCount.value;
            return true;
        }
    }

    class LeviathanTail_SwingEnd
    {
        public static float targetEndNormalized = 0.7344f;
        public static float targetStartNormalized = 0.41f;

        static bool Prefix(LeviathanTail __instance, Animator ___anim)
        {
            LeviathanTail_Flag flag = __instance.gameObject.GetComponent<LeviathanTail_Flag>();
            if (flag == null)
                return true;

            flag.swingCount -= 1;
            if (flag.swingCount == 0)
                return true;

            flag.Invoke("SwingAgain", Mathf.Max(0f, 5.3333f * (0.88f - targetEndNormalized) * (1f / ___anim.speed)));
            return false;
        }
    }
}
