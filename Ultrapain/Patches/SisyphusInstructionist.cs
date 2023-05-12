using HarmonyLib;
using MonoMod.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace Ultrapain.Patches
{
    /*public class SisyphusInstructionistFlag : MonoBehaviour
    {

    }

    [HarmonyPatch(typeof(Sisyphus), nameof(Sisyphus.Knockdown))]
    public class SisyphusInstructionist_Knockdown_Patch
    {
        static void Postfix(Sisyphus __instance, ref EnemyIdentifier ___eid)
        {
            SisyphusInstructionistFlag flag = __instance.GetComponent<SisyphusInstructionistFlag>();
            if (flag != null)
                return;

            __instance.gameObject.AddComponent<SisyphusInstructionistFlag>();

            foreach(EnemySimplifier esi in UnityUtils.GetComponentsInChildrenRecursively<EnemySimplifier>(__instance.transform))
            {
                esi.enraged = true;
            }
            GameObject effect = GameObject.Instantiate(Plugin.enrageEffect, __instance.transform);
            effect.transform.localScale = Vector3.one * 0.2f;
        }
    }*/

    public class SisyphusInstructionist_Start
    {
        public static GameObject _shockwave;
        public static GameObject shockwave
        {
            get {
                if(_shockwave == null && Plugin.shockwave != null)
                {
                    _shockwave = GameObject.Instantiate(Plugin.shockwave);
                    CommonActivator activator = _shockwave.AddComponent<CommonActivator>();
                    //ObjectActivator objectActivator = _shockwave.AddComponent<ObjectActivator>();
                    //objectActivator.originalInstanceID = _shockwave.GetInstanceID();
                    //objectActivator.activator = activator;
                    activator.originalId = _shockwave.GetInstanceID();

                    foreach (Transform t in _shockwave.transform)
                        t.gameObject.SetActive(false);
                    /*Renderer rend = _shockwave.GetComponent<Renderer>();
                    activator.rend = rend;
                    rend.enabled = false;*/
                    Rigidbody rb = _shockwave.GetComponent<Rigidbody>();
                    activator.rb = rb;
                    activator.kinematic = rb.isKinematic;
                    activator.colDetect = rb.detectCollisions;
                    rb.detectCollisions = false;
                    rb.isKinematic = true;
                    AudioSource aud = _shockwave.GetComponent<AudioSource>();
                    activator.aud = aud;
                    aud.enabled = false;
                    /*Collider col = _shockwave.GetComponent<Collider>();
                    activator.col = col;
                    col.enabled = false;*/
                    foreach(Component comp in _shockwave.GetComponents<Component>())
                    {
                        if (comp == null || comp is Transform)
                            continue;
                        if (comp is MonoBehaviour behaviour)
                        {
                            if (behaviour is not CommonActivator && behaviour is not ObjectActivator)
                            {
                                behaviour.enabled = false;
                                activator.comps.Add(behaviour);
                            }
                        }
                    }

                    PhysicalShockwave shockComp = _shockwave.GetComponent<PhysicalShockwave>();
                    shockComp.maxSize = 100f;
                    shockComp.speed = ConfigManager.sisyInstJumpShockwaveSpeed.value;
                    shockComp.damage = ConfigManager.sisyInstJumpShockwaveDamage.value;
                    shockComp.enemy = true;
                    shockComp.enemyType = EnemyType.Sisyphus;
                    _shockwave.transform.localScale = new Vector3(_shockwave.transform.localScale.x, _shockwave.transform.localScale.y * ConfigManager.sisyInstJumpShockwaveSize.value, _shockwave.transform.localScale.z);
                }

                return _shockwave;
            }
        }

        static void Postfix(Sisyphus __instance, ref GameObject ___explosion, ref PhysicalShockwave ___m_ShockwavePrefab)
        {
            //___explosion = shockwave/*___m_ShockwavePrefab.gameObject*/;
            ___m_ShockwavePrefab = shockwave.GetComponent<PhysicalShockwave>();
        }
    }

    /*
     * A bug occurs where if the player respawns, the shockwave prefab gets deleted
     * 
     * Check existence of the prefab on update
     */
    public class SisyphusInstructionist_Update
    {
        static void Postfix(Sisyphus __instance, ref PhysicalShockwave ___m_ShockwavePrefab)
        {
            //___explosion = shockwave/*___m_ShockwavePrefab.gameObject*/;
            if(___m_ShockwavePrefab == null)
                ___m_ShockwavePrefab = SisyphusInstructionist_Start.shockwave.GetComponent<PhysicalShockwave>();
        }
    }

    public class SisyphusInstructionist_SetupExplosion
    {
        static void Postfix(Sisyphus __instance, ref GameObject __0, EnemyIdentifier ___eid)
        {
            GameObject shockwave = GameObject.Instantiate(Plugin.shockwave, __0.transform.position, __0.transform.rotation);
            PhysicalShockwave comp = shockwave.GetComponent<PhysicalShockwave>();

            comp.enemy = true;
            comp.enemyType = EnemyType.Sisyphus;
            comp.maxSize = 100f;
            comp.speed = ConfigManager.sisyInstBoulderShockwaveSpeed.value * ___eid.totalSpeedModifier;
            comp.damage = (int)(ConfigManager.sisyInstBoulderShockwaveDamage.value * ___eid.totalDamageModifier);
            shockwave.transform.localScale = new Vector3(shockwave.transform.localScale.x, shockwave.transform.localScale.y * ConfigManager.sisyInstBoulderShockwaveSize.value, shockwave.transform.localScale.z);
        }

        /*static bool Prefix(Sisyphus __instance, ref GameObject __0, ref Animator ___anim)
        {
            string clipName = ___anim.GetCurrentAnimatorClipInfo(0)[0].clip.name;
            Debug.Log($"Clip name: {clipName}");

            PhysicalShockwave comp = __0.GetComponent<PhysicalShockwave>();
            if (comp == null)
                return true;

            comp.enemy = true;
            comp.enemyType = EnemyType.Sisyphus;
            comp.maxSize = 100f;
            comp.speed = 35f;
            comp.damage = 20;
            __0.transform.localScale = new Vector3(__0.transform.localScale.x, __0.transform.localScale.y / 2, __0.transform.localScale.z);

            GameObject explosion = GameObject.Instantiate(Plugin.sisyphiusExplosion, __0.transform.position, Quaternion.identity);
            __0 = explosion;

            return true;
        }*/
    }

    public class SisyphusInstructionist_StompExplosion
    {
        static bool Prefix(Sisyphus __instance, Transform ___target, EnemyIdentifier ___eid)
        {
            Vector3 vector = __instance.transform.position + Vector3.up;
            if (Physics.Raycast(vector, ___target.position - vector, Vector3.Distance(___target.position, vector), LayerMaskDefaults.Get(LMD.Environment)))
            {
                vector = __instance.transform.position + Vector3.up * 5f;
            }
            GameObject explosion = Object.Instantiate<GameObject>(Plugin.sisyphiusPrimeExplosion, vector, Quaternion.identity);
            foreach(Explosion exp in explosion.GetComponentsInChildren<Explosion>())
            {
                exp.enemy = true;
                exp.toIgnore.Add(EnemyType.Sisyphus);
                exp.maxSize *= ConfigManager.sisyInstStrongerExplosionSizeMulti.value;
                exp.speed *= ConfigManager.sisyInstStrongerExplosionSizeMulti.value * ___eid.totalSpeedModifier;
                exp.damage = (int)(exp.damage * ConfigManager.sisyInstStrongerExplosionDamageMulti.value * ___eid.totalDamageModifier);
            }

            return false;
        }
    }
}
