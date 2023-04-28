using HarmonyLib;
using Steamworks;

namespace Ultrapain.Patches
{
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
