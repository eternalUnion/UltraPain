using HarmonyLib;
using UltrapainExtensions;
using UnityEngine;

namespace Ultrapain.Patches
{
    public class HideousMassFlag : MonoBehaviour
    {

    }
    
    public class HideousMassProjectile : MonoBehaviour
    {
        public float damageBuf = 1f;
        public float speedBuf = 1f;
    }

    [UltrapainPatch]
    [HarmonyPatch(typeof(Mass))]
    public static class HideousMassPatch
    {
		[HarmonyPatch(nameof(Mass.ShootExplosive))]
		[HarmonyPrefix]
		[UltrapainPatch]
        public static bool PrepareHoming(Mass __instance, out bool __state)
		{
			__state = false;
			if (__instance.GetComponent<HideousMassFlag>() == null)
				return true;

			__instance.explosiveProjectile = GameObject.Instantiate(Plugin.hideousMassProjectile.obj);
			__state = true;
			HideousMassProjectile flag = __instance.explosiveProjectile.AddComponent<HideousMassProjectile>();
			flag.damageBuf = __instance.eid.totalDamageModifier;
			flag.speedBuf = __instance.eid.totalSpeedModifier;
			return true;
		}

		public static bool PrepareHomingCheck()
		{
			return ConfigManager.enemyTweakToggle.value && ConfigManager.hideousMassInsigniaToggle.value;
		}

		[HarmonyPatch(nameof(Mass.ShootExplosive))]
		[HarmonyPostfix]
		[UltrapainPatch]
		public static void ClearHoming(Mass __instance, bool __state)
		{
			if (!__state)
				return;

			GameObject.Destroy(__instance.explosiveProjectile);
			__instance.explosiveProjectile = Plugin.hideousMassProjectile.obj;
		}

		public static bool ClearHomingCheck()
		{
			return ConfigManager.enemyTweakToggle.value && ConfigManager.hideousMassInsigniaToggle.value;
		}

		[HarmonyPatch(nameof(Mass.Start))]
		[HarmonyPostfix]
		[UltrapainPatch]
		public static void AddFlag(Mass __instance)
		{
			if (__instance.GetComponent<NonUltrapainEnemy>() != null)
				return;

			__instance.gameObject.AddComponent<HideousMassFlag>();
		}

		public static bool AddFlagCheck()
		{
			return ConfigManager.enemyTweakToggle.value;
		}
	}

    [UltrapainPatch]
    [HarmonyPatch(typeof(Projectile))]
    public static class HideousMassProjectilePatch
    {
        [HarmonyPatch(nameof(Projectile.Explode))]
        [HarmonyPostfix]
        [UltrapainPatch]
        public static void InsigniaExplosion(Projectile __instance)
		{
			HideousMassProjectile flag = __instance.gameObject.GetComponent<HideousMassProjectile>();
			if (flag == null)
				return;

			GameObject createInsignia(float size, int damage)
			{
				GameObject insignia = GameObject.Instantiate(Plugin.virtueInsignia.obj, __instance.transform.position, Quaternion.identity);
				insignia.transform.localScale = new Vector3(size, 1f, size);
				VirtueInsignia comp = insignia.GetComponent<VirtueInsignia>();
				comp.windUpSpeedMultiplier = ConfigManager.hideousMassInsigniaSpeed.value * flag.speedBuf;
				comp.damage = (int)(damage * flag.damageBuf);
				comp.predictive = false;
				comp.hadParent = false;
				comp.noTracking = true;

				return insignia;
			}

			if (ConfigManager.hideousMassInsigniaXtoggle.value)
			{
				GameObject insignia = createInsignia(ConfigManager.hideousMassInsigniaXsize.value, ConfigManager.hideousMassInsigniaXdamage.value);
				insignia.transform.Rotate(new Vector3(0, 0, 90f));
			}
			if (ConfigManager.hideousMassInsigniaYtoggle.value)
			{
				GameObject insignia = createInsignia(ConfigManager.hideousMassInsigniaYsize.value, ConfigManager.hideousMassInsigniaYdamage.value);
			}
			if (ConfigManager.hideousMassInsigniaZtoggle.value)
			{
				GameObject insignia = createInsignia(ConfigManager.hideousMassInsigniaZsize.value, ConfigManager.hideousMassInsigniaZdamage.value);
				insignia.transform.Rotate(new Vector3(90f, 0, 0));
			}
		}

		public static bool InsigniaExplosionCheck()
        {
            return ConfigManager.enemyTweakToggle.value && ConfigManager.hideousMassInsigniaToggle.value;
        }
	}
}
