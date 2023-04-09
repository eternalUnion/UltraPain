using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace DifficultyTweak.Patches
{
    [HarmonyPatch(typeof(Mindflayer))]
    [HarmonyPatch("Start")]
    class Mindflayer_Start_Patch
    {
        static void Postfix(Mindflayer __instance, ref EnemyIdentifier ___eid)
        {
            __instance.gameObject.AddComponent<MindflayerPatch>();
            //___eid.SpeedBuff();
        }
    }

    [HarmonyPatch(typeof(Mindflayer))]
    [HarmonyPatch(nameof(Mindflayer.ShootProjectiles))]
    class Mindflayer_ShootProjectiles_Patch
    {
        static bool Prefix(Mindflayer __instance, ref EnemyIdentifier ___eid, ref LayerMask ___environmentMask, ref bool ___enraged)
        {
            MindflayerPatch counter = __instance.GetComponent<MindflayerPatch>();
            if (counter.shotsLeft == 0)
            {
                counter.shotsLeft = 20;
                __instance.chargeParticle.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
                __instance.cooldown = (float)UnityEngine.Random.Range(4, 5);
                return false;
            }

            Quaternion randomRotation = Quaternion.LookRotation(MonoSingleton<PlayerTracker>.Instance.GetTarget().position - __instance.transform.position);
            randomRotation.eulerAngles += new Vector3(UnityEngine.Random.Range(-5.0f, 5.0f), UnityEngine.Random.Range(-5.0f, 5.0f), UnityEngine.Random.Range(-5.0f, 5.0f));
            Projectile componentInChildren = GameObject.Instantiate(Plugin.homingProjectile.gameObject, __instance.transform.position + __instance.transform.forward, randomRotation).GetComponentInChildren<Projectile>();

            Vector3 randomPos = __instance.tentacles[UnityEngine.Random.RandomRangeInt(0, __instance.tentacles.Length)].position;
            if (!Physics.Raycast(__instance.transform.position, randomPos - __instance.transform.position, Vector3.Distance(randomPos, __instance.transform.position), ___environmentMask))
                componentInChildren.transform.position = randomPos;
                
            componentInChildren.speed = 10f * ___eid.totalSpeedModifier;
            //componentInChildren.turnSpeed = 150f;
            componentInChildren.target = MonoSingleton<PlayerTracker>.Instance.GetTarget();
            componentInChildren.safeEnemyType = EnemyType.Mindflayer;
            componentInChildren.damage *= ___eid.totalDamageModifier;
            counter.shotsLeft -= 1;
            __instance.Invoke("ShootProjectiles", 0.02f / ___eid.totalSpeedModifier);

            return false;
        }
    }

    [HarmonyPatch(typeof(SwingCheck2))]
    [HarmonyPatch("CheckCollision")]
    class SwingCheck2_CheckCollision_Patch
    {
        static FieldInfo goForward = typeof(Mindflayer).GetField("goForward", BindingFlags.NonPublic | BindingFlags.Instance);
        static MethodInfo meleeAttack = typeof(Mindflayer).GetMethod("MeleeAttack", BindingFlags.NonPublic | BindingFlags.Instance);

        static void Postfix(SwingCheck2 __instance, Collider __0)
        {
            if (__0.gameObject.tag != "Player" || __0.gameObject.layer == 15)
                return;

            if (__instance.transform.parent == null)
                return;

            GameObject parent = __instance.transform.parent.gameObject;
            Mindflayer mf = parent.GetComponent<Mindflayer>();

            if (mf == null)
                return;

            MindflayerPatch patch = mf.gameObject.GetComponent<MindflayerPatch>();

            Debug.Log("Patch check");

            if (patch.swingComboLeft > 0)
            {
                patch.swingComboLeft -= 1;
                __instance.DamageStop();
                goForward.SetValue(mf, false);
                meleeAttack.Invoke(mf, new object[] { });
            }
            else
                patch.swingComboLeft = 2;
        }
    }

    [HarmonyPatch(typeof(Mindflayer))]
    [HarmonyPatch("MeleeTeleport")]
    class Mindflayer_MeleeTeleport_Patch
    {
        static bool Prefix(Mindflayer __instance, ref EnemyIdentifier ___eid, ref LayerMask ___environmentMask, ref bool ___goingLeft, ref Animator ___anim, ref bool ___enraged)
        {
            if (___eid.drillers.Count > 0)
                return false;

            Vector3 targetPosition = MonoSingleton<PlayerTracker>.Instance.PredictPlayerPosition(0.9f) - Vector3.down * 2;
            float distance = Vector3.Distance(__instance.transform.position, targetPosition);

            Ray targetRay = new Ray(__instance.transform.position, targetPosition - __instance.transform.position);
            RaycastHit hit;
            if (Physics.Raycast(targetRay, out hit, distance, ___environmentMask, QueryTriggerInteraction.Ignore))
            {
                targetPosition = targetRay.GetPoint(Mathf.Max(0.0f, hit.distance - 1.0f));
            }

            MonoSingleton<HookArm>.Instance.StopThrow(1f, true);
            __instance.transform.position = targetPosition;
            ___goingLeft = !___goingLeft;

            GameObject.Instantiate<GameObject>(__instance.teleportSound, __instance.transform.position, Quaternion.identity);
            GameObject gameObject = GameObject.Instantiate<GameObject>(__instance.decoy, __instance.transform.GetChild(0).position, __instance.transform.GetChild(0).rotation);
            Animator componentInChildren = gameObject.GetComponentInChildren<Animator>();
            AnimatorStateInfo currentAnimatorStateInfo = ___anim.GetCurrentAnimatorStateInfo(0);
            componentInChildren.Play(currentAnimatorStateInfo.shortNameHash, 0, currentAnimatorStateInfo.normalizedTime);
            componentInChildren.speed = 0f;
            if (___enraged)
            {
                gameObject.GetComponent<MindflayerDecoy>().enraged = true;
            }

            ___anim.speed = 0f;
            __instance.CancelInvoke("ResetAnimSpeed");
            __instance.Invoke("ResetAnimSpeed", 0.25f / ___eid.totalSpeedModifier);

            return false;
        }
    }

    [HarmonyPatch(typeof(SwingCheck2))]
    [HarmonyPatch("DamageStop")]
    class SwingCheck2_DamageStop_Patch
    {
        static void Postfix(SwingCheck2 __instance)
        {
            if (__instance.transform.parent == null)
                return;
            GameObject parent = __instance.transform.parent.gameObject;
            Mindflayer mf = parent.GetComponent<Mindflayer>();
            if (mf == null)
                return;

            MindflayerPatch patch = parent.GetComponent<MindflayerPatch>();
            patch.swingComboLeft = 2;
        }
    }

    class MindflayerPatch : MonoBehaviour
    {
        public int shotsLeft = 20;
        public int swingComboLeft = 2;
    }
}
