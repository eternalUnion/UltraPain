using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Ultrapain.Patches
{
    class SisyphusPrime_Start
    {
        static void Postfix(SisyphusPrime __instance)
        {
            if (ConfigManager.sisPrimeEarlyPhase2.value)
            {
                __instance.enraged = true;
                __instance.currentPassiveEffect = GameObject.Instantiate(__instance.passiveEffect, __instance.transform.position + Vector3.up * 3.5f, Quaternion.identity);
                __instance.currentPassiveEffect.transform.SetParent(__instance.transform);
                foreach (EnemyIdentifierIdentifier enemyIdentifierIdentifier in __instance.GetComponentsInChildren<EnemyIdentifierIdentifier>())
                {
                    GameObject.Instantiate(__instance.flameEffect, enemyIdentifierIdentifier.transform);
                }
                GameObject.Instantiate(__instance.phaseChangeEffect, __instance.mach.chest.transform.position, Quaternion.identity);
                UltrakillEvent ultrakillEvent = __instance.onPhaseChange;
                if (ultrakillEvent != null)
                    ultrakillEvent.Invoke();
            }
        }
    }

    class SisyphusPrime_Parryable
    {
        static bool Prefix(SisyphusPrime __instance)
        {
            if (__instance.enraged)
            {
                if (ConfigManager.sisPrimeUnparryablePhase2.value)
                    return false;
            }
            else
            {
                if (ConfigManager.sisPrimeUnparryablePhase1.value)
                    return false;
            }

            return true;
        }
    }
}
