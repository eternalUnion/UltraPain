using Discord;
using HarmonyLib;
using System.Text.RegularExpressions;
using UltrapainExtensions;

namespace Ultrapain.Patches
{
    [UltrapainPatch]
    [HarmonyPatch(typeof(DiscordController))]
    public static class DiscordControllerPatch
    {
        [HarmonyPatch(nameof(DiscordController.SendActivity))]
        [HarmonyPrefix]
        [UltrapainPatch]
        public static bool ChangeRichPresenceDifficulty(DiscordController __instance)
        {
            if (!Plugin.realUltrapainDifficulty)
                return true;

            if (__instance.cachedActivity.State != null && __instance.cachedActivity.State == "DIFFICULTY: UKMD")
            {
                Regex rich = new Regex(@"<[^>]*>");
                string text = $"DIFFICULTY: {ConfigManager.pluginName.value}";
                if (rich.IsMatch(text))
                {
					__instance.cachedActivity.State = rich.Replace(text, string.Empty);
                }
                else
                {
					__instance.cachedActivity.State = text;
                }
            }
            return true;
        }
    
        public static bool ChangeRichPresenceDifficultyCheck()
        {
            return ConfigManager.discordRichPresenceToggle.value;
        }
	}
}
