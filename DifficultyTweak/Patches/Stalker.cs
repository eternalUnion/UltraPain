using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.SceneManagement;

namespace DifficultyTweak.Patches
{
    [HarmonyPatch(typeof(Stalker), nameof(Stalker.SandExplode))]
    public class Stalker_SandExplode_Patch
    {
        static bool Prefix(Stalker __instance, ref int ___difficulty, ref EnemyIdentifier ___eid)
        {
            if (!Plugin.ultrapainDifficulty || !ConfigManager.enemyTweakToggle.value || !ConfigManager.stalkerSurviveExplosion.value || SceneManager.GetActiveScene().name == "Level 4-2")
                return true;

            ___difficulty = 4;

            return true;
        }
    }
}
