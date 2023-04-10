using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace DifficultyTweak.Patches
{
    [HarmonyPatch(typeof(Stalker), nameof(Stalker.SandExplode))]
    public class Stalker_SandExplode_Patch
    {
        static bool Prefix(Stalker __instance, ref int ___difficulty, ref EnemyIdentifier ___eid)
        {
            ___difficulty = 4;

            return true;
        }
    }
}
