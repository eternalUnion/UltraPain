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

    class OrbitalExplosionInfo : MonoBehaviour
    {
        public bool active = true;
        public string id;
        public int points;
    }

    class Grenade_Explode
    {
        class StateInfo
        {
            public bool state = false;

            public string id;
            public int points;
            public GameObject templateExplosion;
        }

        static bool Prefix(Grenade __instance, ref float __3, out StateInfo __state,
            bool __1, bool __2)
        {
            __state = new StateInfo();

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
                    if (__1)
                    {
                        __state.templateExplosion = GameObject.Instantiate(__instance.harmlessExplosion, new Vector3(1000000, 1000000, 1000000), Quaternion.identity);
                        __instance.harmlessExplosion = __state.templateExplosion;
                    }
                    else if (__2)
                    {
                        __state.templateExplosion = GameObject.Instantiate(__instance.superExplosion, new Vector3(1000000, 1000000, 1000000), Quaternion.identity);
                        __instance.superExplosion = __state.templateExplosion;
                    }
                    else
                    {
                        __state.templateExplosion = GameObject.Instantiate(__instance.explosion, new Vector3(1000000, 1000000, 1000000), Quaternion.identity);
                        __instance.explosion = __state.templateExplosion;
                    }
                    OrbitalExplosionInfo info = __state.templateExplosion.AddComponent<OrbitalExplosionInfo>();

                    __state.state = true;
                    if(Coin_ReflectRevolver.shootingAltBeam == null)
                    {
                        __3 += ConfigManager.orbStrikeRevolverExtraSize.value;
                        info.id = ConfigManager.orbStrikeRevolverStyleText.guid;
                        info.points = ConfigManager.orbStrikeRevolverStylePoint.value;
                    }
                    else if(Coin_ReflectRevolver.shootingAltBeam.TryGetComponent(out RevolverBeam beam))
                    {
                        if (beam.beamType == BeamType.Revolver)
                        {
                            if (beam.strongAlt)
                            {
                                __3 += ConfigManager.orbStrikeRevolverChargedExtraSize.value;
                                info.id = ConfigManager.orbStrikeRevolverChargedStyleText.guid;
                                info.points = ConfigManager.orbStrikeRevolverChargedStylePoint.value;
                            }
                            else
                            {
                                __3 += ConfigManager.orbStrikeRevolverExtraSize.value;
                                info.id = ConfigManager.orbStrikeRevolverStyleText.guid;
                                info.points = ConfigManager.orbStrikeRevolverStylePoint.value;
                            }
                        }
                        else if (beam.beamType == BeamType.Railgun && beam.hitAmount > 500)
                        {
                            __3 += ConfigManager.orbStrikeElectricCannonExtraSize.value;
                            info.id = ConfigManager.orbStrikeElectricCannonStyleText.guid;
                            info.points = ConfigManager.orbStrikeElectricCannonStylePoint.value;
                        }
                        else if (beam.beamType == BeamType.Railgun)
                        {
                            __3 += ConfigManager.orbStrikeMaliciousCannonExtraSize.value;
                            info.id = ConfigManager.orbStrikeMaliciousCannonStyleText.guid;
                            info.points = ConfigManager.orbStrikeMaliciousCannonStylePoint.value;
                        }
                        else
                            __state.state = false;
                    }

                    Debug.Log("Applied orbital strike bonus");
                }
            }

            return true;
        }

        static void Postfix(Grenade __instance, StateInfo __state)
        {
            if (__state.templateExplosion != null)
                GameObject.Destroy(__state.templateExplosion);

            if (!__state.state)
                return;

            /*Collider[] explosions = Physics.OverlapSphere(__instance.transform.position, 1f, 1 << 23, QueryTriggerInteraction.Collide);
            if(explosions.Length == 0)
            {
                Debug.LogWarning("Could not find any explosions to add the orbital stats to");
                return;
            }

            foreach(Collider col in explosions)
            {
                OrbitalExplosionInfo info = col.gameObject.AddComponent<OrbitalExplosionInfo>();
                info.id = __state.id;
                info.points = __state.points;
            }*/
        }
    }

    class Explosion_CollideOrbital
    {
        static bool Prefix(Explosion __instance, Collider __0)
        {
            OrbitalExplosionInfo flag = __instance.transform.parent.GetComponent<OrbitalExplosionInfo>();
            if (flag == null || !flag.active)
                return true;

            if ( __0.gameObject.tag != "Player" && (__0.gameObject.layer == 10 || __0.gameObject.layer == 11)
                && __instance.canHit != AffectedSubjects.PlayerOnly)
            {
                EnemyIdentifierIdentifier componentInParent = __0.GetComponentInParent<EnemyIdentifierIdentifier>();
                if (componentInParent != null && componentInParent.eid != null && !componentInParent.eid.blessed && !componentInParent.eid.dead)
                {
                    flag.active = false;
                    StyleHUD.Instance.AddPoints(flag.points, flag.id);
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
