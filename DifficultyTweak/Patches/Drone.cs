using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DifficultyTweak.Patches
{
    [HarmonyPatch(typeof(Drone))]
    [HarmonyPatch("Start")]
    class Drone_Start_Patch
    {
        static void Postfix(Drone __instance, ref EnemyIdentifier ___eid)
        {
            if (___eid.enemyType != EnemyType.Drone)
                return;

            __instance.gameObject.AddComponent<DroneFlag>();
        }
    }

    [HarmonyPatch(typeof(Drone))]
    [HarmonyPatch("Shoot")]
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
    }
}
