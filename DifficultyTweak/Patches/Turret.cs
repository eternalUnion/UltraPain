using HarmonyLib;
using UnityEngine;

namespace DifficultyTweak.Patches
{
    class TurretFlag : MonoBehaviour
    {
        public int shootCountRemaining = ConfigManager.turretBurstFireCount.value;
    }

    class TurretStart
    {
        static void Postfix(Turret __instance)
        {
            __instance.gameObject.AddComponent<TurretFlag>();
        }
    }

    class TurretShoot
    {
        static bool Prefix(Turret __instance, ref EnemyIdentifier ___eid, ref RevolverBeam ___beam, ref Transform ___shootPoint,
            ref float ___aimTime, ref float ___maxAimTime, ref float ___nextBeepTime, ref float ___flashTime)
        {
            TurretFlag flag = __instance.GetComponent<TurretFlag>();
            if (flag == null)
                return true;

            if (flag.shootCountRemaining > 0)
            {
                RevolverBeam revolverBeam = GameObject.Instantiate<RevolverBeam>(___beam, new Vector3(__instance.transform.position.x, ___shootPoint.transform.position.y, __instance.transform.position.z), ___shootPoint.transform.rotation);
                revolverBeam.alternateStartPoint = ___shootPoint.transform.position;
                RevolverBeam revolverBeam2;
                if (___eid.totalDamageModifier != 1f && revolverBeam.TryGetComponent<RevolverBeam>(out revolverBeam2))
                {
                    revolverBeam2.damage *= ___eid.totalDamageModifier;
                }

                ___nextBeepTime = 0;
                ___flashTime = 0;
                ___aimTime = ___maxAimTime - ConfigManager.turretBurstFireDelay.value;
                if (___aimTime < 0)
                    ___aimTime = 0;

                flag.shootCountRemaining -= 1;
                return false;
            }
            else
                flag.shootCountRemaining = ConfigManager.turretBurstFireCount.value;

            return true;
        }
    }

    class TurretAim
    {
        static void Postfix(Turret __instance)
        {
            TurretFlag flag = __instance.GetComponent<TurretFlag>();
            if (flag == null)
                return;

            flag.shootCountRemaining = ConfigManager.turretBurstFireCount.value;
        }
    }
}
