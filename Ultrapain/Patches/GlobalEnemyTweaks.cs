using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UltrapainExtensions;
using UnityEngine;

namespace Ultrapain.Patches
{
    [UltrapainPatch]
    [HarmonyPatch(typeof(EnemyIdentifier))]
    public static class EnemyIdentifierPatch
    {
        [HarmonyPatch(nameof(EnemyIdentifier.UpdateModifiers))]
        [HarmonyPostfix]
        [UltrapainPatch]
        public static void AddBuffs(EnemyIdentifier __instance)
        {
            NonUltrapainEnemy customFlag = __instance.GetComponent<NonUltrapainEnemy>();

            ConfigManager.EidStatContainer container = ConfigManager.enemyStats[__instance.enemyType];

            if (__instance.enemyType == EnemyType.V2)
            {
                V2 comp = __instance.GetComponent<V2>();
                if (comp != null && comp.secondEncounter)
                {
                    container = ConfigManager.enemyStats[EnemyType.V2Second];
                }
            }
            else if (__instance.enemyType == EnemyType.Filth)
            {
                if (__instance.gameObject.GetComponent<CancerousRodent>() != null)
                {
					container = ConfigManager.enemyStats[EnemyType.CancerousRodent];
				}
            }
			else if (__instance.enemyType == EnemyType.Cerberus)
			{
				if (__instance.gameObject.GetComponent<CancerousRodent>() != null)
				{
					container = ConfigManager.enemyStats[EnemyType.VeryCancerousRodent];
				}
			}

			if (customFlag == null || !customFlag.disableStatEditor)
            {
                __instance.totalHealthModifier *= container.health.value;
                __instance.totalDamageModifier *= container.damage.value;
                __instance.totalSpeedModifier *= container.speed.value;
            }

            if (customFlag == null || !customFlag.disableResistanceEditor)
            {
                List<string> weakness = new List<string>();
                List<float> weaknessMulti = new List<float>();
                foreach (KeyValuePair<string, float> weaknessPair in container.resistanceDict)
                {
                    weakness.Add(weaknessPair.Key);

                    int index = Array.IndexOf(__instance.weaknesses, weaknessPair.Key);
                    if (index >= 0)
                    {
                        float defaultResistance = 1f / __instance.weaknessMultipliers[index];
                        if (defaultResistance > weaknessPair.Value)
                            weaknessMulti.Add(1f / defaultResistance);
                        else
                            weaknessMulti.Add(1f / weaknessPair.Value);
                    }
                    else
                        weaknessMulti.Add(1f / weaknessPair.Value);
                }
                for (int i = 0; i < __instance.weaknessMultipliers.Length; i++)
                {
                    if (container.resistanceDict.ContainsKey(__instance.weaknesses[i]))
                        continue;
                    weakness.Add(__instance.weaknesses[i]);
                    weaknessMulti.Add(__instance.weaknessMultipliers[i]);
                }

                __instance.weaknesses = weakness.ToArray();
                __instance.weaknessMultipliers = weaknessMulti.ToArray();
            }
        }
    
        public static bool AddBuffsCheck()
        {
            return ConfigManager.enemyTweakToggle.value;
        }
	
        public enum DamageCause
		{
			Explosion,
			Projectile,
			Fire,
			Melee,
			Unknown
		}

		public static DamageCause currentCause = DamageCause.Unknown;
		public static bool friendlyBurn = false;

        [HarmonyPatch(nameof(EnemyIdentifier.DeliverDamage))]
        [HarmonyPrefix]
		[HarmonyBefore]
        [UltrapainPatch]
		public static bool FriendlyFireDamageReduction(EnemyIdentifier __instance, ref float __3)
		{
			if (currentCause != DamageCause.Unknown && (__instance.hitter == "enemy" || __instance.hitter == "ffexplosion"))
			{
				switch (currentCause)
				{
					case DamageCause.Projectile:
						//if (ConfigManager.friendlyFireDamageOverrideProjectile.normalizedValue == 0)
						//    return false;
						__3 *= ConfigManager.friendlyFireDamageOverrideProjectile.normalizedValue;
						break;
					case DamageCause.Explosion:
						//if (ConfigManager.friendlyFireDamageOverrideProjectile.normalizedValue == 0)
						//    return false;
						__3 *= ConfigManager.friendlyFireDamageOverrideExplosion.normalizedValue;
						break;
					case DamageCause.Melee:
						//if (ConfigManager.friendlyFireDamageOverrideProjectile.normalizedValue == 0)
						//    return false;
						__3 *= ConfigManager.friendlyFireDamageOverrideMelee.normalizedValue;
						break;
				}
			}

			return true;
		}
	
        public static bool FriendlyFireDamageReductionCheck()
        {
            return ConfigManager.enemyTweakToggle.value && ConfigManager.friendlyFireDamageOverrideToggle.value;
        }
	}

	[UltrapainPatch]
	[HarmonyPatch(typeof(Explosion))]
	public static class ExplosionFriendlyFirePatch
    {
        [HarmonyPatch(nameof(Explosion.Collide))]
        [HarmonyPrefix]
        [UltrapainPatch]
		public static bool DetectFriendlyFire(Explosion __instance)
		{
			EnemyIdentifierPatch.currentCause = EnemyIdentifierPatch.DamageCause.Explosion;
			if ((__instance.enemy || __instance.friendlyFire) && __instance.canHit != AffectedSubjects.PlayerOnly)
				EnemyIdentifierPatch.friendlyBurn = true;
			return true;
		}

        public static bool DetectFriendlyFireCheck()
        {
            return ConfigManager.friendlyFireDamageOverrideToggle.value;
        }

		[HarmonyPatch(nameof(Explosion.Collide))]
		[HarmonyPostfix]
		[UltrapainPatch]
		public static void ResetFriendlyFire()
		{
			EnemyIdentifierPatch.currentCause = EnemyIdentifierPatch.DamageCause.Unknown;
			EnemyIdentifierPatch.friendlyBurn = false;
		}

		public static bool ResetFriendlyFireCheck()
		{
			return ConfigManager.friendlyFireDamageOverrideToggle.value;
		}
	}

    [UltrapainPatch]
    [HarmonyPatch(typeof(PhysicalShockwave))]
    public static class PhysicalShockwaveFriendlyFirePatch
    {
		[HarmonyPatch(nameof(PhysicalShockwave.CheckCollision))]
		[HarmonyPrefix]
		[UltrapainPatch]
		public static bool DetectCause()
		{
			EnemyIdentifierPatch.currentCause = EnemyIdentifierPatch.DamageCause.Explosion;
			return true;
		}

        public static bool DetectCauseCheck()
        {
            return ConfigManager.friendlyFireDamageOverrideToggle.value;
        }

		[HarmonyPatch(nameof(PhysicalShockwave.CheckCollision))]
		[HarmonyPostfix]
		[UltrapainPatch]
		public static void ResetCause()
		{
			EnemyIdentifierPatch.currentCause = EnemyIdentifierPatch.DamageCause.Unknown;
		}

		public static bool ResetCauseCheck()
		{
			return ConfigManager.friendlyFireDamageOverrideToggle.value;
		}
	}

	[UltrapainPatch]
	[HarmonyPatch(typeof(VirtueInsignia))]
	public static class VirtueInsigniaFriendlyFirePatch
    {
		[HarmonyPatch(nameof(VirtueInsignia.OnTriggerEnter))]
		[HarmonyPrefix]
		[UltrapainPatch]
		public static bool DetectCause(VirtueInsignia __instance)
        {
            if (__instance.gameObject.name == "PlayerSpawned")
                return true;

            EnemyIdentifierPatch.currentCause = EnemyIdentifierPatch.DamageCause.Fire;
            EnemyIdentifierPatch.friendlyBurn = true;
            return true;
        }

		public static bool DetectCauseCheck()
		{
			return ConfigManager.friendlyFireDamageOverrideToggle.value;
		}

		[HarmonyPatch(nameof(VirtueInsignia.OnTriggerEnter))]
		[HarmonyPostfix]
		[UltrapainPatch]
		public static void ResetCause()
        {
            EnemyIdentifierPatch.currentCause = EnemyIdentifierPatch.DamageCause.Unknown;
            EnemyIdentifierPatch.friendlyBurn = false;
        }

		public static bool ResetCauseCheck()
		{
			return ConfigManager.friendlyFireDamageOverrideToggle.value;
		}
	}

	[UltrapainPatch]
	[HarmonyPatch(typeof(SwingCheck2))]
	public static class SwingCheck2FriendlyFirePatch
    {
		[HarmonyPatch(nameof(SwingCheck2.CheckCollision))]
		[HarmonyPrefix]
		[UltrapainPatch]
		public static bool DetectCause()
        {
            EnemyIdentifierPatch.currentCause = EnemyIdentifierPatch.DamageCause.Melee;
            return true;
        }

		public static bool DetectCauseCheck()
		{
			return ConfigManager.friendlyFireDamageOverrideToggle.value;
		}

		[HarmonyPatch(nameof(SwingCheck2.CheckCollision))]
		[HarmonyPostfix]
		[UltrapainPatch]
		public static void ResetCause()
        {
            EnemyIdentifierPatch.currentCause = EnemyIdentifierPatch.DamageCause.Unknown;
        }

		public static bool ResetCauseCheck()
		{
			return ConfigManager.friendlyFireDamageOverrideToggle.value;
		}
	}

	[UltrapainPatch]
	[HarmonyPatch(typeof(Projectile))]
	public static class ProjectileFriendlyFirePatch
    {
		[HarmonyPatch(nameof(Projectile.Collided))]
		[HarmonyPrefix]
		[UltrapainPatch]
		public static bool DetectCause()
        {
            EnemyIdentifierPatch.currentCause = EnemyIdentifierPatch.DamageCause.Projectile;
            return true;
        }

		public static bool DetectCauseCheck()
		{
			return ConfigManager.friendlyFireDamageOverrideToggle.value;
		}

		[HarmonyPatch(nameof(Projectile.Collided))]
		[HarmonyPostfix]
		[UltrapainPatch]
		public static void ResetCause()
        {
            EnemyIdentifierPatch.currentCause = EnemyIdentifierPatch.DamageCause.Unknown;
        }

		public static bool ResetCauseCheck()
		{
			return ConfigManager.friendlyFireDamageOverrideToggle.value;
		}
	}

	[UltrapainPatch]
	[HarmonyPatch(typeof(Flammable))]
	public static class FlammableFriendlyFirePatch
    {
		[HarmonyPatch(nameof(Flammable.Burn))]
		[HarmonyPrefix]
		[UltrapainPatch]
		public static bool DetectCause(Flammable __instance, ref float __0)
        {
            if (EnemyIdentifierPatch.friendlyBurn)
            {
                if (ConfigManager.friendlyFireDamageOverrideFire.normalizedValue == 0)
                    return false;
                __0 *= ConfigManager.friendlyFireDamageOverrideFire.normalizedValue;
            }
            return true;
        }

		public static bool DetectCauseCheck()
		{
			return ConfigManager.friendlyFireDamageOverrideToggle.value;
		}
	}

	[UltrapainPatch]
	[HarmonyPatch(typeof(FireZone))]
	public static class StreetCleanerFriendlyFirePatch
	{
		[HarmonyPatch(nameof(FireZone.OnTriggerStay))]
		[HarmonyPrefix]
		[UltrapainPatch]
		public static bool DetectCause(FireZone __instance)
        {
            if (__instance.source != FlameSource.Streetcleaner)
                return true;

            EnemyIdentifierPatch.friendlyBurn = true;
            return true;
        }

		public static bool DetectCauseCheck()
		{
			return ConfigManager.friendlyFireDamageOverrideToggle.value;
		}

		[HarmonyPatch(nameof(FireZone.OnTriggerStay))]
		[HarmonyPostfix]
		[UltrapainPatch]
		public static void ResetCause(FireZone __instance)
        {
            if (__instance.source == FlameSource.Streetcleaner)
                EnemyIdentifierPatch.friendlyBurn = false;
        }

		public static bool ResetCauseCheck()
		{
			return ConfigManager.friendlyFireDamageOverrideToggle.value;
		}
	}
}
