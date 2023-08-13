using HarmonyLib;
using System;
using System.ComponentModel;
using UltrapainExtensions;
using UnityEngine;

namespace Ultrapain.Patches
{
    public class MaliciousFaceFlag : MonoBehaviour
    {
        public bool charging = false;
    }

    [UltrapainPatch]
    [HarmonyPatch(typeof(SpiderBody))]
    public static class MaliciousFacePatch
    {
        [HarmonyPatch(nameof(SpiderBody.ChargeBeam))]
        [HarmonyPostfix]
        [UltrapainPatch]
        public static void DetectCharge(SpiderBody __instance)
		{
			if (__instance.TryGetComponent(out MaliciousFaceFlag flag))
				flag.charging = true;
		}

        public static bool DetectChargeCheck()
        {
            return ConfigManager.enemyTweakToggle.value;
        }

		[HarmonyPatch(nameof(SpiderBody.BeamChargeEnd))]
		[HarmonyPrefix]
		[UltrapainPatch]
		public static bool ChangeBeamCount(SpiderBody __instance)
		{
			if (__instance.TryGetComponent(out MaliciousFaceFlag flag) && flag.charging)
			{
				if (__instance.health < __instance.maxHealth / 2)
					__instance.beamsAmount = ConfigManager.maliciousFaceBeamCountEnraged.value;
				else
					__instance.beamsAmount = ConfigManager.maliciousFaceBeamCountNormal.value;

				flag.charging = false;
			}

			return true;
		}
	
        public static bool ChangeBeamCountCheck()
        {
            return ConfigManager.enemyTweakToggle.value;
        }

		[HarmonyPatch(nameof(SpiderBody.ShootProj))]
		[HarmonyPostfix]
		[UltrapainPatch]
		public static void ConfigureProjectile(SpiderBody __instance)
		{
			if (__instance.gameObject.GetComponent<MaliciousFaceFlag>() == null)
				return;

			Projectile proj = __instance.currentProj.GetComponent<Projectile>();
			proj.target = MonoSingleton<PlayerTracker>.Instance.GetTarget();
			proj.speed = ConfigManager.maliciousFaceHomingProjectileSpeed.value;
			proj.turningSpeedMultiplier = ConfigManager.maliciousFaceHomingProjectileTurnSpeed.value;
			proj.damage = ConfigManager.maliciousFaceHomingProjectileDamage.value;
			proj.safeEnemyType = EnemyType.MaliciousFace;
			proj.speed *= __instance.eid.totalSpeedModifier;
			proj.damage *= __instance.eid.totalDamageModifier;
			__instance.currentProj.SetActive(true);
		}

        public static bool ConfigureProjectileCheck()
        {
            return ConfigManager.enemyTweakToggle.value && ConfigManager.maliciousFaceHomingProjectileToggle.value;
        }

		[HarmonyPatch(nameof(SpiderBody.Enrage))]
		[HarmonyPostfix]
		[UltrapainPatch]
		public static void BuffOnEnrage(SpiderBody __instance)
		{
            if (__instance.gameObject.GetComponent<MaliciousFaceFlag>() == null)
                return;

			EnemyIdentifier comp = __instance.GetComponent<EnemyIdentifier>();
			for (int i = 0; i < ConfigManager.maliciousFaceRadianceAmount.value; i++)
				comp.BuffAll();
			comp.UpdateBuffs(false);
		}

        public static bool BuffOnEnrageCheck()
        {
            return ConfigManager.enemyTweakToggle.value && ConfigManager.maliciousFaceRadianceOnEnrage.value;
        }

		[HarmonyPatch(nameof(SpiderBody.Start))]
		[HarmonyPostfix]
		[UltrapainPatch]
		public static void AddFlag(SpiderBody __instance)
		{
			if (__instance.gameObject.GetComponent<NonUltrapainEnemy>() != null)
				return;

			__instance.gameObject.AddComponent<MaliciousFaceFlag>();

			if (ConfigManager.maliciousFaceHomingProjectileToggle.value)
			{
				__instance.proj = Plugin.homingProjectile.obj;
				__instance.maxBurst = Math.Max(0, ConfigManager.maliciousFaceHomingProjectileCount.value - 1);
			}
		}

		public static bool AddFlagCheck()
		{
			return ConfigManager.enemyTweakToggle.value;
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
