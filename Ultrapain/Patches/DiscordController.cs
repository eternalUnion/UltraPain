using Discord;
using HarmonyLib;
using System.Text.RegularExpressions;


namespace Ultrapain.Patches
{
    class DiscordController_SendActivity_Patch
    {
        static bool Prefix(DiscordController __instance, ref Activity ___cachedActivity)
        {
            if (___cachedActivity.State != null && ___cachedActivity.State == "DIFFICULTY: UKMD")
            {
                Regex rich = new Regex(@"<[^>]*>");
                string text = $"DIFFICULTY: {ConfigManager.pluginName.value}";
                if (rich.IsMatch(text))
                {
                    ___cachedActivity.State = rich.Replace(text, string.Empty);
                }
            }
            return true;
        }
    }
}
