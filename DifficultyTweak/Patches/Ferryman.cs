using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.AI;

namespace DifficultyTweak.Patches
{
    class FerrymanFlag : MonoBehaviour
    {
        private int currentCombo = 0;
        public List<int> randomComboPattern = new List<int>();
        public int remainingCombo = ConfigManager.ferrymanComboCount.value;

        void Start()
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
            currentCombo++;
            if (currentCombo >= randomComboPattern.Count)
                currentCombo = 0;
            return randomComboPattern[currentCombo];
        }
    }

    [HarmonyPatch(typeof(Ferryman), "Start")]
    class FerrymanStart
    {
        static void Postfix(Ferryman __instance)
        {
            __instance.gameObject.AddComponent<FerrymanFlag>();
        }
    }

    [HarmonyPatch(typeof(Ferryman), /*"StopAction"*/"StopMoving")]
    class FerrymanStopAction
    {
        public static MethodInfo SnapToGround = typeof(Ferryman).GetMethod("SnapToGround", BindingFlags.Instance | BindingFlags.NonPublic);

        static void Postfix(Ferryman __instance, ref Animator ___anim, ref bool ___inAction, ref bool ___tracking, ref NavMeshAgent ___nma,
            ref bool ___useMain, ref bool ___useOar, ref bool ___useKick, ref bool ___backTrailActive)
        {
            FerrymanFlag flag = __instance.gameObject.GetComponent<FerrymanFlag>();
            if (flag == null)
                return;

            if (!Plugin.ultrapainDifficulty || !ConfigManager.enemyTweakToggle.value || !ConfigManager.ferrymanComboToggle.value)
                return;

            string clipName = ___anim.GetCurrentAnimatorClipInfo(0)[0].clip.name;
            if (clipName != "OarCombo" && clipName != "KickCombo" && clipName != "Stinger" && clipName != "BackstepAttack")
                return;

            AnimationClip clip = ___anim.GetCurrentAnimatorClipInfo(0)[0].clip;
            float time = clip.events.Where(obj => obj.functionName == "StopMoving").Last().time;
            if (___anim.GetCurrentAnimatorStateInfo(0).normalizedTime < time / clip.length)
                return;

            if (flag.remainingCombo == ConfigManager.ferrymanComboCount.value && clipName == "KickCombo")
                flag.remainingCombo -= 1;

            flag.remainingCombo -= 1;
            if(flag.remainingCombo <= 0)
            {
                flag.remainingCombo = ConfigManager.ferrymanComboCount.value;
                Debug.Log("===========");
                return;
            }

            int attackType = flag.GetNextCombo();
            Debug.Log(attackType);

            if (attackType == 0)
            {
                // time = 0.8347
                // total = 2.4667
                ___anim.Play("OarCombo", 0, (0.8347f * (1f - ConfigManager.ferrymanAttackDelay.value)) / 2.4667f);

                SnapToGround.Invoke(__instance, new object[0]);
                ___inAction = true;
                ___tracking = true;
                if (___nma.isOnNavMesh)
                {
                    ___nma.SetDestination(__instance.transform.position);
                }
                //__instance.anim.SetTrigger("OarCombo");
                ___backTrailActive = true;
                ___useMain = true;
                ___useOar = true;
                ___useKick = false;
            }
            else if(attackType == 1)
            {
                // time = 0.8347
                // total = 2.4667
                ___anim.Play("KickCombo", 0, (0.8347f * (1f - ConfigManager.ferrymanAttackDelay.value)) / 2.4667f);

                SnapToGround.Invoke(__instance, new object[0]);
                ___inAction = true;
                ___tracking = true;
                if (___nma.isOnNavMesh)
                {
                    ___nma.SetDestination(__instance.transform.position);
                }
                //__instance.anim.SetTrigger("KickCombo");
                ___backTrailActive = true;
                ___useMain = true;
                ___useOar = false;
                ___useKick = true;
            }
            else
            {
                // time = 0.6524
                // total = 1.3
                ___anim.Play("Stinger", 0, (0.6524f * (1f - ConfigManager.ferrymanAttackDelay.value)) / 1.3f);

                SnapToGround.Invoke(__instance, new object[0]);
                ___inAction = true;
                ___tracking = true;
                if (___nma.isOnNavMesh)
                {
                    ___nma.SetDestination(__instance.transform.position);
                }
                //__instance.anim.SetTrigger("KickCombo");
                ___backTrailActive = true;
                ___useMain = true;
                ___useOar = true;
                ___useKick = false;
            }
        }
    }
}
