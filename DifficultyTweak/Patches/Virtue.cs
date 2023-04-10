using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace DifficultyTweak.Patches
{
    [HarmonyPatch(typeof(Drone))]
    [HarmonyPatch("Start")]
    class Virtue_Start_Patch
    {
        static void Postfix(Drone __instance, ref EnemyIdentifier ___eid)
        {
            if (___eid.enemyType != EnemyType.Virtue)
                return;

            __instance.gameObject.AddComponent<VirtueFlag>();
        }
    }

    [HarmonyPatch(typeof(Drone))]
    [HarmonyPatch("Death")]
    class Virtue_Death_Patch
    {
        static bool Prefix(Drone __instance, ref EnemyIdentifier ___eid)
        {
            if(___eid.enemyType != EnemyType.Virtue)
                return true;

            __instance.GetComponent<VirtueFlag>().DestroyProjectiles();
            return true;
        }
    }

    class VirtueFlag : MonoBehaviour
    {
        public AudioSource lighningBoltSFX;
        public GameObject ligtningBoltAud;
        public Transform windupObj;
        private EnemyIdentifier eid;

        public void Awake()
        {
            eid = GetComponent<EnemyIdentifier>();
            ligtningBoltAud = Instantiate(Plugin.lighningBoltSFX, transform);
            lighningBoltSFX = ligtningBoltAud.GetComponent<AudioSource>();
        }

        public void SpawnLightningBolt()
        {
            LightningStrikeExplosive lightningStrikeExplosive = Instantiate(Plugin.lighningStrikeExplosive.gameObject, windupObj.transform.position, Quaternion.identity).GetComponent<LightningStrikeExplosive>();
            lightningStrikeExplosive.safeForPlayer = false;
            lightningStrikeExplosive.damageMultiplier = eid.totalDamageModifier;

            if(windupObj != null)
                Destroy(windupObj.gameObject);
        }

        public void DestroyProjectiles()
        {
            CancelInvoke("SpawnLightningBolt");

            if (windupObj != null)
                Destroy(windupObj.gameObject);
        }
    }

    [HarmonyPatch(typeof(Drone))]
    [HarmonyPatch("SpawnInsignia")]
    class Virtue_SpawnInsignia_Patch
    {
        public static float horizontalInsigniaScale = 0.5f;

        static bool Prefix(Drone __instance, ref EnemyIdentifier ___eid, ref int ___difficulty, out bool __state)
        {
            __state = false;
            if (___eid.enemyType != EnemyType.Virtue)
                return true;

            if (!__instance.enraged)
            {
                __state = true;
                return true;
            }

            Vector3 predictedPos;
            if (___difficulty <= 1)
                predictedPos = MonoSingleton<PlayerTracker>.Instance.GetPlayer().position;
            else
            {
                Vector3 vector = new Vector3(MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().x, 0f, MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().z);
                predictedPos = MonoSingleton<PlayerTracker>.Instance.GetPlayer().position + vector.normalized * Mathf.Min(vector.magnitude, 5.0f);
            }

            GameObject currentWindup = GameObject.Instantiate<GameObject>(Plugin.lighningStrikeWindup.gameObject, predictedPos, Quaternion.identity);
            foreach (Follow follow in currentWindup.GetComponents<Follow>())
            {
                if (follow.speed != 0f)
                {
                    if (___difficulty >= 2)
                    {
                        follow.speed *= (float)___difficulty;
                    }
                    else if (___difficulty == 1)
                    {
                        follow.speed /= 2f;
                    }
                    else
                    {
                        follow.enabled = false;
                    }
                    follow.speed *= ___eid.totalSpeedModifier;
                }
            }

            VirtueFlag flag = __instance.GetComponent<VirtueFlag>();
            flag.lighningBoltSFX.Play();
            flag.windupObj = currentWindup.transform;
            flag.Invoke("SpawnLightningBolt", 3.0f);
            return false;
        }

        static void Postfix(Drone __instance, ref EnemyIdentifier ___eid, ref int ___difficulty, ref Transform ___target, bool __state)
        {
            if (!__state)
                return;

            GameObject createInsignia(Drone __instance, ref EnemyIdentifier ___eid, ref int ___difficulty, ref Transform ___target)
            {
                GameObject gameObject = GameObject.Instantiate<GameObject>(__instance.projectile, ___target.transform.position, Quaternion.identity);
                VirtueInsignia component = gameObject.GetComponent<VirtueInsignia>();
                component.target = MonoSingleton<PlayerTracker>.Instance.GetPlayer();
                component.parentDrone = __instance;
                component.hadParent = true;
                __instance.chargeParticle.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
                if (__instance.enraged)
                {
                    component.predictive = true;
                }
                if (___difficulty == 1)
                {
                    component.windUpSpeedMultiplier = 0.875f;
                }
                else if (___difficulty == 0)
                {
                    component.windUpSpeedMultiplier = 0.75f;
                }
                if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.Platformer)
                {
                    gameObject.transform.localScale *= 0.75f;
                    component.windUpSpeedMultiplier *= 0.875f;
                }
                component.windUpSpeedMultiplier *= ___eid.totalSpeedModifier;
                component.damage = Mathf.RoundToInt((float)component.damage * ___eid.totalDamageModifier);

                return gameObject;
            }

            GameObject xAxisInsignia = createInsignia(__instance, ref ___eid, ref ___difficulty, ref ___target);
            xAxisInsignia.transform.Rotate(new Vector3(90, 0, 0));
            xAxisInsignia.transform.localScale = new Vector3(xAxisInsignia.transform.localScale.x * horizontalInsigniaScale, xAxisInsignia.transform.localScale.y, xAxisInsignia.transform.localScale.z * horizontalInsigniaScale);
            GameObject zAxisInsignia = createInsignia(__instance, ref ___eid, ref ___difficulty, ref ___target);
            zAxisInsignia.transform.Rotate(new Vector3(0, 0, 90));
            zAxisInsignia.transform.localScale = new Vector3(zAxisInsignia.transform.localScale.x * horizontalInsigniaScale, zAxisInsignia.transform.localScale.y, zAxisInsignia.transform.localScale.z * horizontalInsigniaScale);
        }
    }
}
