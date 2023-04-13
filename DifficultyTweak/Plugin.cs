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
using DifficultyTweak.Patches;
using System.Linq;
using UnityEngine.UI;

using PluginConfig;
using PluginConfig.API;
using UnityEngine.EventSystems;

namespace DifficultyTweak
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInDependency("com.eternalUnion.pluginConfigurator")]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.eternalUnion.ultraPain";
        public const string PLUGIN_NAME = "Ultra Pain";
        public const string PLUGIN_VERSION = "1.0.0";

        public static Plugin instance;

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

        public static GameObject projectileSpread;
        public static GameObject homingProjectile;
        public static GameObject hideousMassProjectile;
        public static GameObject decorativeProjectile2;
        public static GameObject shotgunGrenade;
        public static GameObject beam;
        public static GameObject turretBeam;
        public static GameObject lighningStrikeExplosive;
        public static GameObject lighningStrikeWindup;
        public static GameObject explosion;
        public static GameObject virtueInsignia;

        public static GameObject maliciousFace;
        public static GameObject idol;
        public static GameObject ferryman;

        public static GameObject enrageEffect;


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
            projectileSpread = bundle0.LoadAsset<GameObject>("assets/prefabs/projectilespread.prefab");
            //[bundle-0][assets/prefabs/projectilehoming.prefab]
            homingProjectile = bundle0.LoadAsset<GameObject>("assets/prefabs/projectilehoming.prefab");
            //[bundle-1][assets/prefabs/projectiledecorative 2.prefab]
            decorativeProjectile2 = bundle1.LoadAsset<GameObject>("assets/prefabs/projectiledecorative 2.prefab");
            //[bundle-0][assets/prefabs/grenade.prefab]
            shotgunGrenade = bundle0.LoadAsset<GameObject>("assets/prefabs/grenade.prefab");
            //[bundle-0][assets/prefabs/turretbeam.prefab]
            turretBeam = bundle0.LoadAsset<GameObject>("assets/prefabs/turretbeam.prefab");
            //[bundle-0][assets/prefabs/dronemaliciousbeam.prefab]
            beam = bundle0.LoadAsset<GameObject>("assets/prefabs/dronemaliciousbeam.prefab");
            //[unhardened-bundle-0][assets/prefabs/lightningstrikeexplosive.prefab]
            lighningStrikeExplosive = uhbundle0.LoadAsset<GameObject>("assets/prefabs/lightningstrikeexplosive.prefab");
            //[unhardened-bundle-0][assets/particles/lightningboltwindupfollow variant.prefab]
            lighningStrikeWindup = uhbundle0.LoadAsset<GameObject>("assets/particles/lightningboltwindupfollow variant.prefab");
            //[bundle-0][assets/prefabs/enemies/spider.prefab]
            maliciousFace = bundle0.LoadAsset<GameObject>("assets/prefabs/enemies/spider.prefab");
            //[bundle-0][assets/prefabs/enemies/idol.prefab]
            idol = bundle0.LoadAsset<GameObject>("assets/prefabs/enemies/idol.prefab");
            //[bundle-0][assets/prefabs/enemies/ferryman.prefab]
            ferryman = bundle0.LoadAsset<GameObject>("assets/prefabs/enemies/ferryman.prefab");
            //[bundle-0][assets/prefabs/explosion.prefab]
            explosion = bundle0.LoadAsset<GameObject>("assets/prefabs/explosion.prefab");
            //[bundle-0][assets/prefabs/virtueinsignia.prefab]
            virtueInsignia = bundle0.LoadAsset<GameObject>("assets/prefabs/virtueinsignia.prefab");
            //[bundle-0][assets/prefabs/projectileexplosivehh.prefab]
            hideousMassProjectile = bundle0.LoadAsset<GameObject>("assets/prefabs/projectileexplosivehh.prefab");
            //[bundle-0][assets/particles/rageeffect.prefab]
            enrageEffect = bundle0.LoadAsset<GameObject>("assets/particles/rageeffect.prefab");

            hideousMassProjectile.AddComponent<HideousMassProjectile>();
        }

        public static bool ultrapainDifficulty = false;
        public static GameObject currentDifficultyButton;
        public static GameObject currentDifficultyPanel;
        public void OnSceneChange(Scene before, Scene after)
        {
            StyleIDs.RegisterIDs();

            if(SceneManager.GetActiveScene().name == "Main Menu")
            {
                //Canvas/Difficulty Select (1)/Violent
                Transform difficultySelect = SceneManager.GetActiveScene().GetRootGameObjects().Where(obj => obj.name == "Canvas").First().transform.Find("Difficulty Select (1)");
                GameObject ultrapainButton = GameObject.Instantiate(difficultySelect.Find("Violent").gameObject, difficultySelect);
                currentDifficultyButton = ultrapainButton;

                ultrapainButton.transform.Find("Name").GetComponent<Text>().text = ConfigManager.pluginName.value;
                RectTransform ultrapainTrans = ultrapainButton.GetComponent<RectTransform>();
                ultrapainTrans.anchoredPosition = new Vector2(20f, -104f);

                //Canvas/Difficulty Select (1)/Violent Info
                GameObject info = GameObject.Instantiate(difficultySelect.Find("Violent Info").gameObject, difficultySelect);
                currentDifficultyPanel = info;
                info.transform.Find("Text").GetComponent<Text>().text =
                    """
                    Fast and aggressive enemies with unique attack patterns.

                    Quick thinking, mobility options, and a decent understanding of the vanilla game are essential.

                    <color=red>Recommended for players who have gotten used to VIOLENT's changes and are looking to freshen up their gameplay with unique enemy mechanics.</color>
                    """;
                info.transform.Find("Title (1)").GetComponent<Text>().text = $"--{ConfigManager.pluginName.value}--";
                info.transform.Find("Title (1)").GetComponent<Text>().resizeTextForBestFit = true;
                info.transform.Find("Title (1)").GetComponent<Text>().horizontalOverflow = HorizontalWrapMode.Wrap;
                info.transform.Find("Title (1)").GetComponent<Text>().verticalOverflow = VerticalWrapMode.Truncate;
                info.SetActive(false);

                EventTrigger evt = ultrapainButton.GetComponent<EventTrigger>();
                evt.triggers.Clear();

                /*EventTrigger.TriggerEvent activate = new EventTrigger.TriggerEvent();
                activate.AddListener((BaseEventData data) => info.SetActive(true));
                EventTrigger.TriggerEvent deactivate = new EventTrigger.TriggerEvent();
                activate.AddListener((BaseEventData data) => info.SetActive(false));*/

                EventTrigger.Entry trigger1 = new EventTrigger.Entry() { eventID = EventTriggerType.PointerEnter };
                trigger1.callback.AddListener((BaseEventData data) => info.SetActive(true));
                EventTrigger.Entry trigger2 = new EventTrigger.Entry() { eventID = EventTriggerType.PointerExit };
                trigger2.callback.AddListener((BaseEventData data) => info.SetActive(false));

                evt.triggers.Add(trigger1);
                evt.triggers.Add(trigger2);

                /*
                 Fast and aggressive enemies and high damage. 

Quick thinking, mobility options and situational awareness are essential.

<color=red>Recommended for players who have already gotten used to the game's pace and mechanics.</color>
                 */
            }
        }

        public static class StyleIDs
        {
            public static string fistfulOfNades = "eternalUnion.fistfulOfNades";
            public static string rocketBoost = "eternalUnion.rocketBoost";
            
            public static void RegisterIDs()
            {
                if (MonoSingleton<StyleHUD>.Instance == null)
                    return;

                MonoSingleton<StyleHUD>.Instance.RegisterStyleItem(StyleIDs.fistfulOfNades, ConfigManager.grenadeBoostStyleText.value);
                //"<color=magenta>ABBYBOOST</color>"
                MonoSingleton<StyleHUD>.Instance.RegisterStyleItem(StyleIDs.rocketBoost, ConfigManager.rocketBoostStyleText.value);

                Debug.Log("Registered all style ids");
            }
        }

        public static Harmony harmony;

        public void Awake()
        {
            instance = this;
            LoadPrefabs();

            // Plugin startup logic 
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            harmony = new Harmony(PLUGIN_GUID);
            harmony.PatchAll();
            ConfigManager.Initialize();

            SceneManager.activeSceneChanged += OnSceneChange;
        }
    }
}
