using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Ultrapain.Patches
{
    /*public class ObjectActivator : MonoBehaviour
    {
        public int originalInstanceID = 0;
        public MonoBehaviour activator;

        void Start()
        {
            if (gameObject.GetInstanceID() == originalInstanceID)
                return;
            activator?.Invoke("OnClone", 0f);
        }
    }*/

    public class CommonActivator : MonoBehaviour
    {
        public int originalId;
        public Renderer rend;

        public Rigidbody rb;
        public bool kinematic;
        public bool colDetect;

        public Collider col;

        public AudioSource aud;

        public List<MonoBehaviour> comps = new List<MonoBehaviour>();

        void Awake()
        {
            if (originalId == gameObject.GetInstanceID())
                return;

            if (rend != null)
                rend.enabled = true;

            if (rb != null)
            {
                rb.isKinematic = kinematic;
                rb.detectCollisions = colDetect;
            }

            if (col != null)
                col.enabled = true;

            if (aud != null)
                aud.enabled = true;

            foreach (MonoBehaviour comp in comps)
                comp.enabled = true;

            foreach (Transform child in gameObject.transform)
                child.gameObject.SetActive(true);
        }
    }

    public class GrenadeExplosionOverride : MonoBehaviour
    {
        public bool harmlessMod = false;
        public float harmlessSize = 1f;
        public float harmlessSpeed = 1f;
        public float harmlessDamage = 1f;

        public bool normalMod = false;
        public float normalSize = 1f;
        public float normalSpeed = 1f;
        public float normalDamage = 1f;

        public bool superMod = false;
        public float superSize = 1f;
        public float superSpeed = 1f;
        public float superDamage = 1f;

        struct StateInfo
        {
            public GameObject tempHarmless;
            public GameObject tempNormal;
            public GameObject tempSuper;

            public StateInfo()
            {
                tempHarmless = tempNormal = tempSuper = null;
            }
        }

        [HarmonyBefore]
        static bool Prefix(Grenade __instance, out StateInfo __state)
        {
            __state = new StateInfo();

            GrenadeExplosionOverride flag = __instance.GetComponent<GrenadeExplosionOverride>();
            if (flag == null)
                return true;

            if (flag.harmlessMod)
            {
                __state.tempHarmless = __instance.harmlessExplosion = GameObject.Instantiate(__instance.harmlessExplosion);
                foreach (Explosion exp in __instance.harmlessExplosion.GetComponentsInChildren<Explosion>())
                {
                    exp.damage = (int)(exp.damage * flag.harmlessDamage);
                    exp.maxSize *= flag.harmlessSize;
                    exp.speed *= flag.harmlessSize * flag.harmlessSpeed;
                }
            }

            if (flag.normalMod)
            {
                __state.tempNormal = __instance.explosion = GameObject.Instantiate(__instance.explosion);
                foreach (Explosion exp in __instance.explosion.GetComponentsInChildren<Explosion>())
                {
                    exp.damage = (int)(exp.damage * flag.normalDamage);
                    exp.maxSize *= flag.normalSize;
                    exp.speed *= flag.normalSize * flag.normalSpeed;
                }
            }

            if (flag.superMod)
            {
                __state.tempSuper = __instance.superExplosion = GameObject.Instantiate(__instance.superExplosion);
                foreach (Explosion exp in __instance.superExplosion.GetComponentsInChildren<Explosion>())
                {
                    exp.damage = (int)(exp.damage * flag.superDamage);
                    exp.maxSize *= flag.superSize;
                    exp.speed *= flag.superSize * flag.superSpeed;
                }
            }

            return true;
        }

        static void Postfix(StateInfo __state)
        {
            if (__state.tempHarmless != null)
                GameObject.Destroy(__state.tempHarmless);
            if (__state.tempNormal != null)
                GameObject.Destroy(__state.tempNormal);
            if (__state.tempSuper != null)
                GameObject.Destroy(__state.tempSuper);
        }
    }
}
