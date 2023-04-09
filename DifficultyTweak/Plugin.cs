using BepInEx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UniverseLib;
using UniverseLib.Runtime;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using HarmonyLib;
using System.Runtime.CompilerServices;
using System.IO;
using UnityEngine.Experimental.AssetBundlePatching;

namespace DifficultyTweak
{
    public class GameObjectReference
    {
        public GameObject gameObject = null;

        public bool isNull
        {
            get => gameObject == null;
        }
    }

    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static string bundlePath = Path.Combine(Environment.CurrentDirectory, "ULTRAKILL_Data", "StreamingAssets", "Magenta", "Bundles");
        public static AssetBundle GetAssetBundle(string name)
        {
            AssetManager manager = MonoSingleton<AssetManager>.Instance;
            AssetBundle bundle = null;

            manager?.loadedBundles.TryGetValue(name, out bundle);

            if (bundle != null)
                return bundle;

            bundle = AssetBundle.LoadFromFile(Path.Combine(bundlePath, name));
            MonoSingleton<AssetManager>.Instance?.loadedBundles.Add(name, bundle);

            return bundle;
        }

        public static Plugin instance;

        const string PLUGIN_GUID = "com.eternalUnion.ultraPain";
        const string PLUGIN_NAME = "Ultra Pain";
        const string PLUGIN_VERSION = "1.0.0";

        public static Vector3 PredictPlayerPosition(Collider safeCollider, float speedMod)
        {   
            Transform target = MonoSingleton<PlayerTracker>.Instance.GetTarget();
            if (MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().magnitude == 0f)
                return target.position;
            RaycastHit raycastHit;
            if (Physics.Raycast(target.position, MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity(), out raycastHit, MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().magnitude * 0.35f / speedMod, 4096, QueryTriggerInteraction.Collide) && raycastHit.collider == safeCollider)
                return target.position;
            else if (Physics.Raycast(target.position, MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity(), out raycastHit, MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity().magnitude * 0.35f / speedMod, LayerMaskDefaults.Get(LMD.EnvironmentAndBigEnemies), QueryTriggerInteraction.Collide))
            {
                return raycastHit.point;
            }
            else {
                Vector3 projectedPlayerPos = target.position + MonoSingleton<PlayerTracker>.Instance.GetPlayerVelocity() * 0.35f / speedMod;
                return new Vector3(projectedPlayerPos.x, target.transform.position.y + (target.transform.position.y - projectedPlayerPos.y) * 0.5f, projectedPlayerPos.z);
            }
        }

        public static GameObjectReference projectileSpread = new GameObjectReference();
        public static GameObjectReference homingProjectile = new GameObjectReference();
        public static GameObjectReference decorativeProjectile2 = new GameObjectReference();
        public static GameObjectReference shotgunGrenade = new GameObjectReference();
        public static GameObjectReference beam = new GameObjectReference();
        public static GameObjectReference turretBeam = new GameObjectReference();
        public static GameObjectReference lighningStrikeExplosive = new GameObjectReference();
        public static GameObjectReference lighningStrikeWindup = new GameObjectReference();

        public static GameObjectReference maliciousFace = new GameObjectReference();
        public static GameObjectReference idol = new GameObjectReference();
        public static GameObjectReference ferryman = new GameObjectReference();


        // Singleton prefabs
        private static GameObject _soliderBullet;

        // Variables
        public static float SoliderShootAnimationStart = 1.2f;
        public static float SoliderGrenadeForce = 10000f;

        public static float SwordsMachineKnockdownTimeNormalized = 0.8f;
        public static float SwordsMachineCoreSpeed = 80f;

        public static float MinGrenadeParryVelocity = 40f;

        private static GameObject _maliciousFaceBullet;
        public static GameObject maliciousFaceBullet
        {
            get
            {
                if (_maliciousFaceBullet == null)
                {
                    _maliciousFaceBullet = GameObject.Instantiate(Plugin.homingProjectile.gameObject);
                    _maliciousFaceBullet.SetActive(false);

                    Projectile proj = _maliciousFaceBullet.GetComponent<Projectile>();
                    proj.safeEnemyType = EnemyType.MaliciousFace;
                    proj.turningSpeedMultiplier = 0.2f;
                    proj.speed = 20;
                }

                return _maliciousFaceBullet;
            }
        }

        public static GameObject _lighningBoltSFX;
        public static GameObject lighningBoltSFX
        {
            get
            {
                if(_lighningBoltSFX == null)
                    _lighningBoltSFX = ferryman.gameObject.transform.Find("LightningBoltChimes").gameObject;

                return _lighningBoltSFX;
            }
        }

        public void LoadPrefabs()
        {
            AssetBundle bundle0 = GetAssetBundle("bundle-0");
            AssetBundle bundle1 = GetAssetBundle("bundle-1");
            AssetBundle uhbundle0 = GetAssetBundle("unhardened-bundle-0");

            //[bundle-0][assets/prefabs/projectilespread.prefab]
            projectileSpread.gameObject = bundle0.LoadAsset<GameObject>("assets/prefabs/projectilespread.prefab");
            //[bundle-0][assets/prefabs/projectilehoming.prefab]
            homingProjectile.gameObject = bundle0.LoadAsset<GameObject>("assets/prefabs/projectilehoming.prefab");
            //[bundle-1][assets/prefabs/projectiledecorative 2.prefab]
            decorativeProjectile2.gameObject = bundle1.LoadAsset<GameObject>("assets/prefabs/projectiledecorative 2.prefab");
            //[bundle-0][assets/prefabs/grenade.prefab]
            shotgunGrenade.gameObject = bundle0.LoadAsset<GameObject>("assets/prefabs/grenade.prefab");
            //[bundle-0][assets/prefabs/turretbeam.prefab]
            turretBeam.gameObject = bundle0.LoadAsset<GameObject>("assets/prefabs/turretbeam.prefab");
            //[bundle-0][assets/prefabs/dronemaliciousbeam.prefab]
            beam.gameObject = bundle0.LoadAsset<GameObject>("assets/prefabs/dronemaliciousbeam.prefab");
            //[unhardened-bundle-0][assets/prefabs/lightningstrikeexplosive.prefab]
            lighningStrikeExplosive.gameObject = uhbundle0.LoadAsset<GameObject>("assets/prefabs/lightningstrikeexplosive.prefab");
            //[unhardened-bundle-0][assets/particles/lightningboltwindupfollow variant.prefab]
            lighningStrikeWindup.gameObject = uhbundle0.LoadAsset<GameObject>("assets/particles/lightningboltwindupfollow variant.prefab");
            //[bundle-0][assets/prefabs/enemies/spider.prefab]
            maliciousFace.gameObject = bundle0.LoadAsset<GameObject>("assets/prefabs/enemies/spider.prefab");
            //[bundle-0][assets/prefabs/enemies/idol.prefab]
            idol.gameObject = bundle0.LoadAsset<GameObject>("assets/prefabs/enemies/idol.prefab");
            //[bundle-0][assets/prefabs/enemies/ferryman.prefab]
            ferryman.gameObject = bundle0.LoadAsset<GameObject>("assets/prefabs/enemies/ferryman.prefab");
        }

        public void OnSceneChange(Scene before, Scene after)
        {
            StyleIDs.RegisterIDs();
        }

        public static class StyleIDs
        {
            public static string fistfulOfNades = "eternalUnion.fistfulOfNades";
            public static string rocketBoost = "eternalUnion.rocketBoost";
            
            public static void RegisterIDs()
            {
                if (MonoSingleton<StyleHUD>.Instance == null)
                    return;

                MonoSingleton<StyleHUD>.Instance.RegisterStyleItem(StyleIDs.fistfulOfNades, "<color=cyan>FISTFUL OF 'NADE</color>");
                MonoSingleton<StyleHUD>.Instance.RegisterStyleItem(StyleIDs.rocketBoost, "<color=lime>ROCKET BOOST</color>");

                Debug.Log("Registered all style ids");
            }
        }

        public void Awake()
        {
            instance = this;
            LoadPrefabs();

            // Plugin startup logic 
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            new Harmony(PLUGIN_GUID).PatchAll();
            SceneManager.activeSceneChanged += OnSceneChange;
        }

        public void Update()
        {

        }
    }
}
