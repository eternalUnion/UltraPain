using HarmonyLib;
using Steamworks;
using System.Text.RegularExpressions;

namespace Ultrapain.Patches
{
    class SteamFriends_SetRichPresence_Patch
    {
        static bool Prefix(string __0, ref string __1)
        {
            if (__1.ToLower() != "ukmd")
                return true;

            Regex rich = new Regex(@"<[^>]*>");
            string text = ConfigManager.pluginName.value;
            if (rich.IsMatch(text))
            {
                __1 = rich.Replace(text, string.Empty);
            }
            else
            {
                __1 = text;
            }

            return true;
        }
    }
}
