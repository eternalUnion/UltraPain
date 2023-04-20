using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace DifficultyTweak.Patches
{
    class V2FirstFlag : MonoBehaviour
    {
        public Collider v2collider;
        public float punchCooldown = 0f;

        void Update()
        {
            if (punchCooldown > 0)
                punchCooldown = Mathf.MoveTowards(punchCooldown, 0f, Time.deltaTime);
        }

        public void PunchShockwave()
        {
            GameObject blast = Instantiate(Plugin.blastwave, v2collider.bounds.center, Quaternion.identity);
            blast.transform.LookAt(PlayerTracker.Instance.GetTarget());
            blast.transform.position += blast.transform.forward * 2f;

            Explosion exp = blast.GetComponentInChildren<Explosion>();
            if (exp != null)
            {
                exp.enemy = true;
                exp.damage = ConfigManager.v2FirstKnuckleBlasterExplosionDamage.value;
                exp.maxSize = ConfigManager.v2FirstKnuckleBlasterSize.value;
                exp.speed = ConfigManager.v2FirstKnuckleBlasterSpeed.value;
                exp.hitterWeapon = "";
                exp.harmless = false;
                exp.playerDamageOverride = -1;
                exp.canHit = AffectedSubjects.All;
                exp.toIgnore.Add(EnemyType.V2);
            }
        }
    }

    [HarmonyPatch(typeof(V2), "Update")]
    class V2FirstUpdate
    {
        static MethodInfo ShootWeapon = typeof(V2).GetMethod("ShootWeapon", BindingFlags.Instance | BindingFlags.NonPublic);
        public static Transform targetGrenade;

        static bool Prefix(V2 __instance, ref int ___currentWeapon, ref Transform ___overrideTarget, ref Rigidbody ___overrideTargetRb, ref float ___shootCooldown,
            ref bool ___aboutToShoot, ref EnemyIdentifier ___eid)
        {
            if (__instance.secondEncounter)
                return true;

            V2FirstFlag flag = __instance.GetComponent<V2FirstFlag>();
            if (flag == null)
                return true;

            float distanceToPlayer = Vector3.Distance(__instance.transform.position, PlayerTracker.Instance.GetTarget().transform.position);
            if (ConfigManager.v2FirstKnuckleBlasterHitPlayerToggle.value && distanceToPlayer <= ConfigManager.v2FirstKnuckleBlasterHitPlayerMinDistance.value && flag.punchCooldown == 0)
            {
                Debug.Log("V2: Trying to punch");
                flag.punchCooldown = ConfigManager.v2FirstKnuckleBlasterCooldown.value;
                NewMovement.Instance.GetHurt(ConfigManager.v2FirstKnuckleBlasterHitDamage.value, true, 1, false, false);
                flag.Invoke("PunchShockwave", 1f);
            }

            if (ConfigManager.v2FirstKnuckleBlasterDeflectShotgunToggle.value && flag.punchCooldown == 0)
                foreach (Collider col in Physics.OverlapSphere(flag.v2collider.bounds.center, 10f, 1 << 14, QueryTriggerInteraction.Collide))
                {
                    Projectile proj = col.gameObject.GetComponent<Projectile>();
                    if (proj == null)
                        continue;

                    if (proj.playerBullet)
                    {
                        Debug.Log("V2: Trying to deflect projectiles");
                        flag.PunchShockwave();
                        flag.punchCooldown = ConfigManager.v2FirstKnuckleBlasterCooldown.value;
                        break;
                    }
                }

            return true;
        }
    }

    /*[HarmonyPatch(typeof(V2), "AltShootWeapon")]
    class V2AltShootWeapon
    {
        static void Postfix(V2 __instance, ref int ___currentWeapon)
        {
            if (__instance.secondEncounter)
                return;

            // Threw a core
            if(___currentWeapon == 1)
            {
                V2SwitchWeapon.SwitchWeapon.Invoke(__instance, new object[1] {3});
                //Grenade target = GrenadeList.Instance.grenadeList.OrderBy((Grenade g) => Vector3.Distance(g.transform.position, __instance.transform.position)).First();
            }
        }
    }*/

    [HarmonyPatch(typeof(V2), "ShootWeapon")]
    class V2FirstShootWeapon
    {
        static bool Prefix(V2 __instance, ref int ___currentWeapon)
        {
            if (__instance.secondEncounter)
                return true;

            V2FirstFlag flag = __instance.GetComponent<V2FirstFlag>();
            if (flag == null)
                return true;

            // PISTOL
            if (___currentWeapon == 0 && ConfigManager.v2FirstCoreSnipeToggle.value)
            {
                Transform closestGrenade = V2Utils.GetClosestGrenade();
                if (closestGrenade != null)
                {
                    float distanceToPlayer = Vector3.Distance(closestGrenade.position, PlayerTracker.Instance.GetTarget().position);
                    float distanceToV2 = Vector3.Distance(closestGrenade.position, flag.v2collider.bounds.center);
                    if (distanceToPlayer <= ConfigManager.v2FirstCoreSnipeMaxDistanceToPlayer.value && distanceToV2 >= ConfigManager.v2FirstCoreSnipeMinDistanceToV2.value)
                    {
                        Debug.Log("Attempting to shoot the grenade");
                        GameObject revolverBeam = GameObject.Instantiate(Plugin.revolverBeam, __instance.transform.position + __instance.transform.forward, Quaternion.identity);
                        revolverBeam.transform.LookAt(closestGrenade.position);
                        if (revolverBeam.TryGetComponent<RevolverBeam>(out RevolverBeam comp))
                        {
                            comp.beamType = BeamType.Enemy;
                        }

                        __instance.ForceDodge(V2Utils.GetDirectionAwayFromTarget(flag.v2collider.bounds.center, closestGrenade.transform.position));
                        return false;
                    }
                }
            }

            return true;
        }
    }

    class V2FirstStart
    {
        static void Postfix(V2 __instance)
        {
            if (__instance.secondEncounter)
                return;

            V2FirstFlag flag = __instance.gameObject.AddComponent<V2FirstFlag>();
            flag.v2collider = __instance.GetComponent<Collider>();
        }
    }
}
