using HarmonyLib;
using UltrapainExtensions;
using UnityEngine;

namespace Ultrapain.Patches
{
    public class CerberusFlag : MonoBehaviour
    {
        public int extraDashesRemaining = ConfigManager.cerberusTotalDashCount.value - 1;
        public Transform head;
        public float lastParryTime;
        private EnemyIdentifier eid;

        private void Awake()
        {
            eid = GetComponent<EnemyIdentifier>();
            head = transform.Find("Armature/Control/Waist/Chest/Chest_001/Head");
            if (head == null)
                head = UnityUtils.GetChildByTagRecursively(transform, "Head");
        }

        public void MakeParryable()
        {
            lastParryTime = Time.time;
            GameObject flash = GameObject.Instantiate(Plugin.parryableFlash.obj, head.transform.position, head.transform.rotation, head);
            flash.transform.LookAt(CameraController.Instance.transform);
            flash.transform.position += flash.transform.forward;
            flash.transform.Rotate(Vector3.up, 90, Space.Self);
        }
    }

    [UltrapainPatch]
    [HarmonyPatch(typeof(StatueBoss))]
    public static class CerberusPatches
    {
        [HarmonyPatch(nameof(StatueBoss.StopTracking))]
        [HarmonyPostfix]
		[UltrapainPatch]
		public static void MakeParyableBeforeDash(StatueBoss __instance)
		{
			CerberusFlag flag = __instance.GetComponent<CerberusFlag>();
			if (flag == null)
				return;

			if (__instance.anim.GetCurrentAnimatorClipInfo(0)[0].clip.name != "Tackle")
				return;

			flag.MakeParryable();
		}

		public static bool MakeParyableBeforeDashCheck()
        {
            return ConfigManager.enemyTweakToggle.value && ConfigManager.cerberusParryable.value;
        }

        [HarmonyPatch(nameof(StatueBoss.Stomp))]
        [HarmonyPostfix]
        [UltrapainPatch]
		public static void MakeParyableBeforeStomp(StatueBoss __instance)
		{
			CerberusFlag flag = __instance.GetComponent<CerberusFlag>();
			if (flag == null)
				return;

			flag.MakeParryable();
		}

		public static bool MakeParyableBeforeStompCheck()
		{
			return ConfigManager.enemyTweakToggle.value && ConfigManager.cerberusParryable.value;
		}

		[HarmonyPatch(nameof(StatueBoss.StopDash))]
		[HarmonyPostfix]
		[UltrapainPatch]
		public static void ExtraDash(StatueBoss __instance)
		{
			CerberusFlag flag = __instance.GetComponent<CerberusFlag>();
			if (flag == null)
				return;

			if (flag.extraDashesRemaining > 0)
			{
				flag.extraDashesRemaining -= 1;
				__instance.Tackle();
				__instance.tackleChance -= 20;
			}
			else
				flag.extraDashesRemaining = ConfigManager.cerberusTotalDashCount.value - 1;
		}

		public static bool ExtraDashCheck()
		{
			return ConfigManager.enemyTweakToggle.value && ConfigManager.cerberusDashToggle.value;
		}

		[HarmonyPatch(nameof(StatueBoss.Start))]
		[HarmonyPostfix]
		[UltrapainPatch]
		static void AddFlag(StatueBoss __instance)
		{
			if (__instance.GetComponent<NonUltrapainEnemy>() != null)
				return;

			__instance.gameObject.AddComponent<CerberusFlag>();
		}
	
		static bool AddFlagCheck()
		{
			return ConfigManager.enemyTweakToggle.value;
		}
	}

    [UltrapainPatch]
    [HarmonyPatch(typeof(Statue))]
    public static class CerberusStatuePatches
    {
		[HarmonyPatch(nameof(Statue.GetHurt))]
		[HarmonyPrefix]
		[UltrapainPatch]
		public static bool GetParryableDamage(Statue __instance)
		{
			CerberusFlag flag = __instance.GetComponent<CerberusFlag>();
			if (flag == null)
				return true;

			if (__instance.eid.hitter != "punch" && __instance.eid.hitter != "shotgunzone")
				return true;

			float deltaTime = Time.time - flag.lastParryTime;
			if (deltaTime > ConfigManager.cerberusParryableDuration.value / __instance.eid.totalSpeedModifier)
				return true;

			flag.lastParryTime = 0;
			__instance.eid.SimpleDamage(ConfigManager.cerberusParryDamage.value);
			if (__instance.eid.hitter == "punch")
				MonoSingleton<FistControl>.Instance.currentPunch.Parry(false, __instance.eid);
			return true;
		}
	
        public static bool GetParryableDamageCheck()
        {
            return ConfigManager.enemyTweakToggle.value && ConfigManager.cerberusParryable.value;
        }
	}
}
