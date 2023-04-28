using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Ultrapain.Patches
{
    class FleshPrisonProjectile : MonoBehaviour
    {
        void Start()
        {
            GetComponent<Rigidbody>().AddForce(Vector3.up * 50f, ForceMode.VelocityChange);
        }
    }

    class FleshPrisonRotatingInsignia : MonoBehaviour
    {
        List<VirtueInsignia> insignias = new List<VirtueInsignia>();
        public FleshPrison prison;

        void SpawnInsignias()
        {
            insignias.Clear();

            int projectileCount = ConfigManager.fleshPrisonSpinAttackCount.value;
            float anglePerProjectile = 360f / projectileCount;
            float distance = ConfigManager.fleshPrisonSpinAttackDistance.value;

            Vector3 currentNormal = Vector3.forward;
            for (int i = 0; i < projectileCount; i++)
            {
                GameObject insignia = Instantiate(Plugin.virtueInsignia, transform.position + currentNormal * distance, Quaternion.identity);
                insignia.transform.parent = gameObject.transform;
                VirtueInsignia comp = insignia.GetComponent<VirtueInsignia>();
                comp.hadParent = false;
                comp.noTracking = true;
                comp.predictive = true;
                comp.predictiveVersion = null;
                comp.otherParent = transform;
                comp.target = insignia.transform;
                comp.windUpSpeedMultiplier = ConfigManager.fleshPrisonSpinAttackActivateSpeed.value;
                comp.damage = ConfigManager.fleshPrisonSpinAttackDamage.value;
                float size = Mathf.Abs(ConfigManager.fleshPrisonSpinAttackSize.value);
                insignia.transform.localScale = new Vector3(size, insignia.transform.localScale.y, size);
                insignias.Add(comp);
                currentNormal = Quaternion.Euler(0, anglePerProjectile, 0) * currentNormal;
            }
        }

        FieldInfo inAction;
        void Start()
        {
            SpawnInsignias();
            inAction = typeof(FleshPrison).GetField("inAction", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public float anglePerSecond = ConfigManager.fleshPrisonSpinAttackTurnSpeed.value;
        bool markedForDestruction = false;
        void Update()
        {
            transform.Rotate(new Vector3(0, anglePerSecond * Time.deltaTime, 0));

            if (!markedForDestruction && (prison == null || !(bool)inAction.GetValue(prison)))
            {
                markedForDestruction = true;
                return;
            }

            if (insignias.Count == 0 || insignias[0] == null)
                if (markedForDestruction)
                    Destroy(gameObject);
                else
                    SpawnInsignias();
        }
    }

    class FleshPrisonStart
    {
        static void Postfix(FleshPrison __instance)
        {
            if (__instance.altVersion)
                return;

            //__instance.homingProjectile = GameObject.Instantiate(Plugin.hideousMassProjectile, Vector3.positiveInfinity, Quaternion.identity);
            //__instance.homingProjectile.hideFlags = HideFlags.HideAndDontSave;
            //SceneManager.MoveGameObjectToScene(__instance.homingProjectile, SceneManager.GetSceneByName(""));
            //__instance.homingProjectile.AddComponent<FleshPrisonProjectile>();
        }
    }

    class FleshPrisonShoot
    {
        static void Postfix(FleshPrison __instance, ref Animator ___anim)
        {
            if (__instance.altVersion)
                return;

            GameObject obj = new GameObject();
            obj.transform.position = __instance.transform.position + Vector3.up;
            FleshPrisonRotatingInsignia flag = obj.AddComponent<FleshPrisonRotatingInsignia>();
            flag.prison = __instance;
        }
    }

    /*[HarmonyPatch(typeof(FleshPrison), "SpawnInsignia")]
    class FleshPrisonInsignia
    {
        static bool Prefix(FleshPrison __instance, ref bool ___inAction, ref float ___fleshDroneCooldown, EnemyIdentifier ___eid,
            Statue ___stat, float ___maxHealth)
        {
            if (__instance.altVersion)
                return true;

            if (!Plugin.ultrapainDifficulty || !ConfigManager.enemyTweakToggle.value)
                return true;

            ___inAction = false;

            GameObject CreateInsignia()
            {
                GameObject gameObject = GameObject.Instantiate<GameObject>(__instance.insignia, MonoSingleton<PlayerTracker>.Instance.GetPlayer().position, Quaternion.identity);
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
                    virtueInsignia.charges++;
                    virtueInsignia.windUpSpeedMultiplier = 0.5f;
                    virtueInsignia.windUpSpeedMultiplier *= ___eid.totalSpeedModifier;
                    virtueInsignia.damage = Mathf.RoundToInt((float)virtueInsignia.damage * ___eid.totalDamageModifier);
                    virtueInsignia.target = MonoSingleton<PlayerTracker>.Instance.GetPlayer();
                    virtueInsignia.predictiveVersion = null;
                    Light light = gameObject.AddComponent<Light>();
                    light.range = 30f;
                    light.intensity = 50f;
                }
                gameObject.transform.localScale = new Vector3(5f, 2f, 5f);
                GoreZone componentInParent = __instance.GetComponentInParent<GoreZone>();
                if (componentInParent)
                {
                    gameObject.transform.SetParent(componentInParent.transform, true);
                }
                else
                {
                    gameObject.transform.SetParent(__instance.transform, true);
                }

                return gameObject;
            }

            GameObject InsigniaY = CreateInsignia();
            GameObject InsigniaX = CreateInsignia();
            GameObject InsigniaZ = CreateInsignia();

            InsigniaX.transform.eulerAngles = new Vector3(0, MonoSingleton<PlayerTracker>.Instance.GetTarget().transform.rotation.eulerAngles.y, 0);
            InsigniaX.transform.Rotate(new Vector3(90, 0, 0), Space.Self);
            InsigniaZ.transform.eulerAngles = new Vector3(0, MonoSingleton<PlayerTracker>.Instance.GetTarget().transform.rotation.eulerAngles.y, 0);
            InsigniaZ.transform.Rotate(new Vector3(0, 0, 90), Space.Self);

            if (___fleshDroneCooldown < 1f)
            {
                ___fleshDroneCooldown = 1f;
            }

            return false;
        }
    }*/
}
