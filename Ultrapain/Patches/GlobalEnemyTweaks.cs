using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Ultrapain.Patches
{
    class EnemyIdentifier_DeliverDamage_FF
    {
        [HarmonyBefore]
        static bool Prefix(EnemyIdentifier __instance, ref float __3)
        {
            if(__instance.hitter == "enemy" || __instance.hitter == "ffexplosion")
            {
                __3 *= ConfigManager.friendlyFireDamageOverride.normalizedValue;
            }

            return true;
        }
    }
}
