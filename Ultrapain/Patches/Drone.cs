using HarmonyLib;
using System;
using UnityEngine;

namespace Ultrapain.Patches
{
    class Drone_Start_Patch
    {
        static void Postfix(Drone __instance, ref EnemyIdentifier ___eid)
        {
            if (___eid.enemyType != EnemyType.Drone)
                return;

            __instance.gameObject.AddComponent<DroneFlag>();
        }
    }

    class Drone_Shoot_Patch
    {
        static bool Prefix(Drone __instance, ref EnemyIdentifier ___eid)
        {
            DroneFlag flag = __instance.GetComponent<DroneFlag>();
            if(flag == null)
                return true;

            DroneFlag.Firemode mode = flag.currentMode;

            while (true)
            {
                if(mode == DroneFlag.Firemode.Explosive && !ConfigManager.droneExplosionBeamToggle.value)
                {
                    mode = DroneFlag.GetNextFireMode(mode);
                    continue;
                }
                if(mode == DroneFlag.Firemode.TurretBeam && !ConfigManager.droneSentryBeamToggle.value)
                {
                    mode = DroneFlag.GetNextFireMode(mode);
                    continue;
                }

                break;
            }

            flag.currentMode = DroneFlag.GetNextFireMode(mode);

            if (mode == DroneFlag.Firemode.Projectile)
                return true;
            if (mode == DroneFlag.Firemode.Explosive)
            {
                GameObject beam = GameObject.Instantiate(Plugin.beam.gameObject, __instance.transform.position + __instance.transform.forward, __instance.transform.rotation);

                RevolverBeam revBeam = beam.GetComponent<RevolverBeam>();
                revBeam.hitParticle = Plugin.shotgunGrenade.gameObject.GetComponent<Grenade>().explosion;
                revBeam.damage /= 2;
                revBeam.damage *= ___eid.totalDamageModifier;

                return false;
            }
            if(mode == DroneFlag.Firemode.TurretBeam)
            {
                GameObject turretBeam = GameObject.Instantiate(Plugin.turretBeam.gameObject, __instance.transform.position + __instance.transform.forward * 2f, __instance.transform.rotation);
                if (turretBeam.TryGetComponent<RevolverBeam>(out RevolverBeam revBeam))
                {
                    revBeam.damage = ConfigManager.droneSentryBeamDamage.value;
                    revBeam.damage *= ___eid.totalDamageModifier;
                    revBeam.alternateStartPoint = __instance.transform.position + __instance.transform.forward;
                    revBeam.ignoreEnemyType = EnemyType.Drone;
                }

                return false;
            }

            Debug.LogError($"Drone fire mode in impossible state. Current value: {mode} : {(int)mode}");
            return true;
        }
    }

    class DroneFlag : MonoBehaviour
    {
        public enum Firemode : int
        {
            Projectile = 0,
            Explosive,
            TurretBeam
        }

        public Firemode currentMode = Firemode.Projectile;
        private static Firemode[] allModes = Enum.GetValues(typeof(Firemode)) as Firemode[];

        public static Firemode GetNextFireMode(Firemode mode)
        {
            return (Enum.IsDefined(typeof(Firemode) ,(int)mode + 1)) ? (Firemode)((int)mode + 1) : (Firemode)(0);

            /*int nextMode = Array.IndexOf(allModes, mode) + 1;
            if (nextMode >= allModes.Length)
                nextMode = 0;

            return allModes[nextMode];*/
        }

        public bool homingTowardsPlayer = false;

        Transform target;
        Rigidbody rb;

        private void Update()
        {
            if(homingTowardsPlayer)
            {
                if(target == null)
                    target = PlayerTracker.Instance.GetTarget();
                if (rb == null)
                    rb = GetComponent<Rigidbody>();

                Quaternion to = Quaternion.LookRotation(target.position/* + MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity()*/ - transform.position);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, to, Time.deltaTime * ConfigManager.droneHomeTurnSpeed.value);
                rb.velocity = transform.forward * rb.velocity.magnitude;
            }
        }
    }

    class Drone_Death_Patch
    {
        static bool Prefix(Drone __instance, EnemyIdentifier ___eid)
        {
            if (___eid.enemyType != EnemyType.Drone || __instance.crashing)
                return true;

            DroneFlag flag = __instance.GetComponent<DroneFlag>();
            if (flag == null)
                return true;

            if (___eid.hitter == "heavypunch" || ___eid.hitter == "punch")
                return true;

            flag.homingTowardsPlayer = true;
            return true;
        }
    }

    class Drone_GetHurt_Patch
    {
        static bool Prefix(Drone __instance, EnemyIdentifier ___eid, bool ___parried)
        {
            if((___eid.hitter == "shotgunzone" || ___eid.hitter == "punch") && !___parried)
            {
                DroneFlag flag = __instance.GetComponent<DroneFlag>();
                if (flag == null)
                    return true;
                flag.homingTowardsPlayer = false;
            }

            return true;
        }
    }
}
