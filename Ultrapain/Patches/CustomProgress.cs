using HarmonyLib;
using System;
using System.Collections.Generic;

namespace Ultrapain.Patches
{
    [HarmonyPatch(typeof(PrefsManager))]
    class PrefsManagerPatch
    {
        [HarmonyPatch(MethodType.Constructor)]
        [HarmonyPostfix]
        static void Postfix(PrefsManager __instance)
        {
            __instance.propertyValidators.Clear();
        }

        [HarmonyPatch(nameof(PrefsManager.EnsureValid))]
        [HarmonyPrefix]
		static bool Prefix(string __0, object __1, ref object __result)
		{
			__result = __1;
			return false;
		}
	}
}
