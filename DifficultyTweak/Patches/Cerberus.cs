using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DifficultyTweak.Patches
{
    class CerberusFlag : MonoBehaviour
    {
        public int extraDashesRemaining = 2;
    }

    [HarmonyPatch(typeof(StatueBoss), "StopDash")]
    class StatueBoss_StopDash_Patch
    {
        static void Postfix(StatueBoss __instance)
        {
            CerberusFlag flag = __instance.GetComponent<CerberusFlag>();
            if (flag == null)
                return;

            if (flag.extraDashesRemaining > 0)
            {
                flag.extraDashesRemaining -= 1;
                __instance.SendMessage("Tackle");
            }
            else
                flag.extraDashesRemaining = 2;
        }
    }

    [HarmonyPatch(typeof(StatueBoss), "Start")]
    class StatueBoss_Start_Patch
    {
        static void Postfix(StatueBoss __instance)
        {
            __instance.gameObject.AddComponent<CerberusFlag>();
        }
    }
}
