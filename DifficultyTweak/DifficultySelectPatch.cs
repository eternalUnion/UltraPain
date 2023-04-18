using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UI;

namespace DifficultyTweak
{
    [HarmonyPatch(typeof(DifficultySelectButton), nameof(DifficultySelectButton.SetDifficulty))]
    public class DifficultySelectPatch
    {
        static void Postfix(DifficultySelectButton __instance)
        {
            string difficultyName = __instance.transform.Find("Name").GetComponent<Text>().text;
            Plugin.ultrapainDifficulty = difficultyName == ConfigManager.pluginName.value || ConfigManager.globalDifficultySwitch.value;
        }
    }
}
