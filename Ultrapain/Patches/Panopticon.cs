using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

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

            xInsignia.transform.localScale *= ConfigManager.panopticonAxisBeamSizeMulti.value;
            zInsignia.transform.localScale *= ConfigManager.panopticonAxisBeamSizeMulti.value;

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
}
