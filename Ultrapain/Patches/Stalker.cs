using HarmonyLib;

namespace Ultrapain.Patches
{
    public class Stalker_SandExplode_Patch
    {
        static bool Prefix(Stalker __instance, ref int ___difficulty, ref EnemyIdentifier ___eid)
        {
            ___difficulty = 4;
            return true;
        }
    }
}
