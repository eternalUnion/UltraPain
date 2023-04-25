using HarmonyLib;
using MonoMod.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace DifficultyTweak.Patches
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
        private static GameObject _shockwave;
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
                    shockComp.speed = 35f;
                    shockComp.damage = 20;
                    shockComp.enemy = true;
                    shockComp.enemyType = EnemyType.Sisyphus;
                    _shockwave.transform.localScale = new Vector3(_shockwave.transform.localScale.x, _shockwave.transform.localScale.y * 2f, _shockwave.transform.localScale.z);
                }

                return _shockwave;
            }
        }

        static void Postfix(Sisyphus __instance, ref GameObject ___explosion, ref PhysicalShockwave ___m_ShockwavePrefab)
        {
            ___explosion = shockwave/*___m_ShockwavePrefab.gameObject*/;
            ___m_ShockwavePrefab = shockwave.GetComponent<PhysicalShockwave>();
        }
    }

    public class SisyphusInstructionist_SetupExplosion
    {
        static bool Prefix(Sisyphus __instance, ref GameObject __0, ref Animator ___anim)
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
        }
    }

    public class SisyphusInstructionist_StompExplosion
    {
        static bool Prefix(Sisyphus __instance, Transform ___target)
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
                exp.maxSize /= 2;
                exp.speed /= 2;
                exp.damage /= 2;
            }

            return false;
        }
    }
}
