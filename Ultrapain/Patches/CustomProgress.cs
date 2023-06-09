﻿using System;
using System.Collections.Generic;

namespace Ultrapain.Patches
{
    /*[HarmonyPatch(typeof(GameProgressSaver), "GetGameProgress", new Type[] { typeof(int) })]
    class CustomProgress_GetGameProgress
    {
        static bool Prefix(ref int __0)
        {
            if (Plugin.ultrapainDifficulty)
                __0 = 100;
            return true;
        }
    }

    [HarmonyPatch(typeof(GameProgressSaver), "GetGameProgress", new Type[] { typeof(string), typeof(int) }, new ArgumentType[] { ArgumentType.Out, ArgumentType.Normal })]
    class CustomProgress_GetGameProgress2
    {
        static bool Prefix(ref int __1)
        {
            if (Plugin.ultrapainDifficulty)
                __1 = 100;
            return true;
        }
    }

    [HarmonyPatch(typeof(GameProgressSaver), "GetPrime")]
    class CustomProgress_GetPrime
    {
        static bool Prefix(ref int __1)
        {
            if (Plugin.ultrapainDifficulty)
                __1 = 100;
            return true;
        }
    }

    [HarmonyPatch(typeof(GameProgressSaver), "GetPrime")]
    class CustomProgress_GetProgress
    {
        static bool Prefix(ref int __0)
        {
            if (Plugin.ultrapainDifficulty)
                __0 = 100;
            return true;
        }
    }

    [HarmonyPatch(typeof(RankData), MethodType.Constructor, new Type[] { typeof(StatsManager) })]
    class CustomProgress_RankDataCTOR
    {
        static bool Prefix(RankData __instance, out int __state)
        {
            __state = -1;
            if (Plugin.ultrapainDifficulty)
            {
                __state = MonoSingleton<PrefsManager>.Instance.GetInt("difficulty", 0);
                MonoSingleton<PrefsManager>.Instance.SetInt("difficulty", 100);
            }

            return true;
        }

        static void Postfix(RankData __instance, int __state)
        {
            if (__state >= 0)
            {
                MonoSingleton<PrefsManager>.Instance.SetInt("difficulty", __state);
            }
        }
    }*/

    /*[HarmonyPatch(typeof(PrefsManager), "GetInt")]
    class StatsManager_DifficultyOverride
    {
        static bool Prefix(string __0, ref int __result)
        {
            if (__0 == "difficulty" && Plugin.realUltrapainDifficulty)
            {
                __result = 5;
                return false;
            }

            return true;
        }
    }*/

    class PrefsManager_Ctor
    {
        static void Postfix(ref Dictionary<string, Func<object, object>> ___propertyValidators)
        {
            ___propertyValidators.Clear();
        }
    }

    class PrefsManager_EnsureValid
    {
        static bool Prefix(string __0, object __1, ref object __result)
        {
            __result = __1;
            return false;
        }
    }
}
