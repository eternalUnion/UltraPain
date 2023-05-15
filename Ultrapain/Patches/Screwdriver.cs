using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Ultrapain.Patches
{
    class Harpoon_OnTriggerEnter_Patch
    {
        public static float forwardForce = 10f;
        public static float upwardForce = 10f;

        static bool Prefix(Harpoon __instance, Collider __0)
        {
            if (!__instance.drill)
                return true;

            Coin sourceCoin = __0.gameObject.GetComponent<Coin>();
            if (sourceCoin != null)
            {
                Quaternion currentRotation = Quaternion.Euler(0, __0.transform.eulerAngles.y, 0);
                int totalCoinCount = ConfigManager.screwDriverCoinSplitCount.value;
                float rotationPerIteration = 360f / totalCoinCount;
            
                for(int i = 0; i < totalCoinCount; i++)
                {
                    GameObject coinClone = GameObject.Instantiate(Plugin.coin, __instance.transform.position, currentRotation);
                    Coin comp = coinClone.GetComponent<Coin>();
                    comp.sourceWeapon = sourceCoin.sourceWeapon;
                    comp.power = sourceCoin.power;
                    Rigidbody rb = coinClone.GetComponent<Rigidbody>();

                    rb.AddForce(coinClone.transform.forward * forwardForce + Vector3.up * upwardForce, ForceMode.VelocityChange);
                    currentRotation = Quaternion.Euler(0, currentRotation.eulerAngles.y + rotationPerIteration, 0);
                }

                GameObject.Destroy(__0.gameObject);
                GameObject.Destroy(__instance.gameObject);
                return false;
            }

            return true;
        }
    }
}
