using HarmonyLib;
using UltrapainExtensions;
using UnityEngine;

namespace Ultrapain.Patches
{
    public class FilthFlag : MonoBehaviour
    {

    }

	[UltrapainPatch]
	[HarmonyPatch(typeof(ZombieMelee))]
	public static class FilthPatch
	{
        [HarmonyPatch(nameof(ZombieMelee.Start))]
        [HarmonyPostfix]
        [UltrapainPatch]
        public static void AddFlag(ZombieMelee __instance)
        {
            if (__instance.GetComponent<NonUltrapainEnemy>() != null)
                return;

            if (__instance.eid.enemyType != EnemyType.Filth)
                return;

            __instance.gameObject.AddComponent<FilthFlag>();
        }

        public static bool AddFlagCheck()
        {
            return ConfigManager.enemyTweakToggle.value;
        }
	}

	[UltrapainPatch]
    [HarmonyPatch(typeof(SwingCheck2))]
    public static class FilthSwingPatch
    {
		[HarmonyPatch(nameof(SwingCheck2.CheckCollision))]
		[HarmonyPrefix]
		[UltrapainPatch]
        public static bool Explode(SwingCheck2 __instance, Collider __0)
		{
			if (__0.gameObject.tag != "Player")
				return true;

			if (__instance.transform.parent == null)
				return true;

			EnemyIdentifier eid = __instance.transform.parent.gameObject.GetComponent<EnemyIdentifier>();
			if (eid == null || eid.enemyType != EnemyType.Filth || eid.gameObject.GetComponent<FilthFlag>() == null)
				return true;

			GameObject expObj = GameObject.Instantiate(Plugin.explosion.obj, eid.transform.position, Quaternion.identity);
			foreach (Explosion exp in expObj.GetComponentsInChildren<Explosion>())
			{
				exp.enemy = true;
				exp.damage = (int)(ConfigManager.filthExplosionDamage.value * __instance.eid.totalDamageModifier);
				exp.maxSize *= ConfigManager.filthExplosionSize.value;
				exp.speed *= ConfigManager.filthExplosionSize.value;
				exp.toIgnore.Add(EnemyType.Filth);
			}

			if (ConfigManager.filthExplodeKills.value)
			{
				eid.Death();
			}

			return false;
		}

		public static bool ExplodeCheck()
		{
			return ConfigManager.enemyTweakToggle.value && ConfigManager.filthExplodeToggle.value;
		}
	}
}
