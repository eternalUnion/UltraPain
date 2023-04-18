using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DifficultyTweak.Patches
{
    [HarmonyPatch(typeof(MinosPrime), "ProjectileCharge")]
    class MinosPrimeCharge
    {
        static void Postfix(MinosPrime __instance)
        {
            Transform player = MonoSingleton<PlayerTracker>.Instance.GetPlayer();

            float min = ConfigManager.minosPrimeRandomTeleportMinDistance.value;
            float max = ConfigManager.minosPrimeRandomTeleportMaxDistance.value;

            Vector3 unitSphere = UnityEngine.Random.onUnitSphere;
            unitSphere.y = Mathf.Abs(unitSphere.y);
            float distance = UnityEngine.Random.Range(min, max);

            Ray ray = new Ray(player.position, unitSphere);

            LayerMask mask = new LayerMask();
            mask.value |= 256 | 16777216;
            if (Physics.Raycast(ray, out RaycastHit hit, max, mask, QueryTriggerInteraction.Ignore))
            {
                if (hit.distance < min)
                    return;
                __instance.Teleport(ray.GetPoint(hit.distance - 5), __instance.transform.position);
            }
            else
                __instance.Teleport(ray.GetPoint(distance), __instance.transform.position);
        }
    }
}
