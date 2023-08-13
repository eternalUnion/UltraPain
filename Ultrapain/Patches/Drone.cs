using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using UltrapainExtensions;
using UnityEngine;

namespace Ultrapain.Patches
{
	public class DroneFlag : MonoBehaviour
	{
		public enum Firemode : int
		{
			Projectile = 0,
			Explosive,
			TurretBeam
		}

		public ParticleSystem particleSystem;
		public LineRenderer lr;
		public Firemode currentMode = Firemode.Projectile;
		private static Firemode[] allModes = Enum.GetValues(typeof(Firemode)) as Firemode[];

		static Material whiteMat;
		public void Awake()
		{
			lr = gameObject.AddComponent<LineRenderer>();
			lr.enabled = false;
			lr.receiveShadows = false;
			lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			lr.startWidth = lr.endWidth = lr.widthMultiplier = 0.025f;

			if (whiteMat == null)
				whiteMat = Plugin.turretComp.aimLine.material;

			lr.material = whiteMat;
		}

		public void SetLineColor(Color c)
		{
			Gradient gradient = new Gradient();
			GradientColorKey[] array = new GradientColorKey[1];
			array[0].color = c;
			GradientAlphaKey[] array2 = new GradientAlphaKey[1];
			array2[0].alpha = 1f;
			gradient.SetKeys(array, array2);
			lr.colorGradient = gradient;
		}

		public void LineRendererColorToWarning()
		{
			SetLineColor(ConfigManager.droneSentryBeamLineWarningColor.value);
		}

		public float attackDelay = -1;
		public bool homingTowardsPlayer = false;

		Transform target;
		Rigidbody rb;

		private void Update()
		{
			if (homingTowardsPlayer)
			{
				if (target == null)
					target = PlayerTracker.Instance.GetTarget();
				if (rb == null)
					rb = GetComponent<Rigidbody>();

				Quaternion to = Quaternion.LookRotation(target.position/* + MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity()*/ - transform.position);
				transform.rotation = Quaternion.RotateTowards(transform.rotation, to, Time.deltaTime * ConfigManager.droneHomeTurnSpeed.value);
				rb.velocity = transform.forward * rb.velocity.magnitude;
			}

			if (lr.enabled)
			{
				lr.SetPosition(0, transform.position);
				lr.SetPosition(1, transform.position + transform.forward * 1000);
			}
		}
	}

    [UltrapainPatch]
    [HarmonyPatch(typeof(Drone))]
    public static class DronePatch
    {
		public static ParticleSystem antennaFlash;
		
		[HarmonyPatch(nameof(Drone.PlaySound))]
		[HarmonyPrefix]
		[UltrapainPatch]
		public static bool SelectAndDisplayAttack(Drone __instance, ref AudioClip __0)
		{
			if (__instance.eid.enemyType != EnemyType.Drone)
				return true;

			if (__0 == __instance.windUpSound)
			{
				DroneFlag flag = __instance.GetComponent<DroneFlag>();
				if (flag == null)
					return true;

				List<Tuple<DroneFlag.Firemode, float>> chances = new List<Tuple<DroneFlag.Firemode, float>>();
				if (ConfigManager.droneProjectileToggle.value)
					chances.Add(new Tuple<DroneFlag.Firemode, float>(DroneFlag.Firemode.Projectile, ConfigManager.droneProjectileChance.value));
				if (ConfigManager.droneExplosionBeamToggle.value)
					chances.Add(new Tuple<DroneFlag.Firemode, float>(DroneFlag.Firemode.Explosive, ConfigManager.droneExplosionBeamChance.value));
				if (ConfigManager.droneSentryBeamToggle.value)
					chances.Add(new Tuple<DroneFlag.Firemode, float>(DroneFlag.Firemode.TurretBeam, ConfigManager.droneSentryBeamChance.value));

				if (chances.Count == 0 || chances.Sum(item => item.Item2) <= 0)
					flag.currentMode = DroneFlag.Firemode.Projectile;
				else
					flag.currentMode = UnityUtils.GetRandomFloatWeightedItem(chances, item => item.Item2).Item1;

				if (flag.currentMode == DroneFlag.Firemode.Projectile)
				{
					flag.attackDelay = ConfigManager.droneProjectileDelay.value;
					return true;
				}
				else if (flag.currentMode == DroneFlag.Firemode.Explosive)
				{
					flag.attackDelay = ConfigManager.droneExplosionBeamDelay.value;

					GameObject chargeEffect = GameObject.Instantiate(Plugin.chargeEffect.obj.obj, __instance.transform);
					chargeEffect.transform.localPosition = new Vector3(0, 0, 0.8f);
					chargeEffect.transform.localScale = Vector3.zero;

					float duration = ConfigManager.droneExplosionBeamDelay.value / __instance.eid.totalSpeedModifier;
					RemoveOnTime remover = chargeEffect.AddComponent<RemoveOnTime>();
					remover.time = duration;
					CommonLinearScaler scaler = chargeEffect.AddComponent<CommonLinearScaler>();
					scaler.targetTransform = scaler.transform;
					scaler.scaleSpeed = 1f / duration;
					CommonAudioPitchScaler pitchScaler = chargeEffect.AddComponent<CommonAudioPitchScaler>();
					pitchScaler.targetAud = chargeEffect.GetComponent<AudioSource>();
					pitchScaler.scaleSpeed = 1f / duration;

					return false;
				}
				else if (flag.currentMode == DroneFlag.Firemode.TurretBeam)
				{
					flag.attackDelay = ConfigManager.droneSentryBeamDelay.value;
					if (ConfigManager.droneDrawSentryBeamLine.value)
					{
						flag.lr.enabled = true;
						flag.SetLineColor(ConfigManager.droneSentryBeamLineNormalColor.value);
						flag.Invoke("LineRendererColorToWarning", Mathf.Max(0.01f, (flag.attackDelay / __instance.eid.totalSpeedModifier) - ConfigManager.droneSentryBeamLineIndicatorDelay.value));
					}

					if (flag.particleSystem == null)
					{
						if (antennaFlash == null)
							antennaFlash = Plugin.turretComp.antennaFlash;
						flag.particleSystem = GameObject.Instantiate(antennaFlash, __instance.transform);
						flag.particleSystem.transform.localPosition = new Vector3(0, 0, 2);
					}

					flag.particleSystem.Play();
					GameObject flash = GameObject.Instantiate(Plugin.turretFinalFlash.obj, __instance.transform);
					GameObject.Destroy(flash.transform.Find("MuzzleFlash/muzzleflash").gameObject);
					return false;
				}
			}

			return true;
		}
		
		public static bool SelectAndDisplayAttackCheck()
		{
			return ConfigManager.enemyTweakToggle.value;
		}

		[HarmonyPatch(nameof(Drone.Shoot))]
		[HarmonyPrefix]
		[UltrapainPatch]
		public static bool CustomAttack(Drone __instance)
		{
			DroneFlag flag = __instance.GetComponent<DroneFlag>();
			if (flag == null || __instance.crashing)
				return true;

			DroneFlag.Firemode mode = flag.currentMode;

			if (mode == DroneFlag.Firemode.Projectile)
				return true;
			if (mode == DroneFlag.Firemode.Explosive)
			{
				GameObject beam = GameObject.Instantiate(Plugin.beam.obj.gameObject, __instance.transform.position + __instance.transform.forward, __instance.transform.rotation);

				RevolverBeam revBeam = beam.GetComponent<RevolverBeam>();
				revBeam.hitParticle = Plugin.shotgunGrenade.obj.gameObject.GetComponent<Grenade>().explosion;
				revBeam.damage /= 2;
				revBeam.damage *= __instance.eid.totalDamageModifier;

				return false;
			}
			if (mode == DroneFlag.Firemode.TurretBeam)
			{
				GameObject turretBeam = GameObject.Instantiate(Plugin.turretBeam.obj.gameObject, __instance.transform.position + __instance.transform.forward * 2f, __instance.transform.rotation);
				if (turretBeam.TryGetComponent<RevolverBeam>(out RevolverBeam revBeam))
				{
					revBeam.damage = ConfigManager.droneSentryBeamDamage.value;
					revBeam.damage *= __instance.eid.totalDamageModifier;
					revBeam.alternateStartPoint = __instance.transform.position + __instance.transform.forward;
					revBeam.ignoreEnemyType = EnemyType.Drone;
				}

				flag.lr.enabled = false;

				return false;
			}

			Debug.LogError($"Drone fire mode in impossible state. Current value: {mode} : {(int)mode}");
			return true;
		}
	
		public static bool CustomAttackCheck()
		{
			return ConfigManager.enemyTweakToggle.value;
		}

		[HarmonyPatch(nameof(Drone.Update))]
		[HarmonyPostfix]
		[UltrapainPatch]
		public static void SetAttackDelay(Drone __instance)
		{
			if (__instance.eid.enemyType != EnemyType.Drone)
				return;

			DroneFlag flag = __instance.GetComponent<DroneFlag>();
			if (flag == null || flag.attackDelay < 0)
				return;

			float attackSpeedDecay = (float)(__instance.difficulty / 2);
			if (__instance.difficulty == 1)
			{
				attackSpeedDecay = 0.75f;
			}
			else if (__instance.difficulty == 0)
			{
				attackSpeedDecay = 0.5f;
			}
			attackSpeedDecay *= __instance.eid.totalSpeedModifier;

			float delay = flag.attackDelay / __instance.eid.totalSpeedModifier;
			__instance.CancelInvoke("Shoot");
			__instance.Invoke("Shoot", delay);
			__instance.attackCooldown = UnityEngine.Random.Range(2f, 4f) + (flag.attackDelay - 0.75f) * attackSpeedDecay;
			flag.attackDelay = -1;
		}

		public static bool SetAttackDelayCheck()
		{
			return ConfigManager.enemyTweakToggle.value;
		}

		[HarmonyPatch(nameof(Drone.Death))]
		[HarmonyPrefix]
		[UltrapainPatch]
		public static bool DroneHome(Drone __instance)
		{
			if (__instance.eid.enemyType != EnemyType.Drone || __instance.crashing)
				return true;

			DroneFlag flag = __instance.GetComponent<DroneFlag>();
			if (flag == null)
				return true;

			if (__instance.eid.hitter == "heavypunch" || __instance.eid.hitter == "punch")
				return true;

			flag.homingTowardsPlayer = true;
			return true;
		}

		public static bool DroneHomeCheck()
		{
			return ConfigManager.enemyTweakToggle.value && ConfigManager.droneHomeToggle.value;
		}

		[HarmonyPatch(nameof(Drone.GetHurt))]
		[HarmonyPrefix]
		[UltrapainPatch]
		public static bool StopHomingOnPunch(Drone __instance)
		{
			if ((__instance.eid.hitter == "shotgunzone" || __instance.eid.hitter == "punch") && !__instance.parried)
			{
				DroneFlag flag = __instance.GetComponent<DroneFlag>();
				if (flag == null)
					return true;
				flag.homingTowardsPlayer = false;
			}

			return true;
		}
		
		public static bool StopHomingOnPunchCheck()
		{
			return ConfigManager.enemyTweakToggle.value && ConfigManager.droneHomeToggle.value;
		}

		[HarmonyPatch(nameof(Drone.Start))]
		[HarmonyPostfix]
		[UltrapainPatch]
		public static void AddFlag(Drone __instance)
		{
			if (__instance.eid.enemyType != EnemyType.Drone)
				return;

			if (__instance.GetComponent<NonUltrapainEnemy>() != null)
				return;

			__instance.gameObject.AddComponent<DroneFlag>();
		}

		public static bool AddFlagCheck()
		{
			return ConfigManager.enemyTweakToggle.value;
		}
	}
}
