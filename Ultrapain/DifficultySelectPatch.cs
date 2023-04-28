using HarmonyLib;
using UnityEngine.UI;

namespace Ultrapain
{
    class DifficultySelectPatch
    {
        static void Postfix(DifficultySelectButton __instance)
        {
            string difficultyName = __instance.transform.Find("Name").GetComponent<Text>().text;
            Plugin.ultrapainDifficulty = difficultyName == ConfigManager.pluginName.value || ConfigManager.globalDifficultySwitch.value;
            Plugin.realUltrapainDifficulty = difficultyName == ConfigManager.pluginName.value;
        }
    }
}
