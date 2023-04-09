using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DifficultyTweak.Patches
{
    class GrenadeParriedFlag : MonoBehaviour
    {
        public int parryCount = 1;
        public bool registeredStyle = false;
        public bool bigExplosionOverride = false;
        public GameObject temporaryExplosion;
        public GameObject weapon;

        public enum GrenadeType
        {
            Core,
            Rocket,
        }

        public GrenadeType grenadeType;
    }

    [HarmonyPatch(typeof(Punch))]
    [HarmonyPatch("CheckForProjectile")]
    class Punch_CheckForProjectile_Patch
    {
        static bool Prefix(Punch __instance, Transform __0, ref bool __result, ref bool ___hitSomething)
        {
            Grenade grn = __0.GetComponent<Grenade>();
            if(grn != null)
            {
                MonoSingleton<TimeController>.Instance.ParryFlash();
                ___hitSomething = true;

                grn.transform.LookAt(Camera.main.transform.position + Camera.main.transform.forward * 100.0f);

                Rigidbody rb = grn.GetComponent<Rigidbody>();
                rb.velocity = Vector3.zero;
                rb.AddRelativeForce(Vector3.forward * Mathf.Max(Plugin.MinGrenadeParryVelocity, rb.velocity.magnitude), ForceMode.VelocityChange);
                rb.velocity = grn.transform.forward * Mathf.Max(Plugin.MinGrenadeParryVelocity, rb.velocity.magnitude);

                /*if (grn.rocket)
                    MonoSingleton<StyleHUD>.Instance.AddPoints(100, Plugin.StyleIDs.rocketBoost, MonoSingleton<GunControl>.Instance.currentWeapon, null);
                else
                    MonoSingleton<StyleHUD>.Instance.AddPoints(100, Plugin.StyleIDs.fistfulOfNades, MonoSingleton<GunControl>.Instance.currentWeapon, null);
                */

                GrenadeParriedFlag flag = grn.GetComponent<GrenadeParriedFlag>();
                if (flag != null)
                    flag.parryCount += 1;
                else
                {
                    flag = grn.gameObject.AddComponent<GrenadeParriedFlag>();
                    flag.grenadeType = (grn.rocket) ? GrenadeParriedFlag.GrenadeType.Rocket : GrenadeParriedFlag.GrenadeType.Core;
                    flag.weapon = MonoSingleton<GunControl>.Instance.currentWeapon;
                }

                __result = true;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Grenade))]
    [HarmonyPatch("Explode")]
    class Grenade_Explode_Patch
    {
        static bool Prefix(Grenade __instance, ref bool __2, ref bool __1, ref bool ___exploded)
        {
            GrenadeParriedFlag flag = __instance.GetComponent<GrenadeParriedFlag>();

            if (__instance.rocket)
            {
                bool rocketParried = flag != null;
                bool rocketHitGround = __1;

                if (rocketParried && rocketHitGround)
                {
                    __1 = false;
                    flag.temporaryExplosion = GameObject.Instantiate(__instance.explosion, Vector3.positiveInfinity, Quaternion.identity);
                    foreach(Explosion e in flag.temporaryExplosion.GetComponentsInChildren<Explosion>())
                    {
                        GrenadeParriedFlag fFlag = e.gameObject.AddComponent<GrenadeParriedFlag>();
                        fFlag.weapon = flag.weapon;
                        fFlag.grenadeType = GrenadeParriedFlag.GrenadeType.Rocket;
                    }

                    __instance.explosion = flag.temporaryExplosion;
                }
            }
            else
            {
                if (flag != null && flag.bigExplosionOverride)
                {
                    __2 = true;
                    Debug.Log("Big explosion");
                }
            }

            return true;
        }

        static void Postfix(Grenade __instance, ref bool ___exploded)
        {
            if (__instance.rocket)
            {
                GrenadeParriedFlag flag = __instance.GetComponent<GrenadeParriedFlag>();
                bool rocketParried = flag != null;

                if (rocketParried && flag.temporaryExplosion != null)
                {
                    GameObject.Destroy(flag.temporaryExplosion);
                    flag.temporaryExplosion = null;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Grenade))]
    [HarmonyPatch("Collision")]
    class Grenade_Collision_Patch
    {
        static bool Prefix(Grenade __instance, Collider __0)
        {
            GrenadeParriedFlag flag = __instance.GetComponent<GrenadeParriedFlag>();
            if (flag == null)
                return true;

            if (__0.gameObject.layer != 14 && __0.gameObject.layer != 20)
            {
                EnemyIdentifierIdentifier enemyIdentifierIdentifier;
                if ((__0.gameObject.layer == 11 || __0.gameObject.layer == 10) && __0.TryGetComponent<EnemyIdentifierIdentifier>(out enemyIdentifierIdentifier) && enemyIdentifierIdentifier.eid)
                {
                    if (enemyIdentifierIdentifier.eid.enemyType != EnemyType.MaliciousFace && flag.grenadeType == GrenadeParriedFlag.GrenadeType.Core)
                    {
                        flag.bigExplosionOverride = true;
                        MonoSingleton<StyleHUD>.Instance.AddPoints(100, Plugin.StyleIDs.fistfulOfNades, MonoSingleton<GunControl>.Instance.currentWeapon, null);
                    }
                }
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Explosion))]
    [HarmonyPatch("Collide")]
    class Explosion_Collide_Patch
    {
        static bool Prefix(Explosion __instance, Collider __0)
        {
            GrenadeParriedFlag flag = __instance.gameObject.GetComponent<GrenadeParriedFlag>();
            if (flag == null || flag.registeredStyle)
                return true;

            

            if (!flag.registeredStyle && __0.gameObject.tag != "Player" && (__0.gameObject.layer == 10 || __0.gameObject.layer == 11)
                && __instance.canHit != AffectedSubjects.PlayerOnly)
            {
                EnemyIdentifierIdentifier componentInParent = __0.GetComponentInParent<EnemyIdentifierIdentifier>();
                if(flag.grenadeType == GrenadeParriedFlag.GrenadeType.Rocket && componentInParent != null && componentInParent.eid != null && !componentInParent.eid.blessed && !componentInParent.eid.dead)
                {
                    flag.registeredStyle = true;
                    MonoSingleton<StyleHUD>.Instance.AddPoints(25, Plugin.StyleIDs.rocketBoost, flag.weapon, null);
                }
            }

            return true;
        }
    }
}
