using HarmonyLib;
using System.Reflection;
using UltrapainExtensions;
using UnityEngine;

namespace Ultrapain.Patches
{
	public class MindflayerFlag : MonoBehaviour
	{
		public int shotsLeft = ConfigManager.mindflayerShootAmount.value;
		public int swingComboLeft = 2;
	}

	[UltrapainPatch]
	[HarmonyPatch(typeof(Mindflayer))]
	public static class MindflayerPatch
	{
        [HarmonyPatch(nameof(Mindflayer.ShootProjectiles))]
        [HarmonyPrefix]
        [UltrapainPatch]
        public static bool CustomProjectileAttack(Mindflayer __instance)
		{
			MindflayerFlag counter = __instance.GetComponent<MindflayerFlag>();
			if (counter == null)
				return true;

			if (counter.shotsLeft == 0)
			{
				counter.shotsLeft = ConfigManager.mindflayerShootAmount.value;
				__instance.chargeParticle.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
				__instance.cooldown = (float)UnityEngine.Random.Range(4, 5);
				return false;
			}

			Quaternion randomRotation = Quaternion.LookRotation(MonoSingleton<PlayerTracker>.Instance.GetTarget().position - __instance.transform.position);
			randomRotation.eulerAngles += new Vector3(UnityEngine.Random.Range(-10.0f, 10.0f), UnityEngine.Random.Range(-10.0f, 10.0f), UnityEngine.Random.Range(-10.0f, 10.0f));
			Projectile componentInChildren = GameObject.Instantiate(Plugin.homingProjectile.obj.obj, __instance.transform.position + __instance.transform.forward, randomRotation).GetComponentInChildren<Projectile>();

			Vector3 randomPos = __instance.tentacles[UnityEngine.Random.RandomRangeInt(0, __instance.tentacles.Length)].position;
			if (!Physics.Raycast(__instance.transform.position, randomPos - __instance.transform.position, Vector3.Distance(randomPos, __instance.transform.position), __instance.environmentMask))
				componentInChildren.transform.position = randomPos;

			int shotCount = ConfigManager.mindflayerShootAmount.value - counter.shotsLeft;
			componentInChildren.transform.position += componentInChildren.transform.forward * Mathf.Clamp(-1 + shotCount * 0.4f, 0, 5);

			componentInChildren.speed = ConfigManager.mindflayerShootInitialSpeed.value * __instance.eid.totalSpeedModifier;
			componentInChildren.turningSpeedMultiplier = ConfigManager.mindflayerShootTurnSpeed.value;
			componentInChildren.target = MonoSingleton<PlayerTracker>.Instance.GetTarget();
			componentInChildren.safeEnemyType = EnemyType.Mindflayer;
			componentInChildren.damage *= __instance.eid.totalDamageModifier;
			componentInChildren.sourceWeapon = __instance.gameObject;
			counter.shotsLeft -= 1;
			__instance.Invoke("ShootProjectiles", ConfigManager.mindflayerShootDelay.value / __instance.eid.totalSpeedModifier);

			return false;
		}

		public static bool CustomProjectileAttackCheck()
        {
            return ConfigManager.enemyTweakToggle.value && ConfigManager.mindflayerShootTweakToggle.value;
        }

		[HarmonyPatch(nameof(Mindflayer.MeleeTeleport))]
		[HarmonyPrefix]
		[UltrapainPatch]
		public static bool Teleport(Mindflayer __instance)
		{
			if (__instance.gameObject.GetComponent<MindflayerFlag>() == null)
				return true;

			if (__instance.eid.drillers.Count > 0)
				return false;

			Vector3 targetPosition = MonoSingleton<PlayerTracker>.Instance.PredictPlayerPosition(0.9f) + new Vector3(0, -10, 0);
			float distance = Vector3.Distance(__instance.transform.position, targetPosition);

			Ray targetRay = new Ray(__instance.transform.position, targetPosition - __instance.transform.position);
			RaycastHit hit;
			if (Physics.Raycast(targetRay, out hit, distance, __instance.environmentMask, QueryTriggerInteraction.Ignore))
			{
				targetPosition = targetRay.GetPoint(Mathf.Max(0.0f, hit.distance - 1.0f));
			}

			MonoSingleton<HookArm>.Instance.StopThrow(1f, true);
			__instance.transform.position = targetPosition;
			__instance.goingLeft = !__instance.goingLeft;

			GameObject.Instantiate(__instance.teleportSound, __instance.transform.position, Quaternion.identity);
			GameObject gameObject = GameObject.Instantiate(__instance.decoy, __instance.transform.GetChild(0).position, __instance.transform.GetChild(0).rotation);
			Animator componentInChildren = gameObject.GetComponentInChildren<Animator>();
			AnimatorStateInfo currentAnimatorStateInfo = __instance.anim.GetCurrentAnimatorStateInfo(0);
			componentInChildren.Play(currentAnimatorStateInfo.shortNameHash, 0, currentAnimatorStateInfo.normalizedTime);
			componentInChildren.speed = 0f;
			if (__instance.enraged)
			{
				gameObject.GetComponent<MindflayerDecoy>().enraged = true;
			}

			__instance.anim.speed = 0f;
			__instance.CancelInvoke("ResetAnimSpeed");
			__instance.Invoke("ResetAnimSpeed", 0.25f / __instance.eid.totalSpeedModifier);

			return false;
		}

		public static bool TeleportCheck()
		{
			return ConfigManager.enemyTweakToggle.value && ConfigManager.mindflayerTeleportComboToggle.value;
		}

		[HarmonyPatch(nameof(Mindflayer.Start))]
		[HarmonyPostfix]
		[UltrapainPatch]
		public static void AddFlag(Mindflayer __instance)
		{
			if (__instance.gameObject.GetComponent<NonUltrapainEnemy>() != null)
				return;

			__instance.gameObject.AddComponent<MindflayerFlag>();
		}

		public static bool AddFlagCheck()
		{
			return ConfigManager.enemyTweakToggle.value;
		}
	}

	[UltrapainPatch]
	[HarmonyPatch(typeof(EnemyIdentifier))]
	public static class MindflayerFriendlyFirePatch
    {
		[HarmonyPatch(nameof(EnemyIdentifier.DeliverDamage))]
		[HarmonyPrefix]
		[UltrapainPatch]
		public static bool ReduceSelfDamage(EnemyIdentifier __instance, ref float __3, GameObject __6)
		{
			if (__instance.enemyType != EnemyType.Mindflayer)
				return true;

			if (__6 == null || __6.GetComponent<Mindflayer>() == null)
				return true;

			__3 *= ConfigManager.mindflayerProjectileSelfDamageMultiplier.value / 100f;
			return true;
		}

		public static bool ReduceSelfDamageCheck()
        {
            return ConfigManager.enemyTweakToggle.value && ConfigManager.mindflayerProjectileSelfDamageMultiplier.normalizedValue != 1;
        }
	}

	[UltrapainPatch]
	[HarmonyPatch(typeof(SwingCheck2))]
	public static class MindflayerMeleePatch
    {
		[HarmonyPatch(nameof(SwingCheck2.CheckCollision))]
		[HarmonyPrefix]
		[UltrapainPatch]
		public static bool PrepareCombo(Collider __0, out int __state)
		{
			__state = __0.gameObject.layer;
			return true;
		}

		public static bool PrepareComboCheck()
		{
			return ConfigManager.enemyTweakToggle.value && ConfigManager.mindflayerTeleportComboToggle.value;
		}

		[HarmonyPatch(nameof(SwingCheck2.CheckCollision))]
		[HarmonyPostfix]
		[UltrapainPatch]
		public static void DoCombo(SwingCheck2 __instance, Collider __0, int __state)
		{
			// if (__0.tag == "Player")
			// 	Debug.Log($"Collision with {__0.name} with tag {__0.tag} and layer {__state}");
			if (__0.gameObject.tag != "Player" || __state == 15)
				return;

			if (__instance.transform.parent == null)
				return;

			Mindflayer mf = __instance.transform.parent.gameObject.GetComponent<Mindflayer>();

			if (mf == null)
				return;

			if (mf.gameObject.GetComponent<MindflayerFlag>() == null)
				return;

			__instance.DamageStop();
			mf.goForward = false;
			mf.MeleeAttack();

			/*if (patch.swingComboLeft > 0)
            {
                patch.swingComboLeft -= 1;
                __instance.DamageStop();
                goForward.SetValue(mf, false);
                meleeAttack.Invoke(mf, new object[] { });
            }
            else
                patch.swingComboLeft = 2;*/
		}

		public static bool DoComboCheck()
		{
			return ConfigManager.enemyTweakToggle.value && ConfigManager.mindflayerTeleportComboToggle.value;
		}
	}

    /*class SwingCheck2_DamageStop_Patch
    {
        static void Postfix(SwingCheck2 __instance)
        {
            if (__instance.transform.parent == null)
                return;
            GameObject parent = __instance.transform.parent.gameObject;
            Mindflayer mf = parent.GetComponent<Mindflayer>();
            if (mf == null)
                return;

            MindflayerFlag patch = parent.GetComponent<MindflayerFlag>();
            patch.swingComboLeft = 2;
        }
    }*/
}
