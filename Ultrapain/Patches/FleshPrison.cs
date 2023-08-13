using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UltrapainExtensions;
using UnityEngine;
using UnityEngine.UI;

namespace Ultrapain.Patches
{
    public class FleshPrisonFlag : MonoBehaviour
    {

    }

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
		public float damageMod = 1f;
		public float speedMod = 1f;

		void SpawnInsignias()
		{
			insignias.Clear();

			int projectileCount = (prison.altVersion ? ConfigManager.panopticonSpinAttackCount.value : ConfigManager.fleshPrisonSpinAttackCount.value);
			float anglePerProjectile = 360f / projectileCount;
			float distance = (prison.altVersion ? ConfigManager.panopticonSpinAttackDistance.value : ConfigManager.fleshPrisonSpinAttackDistance.value);

			Vector3 currentNormal = Vector3.forward;
			for (int i = 0; i < projectileCount; i++)
			{
				GameObject insignia = Instantiate(Plugin.virtueInsignia.obj, transform.position + currentNormal * distance, Quaternion.identity);
				insignia.transform.parent = gameObject.transform;
				VirtueInsignia comp = insignia.GetComponent<VirtueInsignia>();
				comp.hadParent = false;
				comp.noTracking = true;
				comp.predictive = true;
				comp.predictiveVersion = null;
				comp.otherParent = transform;
				comp.target = insignia.transform;
				comp.windUpSpeedMultiplier = (prison.altVersion ? ConfigManager.panopticonSpinAttackActivateSpeed.value : ConfigManager.fleshPrisonSpinAttackActivateSpeed.value) * speedMod;
				comp.damage = (int)((prison.altVersion ? ConfigManager.panopticonSpinAttackDamage.value : ConfigManager.fleshPrisonSpinAttackDamage.value) * damageMod);
				float size = Mathf.Abs(prison.altVersion ? ConfigManager.panopticonSpinAttackSize.value : ConfigManager.fleshPrisonSpinAttackSize.value);
				insignia.transform.localScale = new Vector3(size, insignia.transform.localScale.y, size);
				insignias.Add(comp);
				currentNormal = Quaternion.Euler(0, anglePerProjectile, 0) * currentNormal;
			}
		}

		FieldInfo inAction;
		public float anglePerSecond = 1f;
		void Start()
		{
			SpawnInsignias();
			inAction = typeof(FleshPrison).GetField("inAction", BindingFlags.Instance | BindingFlags.NonPublic);
			anglePerSecond = prison.altVersion ? ConfigManager.panopticonSpinAttackTurnSpeed.value : ConfigManager.fleshPrisonSpinAttackTurnSpeed.value;
			if (UnityEngine.Random.RandomRangeInt(0, 100) < 50)
				anglePerSecond *= -1;
		}

		bool markedForDestruction = false;
		void Update()
		{
			transform.Rotate(new Vector3(0, anglePerSecond * Time.deltaTime * speedMod, 0));

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


	[UltrapainPatch]
    [HarmonyPatch(typeof(FleshPrison))]
    public static class FleshPrisonPatch
    {
        [HarmonyPatch(nameof(FleshPrison.Start))]
        [HarmonyPrefix]
        [UltrapainPatch]
		public static bool FleshObamium(FleshPrison __instance)
		{
			if (__instance.altVersion)
				return true;

			if (__instance.eid == null)
				__instance.eid = __instance.GetComponent<EnemyIdentifier>();
			__instance.eid.overrideFullName = ConfigManager.fleshObamiumName.value;
			return true;
		}

		public static bool FleshObamiumCheck()
		{
			return ConfigManager.fleshObamiumToggle.value;
		}

		[HarmonyPatch(nameof(FleshPrison.Start))]
		[HarmonyPostfix]
		[UltrapainPatch]
		public static void PostFleshObamium(FleshPrison __instance)
		{
			if (__instance.altVersion)
				return;

			GameObject fleshObamium = GameObject.Instantiate(Plugin.fleshObamium, __instance.transform);
			fleshObamium.transform.parent = __instance.transform.Find("fleshprisonrigged/Armature/root/prism/");
			fleshObamium.transform.localScale = new Vector3(36, 36, 36);
			fleshObamium.transform.localPosition = Vector3.zero;
			fleshObamium.transform.localRotation = Quaternion.identity;
			fleshObamium.transform.Rotate(new Vector3(180, 0, 0), Space.Self);
			fleshObamium.GetComponent<MeshRenderer>().material.color = new Color(0.15f, 0.15f, 0.15f, 1f);

			fleshObamium.layer = 24;

			// __instance.transform.Find("FleshPrison2/FleshPrison2_Head").GetComponent<SkinnedMeshRenderer>().enabled = false;

			if (__instance.bossHealth != null)
			{
				__instance.bossHealth.bossName = ConfigManager.fleshObamiumName.value;
				if (__instance.bossHealth.bossBar != null)
				{
					BossHealthBarTemplate temp = __instance.bossHealth.bossBar.GetComponent<BossHealthBarTemplate>();
					temp.bossNameText.text = ConfigManager.fleshObamiumName.value;
					foreach (Text t in temp.textInstances)
						t.text = ConfigManager.fleshObamiumName.value;
				}
			}
		}

		public static bool PostFleshObamiumCheck()
		{
			return ConfigManager.fleshObamiumToggle.value;
		}

		[HarmonyPatch(nameof(FleshPrison.HomingProjectileAttack))]
		[HarmonyPostfix]
		[UltrapainPatch]
		public static void SpinInsignia(FleshPrison __instance)
		{
			if (__instance.GetComponent<FleshPrisonFlag>() == null)
				return;

			GameObject obj = new GameObject();
			obj.transform.position = __instance.transform.position + Vector3.up;
			FleshPrisonRotatingInsignia flag = obj.AddComponent<FleshPrisonRotatingInsignia>();
			flag.prison = __instance;
			flag.damageMod = __instance.eid.totalDamageModifier;
			flag.speedMod = __instance.eid.totalSpeedModifier;
		}
	
		public static bool SpinInsigniaCheck()
		{
			return ConfigManager.enemyTweakToggle.value && ConfigManager.fleshPrisonSpinAttackToggle.value;
		}

		[HarmonyPatch(nameof(FleshPrison.Start))]
		[HarmonyPostfix]
		[UltrapainPatch]
		public static void AddFlag(FleshPrison __instance)
		{
			if (__instance.altVersion)
				return;

			if (__instance.gameObject.GetComponent<NonUltrapainEnemy>() != null)
				return;

			__instance.gameObject.AddComponent<FleshPrisonFlag>();
		}
	
		public static bool AddFlagCheck()
		{
			return ConfigManager.enemyTweakToggle.value;
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
                GameObject gameObject = GameObject.Instantiate(__instance.insignia, MonoSingleton<PlayerTracker>.Instance.GetPlayer().position, Quaternion.identity);
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
