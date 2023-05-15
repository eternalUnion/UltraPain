using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Ultrapain.Patches
{
    class RocketZoneFlag : MonoBehaviour
    {

    }

    class RocketLauncher_Shoot_Patch
    {
        static GameObject normalRocket;
        static LayerMask shotgunZoneLayerMask = new LayerMask() { m_Mask = 3072 };

        static bool Prefix(RocketLauncher __instance, out GameObject __state)
        {
            __state = null;
            bool parry = false;

            RaycastHit[] rhits = Physics.RaycastAll(MonoSingleton<CameraController>.Instance.transform.position, MonoSingleton<CameraController>.Instance.transform.forward, ConfigManager.rocketParryMaxDistance.value, shotgunZoneLayerMask);
            if (rhits.Length != 0)
            {
                foreach (RaycastHit raycastHit in rhits)
                {
                    if (raycastHit.collider.gameObject.tag == "Body")
                    {
                        EnemyIdentifierIdentifier componentInParent = raycastHit.collider.GetComponentInParent<EnemyIdentifierIdentifier>();
                        if (componentInParent)
                        {
                            parry = true;
                            EnemyIdentifier eid = componentInParent.eid;
                            eid.hitter = "shotgunzone";
                            eid.DeliverDamage(raycastHit.collider.gameObject, (eid.transform.position - __instance.transform.position).normalized * 10000f, raycastHit.point, ConfigManager.rocketParryDirectDamage.value, false, 0f, __instance.gameObject, false);
                        }
                    }
                }
            }

            if (!parry || ConfigManager.rocketParryExplosionType.value == ConfigManager.RocketParryExplosionType.NoEffect)
                return true;

            GameObject rocketClone = __state = GameObject.Instantiate(__instance.rocket, new Vector3(1000000, 1000000, 1000000), Quaternion.identity);
            rocketClone.AddComponent<RocketZoneFlag>();
            if (normalRocket == null)
                normalRocket = __instance.rocket;
            __instance.rocket = rocketClone;

            return true;
        }

        static void Postfix(RocketLauncher __instance, GameObject __state)
        {
            if(__state != null)
            {
                GameObject.Destroy(__state);
                __instance.rocket = normalRocket;
            }
        }
    }

    class Grenade_Explode_RocketParry
    {
        static bool Prefix(Grenade __instance, out GameObject __state)
        {
            __state = null;
            if (!__instance.rocket)
                return true;

            if (__instance.GetComponent<RocketZoneFlag>() == null)
                return true;

            GameObject expTemplate = ConfigManager.rocketParryExplosionType.value == ConfigManager.RocketParryExplosionType.BigExplosion ? __instance.superExplosion : __instance.explosion;
            GameObject exp = __state = GameObject.Instantiate(expTemplate, new Vector3(1000000, 1000000, 1000000), Quaternion.identity);
            foreach(Explosion e in exp.GetComponentsInChildren<Explosion>())
            {
                e.maxSize *= ConfigManager.rocketParryExplosionSizeMultiplier.value;
                e.speed *= ConfigManager.rocketParryExplosionSizeMultiplier.value;
                e.damage = (int)(e.damage * ConfigManager.rocketParryExplosionDamageMultiplier.value);
            }

            __instance.explosion = __instance.superExplosion = exp;
            return true;
        }

        static void Postfix(GameObject __state)
        {
            if (__state != null)
                GameObject.Destroy(__state);
        }
    }
}
