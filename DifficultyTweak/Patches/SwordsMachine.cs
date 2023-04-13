using Discord;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DifficultyTweak.Patches
{
    [HarmonyPatch(typeof(SwordsMachine))]
    [HarmonyPatch("Knockdown")]
    class SwordsMachine_Knockdown_Patch
    {
        static bool Prefix(SwordsMachine __instance, bool __0, ref Animator ___anim)
        {
            if (!Plugin.ultrapainDifficulty || !ConfigManager.enemyTweakToggle.value || !ConfigManager.swordsMachineNoLightKnockbackToggle.value)
                return true;

            if (__0 == true)
            {
                __instance.Enrage();
                return false;
            }

            return true;
        }
    }

    /*[HarmonyPatch(typeof(SwordsMachine))]
    [HarmonyPatch("Down")]
    class SwordsMachine_Down_Patch
    {
        static void Postfix(SwordsMachine __instance, ref Animator ___anim, ref Machine ___mach)
        {
            ___anim.Play("Knockdown", 0, Plugin.SwordsMachineKnockdownTimeNormalized);

            __instance.CancelInvoke("CheckLoop");
            ___mach.health = ___mach.symbiote.health;
            __instance.downed = false;
        }
    }

    [HarmonyPatch(typeof(SwordsMachine))]
    [HarmonyPatch("CheckLoop")]
    class SwordsMachine_CheckLoop_Patch
    {
        static bool Prefix(SwordsMachine __instance)
        {
            return false;
        }
    }*/

    /*[HarmonyPatch(typeof(SwordsMachine))]
    [HarmonyPatch("ShootGun")]
    class SwordsMachine_ShootGun_Patch
    {
        static bool Prefix(SwordsMachine __instance)
        {
            if(UnityEngine.Random.RandomRangeInt(0, 2) == 1)
            {
                GameObject grn = GameObject.Instantiate(Plugin.shotgunGrenade.gameObject, __instance.transform.position, __instance.transform.rotation);
                grn.transform.position += grn.transform.forward * 0.5f + grn.transform.up * 0.5f;

                Grenade grnComp = grn.GetComponent<Grenade>();
                grnComp.enemy = true;
                grnComp.CanCollideWithPlayer(true);

                Vector3 playerPosition = MonoSingleton<PlayerTracker>.Instance.gameObject.transform.position;
                float distanceFromPlayer = Vector3.Distance(playerPosition, grn.transform.position);
                Vector3 predictedPosition = MonoSingleton<PlayerTracker>.Instance.PredictPlayerPosition(distanceFromPlayer / 40);

                grn.transform.LookAt(predictedPosition);
                grn.GetComponent<Rigidbody>().maxAngularVelocity = 40;
                grn.GetComponent<Rigidbody>().velocity = grn.transform.forward * 40;

                return false;
            }

            return true;
        }
    }*/

    [HarmonyPatch(typeof(ThrownSword))]
    [HarmonyPatch("Start")]
    class ThrownSword_Start_Patch
    {
        static void Postfix(ThrownSword __instance)
        {
            if (!Plugin.ultrapainDifficulty || !ConfigManager.enemyTweakToggle.value || !ConfigManager.swordsMachineExplosiveSwordToggle.value)
                return;

            __instance.gameObject.AddComponent<ThrownSwordCollisionDetector>();
        }
    }

    [HarmonyPatch(typeof(ThrownSword))]
    [HarmonyPatch("OnTriggerEnter")]
    class ThrownSword_OnTriggerEnter_Patch
    {
        static void Postfix(ThrownSword __instance, Collider __0)
        {
            if (!Plugin.ultrapainDifficulty || !ConfigManager.enemyTweakToggle.value || !ConfigManager.swordsMachineExplosiveSwordToggle.value)
                return;

            if (__0.gameObject.tag == "Player")
            {
                GameObject explosionObj = GameObject.Instantiate(Plugin.shotgunGrenade.gameObject.GetComponent<Grenade>().explosion, __0.gameObject.transform.position, __0.gameObject.transform.rotation);
                foreach (Explosion explosion in explosionObj.GetComponentsInChildren<Explosion>())
                {
                    explosion.enemy = true;
                }
            }
        }
    }

    class ThrownSwordCollisionDetector : MonoBehaviour
    {
        public bool exploded = false;

        public void OnCollisionEnter(Collision other)
        {
            if (exploded)
                return;

            if (other.gameObject.layer != 24)
            {
                Debug.Log($"Hit layer {other.gameObject.layer}");
                return;
            }

            exploded = true;

            GameObject explosionObj = GameObject.Instantiate(Plugin.shotgunGrenade.gameObject.GetComponent<Grenade>().explosion, transform.position, transform.rotation);
            foreach (Explosion explosion in explosionObj.GetComponentsInChildren<Explosion>())
            {
                explosion.enemy = true;
                explosion.damage = ConfigManager.swordsMachineExplosiveSwordDamage.value;
                explosion.maxSize *= ConfigManager.swordsMachineExplosiveSwordSize.value;
                explosion.speed *= ConfigManager.swordsMachineExplosiveSwordSize.value;
            }

            gameObject.GetComponent<ThrownSword>().Invoke("Return", 0.1f);
        }
    }
}
