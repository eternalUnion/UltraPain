using HarmonyLib;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Text;

namespace DifficultyTweak.Patches
{
    [HarmonyPatch(typeof(SteamFriends), nameof(SteamFriends.SetRichPresence))]
    class SteamFriends_SetRichPresence_Patch
    {
        static bool Prefix(string __0, ref string __1)
        {
            if (__0 != "difficulty" || !Plugin.ultrapainDifficulty || !ConfigManager.steamRichPresenceToggle.value)
                return true;

            if (__1.ToLower() != "violent")
                return true;

            __1 = ConfigManager.pluginName.value;
            return true;
        }
    }
}
