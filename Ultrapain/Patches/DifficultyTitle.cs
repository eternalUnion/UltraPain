﻿using HarmonyLib;
using UnityEngine.UI;

namespace Ultrapain.Patches
{
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
