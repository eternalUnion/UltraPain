using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using UnityEngine;

namespace DifficultyTweak.Patches
{
    [HarmonyPatch(typeof(ZombieProjectiles))]
    [HarmonyPatch("Start")]
    class Solider_Start_Patch
    {
        static void Postfix(ZombieProjectiles __instance, ref GameObject ___decProjectile, ref GameObject ___projectile, ref EnemyIdentifier ___eid, ref Animator ___anim)
        {
            if (___eid.enemyType != EnemyType.Soldier)
                return;

            /*___projectile = Plugin.soliderBullet;

            if (Plugin.decorativeProjectile2.gameObject != null)
                ___decProjectile = Plugin.decorativeProjectile2.gameObject;*/

            __instance.gameObject.AddComponent<SoliderShootCounter>();
            ___eid.weakPoint = null;
        }
    }

    [HarmonyPatch(typeof(ZombieProjectiles))]
    [HarmonyPatch("SpawnProjectile")]
    class Solider_SpawnProjectile_Patch
    {
        static void Postfix(ZombieProjectiles __instance, ref EnemyIdentifier ___eid, ref GameObject ___origWP)
        {
            ___eid.weakPoint = null;
        }
    }

    [HarmonyPatch(typeof(ZombieProjectiles))]
    [HarmonyPatch("ThrowProjectile")]
    class Solider_ThrowProjectile_Patch
    {
        static void Postfix(ZombieProjectiles __instance, ref GameObject ___currentProjectile, ref EnemyIdentifier ___eid, ref GameObject ___player, ref Animator ___anim, ref float ___coolDown)
        {
            if (___eid.enemyType != EnemyType.Soldier)
                return;

            ___currentProjectile.GetComponent<ProjectileSpread>().spreadAmount = 10;
            ___currentProjectile.SetActive(true);

            SoliderShootCounter counter = __instance.gameObject.GetComponent<SoliderShootCounter>();
            if (counter.remainingShots > 0)
            {
                counter.remainingShots -= 1;

                ___anim.Play("Shoot", 0, Plugin.SoliderShootAnimationStart / 2f);
                ___anim.fireEvents = true;
                __instance.DamageStart();

                ___coolDown = 0;

                if(counter.remainingShots == 0)
                {
                    GameObject grenade = GameObject.Instantiate(Plugin.shotgunGrenade.gameObject, ___currentProjectile.transform.position, ___currentProjectile.transform.rotation);
                    grenade.transform.Translate(Vector3.forward * 0.5f);

                    Vector3 targetPos = Plugin.PredictPlayerPosition(__instance.GetComponent<Collider>(), ___eid.totalSpeedModifier);
                    grenade.transform.LookAt(targetPos);

                    Rigidbody rb = grenade.GetComponent<Rigidbody>();
                    //rb.maxAngularVelocity = 10000;
                    //foreach (Rigidbody r in grenade.GetComponentsInChildren<Rigidbody>())
                    //    r.maxAngularVelocity = 10000;
                    rb.AddForce(grenade.transform.forward * Plugin.SoliderGrenadeForce);
                    //rb.velocity = ___currentProjectile.transform.forward * Plugin.instance.SoliderGrenadeForce;
                    rb.useGravity = false;

                    grenade.GetComponent<Grenade>().enemy = true;
                    grenade.GetComponent<Grenade>().CanCollideWithPlayer(true);
                }
                return;
            }

            counter.remainingShots = 2;
        }
    }

    class SoliderShootCounter : MonoBehaviour
    {
        public int remainingShots = 2;
    }
}
