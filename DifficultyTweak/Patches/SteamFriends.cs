using HarmonyLib;
using Steamworks;

namespace DifficultyTweak.Patches
{
    [HarmonyPatch(typeof(SteamFriends), nameof(SteamFriends.SetRichPresence))]
    class SteamFriends_SetRichPresence_Patch
    {
        static bool Prefix(string __0, ref string __1)
        {
            if (__1.ToLower() != "ukmd")
                return true;

            __1 = ConfigManager.pluginName.value;
            return true;
        }
    }
}
