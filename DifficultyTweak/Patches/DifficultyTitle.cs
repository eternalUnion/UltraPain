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
            if (___txt.text.Contains("VIOLENT") && Plugin.ultrapainDifficulty)
                ___txt.text = ___txt.text.Replace("VIOLENT", ConfigManager.pluginName.value);

            //if (___txt.text == "-- VIOLENT --" && Plugin.ultrapainDifficulty)
            //    ___txt.text = "-- ULTRAPAIN --";
        }
    }
}
