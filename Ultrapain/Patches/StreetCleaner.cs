using HarmonyLib;
using UnityEngine;

namespace Ultrapain.Patches
{
    class StreetCleaner_Start_Patch
    {
        static void Postfix(Streetcleaner __instance, ref EnemyIdentifier ___eid)
        {
            ___eid.weakPoint = null;
        }
    }

    /*[HarmonyPatch(typeof(Streetcleaner))]
    [HarmonyPatch("StartFire")]
    class StreetCleaner_StartFire_Patch
    {
        static void Postfix(Streetcleaner __instance, ref EnemyIdentifier ___eid)
        {
            __instance.CancelInvoke("StartDamaging");
            __instance.CancelInvoke("StopFire");
            __instance.Invoke("StartDamaging", 0.1f);
        }
    }*/

    /*[HarmonyPatch(typeof(Streetcleaner))]
    [HarmonyPatch("Update")]
    class StreetCleaner_Update_Patch
    {
        static bool cancelStartFireInvoke = false;

        static bool Prefix(Streetcleaner __instance, ref bool ___attacking)
        {
            cancelStartFireInvoke = !___attacking;
            return true;
        }

        static void Postfix(Streetcleaner __instance, ref EnemyIdentifier ___eid)
        {
            if(__instance.IsInvoking("StartFire") && cancelStartFireInvoke)
            {
                __instance.CancelInvoke("StartFire");
                __instance.Invoke("StartFire", 0.1f);
            }
        }
    }*/

    /*[HarmonyPatch(typeof(Streetcleaner))]
    [HarmonyPatch("Dodge")]
    class StreetCleaner_Dodge_Patch
    {
        static bool didDodge = false;

        static bool Prefix(Streetcleaner __instance, ref float ___dodgeCooldown)
        {
            didDodge = !__instance.dead && ___dodgeCooldown == 0;
            return true;
        }

        static void Postfix(Streetcleaner __instance, ref float ___dodgeCooldown)
        {
            if(didDodge)
                ___dodgeCooldown = UnityEngine.Random.Range(0f, 1f);
        }
    }*/

    class BulletCheck_OnTriggerEnter_Patch
    {
        static void Postfix(BulletCheck __instance, Collider __0/*, EnemyIdentifier ___eid*/)
        {
            if (!(__instance.type == CheckerType.Streetcleaner && __0.gameObject.layer == 14))
                return;

            Grenade grn = __0.GetComponent<Grenade>();
            if (grn != null)
            {
                grn.enemy = true;
                grn.CanCollideWithPlayer(true);

                // OLD PREDICTION
                /*Rigidbody rb = __0.GetComponent<Rigidbody>();
                float magnitude = rb.velocity.magnitude;

                //float distance = Vector3.Distance(MonoSingleton<PlayerTracker>.Instance.gameObject.transform.position, __0.transform.position);
                float distance = Vector3.Distance(MonoSingleton<PlayerTracker>.Instance.gameObject.transform.position, __0.transform.position);
                Vector3 predictedPosition = MonoSingleton<PlayerTracker>.Instance.PredictPlayerPosition(1.0f);

                float velocity = Mathf.Clamp(distance, Mathf.Max(magnitude - 5.0f, 0), magnitude + 5);

                __0.transform.LookAt(predictedPosition);
                rb.velocity = __0.transform.forward * velocity;*/

                // NEW PREDICTION
                Vector3 predictedPosition = Tools.PredictPlayerPosition(1);
                __0.transform.LookAt(predictedPosition);
                Rigidbody rb = __0.GetComponent<Rigidbody>();
                rb.velocity = Vector3.zero;
                rb.AddForce(__0.transform.forward * 20000f /* * ___eid.totalSpeedModifier */);
            }
        }
    }
}
