using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Ultrapain.Patches
{
    public class OrbitalStrikeFlag : MonoBehaviour
    {
        public CoinChainList chainList;
        public bool isOrbitalRay = false;
        public bool exploded = false;
        public float activasionDistance;
    }

    public class CoinChainList : MonoBehaviour
    {
        public List<Coin> chainList = new List<Coin>();
        public bool isOrbitalStrike = false;
        public float activasionDistance;
    }

    class Punch_BlastCheck
    {
        [HarmonyBefore(new string[] { "tempy.fastpunch" })]
        static bool Prefix(Punch __instance)
        {
            __instance.blastWave = GameObject.Instantiate(Plugin.explosionWaveKnuckleblaster, new Vector3(1000000, 1000000, 1000000), Quaternion.identity);
            __instance.blastWave.AddComponent<OrbitalStrikeFlag>();
            return true;
        }

        [HarmonyBefore(new string[] { "tempy.fastpunch" })]
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
            if (___hitColliders.Contains(__0)/* || __instance.transform.parent.GetComponent<OrbitalStrikeFlag>() == null*/)
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
            OrbitalStrikeFlag orbitalBeamFlag = null;

            if (___altBeam != null)
            {
                orbitalBeamFlag = ___altBeam.GetComponent<OrbitalStrikeFlag>();
                if (orbitalBeamFlag == null)
                {
                    orbitalBeamFlag = ___altBeam.AddComponent<OrbitalStrikeFlag>();
                    GameObject obj = new GameObject();
                    obj.AddComponent<RemoveOnTime>().time = 5f;
                    flag = obj.AddComponent<CoinChainList>();
                    orbitalBeamFlag.chainList = flag;
                }
                else
                    flag = orbitalBeamFlag.chainList;
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
                if (distance >= ConfigManager.orbStrikeMinDistance.value)
                {
                    flag.isOrbitalStrike = true;
                    flag.activasionDistance = distance;
                    if (orbitalBeamFlag != null)
                    {
                        orbitalBeamFlag.isOrbitalRay = true;
                        orbitalBeamFlag.activasionDistance = distance;
                    }
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

    class RevolverBeam_Start
    {
        static bool Prefix(RevolverBeam __instance)
        {
            OrbitalStrikeFlag flag = __instance.GetComponent<OrbitalStrikeFlag>();
            if (flag != null && flag.isOrbitalRay)
            {
                RevolverBeam_ExecuteHits.orbitalBeam = __instance;
                RevolverBeam_ExecuteHits.orbitalBeamFlag = flag;
            }

            return true;
        }
    }

    class RevolverBeam_ExecuteHits
    {
        public static bool isOrbitalRay = false;
        public static RevolverBeam orbitalBeam = null;
        public static OrbitalStrikeFlag orbitalBeamFlag = null;

        static bool Prefix(RevolverBeam __instance)
        {
            OrbitalStrikeFlag flag = __instance.GetComponent<OrbitalStrikeFlag>();
            if (flag != null && flag.isOrbitalRay)
            {
                isOrbitalRay = true;
                orbitalBeam = __instance;
                orbitalBeamFlag = flag;
            }

            return true;
        }

        static void Postfix()
        {
            isOrbitalRay = false;
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
                    info.id = "";

                    __state.state = true;
                    float damageMulti = 1f;
                    float sizeMulti = 1f;
                    
                    // REVOLVER NORMAL
                    if (Coin_ReflectRevolver.shootingAltBeam == null)
                    {
                        if (ConfigManager.orbStrikeRevolverGrenade.value)
                        {
                            damageMulti += ConfigManager.orbStrikeRevolverGrenadeExtraDamage.value;
                            sizeMulti += ConfigManager.orbStrikeRevolverGrenadeExtraSize.value;
                            info.id = ConfigManager.orbStrikeRevolverStyleText.guid;
                            info.points = ConfigManager.orbStrikeRevolverStylePoint.value;
                        }
                    }
                    else if (Coin_ReflectRevolver.shootingAltBeam.TryGetComponent(out RevolverBeam beam))
                    {
                        if (beam.beamType == BeamType.Revolver)
                        {
                            // REVOLVER CHARGED (NORMAL + ALT. IF DISTINCTION IS NEEDED, USE beam.strongAlt FOR ALT)
                            if (beam.ultraRicocheter)
                            {
                                if (ConfigManager.orbStrikeRevolverChargedGrenade.value)
                                {
                                    damageMulti += ConfigManager.orbStrikeRevolverChargedGrenadeExtraDamage.value;
                                    sizeMulti += ConfigManager.orbStrikeRevolverChargedGrenadeExtraSize.value;
                                    info.id = ConfigManager.orbStrikeRevolverChargedStyleText.guid;
                                    info.points = ConfigManager.orbStrikeRevolverChargedStylePoint.value;
                                }
                            }
                            // REVOLVER ALT
                            else
                            {
                                if (ConfigManager.orbStrikeRevolverGrenade.value)
                                {
                                    damageMulti += ConfigManager.orbStrikeRevolverGrenadeExtraDamage.value;
                                    sizeMulti += ConfigManager.orbStrikeRevolverGrenadeExtraSize.value;
                                    info.id = ConfigManager.orbStrikeRevolverStyleText.guid;
                                    info.points = ConfigManager.orbStrikeRevolverStylePoint.value;
                                }
                            }
                        }
                        // ELECTRIC RAILCANNON
                        else if (beam.beamType == BeamType.Railgun && beam.hitAmount > 500)
                        {
                            if (ConfigManager.orbStrikeElectricCannonGrenade.value)
                            {
                                damageMulti += ConfigManager.orbStrikeElectricCannonExplosionDamage.value;
                                sizeMulti += ConfigManager.orbStrikeElectricCannonExplosionSize.value;
                                info.id = ConfigManager.orbStrikeElectricCannonStyleText.guid;
                                info.points = ConfigManager.orbStrikeElectricCannonStylePoint.value;
                            }
                        }
                        // MALICIOUS RAILCANNON
                        else if (beam.beamType == BeamType.Railgun)
                        {
                            if (ConfigManager.orbStrikeMaliciousCannonGrenade.value)
                            {
                                damageMulti += ConfigManager.orbStrikeMaliciousCannonGrenadeExtraDamage.value;
                                sizeMulti += ConfigManager.orbStrikeMaliciousCannonGrenadeExtraSize.value;
                                info.id = ConfigManager.orbStrikeMaliciousCannonStyleText.guid;
                                info.points = ConfigManager.orbStrikeMaliciousCannonStylePoint.value;
                            }
                        }
                        else
                            __state.state = false;
                    }
                    else
                        __state.state = false;

                    if(sizeMulti != 1 || damageMulti != 1)
                        foreach(Explosion exp in __state.templateExplosion.GetComponentsInChildren<Explosion>())
                        {
                            exp.maxSize *= sizeMulti;
                            exp.speed *= sizeMulti;
                            exp.damage = (int)(exp.damage * damageMulti);
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
        }
    }

    class Cannonball_Explode
    {
        static bool Prefix(Cannonball __instance, GameObject ___interruptionExplosion, ref GameObject ___breakEffect)
        {
            if ((Coin_ReflectRevolver.coinIsShooting && Coin_ReflectRevolver.shootingCoin != null) || (Time.time - Coin_ReflectRevolver.lastCoinTime <= 0.1f))
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

                if (list != null && list.isOrbitalStrike && ___interruptionExplosion != null)
                {
                    float damageMulti = 1f;
                    float sizeMulti = 1f;
                    GameObject explosion = GameObject.Instantiate<GameObject>(___interruptionExplosion, __instance.transform.position, Quaternion.identity);
                    OrbitalExplosionInfo info = explosion.AddComponent<OrbitalExplosionInfo>();
                    info.id = "";

                    // REVOLVER NORMAL
                    if (Coin_ReflectRevolver.shootingAltBeam == null)
                    {
                        if (ConfigManager.orbStrikeRevolverGrenade.value)
                        {
                            damageMulti += ConfigManager.orbStrikeRevolverGrenadeExtraDamage.value;
                            sizeMulti += ConfigManager.orbStrikeRevolverGrenadeExtraSize.value;
                            info.id = ConfigManager.orbStrikeRevolverStyleText.guid;
                            info.points = ConfigManager.orbStrikeRevolverStylePoint.value;
                        }
                    }
                    else if (Coin_ReflectRevolver.shootingAltBeam.TryGetComponent(out RevolverBeam beam))
                    {
                        if (beam.beamType == BeamType.Revolver)
                        {
                            // REVOLVER CHARGED (NORMAL + ALT. IF DISTINCTION IS NEEDED, USE beam.strongAlt FOR ALT)
                            if (beam.ultraRicocheter)
                            {
                                if (ConfigManager.orbStrikeRevolverChargedGrenade.value)
                                {
                                    damageMulti += ConfigManager.orbStrikeRevolverChargedGrenadeExtraDamage.value;
                                    sizeMulti += ConfigManager.orbStrikeRevolverChargedGrenadeExtraSize.value;
                                    info.id = ConfigManager.orbStrikeRevolverChargedStyleText.guid;
                                    info.points = ConfigManager.orbStrikeRevolverChargedStylePoint.value;
                                }
                            }
                            // REVOLVER ALT
                            else
                            {
                                if (ConfigManager.orbStrikeRevolverGrenade.value)
                                {
                                    damageMulti += ConfigManager.orbStrikeRevolverGrenadeExtraDamage.value;
                                    sizeMulti += ConfigManager.orbStrikeRevolverGrenadeExtraSize.value;
                                    info.id = ConfigManager.orbStrikeRevolverStyleText.guid;
                                    info.points = ConfigManager.orbStrikeRevolverStylePoint.value;
                                }
                            }
                        }
                        // ELECTRIC RAILCANNON
                        else if (beam.beamType == BeamType.Railgun && beam.hitAmount > 500)
                        {
                            if (ConfigManager.orbStrikeElectricCannonGrenade.value)
                            {
                                damageMulti += ConfigManager.orbStrikeElectricCannonExplosionDamage.value;
                                sizeMulti += ConfigManager.orbStrikeElectricCannonExplosionSize.value;
                                info.id = ConfigManager.orbStrikeElectricCannonStyleText.guid;
                                info.points = ConfigManager.orbStrikeElectricCannonStylePoint.value;
                            }
                        }
                        // MALICIOUS RAILCANNON
                        else if (beam.beamType == BeamType.Railgun)
                        {
                            if (ConfigManager.orbStrikeMaliciousCannonGrenade.value)
                            {
                                damageMulti += ConfigManager.orbStrikeMaliciousCannonGrenadeExtraDamage.value;
                                sizeMulti += ConfigManager.orbStrikeMaliciousCannonGrenadeExtraSize.value;
                                info.id = ConfigManager.orbStrikeMaliciousCannonStyleText.guid;
                                info.points = ConfigManager.orbStrikeMaliciousCannonStylePoint.value;
                            }
                        }
                    }

                    if (sizeMulti != 1 || damageMulti != 1)
                        foreach (Explosion exp in explosion.GetComponentsInChildren<Explosion>())
                        {
                            exp.maxSize *= sizeMulti;
                            exp.speed *= sizeMulti;
                            exp.damage = (int)(exp.damage * damageMulti);
                        }

                    if (MonoSingleton<PrefsManager>.Instance.GetBoolLocal("simpleExplosions", false))
                    {
                        ___breakEffect = null;
                    }
                    __instance.Break();

                    return false;
                }
            }

            return true;
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
                if (componentInParent != null && componentInParent.eid != null && !componentInParent.eid.blessed/* && !componentInParent.eid.dead*/)
                {
                    flag.active = false;
                    if(flag.id != "")
                        StyleHUD.Instance.AddPoints(flag.points, flag.id);
                }
            }

            return true;
        }
    }

    class EnemyIdentifier_DeliverDamage
    {
        static Coin lastExplosiveCoin = null;

        class StateInfo
        {
            public bool canPostStyle = false;
            public OrbitalExplosionInfo info = null;
        }

        static bool Prefix(EnemyIdentifier __instance, out StateInfo __state, Vector3 __2, ref float __3)
        {
            //if (Coin_ReflectRevolver.shootingCoin == lastExplosiveCoin)
            //    return true;

            __state = new StateInfo();
            bool causeExplosion = false;

            if (__instance.dead)
                return true;

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
                    causeExplosion = true;
                }
            }
            else if (RevolverBeam_ExecuteHits.isOrbitalRay && RevolverBeam_ExecuteHits.orbitalBeam != null)
            {
                if (RevolverBeam_ExecuteHits.orbitalBeamFlag != null && !RevolverBeam_ExecuteHits.orbitalBeamFlag.exploded)
                {
                    causeExplosion = true;
                }
            }

            if(causeExplosion)
            {
                __state.canPostStyle = true;

                // REVOLVER NORMAL
                if (Coin_ReflectRevolver.shootingAltBeam == null)
                {
                    if(ConfigManager.orbStrikeRevolverExplosion.value)
                    {
                        GameObject explosion = GameObject.Instantiate(Plugin.explosion, /*__instance.gameObject.transform.position*/__2, Quaternion.identity);
                        foreach (Explosion exp in explosion.GetComponentsInChildren<Explosion>())
                        {
                            exp.enemy = false;
                            exp.hitterWeapon = "";
                            exp.maxSize *= ConfigManager.orbStrikeRevolverExplosionSize.value;
                            exp.speed *= ConfigManager.orbStrikeRevolverExplosionSize.value;
                            exp.damage = (int)(exp.damage * ConfigManager.orbStrikeRevolverExplosionDamage.value);
                        }

                        OrbitalExplosionInfo info = explosion.AddComponent<OrbitalExplosionInfo>();
                        info.id = ConfigManager.orbStrikeRevolverStyleText.guid;
                        info.points = ConfigManager.orbStrikeRevolverStylePoint.value;
                        __state.info = info;
                    }
                }
                else if (Coin_ReflectRevolver.shootingAltBeam.TryGetComponent(out RevolverBeam beam))
                {
                    if (beam.beamType == BeamType.Revolver)
                    {
                        // REVOLVER CHARGED (NORMAL + ALT. IF DISTINCTION IS NEEDED, USE beam.strongAlt FOR ALT)
                        if (beam.ultraRicocheter)
                        {
                            if(ConfigManager.orbStrikeRevolverChargedInsignia.value)
                            {
                                GameObject insignia = GameObject.Instantiate(Plugin.virtueInsignia, /*__instance.transform.position*/__2, Quaternion.identity);
                                // This is required for ff override to detect this insignia as non ff attack
                                insignia.gameObject.name = "PlayerSpawned";
                                float horizontalSize = ConfigManager.orbStrikeRevolverChargedInsigniaSize.value;
                                insignia.transform.localScale = new Vector3(horizontalSize, insignia.transform.localScale.y, horizontalSize);
                                VirtueInsignia comp = insignia.GetComponent<VirtueInsignia>();
                                comp.windUpSpeedMultiplier = ConfigManager.orbStrikeRevolverChargedInsigniaDelayBoost.value;
                                comp.damage = ConfigManager.orbStrikeRevolverChargedInsigniaDamage.value;
                                comp.predictive = false;
                                comp.hadParent = false;
                                comp.noTracking = true;

                                StyleHUD.Instance.AddPoints(ConfigManager.orbStrikeRevolverChargedStylePoint.value, ConfigManager.orbStrikeRevolverChargedStyleText.guid);
                                __state.canPostStyle = false;
                            }
                        }
                        // REVOLVER ALT
                        else
                        {
                            if (ConfigManager.orbStrikeRevolverExplosion.value)
                            {
                                GameObject explosion = GameObject.Instantiate(Plugin.explosion, /*__instance.gameObject.transform.position*/__2, Quaternion.identity);
                                foreach (Explosion exp in explosion.GetComponentsInChildren<Explosion>())
                                {
                                    exp.enemy = false;
                                    exp.hitterWeapon = "";
                                    exp.maxSize *= ConfigManager.orbStrikeRevolverExplosionSize.value;
                                    exp.speed *= ConfigManager.orbStrikeRevolverExplosionSize.value;
                                    exp.damage = (int)(exp.damage * ConfigManager.orbStrikeRevolverExplosionDamage.value);
                                }

                                OrbitalExplosionInfo info = explosion.AddComponent<OrbitalExplosionInfo>();
                                info.id = ConfigManager.orbStrikeRevolverStyleText.guid;
                                info.points = ConfigManager.orbStrikeRevolverStylePoint.value;
                                __state.info = info;
                            }
                        }
                    }
                    // ELECTRIC RAILCANNON
                    else if (beam.beamType == BeamType.Railgun && beam.hitAmount > 500)
                    {
                        if(ConfigManager.orbStrikeElectricCannonExplosion.value)
                        {
                            GameObject lighning = GameObject.Instantiate(Plugin.lightningStrikeExplosive, /*__instance.gameObject.transform.position*/ __2, Quaternion.identity);
                            foreach (Explosion exp in lighning.GetComponentsInChildren<Explosion>())
                            {
                                exp.enemy = false;
                                exp.hitterWeapon = "";

                                if (exp.damage == 0)
                                    exp.maxSize /= 2;

                                exp.maxSize *= ConfigManager.orbStrikeElectricCannonExplosionSize.value;
                                exp.speed *= ConfigManager.orbStrikeElectricCannonExplosionSize.value;
                                exp.damage = (int)(exp.damage * ConfigManager.orbStrikeElectricCannonExplosionDamage.value);

                                exp.canHit = AffectedSubjects.All;
                            }

                            OrbitalExplosionInfo info = lighning.AddComponent<OrbitalExplosionInfo>();
                            info.id = ConfigManager.orbStrikeElectricCannonStyleText.guid;
                            info.points = ConfigManager.orbStrikeElectricCannonStylePoint.value;
                            __state.info = info;
                        }
                    }
                    // MALICIOUS RAILCANNON
                    else if (beam.beamType == BeamType.Railgun)
                    {
                        // UNUSED
                        causeExplosion = false;
                    }
                    // MALICIOUS BEAM
                    else if (beam.beamType == BeamType.MaliciousFace)
                    {
                        GameObject explosion = GameObject.Instantiate(Plugin.sisyphiusPrimeExplosion, /*__instance.gameObject.transform.position*/__2, Quaternion.identity);
                        foreach (Explosion exp in explosion.GetComponentsInChildren<Explosion>())
                        {
                            exp.enemy = false;
                            exp.hitterWeapon = "";
                            exp.maxSize *= ConfigManager.maliciousChargebackExplosionSizeMultiplier.value;
                            exp.speed *= ConfigManager.maliciousChargebackExplosionSizeMultiplier.value;
                            exp.damage = (int)(exp.damage * ConfigManager.maliciousChargebackExplosionDamageMultiplier.value);
                        }

                        OrbitalExplosionInfo info = explosion.AddComponent<OrbitalExplosionInfo>();
                        info.id = ConfigManager.maliciousChargebackStyleText.guid;
                        info.points = ConfigManager.maliciousChargebackStylePoint.value;
                        __state.info = info;
                    }
                    // SENTRY BEAM
                    else if (beam.beamType == BeamType.Enemy)
                    {
                        StyleHUD.Instance.AddPoints(ConfigManager.sentryChargebackStylePoint.value, ConfigManager.sentryChargebackStyleText.formattedString);

                        if (ConfigManager.sentryChargebackExtraBeamCount.value > 0)
                        {
                            List<Tuple<EnemyIdentifier, float>> enemies = UnityUtils.GetClosestEnemies(__2, ConfigManager.sentryChargebackExtraBeamCount.value, UnityUtils.doNotCollideWithPlayerValidator);
                            foreach (Tuple<EnemyIdentifier, float> enemy in enemies)
                            {
                                RevolverBeam newBeam = GameObject.Instantiate(beam, beam.transform.position, Quaternion.identity);
                                newBeam.hitEids.Add(__instance);
                                newBeam.transform.LookAt(enemy.Item1.transform);
                                GameObject.Destroy(newBeam.GetComponent<OrbitalStrikeFlag>());
                            }
                        }

                        RevolverBeam_ExecuteHits.isOrbitalRay = false;
                    }
                }

                if (causeExplosion && RevolverBeam_ExecuteHits.orbitalBeamFlag != null)
                    RevolverBeam_ExecuteHits.orbitalBeamFlag.exploded = true;

                Debug.Log("Applied orbital strike explosion");
            }

            return true;
        }

        static void Postfix(EnemyIdentifier __instance, StateInfo __state)
        {
            if(__state.canPostStyle && __instance.dead && __state.info != null)
            {
                __state.info.active = false;
                if (__state.info.id != "")
                    StyleHUD.Instance.AddPoints(__state.info.points, __state.info.id);
            }
        }
    }

    class RevolverBeam_HitSomething
    {
        static bool Prefix(RevolverBeam __instance, out GameObject __state)
        {
            __state = null;

            if (RevolverBeam_ExecuteHits.orbitalBeam == null)
                return true;
            if (__instance.beamType != BeamType.Railgun)
                return true;
            if (__instance.hitAmount != 1)
                return true;

            if (RevolverBeam_ExecuteHits.orbitalBeam.GetInstanceID() == __instance.GetInstanceID())
            {
                if (!RevolverBeam_ExecuteHits.orbitalBeamFlag.exploded && ConfigManager.orbStrikeMaliciousCannonExplosion.value)
                {
                    Debug.Log("MALICIOUS EXPLOSION EXTRA SIZE");

                    GameObject tempExp = GameObject.Instantiate(__instance.hitParticle, new Vector3(1000000, 1000000, 1000000), Quaternion.identity);
                    foreach (Explosion exp in tempExp.GetComponentsInChildren<Explosion>())
                    {
                        exp.maxSize *= ConfigManager.orbStrikeMaliciousCannonExplosionSizeMultiplier.value;
                        exp.speed *= ConfigManager.orbStrikeMaliciousCannonExplosionSizeMultiplier.value;
                        exp.damage = (int)(exp.damage * ConfigManager.orbStrikeMaliciousCannonExplosionDamageMultiplier.value);
                    }
                    __instance.hitParticle = tempExp;

                    OrbitalExplosionInfo info = tempExp.AddComponent<OrbitalExplosionInfo>();
                    info.id = ConfigManager.orbStrikeMaliciousCannonStyleText.guid;
                    info.points = ConfigManager.orbStrikeMaliciousCannonStylePoint.value;

                    RevolverBeam_ExecuteHits.orbitalBeamFlag.exploded = true;
                }
                Debug.Log("Already exploded");
            }
            else
                Debug.Log("Not the same instance");

            return true;
        }

        static void Postfix(RevolverBeam __instance, GameObject __state)
        {
            if (__state != null)
                GameObject.Destroy(__state);
        }
    }
}
