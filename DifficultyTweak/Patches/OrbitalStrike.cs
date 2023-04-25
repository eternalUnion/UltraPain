using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DifficultyTweak.Patches
{
    public class OrbitalStrikeFlag : MonoBehaviour
    {
        public CoinChainList chainList;
    }

    public class CoinChainList : MonoBehaviour
    {
        public List<Coin> chainList = new List<Coin>();
        public bool isOrbitalStrike = false;
    }

    class Punch_BlastCheck
    {
        static bool Prefix(Punch __instance)
        {
            __instance.blastWave = GameObject.Instantiate(Plugin.explosionWaveKnuckleblaster, new Vector3(1000000, 1000000, 1000000), Quaternion.identity);
            __instance.blastWave.AddComponent<OrbitalStrikeFlag>();
            return true;
        }

        static void Postfix(Punch __instance)
        {
            GameObject.Destroy(__instance.blastWave);
            __instance.blastWave = Plugin.explosionWaveKnuckleblaster;
        }
    }

    class Explosion_Collide
    {
        static bool Prefix(Explosion __instance, Collider __0, List<Collider> ___hitColliders)
        {
            if (___hitColliders.Contains(__0) || __instance.transform.parent.GetComponent<OrbitalStrikeFlag>() == null)
                return true;

            Coin coin = __0.GetComponent<Coin>();
            if (coin != null)
            {
                OrbitalStrikeFlag flag = coin.GetComponent<OrbitalStrikeFlag>();
                if(flag == null)
                {
                    coin.gameObject.AddComponent<OrbitalStrikeFlag>();
                    Debug.Log("Added orbital strike flag");
                }
            }

            return true;
        }
    }

    class Coin_DelayedReflectRevolver
    {
        static void Postfix(Coin __instance, GameObject ___altBeam)
        {
            CoinChainList flag = null;

            if (___altBeam != null)
            {
                OrbitalStrikeFlag orbitalFlag = ___altBeam.GetComponent<OrbitalStrikeFlag>();
                if (orbitalFlag == null)
                {
                    orbitalFlag = ___altBeam.AddComponent<OrbitalStrikeFlag>();
                    GameObject obj = new GameObject();
                    obj.AddComponent<RemoveOnTime>().time = 5f;
                    flag = obj.AddComponent<CoinChainList>();
                    orbitalFlag.chainList = flag;
                }
                else
                    flag = orbitalFlag.chainList;
            }
            else
            {
                if (__instance.ccc == null)
                {
                    GameObject obj = new GameObject();
                    __instance.ccc = obj.AddComponent<CoinChainCache>();
                    obj.AddComponent<RemoveOnTime>().time = 5f;
                }

                flag = __instance.ccc.gameObject.GetComponent<CoinChainList>();
                if(flag == null)
                    flag = __instance.ccc.gameObject.AddComponent<CoinChainList>();
            }

            if (flag == null)
                return;

            if (!flag.isOrbitalStrike && flag.chainList.Count != 0 && __instance.GetComponent<OrbitalStrikeFlag>() != null)
            {
                Coin lastCoin = flag.chainList.LastOrDefault();
                float distance = Vector3.Distance(__instance.transform.position, lastCoin.transform.position);
                if (distance >= 20f)
                {
                    flag.isOrbitalStrike = true;
                    Debug.Log("Coin valid for orbital strike");
                }
            }

            if (flag.chainList.Count == 0 || flag.chainList.LastOrDefault() != __instance)
                flag.chainList.Add(__instance);
        }
    }

    class Coin_ReflectRevolver
    {
        public static bool coinIsShooting = false;
        public static Coin shootingCoin = null;
        public static GameObject shootingAltBeam;
        public static float lastCoinTime = 0;

        static bool Prefix(Coin __instance, GameObject ___altBeam)
        {
            coinIsShooting = true;
            shootingCoin = __instance;
            lastCoinTime = Time.time;
            shootingAltBeam = ___altBeam;

            return true;
        }

        static void Postfix(Coin __instance)
        {
            coinIsShooting = false;
        }
    }

    class Grenade_Explode
    {
        static bool Prefix(Grenade __instance, ref float __3)
        {
            if((Coin_ReflectRevolver.coinIsShooting && Coin_ReflectRevolver.shootingCoin != null) || (Time.time - Coin_ReflectRevolver.lastCoinTime <= 0.1f))
            {
                CoinChainList list = null;
                if (Coin_ReflectRevolver.shootingAltBeam != null)
                {
                    OrbitalStrikeFlag orbitalFlag = Coin_ReflectRevolver.shootingAltBeam.GetComponent<OrbitalStrikeFlag>();
                    if (orbitalFlag != null)
                        list = orbitalFlag.chainList;
                }
                else if (Coin_ReflectRevolver.shootingCoin != null && Coin_ReflectRevolver.shootingCoin.ccc != null)
                    list = Coin_ReflectRevolver.shootingCoin.ccc.GetComponent<CoinChainList>();

                if (list != null && list.isOrbitalStrike)
                {
                    __3 += 0.7f;
                    Debug.Log("Applied orbital strike bonus");
                }
            }

            return true;
        }
    }

    class EnemyIdentifier_DeliverDamage
    {
        static Coin lastExplosiveCoin = null;

        static bool Prefix(EnemyIdentifier __instance)
        {
            //if (Coin_ReflectRevolver.shootingCoin == lastExplosiveCoin)
            //    return true;

            if ((Coin_ReflectRevolver.coinIsShooting && Coin_ReflectRevolver.shootingCoin != null)/* || (Time.time - Coin_ReflectRevolver.lastCoinTime <= 0.1f)*/)
            {
                CoinChainList list = null;
                if (Coin_ReflectRevolver.shootingAltBeam != null)
                {
                    OrbitalStrikeFlag orbitalFlag = Coin_ReflectRevolver.shootingAltBeam.GetComponent<OrbitalStrikeFlag>();
                    if (orbitalFlag != null)
                        list = orbitalFlag.chainList;
                }
                else if (Coin_ReflectRevolver.shootingCoin != null && Coin_ReflectRevolver.shootingCoin.ccc != null)
                    list = Coin_ReflectRevolver.shootingCoin.ccc.GetComponent<CoinChainList>();

                if (list != null && list.isOrbitalStrike)
                {
                    lastExplosiveCoin = Coin_ReflectRevolver.shootingCoin;
                    GameObject explosion = GameObject.Instantiate(Plugin.explosion, __instance.gameObject.transform.position, Quaternion.identity);
                    foreach(Explosion exp in explosion.GetComponentsInChildren<Explosion>())
                    {
                        exp.enemy = false;
                        exp.hitterWeapon = "";
                    }
                    Debug.Log("Applied orbital strike explosion");
                }
            }

            return true;
        }
    }
}
