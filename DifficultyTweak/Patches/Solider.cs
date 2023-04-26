using HarmonyLib;
using UnityEngine;

namespace DifficultyTweak.Patches
{
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
        }
    }

    class Solider_SpawnProjectile_Patch
    {
        static void Postfix(ZombieProjectiles __instance, ref EnemyIdentifier ___eid, ref GameObject ___origWP)
        {
            if (___eid.enemyType != EnemyType.Soldier)
                return;

            ___eid.weakPoint = null;
        }
    }

    class SoliderGrenadeFlag : MonoBehaviour
    {
        public GameObject tempExplosion;
    }

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

                if(counter.remainingShots == 0 && ConfigManager.soliderShootGrenadeToggle.value)
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
                    grenade.AddComponent<SoliderGrenadeFlag>();
                }
                return;
            }

            counter.remainingShots = ConfigManager.soliderShootCount.value;
        }
    }

    class Grenade_Explode_Patch
    {
        static bool Prefix(Grenade __instance, out bool __state)
        {
            __state = false;
            SoliderGrenadeFlag flag = __instance.GetComponent<SoliderGrenadeFlag>();
            if (flag == null)
                return true;

            flag.tempExplosion = GameObject.Instantiate(__instance.explosion);
            __state = true;
            foreach(Explosion e in flag.tempExplosion.GetComponentsInChildren<Explosion>())
            {
                e.damage = ConfigManager.soliderGrenadeDamage.value;
                e.maxSize *= ConfigManager.soliderGrenadeSize.value;
                e.speed *= ConfigManager.soliderGrenadeSize.value;
            }
            __instance.explosion = flag.tempExplosion;

            return true;
        }

        static void Postfix(Grenade __instance, bool __state)
        {
            if (!__state)
                return;

            SoliderGrenadeFlag flag = __instance.GetComponent<SoliderGrenadeFlag>();
            GameObject.Destroy(flag.tempExplosion);
        }
    }

    class SoliderShootCounter : MonoBehaviour
    {
        public int remainingShots = ConfigManager.soliderShootCount.value;
    }
}
