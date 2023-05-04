using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Ultrapain.Patches
{
    class Leviathan_Flag : MonoBehaviour
    {
        private LeviathanHead comp;
        private Animator anim;

        private GameObject currentProjectileEffect;
        private AudioSource currentProjectileAud;
        private Transform shootPoint;
        public float currentProjectileSize = 0;
        public float beamChargeRate = 12f / 1f;

        public int maxBeamCount = 2;
        public int beamRemaining = 0;

        private static FieldInfo ___inAction = typeof(LeviathanHead).GetField("inAction", BindingFlags.NonPublic | BindingFlags.Instance);

        private void Awake()
        {
            comp = GetComponent<LeviathanHead>();
            anim = GetComponent<Animator>();
        }

        public bool charging = false;
        private void Update()
        {
            if (charging && currentProjectileEffect != null)
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

            beamRemaining = maxBeamCount;
            Invoke("PrepareForFire", 1f);
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
            //comp.lookAtPlayer = false;

            //targetGrenade = FindTargetGrenade();

            //targetShootPoint = NewMovement.Instance.playerCollider.bounds.center;
            /*if (targetGrenade == null)
            {
                comp.lookAtPlayer = false;
            }
            else
            {
                comp.lookAtPlayer = true;
            }*/

            Invoke("Shoot", 0.5f);
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
                //proj.transform.rotation = Quaternion.LookRotation(targetShootPoint - proj.transform.position);
                proj.transform.rotation = Quaternion.LookRotation(NewMovement.Instance.playerCollider.bounds.center - proj.transform.position);
                proj.transform.eulerAngles += RandomVector(-5f, 5f);
            }
            proj.transform.localScale = new Vector3(2f, 1f, 2f);

            if (proj.TryGetComponent(out RevolverBeam projComp))
            {
                GameObject expClone = GameObject.Instantiate(projComp.hitParticle, new Vector3(1000000, 1000000, 1000000), Quaternion.identity);

                foreach (Explosion exp in expClone.GetComponentsInChildren<Explosion>())
                {
                    exp.maxSize *= 1.5f;
                    exp.speed *= 1.5f;
                }

                projComp.damage *= 2;
                projComp.hitParticle = expClone;
            }

            beamRemaining -= 1;
            if (beamRemaining <= 0)
            {
                Destroy(currentProjectileEffect);
                currentProjectileSize = 0;

                comp.lookAtPlayer = false;
                anim.SetBool("ProjectileBurst", false);
                ___inAction.SetValue(comp, false);
                targetGrenade = null;
            }
            else
            {
                comp.lookAtPlayer = true;
                Invoke("PrepareForFire", 0.5f);
            }
        }
    }

    class LeviathanTail_Flag : MonoBehaviour
    {
        public int maxSwingCount = 3;
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
            __instance.gameObject.AddComponent<Leviathan_Flag>();
        }
    }

    class Leviathan_FixedUpdate
    {
        static bool Roll(float chancePercent)
        {
            return UnityEngine.Random.Range(0, 99.9f) <= chancePercent;
        }

        struct StateInfo
        {
            public GameObject oldProjectile;
            public GameObject currentProjectile;

            public StateInfo(GameObject oldProjectile, GameObject currentProjectile)
            {
                this.oldProjectile = oldProjectile;
                this.currentProjectile = currentProjectile;
            }
        }

        static bool Prefix(LeviathanHead __instance, LeviathanController ___lcon, bool ___projectileBursting, float ___projectileBurstCooldown, out StateInfo __state)
        {
            __state = new StateInfo(null, null);
            if (___projectileBursting && ___projectileBurstCooldown == 0)
            {
                __state.oldProjectile = MonoSingleton<DefaultReferenceManager>.Instance.projectile;

                if (Roll(ConfigManager.leviathanProjectileYellowChance.value))
                {
                    GameObject proj = MonoSingleton<DefaultReferenceManager>.Instance.projectile = GameObject.Instantiate(Plugin.hideousMassProjectile);
                    __state.currentProjectile = proj;
                    Projectile comp = proj.GetComponent<Projectile>();
                    comp.target = MonoSingleton<PlayerTracker>.Instance.GetTarget();
                    comp.safeEnemyType = EnemyType.Leviathan;

                    // values from p2 flesh prison
                    comp.turnSpeed *= 4f;
                    comp.turningSpeedMultiplier *= 4f;
                    comp.predictiveHomingMultiplier = 1.25f;

                    comp.speed *= ___lcon.eid.totalSpeedModifier;
                    comp.damage *= ___lcon.eid.totalDamageModifier;
                }
                else if (Roll(ConfigManager.leviathanProjectileBlueChance.value))
                {
                    GameObject proj = MonoSingleton<DefaultReferenceManager>.Instance.projectile = GameObject.Instantiate(Plugin.homingProjectile);
                    __state.currentProjectile = proj;
                    Projectile comp = proj.GetComponent<Projectile>();
                    comp.target = MonoSingleton<PlayerTracker>.Instance.GetTarget();
                    comp.safeEnemyType = EnemyType.Leviathan;

                    // values from mindflayer
                    comp.turningSpeedMultiplier = 0.5f;
                    comp.speed *= ___lcon.eid.totalSpeedModifier;
                    comp.damage *= ___lcon.eid.totalDamageModifier;
                }
                else
                    __state.oldProjectile = null;
            }

            return true;
        }

        static void Postfix(StateInfo __state)
        {
            if (__state.oldProjectile != null)
            {
                MonoSingleton<DefaultReferenceManager>.Instance.projectile = __state.oldProjectile;
                GameObject.Destroy(__state.currentProjectile);
            }
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

            Debug.Log("Attempting to prepare beam for leviathan");
            ___anim.SetBool("ProjectileBurst", true);

            ___projectilesLeftInBurst = 1;
            ___projectileBurstCooldown = 100f;
            ___inAction = true;

            return false;
        }
    }

    class Leviathan_ProjectileBurstStart
    {
        static bool Prefix(LeviathanHead __instance, Transform ___shootPoint)
        {
            Leviathan_Flag flag = __instance.GetComponent<Leviathan_Flag>();
            if (flag == null)
                return true;
            if (flag.charging)
                return false;


            Debug.Log("Attempting to charge beam for leviathan");
            __instance.lookAtPlayer = true;
            flag.ChargeBeam(___shootPoint);
            return false;
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

            flag.swingCount = flag.maxSwingCount;
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
