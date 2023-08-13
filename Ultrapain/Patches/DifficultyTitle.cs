using HarmonyLib;
using UnityEngine.UI;

namespace Ultrapain.Patches
{
    [HarmonyPatch(typeof(DifficultyTitle))]
    public class DifficultyTitlePatch
    {
        [HarmonyPatch(nameof(DifficultyTitle.Check))]
        [HarmonyPostfix]
        static void Postfix(DifficultyTitle __instance)
        {
            if (__instance.txt.text.Contains("ULTRAKILL MUST DIE") && Plugin.realUltrapainDifficulty)
                __instance.txt.text = __instance.txt.text.Replace("ULTRAKILL MUST DIE", ConfigManager.pluginName.value);
        }
    }
}
