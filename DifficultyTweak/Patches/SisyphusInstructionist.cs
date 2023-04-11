using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DifficultyTweak.Patches
{
    /*public class SisyphusInstructionistFlag : MonoBehaviour
    {

    }

    [HarmonyPatch(typeof(Sisyphus), nameof(Sisyphus.Knockdown))]
    public class SisyphusInstructionist_Knockdown_Patch
    {
        static void Postfix(Sisyphus __instance, ref EnemyIdentifier ___eid)
        {
            SisyphusInstructionistFlag flag = __instance.GetComponent<SisyphusInstructionistFlag>();
            if (flag != null)
                return;

            __instance.gameObject.AddComponent<SisyphusInstructionistFlag>();

            foreach(EnemySimplifier esi in UnityUtils.GetComponentsInChildrenRecursively<EnemySimplifier>(__instance.transform))
            {
                esi.enraged = true;
            }
            GameObject effect = GameObject.Instantiate(Plugin.enrageEffect, __instance.transform);
            effect.transform.localScale = Vector3.one * 0.2f;
        }
    }*/
}
