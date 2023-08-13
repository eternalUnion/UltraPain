using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UltrapainExtensions;
using UnityEngine;

namespace Ultrapain.Patches
{
    public class IdolFlag : MonoBehaviour
    {

    }

    [UltrapainPatch]
    [HarmonyPatch(typeof(Idol))]
    public static class IdolPatch
    {
		[HarmonyPatch(nameof(Idol.Death))]
		[HarmonyPostfix]
		[UltrapainPatch]
        public static void ExplodeOnDeath(Idol __instance)
		{
			if (!__instance.gameObject.GetComponent<IdolFlag>())
				return;

			if (ConfigManager.idolExplodionType.value == ConfigManager.IdolExplosionType.Sand)
			{
				GameObject.Instantiate(Plugin.sandExplosion, __instance.transform.position, Quaternion.identity);
				return;
			}

			GameObject tempExplosion = null;
			switch (ConfigManager.idolExplodionType.value)
			{
				case ConfigManager.IdolExplosionType.Normal:
					tempExplosion = Plugin.explosion.obj;
					break;

				case ConfigManager.IdolExplosionType.Big:
					tempExplosion = Plugin.bigExplosion;
					break;

				case ConfigManager.IdolExplosionType.Ligthning:
					tempExplosion = Plugin.lightningStrikeExplosive.obj;
					break;

				case ConfigManager.IdolExplosionType.Sisyphean:
					tempExplosion = Plugin.sisyphiusPrimeExplosion;
					break;
			}

			GameObject explosion = GameObject.Instantiate(tempExplosion, __instance.gameObject.transform.position, Quaternion.identity);
			foreach (Explosion exp in explosion.GetComponentsInChildren<Explosion>())
			{
				exp.enemy = true;
				exp.canHit = AffectedSubjects.All;
				exp.hitterWeapon = "";
				exp.maxSize *= ConfigManager.idolExplosionSizeMultiplier.value;
				exp.speed *= ConfigManager.idolExplosionSizeMultiplier.value;
				exp.damage = (int)(exp.damage * ConfigManager.idolExplosionDamageMultiplier.value);
				exp.enemyDamageMultiplier = ConfigManager.idolExplosionEnemyDamagePercent.value / 100f;
			}
		}

		public static bool ExplodeOnDeathCheck()
		{
			return ConfigManager.enemyTweakToggle.value && ConfigManager.idolExplosionToggle.value;
		}

		[HarmonyPatch(nameof(Idol.Start))]
		[HarmonyPostfix]
		[UltrapainPatch]
		public static void AddFlag(Idol __instance)
		{
			if (__instance.gameObject.GetComponent<NonUltrapainEnemy>() != null)
				return;

			__instance.gameObject.AddComponent<IdolFlag>();
		}

		public static bool AddFlagCheck()
		{
			return ConfigManager.enemyTweakToggle.value;
		}
	}
}
