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

    public class CommonLinearScaler : MonoBehaviour
    {
        public Transform targetTransform;
        public float scaleSpeed = 1f;

        void Update()
        {
            float deltaSize = Time.deltaTime * scaleSpeed;
            targetTransform.localScale = new Vector3(targetTransform.localScale.x + deltaSize, targetTransform.localScale.y + deltaSize, targetTransform.localScale.y + deltaSize);
        }
    }

    public class CommonAudioPitchScaler : MonoBehaviour
    {
        public AudioSource targetAud;
        public float scaleSpeed = 1f;

        void Update()
        {
            float deltaPitch = Time.deltaTime * scaleSpeed;
            targetAud.pitch += deltaPitch;
        }
    }

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
}
