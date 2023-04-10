using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DifficultyTweak.Patches
{
    public class StrayFlag : MonoBehaviour
    {
        //public int extraShotsRemaining = 6;

        public bool throwingCore = false;
    }

    [HarmonyPatch(typeof(ZombieProjectiles), "Start")]
    public class ZombieProjectile_Start_Patch1
    {
        static void Postfix(ZombieProjectiles __instance, ref EnemyIdentifier ___eid)
        {
            if (___eid.enemyType != EnemyType.Stray)
                return;

            StrayFlag flag = __instance.gameObject.AddComponent<StrayFlag>();
            flag.throwingCore = false;
            __instance.projectile = Plugin.homingProjectile;
            __instance.decProjectile = Plugin.decorativeProjectile2;
        }
    }

    [HarmonyPatch(typeof(ZombieProjectiles), "ThrowProjectile")]
    public class ZombieProjectile_DamageEnd_Patch
    {
        //public static float deltaTime = 0.2f;

        public static float projectileSpeed = 75;
        public static float turnSpeedMultiplier = 0.45f;
        public static int projectileDamage = 10;

        public static int explosionDamage = 20;
        public static float coreSpeed = 110f;

        public static Vector3 strayCoreOffset = new Vector3(0, 0, 3);

        static bool Prefix(ZombieProjectiles __instance, ref EnemyIdentifier ___eid, ref GameObject ___currentProjectile, ref GameObject ___camObj)
        {
            if (___eid.enemyType != EnemyType.Stray)
                return true;

            StrayFlag flag = __instance.gameObject.GetComponent<StrayFlag>();
            if (flag == null)
                return true;

            if (flag.throwingCore)
            {
                ___currentProjectile = GameObject.Instantiate<GameObject>(Plugin.shotgunGrenade, __instance.shootPos.position + __instance.shootPos.forward, Quaternion.identity);
                ___currentProjectile.transform.LookAt(___camObj.transform);
                UnityUtils.PrintGameobject(___currentProjectile);

                Grenade grn = ___currentProjectile.GetComponent<Grenade>();
                if (grn != null)
                {
                    Debug.Log("Nade check");
                    grn.rb.AddRelativeForce(Vector3.forward * coreSpeed, ForceMode.VelocityChange);
                    grn.CanCollideWithPlayer(true);
                    foreach (Explosion exp in grn.GetComponentsInChildren<Explosion>())
                    {
                        exp.enemy = true;
                        exp.damage = explosionDamage;
                        exp.maxSize /= 2;
                        exp.speed /= 2;
                        exp.toIgnore.Add(EnemyType.Stray);
                    }
                }

                return false;
            }

            return true;
        }

        static void Postfix(ZombieProjectiles __instance, ref EnemyIdentifier ___eid, ref GameObject ___currentProjectile)
        {
            /*if (___eid.enemyType != EnemyType.Stray)
                return true;

            StrayFlag flag = __instance.gameObject.GetComponent<StrayFlag>();
            if (flag == null)
                return true;

            if (flag.extraShotsRemaining > 0)
            {
                //__instance.SwingEnd();
                ___coolDown = 0;
                __instance.DamageStart();
                ___anim.SetBool("Running", false);
                ___anim.Play("ThrowProjectile", 0, (1.3333f - deltaTime) / 2.0833f);
                flag.extraShotsRemaining -= 1;
                return false;
            }
            else
                flag.extraShotsRemaining = 6;

            return true;*/

            if (___eid.enemyType != EnemyType.Stray)
                return;

            StrayFlag flag = __instance.gameObject.GetComponent<StrayFlag>();
            if (flag == null)
                return;

            if (flag.throwingCore)
            {
                __instance.projectile = Plugin.homingProjectile;
            }
            else
            {
                Projectile proj = ___currentProjectile.GetComponent<Projectile>();
                if(proj != null)
                {
                    Debug.Log("Proj check");
                    proj.target = MonoSingleton<PlayerTracker>.Instance.GetTarget();
                    proj.speed = projectileSpeed;
                    proj.turningSpeedMultiplier = turnSpeedMultiplier;
                    proj.safeEnemyType = EnemyType.Stray;
                    proj.damage = projectileDamage;
                }

                __instance.projectile = Plugin.shotgunGrenade;
            }

            flag.throwingCore = !flag.throwingCore;
        }
    }
}
