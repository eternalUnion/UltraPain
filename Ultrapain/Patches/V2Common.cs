using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ultrapain.Patches
{
    public static class V2Utils
    {
        public static Transform GetClosestGrenade()
        {
            Transform closestTransform = null;
            float closestDistance = 1000000;

            foreach(Grenade g in GrenadeList.Instance.grenadeList)
            {
                float dist = Vector3.Distance(g.transform.position, PlayerTracker.Instance.GetTarget().position);
                if(dist < closestDistance)
                {
                    closestTransform = g.transform;
                    closestDistance = dist;
                }
            }

            foreach (Cannonball c in GrenadeList.Instance.cannonballList)
            {
                float dist = Vector3.Distance(c.transform.position, PlayerTracker.Instance.GetTarget().position);
                if (dist < closestDistance)
                {
                    closestTransform = c.transform;
                    closestDistance = dist;
                }
            }

            return closestTransform;
        }

        public static Vector3 GetDirectionAwayFromTarget(Vector3 center, Vector3 target)
        {
            // Calculate the direction vector from the center to the target
            Vector3 direction = target - center;

            // Set the Y component of the direction vector to 0
            direction.y = 0;

            // Normalize the direction vector
            direction.Normalize();

            // Reverse the direction vector to face away from the target
            direction = -direction;

            return direction;
        }
    }

    class V2CommonExplosion
    {
        static void Postfix(Explosion __instance)
        {
            if (__instance.sourceWeapon == null)
                return;

            V2MaliciousCannon malCanComp = __instance.sourceWeapon.GetComponent<V2MaliciousCannon>();
            if(malCanComp != null)
            {
                Debug.Log("Grenade explosion triggered by V2 malicious cannon");
                __instance.toIgnore.Add(EnemyType.V2);
                __instance.toIgnore.Add(EnemyType.V2Second);
                return;
            }

            EnemyRevolver revComp = __instance.sourceWeapon.GetComponentInChildren<EnemyRevolver>();
            if(revComp != null)
            {
                Debug.Log("Grenade explosion triggered by V2 revolver");
                __instance.toIgnore.Add(EnemyType.V2);
                __instance.toIgnore.Add(EnemyType.V2Second);
                return;
            }
        }
    }

    // SHARPSHOOTER

    class V2CommonRevolverComp : MonoBehaviour
    {
        public bool secondPhase = false;
        public bool shootingForSharpshooter = false;
    }

    class V2CommonRevolverPrepareAltFire
    {
        static bool Prefix(EnemyRevolver __instance, GameObject ___altCharge)
        {
            if(__instance.TryGetComponent<V2CommonRevolverComp>(out V2CommonRevolverComp comp))
            {
                if ((comp.secondPhase && !ConfigManager.v2SecondSharpshooterToggle.value)
                    || (!comp.secondPhase && !ConfigManager.v2FirstSharpshooterToggle.value))
                    return true;

                bool sharp = UnityEngine.Random.Range(0f, 100f) <= (comp.secondPhase ? ConfigManager.v2SecondSharpshooterChance.value : ConfigManager.v2FirstSharpshooterChance.value);

                Transform quad = ___altCharge.transform.Find("MuzzleFlash/Quad");
                MeshRenderer quadRenderer = quad.gameObject.GetComponent<MeshRenderer>();
                quadRenderer.material.color = sharp ? new Color(1f, 0.1f, 0f) : new Color(1f, 1f, 1f);

                comp.shootingForSharpshooter = sharp;
            }

            return true;
        }
    }

    class V2CommonRevolverBulletSharp : MonoBehaviour
    {
        public int reflectionCount = 2;
        public float autoAimAngle = 30f;

        public Projectile proj;
        public float speed = 350f;
        public bool hasTargetPoint = false;
        public Vector3 shootPoint;
        public Vector3 targetPoint;
        public RaycastHit targetHit;
        public bool alreadyHitPlayer = false;
        public bool alreadyReflected = false;

        private void Awake()
        {
            proj = GetComponent<Projectile>();
            proj.speed = 0;
            GetComponent<Rigidbody>().isKinematic = true;
        }

        private void Update()
        {
            if (!hasTargetPoint)
                transform.position += transform.forward * speed;
            else
            {
                if (transform.position != targetPoint)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPoint, Time.deltaTime * speed);
                    if (transform.position == targetPoint)
                        proj.SendMessage("Collided", targetHit.collider);
                }
                else
                    proj.SendMessage("Collided", targetHit.collider);
            }
        }
    }

    class V2CommonRevolverBullet
    {
        static bool Prefix(Projectile __instance, Collider __0)
        {
            V2CommonRevolverBulletSharp comp = __instance.GetComponent<V2CommonRevolverBulletSharp>();
            if (comp == null)
                return true;

            if ((__0.gameObject.tag == "Head" || __0.gameObject.tag == "Body" || __0.gameObject.tag == "Limb" || __0.gameObject.tag == "EndLimb") && __0.gameObject.tag != "Armor")
            {
                EnemyIdentifierIdentifier eii = __instance.GetComponent<EnemyIdentifierIdentifier>();
                if (eii != null)
                {
                    eii.eid.hitter = "enemy";
                    eii.eid.DeliverDamage(__0.gameObject, __instance.transform.forward * 100f, __instance.transform.position, comp.proj.damage / 10f, false, 0f, null, false);
                    return false;
                }
            }

            if (comp.alreadyReflected)
                return false;

            bool isPlayer = __0.gameObject.tag == "Player";
            if (isPlayer)
            {
                if (comp.alreadyHitPlayer)
                    return false;
                NewMovement.Instance.GetHurt(Mathf.RoundToInt(comp.proj.damage), true, 1f, false, false);
                comp.alreadyHitPlayer = true;
                return false;
            }

            if (!comp.hasTargetPoint || comp.transform.position != comp.targetPoint)
                return false;

            if(comp.reflectionCount <= 0)
            {
                comp.alreadyReflected = true;
                return true;
            }

            // REFLECTION
            LayerMask envMask = new LayerMask() { value = 1 << 8 | 1 << 24 };
            GameObject reflectedBullet = GameObject.Instantiate(__instance.gameObject, comp.targetPoint, __instance.transform.rotation);
            V2CommonRevolverBulletSharp reflectComp = reflectedBullet.GetComponent<V2CommonRevolverBulletSharp>();
            reflectComp.reflectionCount -= 1;
            reflectComp.shootPoint = reflectComp.transform.position;
            reflectComp.alreadyReflected = false;
            reflectComp.alreadyHitPlayer = false;

            reflectedBullet.transform.forward = Vector3.Reflect(comp.transform.forward, comp.targetHit.normal).normalized;
            
            Vector3 playerPos = NewMovement.Instance.transform.position;
            Vector3 playerVectorFromBullet = playerPos - reflectedBullet.transform.position;
            float angle = Vector3.Angle(playerVectorFromBullet, reflectedBullet.transform.forward);
            if (angle <= ConfigManager.v2FirstSharpshooterAutoaimAngle.value)
            {
                Quaternion lastRotation = reflectedBullet.transform.rotation;
                reflectedBullet.transform.LookAt(NewMovement.Instance.playerCollider.bounds.center);

                RaycastHit[] hits = Physics.RaycastAll(reflectedBullet.transform.position, reflectedBullet.transform.forward, Vector3.Distance(reflectedBullet.transform.position, playerPos));
                bool hitEnv = false;
                foreach (RaycastHit rayHit in hits)
                {
                    if (rayHit.transform.gameObject.layer == 8 || rayHit.transform.gameObject.layer == 24)
                    {
                        hitEnv = true;
                        break;
                    }
                }

                if (hitEnv)
                {
                    reflectedBullet.transform.rotation = lastRotation;
                }
            }

            if(Physics.Raycast(reflectedBullet.transform.position, reflectedBullet.transform.forward, out RaycastHit hit, float.PositiveInfinity, envMask))
            {
                reflectComp.targetPoint = hit.point;
                reflectComp.targetHit = hit;
                reflectComp.hasTargetPoint = true;
            }
            else
            {
                reflectComp.hasTargetPoint = false;
            }

            comp.alreadyReflected = true;
            GameObject.Instantiate(Plugin.ricochetSfx, reflectedBullet.transform.position, Quaternion.identity);
            return true;
        }
    }

    class V2CommonRevolverAltShoot
    {
        static bool Prefix(EnemyRevolver __instance, EnemyIdentifier ___eid)
        {
            if (__instance.TryGetComponent<V2CommonRevolverComp>(out V2CommonRevolverComp comp) && comp.shootingForSharpshooter)
            {
                __instance.CancelAltCharge();

                Vector3 position = __instance.shootPoint.position;
                if (Vector3.Distance(__instance.transform.position, ___eid.transform.position) > Vector3.Distance(MonoSingleton<NewMovement>.Instance.transform.position, ___eid.transform.position))
                {
                    position = new Vector3(___eid.transform.position.x, __instance.transform.position.y, ___eid.transform.position.z);
                }

                GameObject bullet = GameObject.Instantiate(__instance.altBullet, position, __instance.shootPoint.rotation);
                V2CommonRevolverBulletSharp bulletComp = bullet.AddComponent<V2CommonRevolverBulletSharp>();
                bulletComp.autoAimAngle = comp.secondPhase ? ConfigManager.v2SecondSharpshooterAutoaimAngle.value : ConfigManager.v2FirstSharpshooterAutoaimAngle.value;
                bulletComp.reflectionCount = comp.secondPhase ? ConfigManager.v2SecondSharpshooterReflections.value : ConfigManager.v2FirstSharpshooterReflections.value;
                bulletComp.speed *= comp.secondPhase ? ConfigManager.v2SecondSharpshooterSpeed.value : ConfigManager.v2FirstSharpshooterSpeed.value;

                TrailRenderer rend = UnityUtils.GetComponentInChildrenRecursively<TrailRenderer>(bullet.transform);
                rend.endColor = rend.startColor = new Color(1, 0, 0);

                Projectile component = bullet.GetComponent<Projectile>();
                if (component)
                {
                    component.safeEnemyType = __instance.safeEnemyType;
                    component.damage *= comp.secondPhase ? ConfigManager.v2SecondSharpshooterDamage.value : ConfigManager.v2FirstSharpshooterDamage.value;
                }

                LayerMask envMask = new LayerMask() { value = 1 << 8 | 1 << 24 };

                float v2Height = -1;
                RaycastHit v2Ground;
                if (!Physics.Raycast(position, Vector3.down, out v2Ground, float.PositiveInfinity, envMask))
                    v2Height = v2Ground.distance;

                float playerHeight = -1;
                RaycastHit playerGround;
                if (!Physics.Raycast(NewMovement.Instance.transform.position, Vector3.down, out playerGround, float.PositiveInfinity, envMask))
                    playerHeight = playerGround.distance;

                if (v2Height != -1 && playerHeight != -1)
                {
                    Vector3 playerGroundFromV2 = playerGround.point - v2Ground.point;
                    float distance = Vector3.Distance(playerGround.point, v2Ground.point);
                    float k = playerHeight / v2Height;

                    float d1 = (distance * k) / (1 + k);
                    Vector3 lookPoint = v2Ground.point + (playerGroundFromV2 / distance) * d1;

                    bullet.transform.LookAt(lookPoint);
                }
                else
                {
                    Vector3 mid = ___eid.transform.position + (NewMovement.Instance.transform.position - ___eid.transform.position) * 0.5f;
                    if (Physics.Raycast(mid, Vector3.down, out RaycastHit hit, 1000f, new LayerMask() { value = 1 << 8 | 1 << 24 }))
                    {
                        bullet.transform.LookAt(hit.point);
                    }
                    else
                    {
                        bullet.transform.LookAt(NewMovement.Instance.playerCollider.bounds.center);
                    }
                }

                GameObject.Instantiate(__instance.muzzleFlashAlt, __instance.shootPoint.position, __instance.shootPoint.rotation);

                if (Physics.Raycast(bullet.transform.position, bullet.transform.forward, out RaycastHit predictedHit, float.PositiveInfinity, envMask))
                {
                    bulletComp.targetPoint = predictedHit.point;
                    bulletComp.targetHit = predictedHit;
                    bulletComp.hasTargetPoint = true;
                }
                else
                {
                    bulletComp.hasTargetPoint = false;
                }

                comp.shootingForSharpshooter = false;
                return false;
            }

            return true;
        }
    }
}
