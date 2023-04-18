using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.AI;

namespace DifficultyTweak.Patches
{
    public class StrayFlag : MonoBehaviour
    {
        //public int extraShotsRemaining = 6;
        private Animator anim;
        private EnemyIdentifier eid;

        public GameObject standardProjectile;
        public GameObject standardDecorativeProjectile;

        public int comboRemaining = ConfigManager.strayShootCount.value;
        public bool inCombo = false;

        public float lastSpeed = 1f;

        public enum AttackMode
        {
            ProjectileCombo,
            FastHoming
        }

        public AttackMode currentMode = AttackMode.ProjectileCombo;

        public void Awake()
        {
            anim = GetComponent<Animator>();
            eid = GetComponent<EnemyIdentifier>();
        }

        public void Update()
        {
            if(eid.dead)
            {
                Destroy(this);
                return;
            }

            if (inCombo)
            {
                anim.speed = ZombieProjectile_ThrowProjectile_Patch.animSpeed;
                anim.SetFloat("Speed", ZombieProjectile_ThrowProjectile_Patch.animSpeed);
            }
        }
    }

    [HarmonyPatch(typeof(ZombieProjectiles), "Start")]
    public class ZombieProjectile_Start_Patch1
    {
        static void Postfix(ZombieProjectiles __instance, ref EnemyIdentifier ___eid)
        {
            if (___eid.enemyType != EnemyType.Stray)
                return;

            StrayFlag flag = __instance.gameObject.AddComponent<StrayFlag>();
            flag.standardProjectile = __instance.projectile;
            flag.standardDecorativeProjectile = __instance.decProjectile;
            flag.currentMode = StrayFlag.AttackMode.ProjectileCombo;
            /*__instance.projectile = Plugin.homingProjectile;
            __instance.decProjectile = Plugin.decorativeProjectile2;*/
        }
    }

    [HarmonyPatch(typeof(ZombieProjectiles), "ThrowProjectile")]
    public class ZombieProjectile_ThrowProjectile_Patch
    {
        public static float normalizedTime = 0f;
        public static float animSpeed = 20f;

        public static float projectileSpeed = 75;
        public static float turnSpeedMultiplier = 0.45f;
        public static int projectileDamage = 10;

        public static int explosionDamage = 20;
        public static float coreSpeed = 110f;

        static void Postfix(ZombieProjectiles __instance, ref EnemyIdentifier ___eid, ref Animator ___anim, ref GameObject ___currentProjectile
            , ref NavMeshAgent ___nma, ref Zombie ___zmb)
        {
            if (___eid.enemyType != EnemyType.Stray)
                return;

            StrayFlag flag = __instance.gameObject.GetComponent<StrayFlag>();
            if (flag == null)
                return;

            if (flag.currentMode == StrayFlag.AttackMode.FastHoming)
            {
                Projectile proj = ___currentProjectile.GetComponent<Projectile>();
                if (proj != null)
                {
                    proj.target = MonoSingleton<PlayerTracker>.Instance.GetTarget();
                    proj.speed = projectileSpeed;
                    proj.turningSpeedMultiplier = turnSpeedMultiplier;
                    proj.safeEnemyType = EnemyType.Stray;
                    proj.damage = projectileDamage;
                }

                flag.currentMode = StrayFlag.AttackMode.ProjectileCombo;
                __instance.projectile = flag.standardProjectile;
                __instance.decProjectile = flag.standardDecorativeProjectile;
            }
            else if(flag.currentMode == StrayFlag.AttackMode.ProjectileCombo)
            {
                flag.comboRemaining -= 1;

                if (flag.comboRemaining == 0)
                {
                    flag.comboRemaining = ConfigManager.strayShootCount.value;
                    //flag.currentMode = StrayFlag.AttackMode.FastHoming;
                    flag.inCombo = false;
                    ___anim.speed = flag.lastSpeed;
                    ___anim.SetFloat("Speed", flag.lastSpeed);
                    //__instance.projectile = Plugin.homingProjectile;
                    //__instance.decProjectile = Plugin.decorativeProjectile2;
                }
                else
                {
                    flag.inCombo = true;
                    __instance.swinging = true;
                    __instance.seekingPlayer = false;
                    ___nma.updateRotation = false;
                    __instance.transform.LookAt(new Vector3(___zmb.target.position.x, __instance.transform.position.y, ___zmb.target.position.z));
                    flag.lastSpeed = ___anim.speed;
                    //___anim.Play("ThrowProjectile", 0, ZombieProjectile_ThrowProjectile_Patch.normalizedTime);
                    ___anim.speed = ConfigManager.strayShootSpeed.value;
                    ___anim.SetFloat("Speed", ConfigManager.strayShootSpeed.value);
                    ___anim.SetTrigger("Swing");
                    //___anim.SetFloat("AttackType", 0f);
                    //___anim.StopPlayback();
                    //flag.Invoke("LateCombo", 0.01f);
                    //___anim.runtimeAnimatorController.animationClips.Where(clip => clip.name == "ThrowProjectile").First().
                    //___anim.fireEvents = true;
                }
            }
        }
    }

    [HarmonyPatch(typeof(ZombieProjectiles), nameof(ZombieProjectiles.SpawnProjectile))]
    class Swing
    {
        static void Postfix(ZombieProjectiles __instance, ref EnemyIdentifier ___eid)
        {
            if (___eid.enemyType != EnemyType.Stray)
                return;

            ___eid.weakPoint = null;
        }
    }

    /*[HarmonyPatch(typeof(ZombieProjectiles), "Swing")]
    class Swing
    {
        static void Postfix()
        {
            Debug.Log("Swing()");
        }
    }*/

    [HarmonyPatch(typeof(ZombieProjectiles), "SwingEnd")]
    class SwingEnd
    {
        static bool Prefix(ZombieProjectiles __instance, ref EnemyIdentifier ___eid)
        {
            if (___eid.enemyType != EnemyType.Stray)
                return true;

            StrayFlag flag = __instance.gameObject.GetComponent<StrayFlag>();
            if (flag == null)
                return true;

            if (flag.inCombo)
                return false;

            return true;
        }
    }

    /*[HarmonyPatch(typeof(ZombieProjectiles), "DamageStart")]
    class DamageStart
    {
        static void Postfix()
        {
            Debug.Log("DamageStart()");
        }
    }*/

    [HarmonyPatch(typeof(ZombieProjectiles), "DamageEnd")]
    class DamageEnd
    {
        static bool Prefix(ZombieProjectiles __instance, ref EnemyIdentifier ___eid)
        {
            if (___eid.enemyType != EnemyType.Stray)
                return true;

            StrayFlag flag = __instance.gameObject.GetComponent<StrayFlag>();
            if (flag == null)
                return true;

            if (flag.inCombo)
                return false;

            return true;
        }
    }
}
