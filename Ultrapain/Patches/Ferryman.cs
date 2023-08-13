using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UltrapainExtensions;
using UnityEngine;
using UnityEngine.AI;

namespace Ultrapain.Patches
{
    public class FerrymanFlag : MonoBehaviour
    {
        private int currentCombo = 0;
        public List<int> randomComboPattern = new List<int>();
        public int remainingCombo = ConfigManager.ferrymanComboCount.value;

        private void Start()
        {
            int attackCount = 3;
            int allocationPerAttack = 1;

            for (int attack = 0; attack < attackCount; attack++)
                for (int i = 0; i < allocationPerAttack; i++)
                    randomComboPattern.Add(attack);

            System.Random rng = new System.Random(System.DateTime.Today.Millisecond);
            randomComboPattern.OrderBy(a => rng.Next()).ToList();
        }

        public int GetNextCombo()
        {
            /*currentCombo++;
            if (currentCombo >= randomComboPattern.Count)
                currentCombo = 0;
            return randomComboPattern[currentCombo];*/

            return randomComboPattern[UnityEngine.Random.RandomRangeInt(0, randomComboPattern.Count)];
        }
    }

    [UltrapainPatch]
    [HarmonyPatch(typeof(Ferryman))]
    public static class FerrymanPatch
    {
		[HarmonyPatch(nameof(Ferryman.StopMoving))]
		[HarmonyPostfix]
		[UltrapainPatch]
		public static void Combo(Ferryman __instance)
		{
			FerrymanFlag flag = __instance.gameObject.GetComponent<FerrymanFlag>();
			if (flag == null)
				return;

			if (__instance.bossVersion && __instance.inPhaseChange)
			{
				flag.remainingCombo = ConfigManager.ferrymanComboCount.value;
				return;
			}

			string clipName = __instance.anim.GetCurrentAnimatorClipInfo(0)[0].clip.name;
			if (clipName != "OarCombo" && clipName != "KickCombo" && clipName != "Stinger" && clipName != "BackstepAttack")
				return;

			AnimationClip clip = __instance.anim.GetCurrentAnimatorClipInfo(0)[0].clip;
			float time = clip.events.Where(obj => obj.functionName == "StopMoving").Last().time;
			if (__instance.anim.GetCurrentAnimatorStateInfo(0).normalizedTime < time / clip.length)
				return;

			//if (flag.remainingCombo == ConfigManager.ferrymanComboCount.value && clipName == "KickCombo")
			//    flag.remainingCombo -= 1;

			flag.remainingCombo -= 1;
			if (flag.remainingCombo <= 0)
			{
				flag.remainingCombo = ConfigManager.ferrymanComboCount.value;
				return;
			}

			int attackType = flag.GetNextCombo();

			if (attackType == 0)
			{
				// time = 0.8347
				// total = 2.4667
				__instance.anim.Play("OarCombo", 0, (0.8347f * (1f - ConfigManager.ferrymanAttackDelay.value)) / 2.4667f);

				__instance.SnapToGround();
				__instance.inAction = true;
				__instance.tracking = true;
				if (__instance.nma.isOnNavMesh)
				{
					__instance.nma.SetDestination(__instance.transform.position);
				}
				//__instance.anim.SetTrigger("OarCombo");
				__instance.backTrailActive = true;
				__instance.useMain = true;
				__instance.useOar = true;
				__instance.useKick = false;
			}
			else if (attackType == 1)
			{
				// time = 0.8347
				// total = 2.4667
				__instance.anim.Play("KickCombo", 0, (0.8347f * (1f - ConfigManager.ferrymanAttackDelay.value)) / 2.4667f);

				__instance.SnapToGround();
				__instance.inAction = true;
				__instance.tracking = true;
				if (__instance.nma.isOnNavMesh)
				{
					__instance.nma.SetDestination(__instance.transform.position);
				}
				//__instance.anim.SetTrigger("KickCombo");
				__instance.backTrailActive = true;
				__instance.useMain = true;
				__instance.useOar = false;
				__instance.useKick = true;
			}
			else
			{
				// time = 0.4129
				// total = 1.3
				__instance.anim.Play("Stinger", 0, 0);

				__instance.SnapToGround();
				__instance.inAction = true;
				__instance.tracking = true;
				if (__instance.nma.isOnNavMesh)
				{
					__instance.nma.SetDestination(__instance.transform.position);
				}
				//__instance.anim.SetTrigger("KickCombo");
				__instance.backTrailActive = true;
				__instance.useMain = true;
				__instance.useOar = true;
				__instance.useKick = false;
			}
		}

		public static bool ComboCheck()
		{
			return ConfigManager.enemyTweakToggle.value && ConfigManager.ferrymanComboToggle.value;
		}

		[HarmonyPatch(nameof(Ferryman.Start))]
		[HarmonyPostfix]
		[UltrapainPatch]
		public static void AddFlag(Ferryman __instance)
		{
			if (__instance.GetComponent<NonUltrapainEnemy>() != null)
				return;

			__instance.gameObject.AddComponent<FerrymanFlag>();
		}

		public static bool AddFlagCheck()
		{
			return ConfigManager.enemyTweakToggle.value;
		}

	}
}
