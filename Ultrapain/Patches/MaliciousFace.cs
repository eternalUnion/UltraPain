using HarmonyLib;
using System;
using UnityEngine;

namespace Ultrapain.Patches
{
    class MaliciousFace_Start_Patch
    {
        static void Postfix(SpiderBody __instance, ref GameObject ___proj, ref int ___maxBurst)
        {
            ___proj = Plugin.homingProjectile;
            ___maxBurst = Math.Max(0, ConfigManager.maliciousFaceHomingProjectileCount.value - 1);
        }
    }

    class MaliciousFace_ShootProj_Patch
    {
        /*static bool Prefix(SpiderBody __instance, ref GameObject ___proj, out bool __state)
        {
            __state = false;
            if (!Plugin.ultrapainDifficulty || !ConfigManager.enemyTweakToggle.value || !ConfigManager.maliciousFaceHomingProjectileToggle.value)
                return true;

            ___proj = Plugin.homingProjectile;
            __state = true;

            return true;
        }*/

        static void Postfix(SpiderBody __instance, ref GameObject ___currentProj/*, bool __state*/)
        {
            /*if (!__state)
                return;*/

            Projectile proj = ___currentProj.GetComponent<Projectile>();
            proj.target = MonoSingleton<PlayerTracker>.Instance.GetTarget();
            proj.speed = ConfigManager.maliciousFaceHomingProjectileSpeed.value;
            proj.turningSpeedMultiplier = ConfigManager.maliciousFaceHomingProjectileTurnSpeed.value;
            proj.damage = ConfigManager.maliciousFaceHomingProjectileDamage.value;
            ___currentProj.SetActive(true);
        }
    }

    class MaliciousFace_Enrage_Patch
    {
        static void Postfix(SpiderBody __instance)
        {
            EnemyIdentifier comp = __instance.GetComponent<EnemyIdentifier>();
            for(int i = 0; i < ConfigManager.maliciousFaceRadianceAmount.value; i++)
                comp.BuffAll();
            comp.UpdateBuffs(false);

            //__instance.spark = new GameObject();
        }
    }

    /*[HarmonyPatch(typeof(SpiderBody))]
    [HarmonyPatch("BeamChargeEnd")]
    class MaliciousFace_BeamChargeEnd_Patch
    {
        static void Postfix(SpiderBody __instance, ref bool ___parryable)
        {
            if (__instance.currentEnrageEffect == null)
                return;
            ___parryable = false;
        }
    }*/

    /*[HarmonyPatch(typeof(HookArm))]
    [HarmonyPatch("FixedUpdate")]
    class HookArm_FixedUpdate_MaliciousFacePatch
    {
        static void Postfix(HookArm __instance, ref EnemyIdentifier ___caughtEid)
        {
            if (__instance.state == HookState.Caught && ___caughtEid.enemyType == EnemyType.MaliciousFace)
            {
                if (___caughtEid.GetComponent<SpiderBody>().currentEnrageEffect == null)
                    return;

                //__instance.state = HookState.Pulling;
                ___caughtEid = null;
                __instance.StopThrow();
            }
        }
    }*/
}
