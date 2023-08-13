using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Ultrapain.Patches
{
    class Panopticon_Start
    {
        static void Postfix(FleshPrison __instance)
        {
            if (__instance.altVersion)
                __instance.onFirstHeal = new UltrakillEvent();
        }
    }

    class Obamapticon_Start
    {
        static bool Prefix(FleshPrison __instance)
        {
            if (!__instance.altVersion)
                return true;

            if (__instance.eid == null)
                __instance.eid = __instance.GetComponent<EnemyIdentifier>();
            __instance.eid.overrideFullName = ConfigManager.obamapticonName.value;
            return true;
        }

        static void Postfix(FleshPrison __instance)
        {
            if (!__instance.altVersion)
                return;

            GameObject obamapticon = GameObject.Instantiate(Plugin.obamapticon, __instance.transform);
            obamapticon.transform.parent = __instance.transform.Find("FleshPrison2/Armature/FP2_Root/Head_Root");
            obamapticon.transform.localScale = new Vector3(15.4f, 15.4f, 15.4f);
            obamapticon.transform.localPosition = Vector3.zero;
            obamapticon.transform.localRotation = Quaternion.identity;

            obamapticon.layer = 24;

            __instance.transform.Find("FleshPrison2/FleshPrison2_Head").GetComponent<SkinnedMeshRenderer>().enabled = false;

            if (__instance.bossHealth != null)
            {
                __instance.bossHealth.bossName = ConfigManager.obamapticonName.value;
                if (__instance.bossHealth.bossBar != null)
                {
                    BossHealthBarTemplate temp = __instance.bossHealth.bossBar.GetComponent<BossHealthBarTemplate>();
                    temp.bossNameText.text = ConfigManager.obamapticonName.value;
                    foreach (Text t in temp.textInstances)
                        t.text = ConfigManager.obamapticonName.value;
                }
            }
        }
    }

    class Panopticon_SpawnInsignia
    {
        static bool Prefix(FleshPrison __instance, ref bool ___inAction, Statue ___stat, float ___maxHealth, int ___difficulty,
            ref float ___fleshDroneCooldown, EnemyIdentifier ___eid)
        {
            if (!__instance.altVersion)
                return true;

            ___inAction = false;
            GameObject gameObject = GameObject.Instantiate<GameObject>(__instance.insignia, MonoSingleton<PlayerTracker>.Instance.GetPlayer().position, Quaternion.identity);
            Vector3 playerVelocity = MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity();
            playerVelocity.y = 0f;
            if (playerVelocity.magnitude > 0f)
            {
                gameObject.transform.LookAt(MonoSingleton<PlayerTracker>.Instance.GetPlayer().position + playerVelocity);
            }
            else
            {
                gameObject.transform.Rotate(Vector3.up * UnityEngine.Random.Range(0f, 360f), Space.Self);
            }
            gameObject.transform.Rotate(Vector3.right * 90f, Space.Self);
            VirtueInsignia virtueInsignia;
            if (gameObject.TryGetComponent<VirtueInsignia>(out virtueInsignia))
            {
                virtueInsignia.predictive = true;
                virtueInsignia.noTracking = true;
                virtueInsignia.otherParent = __instance.transform;
                if (___stat.health > ___maxHealth / 2f)
                {
                    virtueInsignia.charges = 2;
                }
                else
                {
                    virtueInsignia.charges = 3;
                }
                if (___difficulty == 3)
                {
                    virtueInsignia.charges++;
                }
                virtueInsignia.windUpSpeedMultiplier = 0.5f;
                virtueInsignia.windUpSpeedMultiplier *= ___eid.totalSpeedModifier;
                virtueInsignia.damage = Mathf.RoundToInt((float)virtueInsignia.damage * ___eid.totalDamageModifier);
                virtueInsignia.target = MonoSingleton<PlayerTracker>.Instance.GetPlayer();
                virtueInsignia.predictiveVersion = null;
                Light light = gameObject.AddComponent<Light>();
                light.range = 30f;
                light.intensity = 50f;
            }
            if (___difficulty >= 2)
            {
                gameObject.transform.localScale = new Vector3(8f, 2f, 8f);
            }
            else if (___difficulty == 1)
            {
                gameObject.transform.localScale = new Vector3(7f, 2f, 7f);
            }
            else
            {
                gameObject.transform.localScale = new Vector3(5f, 2f, 5f);
            }
            GoreZone componentInParent = __instance.GetComponentInParent<GoreZone>();
            if (componentInParent)
            {
                gameObject.transform.SetParent(componentInParent.transform, true);
            }
            else
            {
                gameObject.transform.SetParent(__instance.transform, true);
            }

            // CUSTOM CODE HERE
            GameObject xInsignia = GameObject.Instantiate(gameObject, gameObject.transform.position, gameObject.transform.rotation, gameObject.transform.parent);
            GameObject zInsignia = GameObject.Instantiate(gameObject, gameObject.transform.position, gameObject.transform.rotation, gameObject.transform.parent);
            
            xInsignia.transform.Rotate(xInsignia.transform.right, 90f, Space.World);
            zInsignia.transform.Rotate(zInsignia.transform.forward, 90f, Space.World);

            Quaternion xRot = xInsignia.transform.rotation;
            Quaternion yRot = gameObject.transform.rotation;
            Quaternion zRot = zInsignia.transform.rotation;

            RotateOnSpawn xInsigniaRotation = xInsignia.AddComponent<RotateOnSpawn>();
            RotateOnSpawn zInsigniaRotation = zInsignia.AddComponent<RotateOnSpawn>();
            RotateOnSpawn yInsigniaRotation = gameObject.AddComponent<RotateOnSpawn>();

            xInsignia.transform.rotation = xInsigniaRotation.targetRotation = xRot;
            gameObject.transform.rotation = yInsigniaRotation.targetRotation = yRot;
            zInsignia.transform.rotation = zInsigniaRotation.targetRotation = zRot;

            xInsignia.transform.localScale = new Vector3(xInsignia.transform.localScale.x * ConfigManager.panopticonAxisBeamSizeMulti.value, xInsignia.transform.localScale.y, xInsignia.transform.localScale.z * ConfigManager.panopticonAxisBeamSizeMulti.value);
            zInsignia.transform.localScale = new Vector3(zInsignia.transform.localScale.x * ConfigManager.panopticonAxisBeamSizeMulti.value, zInsignia.transform.localScale.y, zInsignia.transform.localScale.z * ConfigManager.panopticonAxisBeamSizeMulti.value);

            if (___fleshDroneCooldown < 1f)
            {
                ___fleshDroneCooldown = 1f;
            }

            return false;
        }
    }

    class Panopticon_HomingProjectileAttack
    {
        static void Postfix(FleshPrison __instance, EnemyIdentifier ___eid)
        {
            if (!__instance.altVersion)
                return;

            GameObject obj = new GameObject();
            obj.transform.position = __instance.transform.position + Vector3.up;
            FleshPrisonRotatingInsignia flag = obj.AddComponent<FleshPrisonRotatingInsignia>();
            flag.prison = __instance;
            flag.damageMod = ___eid.totalDamageModifier;
            flag.speedMod = ___eid.totalSpeedModifier;
        }
    }

    class Panopticon_SpawnBlackHole
    {
        static void Postfix(FleshPrison __instance, EnemyIdentifier ___eid)
        {
            if (!__instance.altVersion)
                return;

            Vector3 vector = MonoSingleton<PlayerTracker>.Instance.PredictPlayerPosition(0.5f / ___eid.totalSpeedModifier);
            GameObject gameObject = GameObject.Instantiate(Plugin.sisyphusDestroyExplosion, vector, Quaternion.identity);
            GoreZone gz = __instance.GetComponentInParent<GoreZone>();
            gameObject.transform.SetParent(gz == null ? __instance.transform : gz.transform);
            LineRenderer componentInChildren = gameObject.GetComponentInChildren<LineRenderer>();
            if (componentInChildren)
            {
                componentInChildren.SetPosition(0, vector);
                componentInChildren.SetPosition(1, __instance.transform.position);
            }
            foreach (Explosion explosion in gameObject.GetComponentsInChildren<Explosion>())
            {
                explosion.speed *= ___eid.totalSpeedModifier;
                explosion.damage = Mathf.RoundToInt((float)explosion.damage * ___eid.totalDamageModifier);
                explosion.maxSize *= ___eid.totalDamageModifier;
            }
        }
    }

    class Panopticon_SpawnFleshDrones
    {
        struct StateInfo
        {
            public GameObject template;
            public bool changedToEye;
        }

        static bool Prefix(FleshPrison __instance, int ___difficulty, int ___currentDrone, out StateInfo __state)
        {
            __state = new StateInfo();
            if (!__instance.altVersion)
                return true;

            if (___currentDrone % 2 == 0)
            {
                __state.template = __instance.skullDrone;
                __state.changedToEye = true;
                __instance.skullDrone = __instance.fleshDrone;
            }
            else
            {
                __state.template = __instance.fleshDrone;
                __state.changedToEye = false;
                __instance.fleshDrone = __instance.skullDrone;
            }

            return true;
        }

        static void Postfix(FleshPrison __instance, StateInfo __state)
        {
            if (!__instance.altVersion)
                return;

            if (__state.changedToEye)
                __instance.skullDrone = __state.template;
            else
                __instance.fleshDrone = __state.template;
        }
    }

    class Panopticon_BlueProjectile
    {
        public static void BlueProjectileSpawn(FleshPrison instance)
        {
            if (!instance.altVersion || !ConfigManager.panopticonBlueProjToggle.value)
                return;

            int count = ConfigManager.panopticonBlueProjCount.value;
            float deltaAngle = 360f / (count + 1);
            float currentAngle = deltaAngle;

            for (int i = 0; i < count; i++)
            {
                GameObject proj = GameObject.Instantiate(Plugin.homingProjectile.obj, instance.rotationBone.position + instance.rotationBone.up * 16f, instance.rotationBone.rotation);
                proj.transform.position += proj.transform.forward * 5f;
                proj.transform.RotateAround(instance.transform.position, Vector3.up, currentAngle);
                currentAngle += deltaAngle;
                Projectile comp = proj.GetComponent<Projectile>();
                comp.safeEnemyType = EnemyType.FleshPanopticon;
                comp.target = instance.target;
                comp.damage = ConfigManager.panopticonBlueProjDamage.value * instance.eid.totalDamageModifier;
                comp.turningSpeedMultiplier *= ConfigManager.panopticonBlueProjTurnSpeed.value;
                comp.speed = ConfigManager.panopticonBlueProjInitialSpeed.value;
            }
        }

        static MethodInfo m_Panopticon_BlueProjectile_BlueProjectileSpawn = typeof(Panopticon_BlueProjectile).GetMethod("BlueProjectileSpawn", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        static MethodInfo m_GameObject_GetComponent_Projectile = typeof(GameObject).GetMethod("GetComponent", new Type[0]).MakeGenericMethod(new Type[1] { typeof(Projectile) });

        static IEnumerable Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Callvirt && code[i].OperandIs(m_GameObject_GetComponent_Projectile))
                {
                    i += 2;

                    // Push instance reference
                    code.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                    i += 1;
                    // Call the method
                    code.Insert(i, new CodeInstruction(OpCodes.Call, m_Panopticon_BlueProjectile_BlueProjectileSpawn));

                    break;
                }
            }

            return code.AsEnumerable();
        }
    }
}
