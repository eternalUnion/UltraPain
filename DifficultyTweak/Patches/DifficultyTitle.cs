using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UI;

namespace DifficultyTweak.Patches
{
    [HarmonyPatch(typeof(DifficultyTitle), "Check")]
    public class DifficultyTitle_Check_Patch
    {
        static void Postfix(DifficultyTitle __instance, ref Text ___txt)
        {
            if (___txt.text.Contains("ULTRAKILL MUST DIE") && Plugin.realUltrapainDifficulty)
                ___txt.text = ___txt.text.Replace("ULTRAKILL MUST DIE", ConfigManager.pluginName.value);

            //else if (___txt.text == "-- VIOLENT --" && Plugin.ultrapainDifficulty)
            //    ___txt.text = "-- ULTRAPAIN --";
        }
    }
}
