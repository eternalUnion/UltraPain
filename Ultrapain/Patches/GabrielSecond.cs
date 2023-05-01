using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Ultrapain.Patches
{
    class GabrielSecondFlag : MonoBehaviour
    {
        public int maxChaos = 7;
        public int chaosRemaining = 7;
        public GabrielSecond comp;

        public float teleportChance = 20;

        public void ChaoticAttack(float delay)
        {
            if(chaosRemaining == 0)
            {
                chaosRemaining = maxChaos;
                return;
            }

            chaosRemaining -= 1;
            CancelInvoke("CallChaoticAttack");
            Invoke("CallChaoticAttack", delay);
        }

        static MethodInfo BasicCombo = typeof(GabrielSecond).GetMethod("BasicCombo", BindingFlags.Instance | BindingFlags.NonPublic);
        static MethodInfo FastCombo = typeof(GabrielSecond).GetMethod("FastCombo", BindingFlags.Instance | BindingFlags.NonPublic);
        static MethodInfo CombineSwords = typeof(GabrielSecond).GetMethod("CombineSwords", BindingFlags.Instance | BindingFlags.NonPublic);

        public void CallChaoticAttack()
        {
            bool teleported = false;
            if (UnityEngine.Random.Range(0, 100) <= teleportChance)
            {
                Debug.Log("Attemted teleport");
                comp.Teleport(false, false, true, false, false);
                teleported = true;
            }

            switch (UnityEngine.Random.RandomRangeInt(0, 3))
            {
                case 0:
                    BasicCombo.Invoke(comp, new object[0]);
                    break;

                case 1:
                    FastCombo.Invoke(comp, new object[0]);
                    break;

                case 2:
                    if (!comp.secondPhase && !teleported)
                    {
                        Debug.Log("Attemted sword throw teleport");
                        comp.Teleport(false, false, true, false, false);
                    }
                    CombineSwords.Invoke(comp, new object[0]);
                    break;
            }
        }
    }

    class GabrielSecond_Start
    {
        static void Postfix(GabrielSecond __instance)
        {
            __instance.gameObject.AddComponent<GabrielSecondFlag>().comp = __instance;
        }
    }

    class GabrielSecond_FastCombo
    {
        //0.3601
        //0.5075
        //0.8171
        public static float[] attackTimings = new float[]
        {
            0.3601f * 3.75f,
            0.5075f * 3.75f,
            0.8171f * 3.75f,
        };

        static void Postfix(GabrielSecond __instance)
        {
            Debug.Log("Fast combo attack");
            GabrielSecondFlag flag = __instance.GetComponent<GabrielSecondFlag>();
            if (flag == null)
                return;
            flag.ChaoticAttack(attackTimings[UnityEngine.Random.RandomRangeInt(0, 3)]);
        }
    }

    class GabrielSecond_BasicCombo
    {
        //0.3908
        //0.3908
        //0.7567
        public static float[] attackTimings = new float[]
        {
            0.3908f * 4.3333f,
            0.3908f * 4.3333f,
            0.7567f * 4.3333f,
        };

        static void Postfix(GabrielSecond __instance)
        {
            Debug.Log("Basic combo attack");
            GabrielSecondFlag flag = __instance.GetComponent<GabrielSecondFlag>();
            if (flag == null)
                return;
            flag.ChaoticAttack(attackTimings[UnityEngine.Random.RandomRangeInt(0, 3)]);
        }
    }

    class GabrielSecond_ThrowCombo
    {
        static float delay = 3.0667f;

        static void Postfix(GabrielSecond __instance)
        {
            Debug.Log("Throw combo attack");
            GabrielSecondFlag flag = __instance.GetComponent<GabrielSecondFlag>();
            if (flag == null)
                return;
            flag.ChaoticAttack(delay);
        }
    }

    class GabrielSecond_CombineSwords
    {
        static float delay = 0.5f + 1.0833f;

        //0.9079 * 1.0833
        //0.5 + 1.0833
        static void Postfix(GabrielSecond __instance)
        {
            Debug.Log("Combine sword attack");
            GabrielSecondFlag flag = __instance.GetComponent<GabrielSecondFlag>();
            if (flag == null)
                return;
            flag.ChaoticAttack(delay);
        }
    }
}
