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

            if(!(SceneManager.GetActiveScene().name == "Level P-2" && __instance.transform.position == new Vector3(-102, 12.75f, 268)))
            {
                GameObject idol = GameObject.Instantiate(Plugin.idol.gameObject, __instance.transform.position, __instance.transform.rotation, __instance.transform);
                idol.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                idol.AddComponent<IdolFlag>();
                idol.GetComponent<IdolFlag>().parent = __instance.gameObject;
            }

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

            Idol idol = __instance.gameObject.GetComponentInChildren<Idol>();
            if (idol == null)
                return true;

            idol.GetComponent<IdolFlag>().keepAlive = false;
            idol.Death();

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
        static bool Prefix(Drone __instance, ref EnemyIdentifier ___eid, ref int ___difficulty)
        {
            if (___eid.enemyType != EnemyType.Virtue || !__instance.enraged)
                return true;

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
    }

    [HarmonyPatch(typeof(Idol))]
    [HarmonyPatch("Start")]
    class Idol_Start_Patch
    {
        static void Postfix(Idol __instance, ref EnemyIdentifier ___eid)
        {
            __instance.CancelInvoke("SlowUpdate");
            __instance.Invoke("SlowUpdate", 0.2f);
        }
    }

    [HarmonyPatch(typeof(Idol))]
    [HarmonyPatch("Death")]
    class Idol_Death_Patch
    {
        static bool Prefix(Idol __instance, ref EnemyIdentifier ___eid)
        {
            IdolFlag idolFlag = __instance.gameObject.GetComponent<IdolFlag>();
            if (idolFlag != null && idolFlag.parent != null && idolFlag.keepAlive)
                return false;

            return true;
        }
    }

    [HarmonyPatch(typeof(Idol))]
    [HarmonyPatch("SlowUpdate")]
    class Idol_SlowUpdate_Patch
    {
        static bool Prefix(Idol __instance)
        {
            IdolFlag idolFlag = __instance.gameObject.GetComponent<IdolFlag>();
            if (idolFlag != null)
            {
                if (__instance.target != null && !__instance.target.dead)
                {
                    if (__instance.target.enemyType == EnemyType.Virtue)
                        typeof(Idol).GetMethod("ChangeTarget", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(__instance, new object[] { null });
                    else
                        return false;
                }

                List<EnemyIdentifier> currentEnemies = MonoSingleton<EnemyTracker>.Instance.GetCurrentEnemies();
                if (currentEnemies != null && currentEnemies.Count > 0)
                {
                    bool flag = false;
                    float num = float.PositiveInfinity;
                    EnemyIdentifier newTarget = null;
                    int num2 = 1;
                    if (__instance.target && !__instance.target.dead)
                    {
                        num2 = Mathf.Max(MonoSingleton<EnemyTracker>.Instance.GetEnemyRank(__instance.target), 2);
                    }
                    for (int i = 6; i > num2; i--)
                    {
                        for (int j = 0; j < currentEnemies.Count; j++)
                        {
                            if (((!currentEnemies[j].blessed && currentEnemies[j].enemyType != EnemyType.Idol) || currentEnemies[j] == __instance.target) && (MonoSingleton<EnemyTracker>.Instance.GetEnemyRank(currentEnemies[j]) == i || (MonoSingleton<EnemyTracker>.Instance.GetEnemyRank(currentEnemies[j]) <= 2 && i == 2)))
                            {
                                if (currentEnemies[j].enemyType == EnemyType.Virtue)
                                    continue;
                                float num3 = Vector3.Distance(MonoSingleton<PlayerTracker>.Instance.GetPlayer().position, currentEnemies[j].transform.position);
                                if (num3 < num)
                                {
                                    newTarget = currentEnemies[j];
                                    flag = true;
                                    num = num3;
                                }
                            }
                        }
                        if (flag)
                        {
                            typeof(Idol).GetMethod("ChangeTarget", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(__instance, new object[] { newTarget });
                            break;
                        }
                    }

                    if (!flag)
                        typeof(Idol).GetMethod("ChangeTarget", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(__instance, new object[] { null });
                }

                __instance.Invoke("SlowUpdate", 0.2f);
                return false;
            }

            return true;
        }
    }

    class IdolFlag : MonoBehaviour
    {
        public GameObject parent;
        public bool keepAlive = true;
    }
}
