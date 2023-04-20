using Discord;
using HarmonyLib;

namespace DifficultyTweak.Patches
{
    [HarmonyPatch(typeof(DiscordController), "SendActivity")]
    class DiscordController_SendActivity_Patch
    {
        static bool Prefix(DiscordController __instance, ref Activity ___cachedActivity)
        {
            if (___cachedActivity.State != null && ___cachedActivity.State == "DIFFICULTY: UKMD")
                ___cachedActivity.State = $"DIFFICULTY: {ConfigManager.pluginName.value}";
            return true;
        }
    }
}
