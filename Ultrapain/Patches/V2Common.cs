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

    class V2CommonRevolverComp : MonoBehaviour
    {
        public bool shootingForSharpshooter = false;
    }

    class V2CommonRevolverPrepareAltFire
    {
        static bool Prefix(EnemyRevolver __instance, GameObject ___altCharge)
        {
            if(__instance.TryGetComponent<V2CommonRevolverComp>(out V2CommonRevolverComp comp))
            {
                bool sharp = UnityEngine.Random.Range(0f, 100f) <= ConfigManager.v2FirstSharpshooterChance.value;

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
        public int reflectionCount = ConfigManager.v2FirstSharpshooterReflections.value;
        public Collider lastCol;
        public bool collideWithPlayer = true;
        public bool alreadyDeflected = false;

        public Vector3 predictedHit = Vector3.zero;
        public float timeShot;

        public int updateCount = 0;

        private void Awake()
        {
            timeShot = Time.time;
        }

        private void Update()
        {
            updateCount += 1;
        }
    }

    class V2CommonRevolverBullet
    {
        static bool Prefix(Projectile __instance, Collider __0)
        {
            V2CommonRevolverBulletSharp comp = __instance.GetComponent<V2CommonRevolverBulletSharp>();
            if (comp == null)
                return true;
            Projectile proj = __instance.GetComponent<Projectile>();

            LayerMask envMask = new LayerMask() { value = 1 << 8 | 1 << 24 };

            bool isPlayer = __0.gameObject.tag == "Player";
            if (isPlayer && !comp.collideWithPlayer)
                return false;

            if (comp.alreadyDeflected)
                return true;

            if(__0.gameObject.layer == 8 || __0.gameObject.layer == 24 || isPlayer)
            {
                //if (Time.time - comp.shootTime <= 0.1f)
                //    return false;

                if (__0 == comp.lastCol)
                    return false;

                if (comp.reflectionCount <= 0)
                    return true;

                if (!isPlayer && comp.predictedHit != null)
                {
                    /*float dist = Vector3.Distance(comp.predictedHit, __instance.transform.position);
                    if (dist >= 2.5f)
                    {
                        Debug.Log($"Predicted overdistance: {dist}");
                        return false;
                    }*/

                    if(comp.updateCount <= 3)
                    {
                        Debug.Log("Low update, skipping");
                        return false;
                    }
                }

                if(!isPlayer)
                    comp.reflectionCount -= 1;
                comp.lastCol = isPlayer ? null : __0;
                comp.collideWithPlayer = true;
                comp.alreadyDeflected = false;
                if (isPlayer)
                    comp.collideWithPlayer = false;

                GameObject reflectedBullet = GameObject.Instantiate(__instance.gameObject, __instance.transform.position, __instance.transform.rotation);
                comp.alreadyDeflected = true;
                reflectedBullet.name = comp.name;
                if (!isPlayer)
                {
                    if (Physics.Raycast(reflectedBullet.transform.position - reflectedBullet.transform.forward, reflectedBullet.transform.forward, out RaycastHit raycastHit, float.PositiveInfinity, envMask))
                    {
                        reflectedBullet.transform.forward = Vector3.Reflect(reflectedBullet.transform.forward, raycastHit.normal).normalized;
                        reflectedBullet.transform.position = raycastHit.point + reflectedBullet.transform.forward;
                        Debug.Log($"Successfull reflection {comp.reflectionCount}");
                    }

                    Vector3 playerVectorFromBullet = NewMovement.Instance.transform.position - reflectedBullet.transform.position;
                    float angle = Vector3.Angle(playerVectorFromBullet, reflectedBullet.transform.forward);
                    if (angle <= ConfigManager.v2FirstSharpshooterAutoaimAngle.value)
                        reflectedBullet.transform.LookAt(NewMovement.Instance.transform.position);

                    reflectedBullet.transform.position += reflectedBullet.transform.forward;
                    if (Physics.Raycast(reflectedBullet.transform.position, reflectedBullet.transform.forward, out RaycastHit hit, float.PositiveInfinity, envMask))
                        reflectedBullet.GetComponent<V2CommonRevolverBulletSharp>().predictedHit = hit.point;
                }
                else
                {
                    NewMovement.Instance.GetHurt(Mathf.RoundToInt(proj.damage), true, 1f, false, false);
                }

                GameObject.Instantiate(Plugin.ricochetSfx, reflectedBullet.transform.position, Quaternion.identity);
                /*if (isPlayer)
                    return true;*/
                GameObject.Destroy(__instance.gameObject);
                return false;
            }

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

                TrailRenderer rend = UnityUtils.GetComponentInChildrenRecursively<TrailRenderer>(bullet.transform);
                rend.endColor = rend.startColor = new Color(1, 0, 0);

                Projectile component = bullet.GetComponent<Projectile>();
                if (component)
                {
                    component.safeEnemyType = __instance.safeEnemyType;
                    component.speed *= ConfigManager.v2FirstSharpshooterSpeed.value;
                    component.damage *= ConfigManager.v2FirstSharpshooterDamage.value;
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
                    bulletComp.predictedHit = predictedHit.point;
                comp.shootingForSharpshooter = false;
                return false;
            }

            return true;
        }
    }
}
