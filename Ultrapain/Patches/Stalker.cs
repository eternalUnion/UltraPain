using HarmonyLib;

namespace Ultrapain.Patches
{
    public class Stalker_SandExplode_Patch
    {
        static bool Prefix(Stalker __instance, ref int ___difficulty, ref EnemyIdentifier ___eid)
        {
            if (!(StockMapInfo.Instance != null && StockMapInfo.Instance.levelName == "GOD DAMN THE SUN"
                && __instance.transform.parent != null && __instance.transform.parent.name == "Wave 1"
                && __instance.transform.parent.parent != null && __instance.transform.parent.parent.name.StartsWith("5 Stuff")))
                ___difficulty = 4;
            else
                ___difficulty = 3;
            return true;
        }
    }
}
