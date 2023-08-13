using HarmonyLib;
using UnityEngine.UI;

namespace Ultrapain
{
    [HarmonyPatch(typeof(DifficultySelectButton))]
    class DifficultySelectPatch
    {
        [HarmonyPatch(nameof(DifficultySelectButton.SetDifficulty))]
        [HarmonyPostfix]
        static void PostDifficultySelect(DifficultySelectButton __instance)
        {
            string difficultyName = __instance.transform.Find("Name").GetComponent<Text>().text;
            Plugin.realUltrapainDifficulty = difficultyName == ConfigManager.pluginName.value;
            Plugin.ultrapainDifficulty = Plugin.realUltrapainDifficulty || ConfigManager.globalDifficultySwitch.value;
        }
    }
}
