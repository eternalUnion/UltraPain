using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DifficultyTweak.Patches
{
    class CerberusFlag : MonoBehaviour
    {
        public int extraDashesRemaining = ConfigManager.cerberusTotalDashCount.value;
    }

    class StatueBoss_StopDash_Patch
    {
        public static void Postfix(StatueBoss __instance, ref int ___tackleChance)
        {
            CerberusFlag flag = __instance.GetComponent<CerberusFlag>();
            if (flag == null)
                return;

            if (flag.extraDashesRemaining > 0)
            {
                flag.extraDashesRemaining -= 1;
                __instance.SendMessage("Tackle");
                ___tackleChance -= 20;
            }
            else
                flag.extraDashesRemaining = ConfigManager.cerberusTotalDashCount.value - 1;
        }
    }

    class StatueBoss_Start_Patch
    {
        static void Postfix(StatueBoss __instance)
        {
            __instance.gameObject.AddComponent<CerberusFlag>();
        }
    }
}
