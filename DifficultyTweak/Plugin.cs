using BepInEx;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using HarmonyLib;
using System.IO;
using DifficultyTweak.Patches;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Reflection;
using Steamworks;

namespace DifficultyTweak
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInDependency("com.eternalUnion.pluginConfigurator", "1.0.0")]
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
        public static GameObject rocket;
        public static GameObject revolverBullet;
        public static GameObject maliciousCannonBeam;
        public static GameObject lightningBoltSFX;
        public static GameObject revolverBeam;
        public static GameObject blastwave;
        public static GameObject cannonBall;

        public static GameObject idol;
        public static GameObject ferryman;
        public static GameObject minosPrime;

        public static GameObject enrageEffect;
        public static GameObject v2flashUnparryable;

        public static AudioClip cannonBallChargeAudio;

        // Variables
        public static float SoliderShootAnimationStart = 1.2f;
        public static float SoliderGrenadeForce = 10000f;

        public static float SwordsMachineKnockdownTimeNormalized = 0.8f;
        public static float SwordsMachineCoreSpeed = 80f;

        public static float MinGrenadeParryVelocity = 40f;

        public static GameObject _lighningBoltSFX;
        public static GameObject lighningBoltSFX
        {
            get
            {
                if (_lighningBoltSFX == null)
                    _lighningBoltSFX = ferryman.gameObject.transform.Find("LightningBoltChimes").gameObject;

                return _lighningBoltSFX;
            }
        }

        private static bool loadedPrefabs = false;
        public void LoadPrefabs()
        {
            if (loadedPrefabs)
                return;
            loadedPrefabs = true;

            AssetBundle bundle0 = GetAssetBundle("bundle-0");
            AssetBundle bundle1 = GetAssetBundle("bundle-1");
            AssetBundle uhbundle0 = GetAssetBundle("unhardened-bundle-0");
            AssetBundle uhbundle1 = GetAssetBundle("unhardened-bundle-1");

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
            //[bundle-0][assets/particles/v2flashunparriable.prefab]
            v2flashUnparryable = bundle0.LoadAsset<GameObject>("assets/particles/v2flashunparriable.prefab");
            //[bundle-0][assets/prefabs/rocket.prefab]
            rocket = bundle0.LoadAsset<GameObject>("assets/prefabs/rocket.prefab");
            //[bundle-0][assets/prefabs/revolverbullet.prefab]
            revolverBullet = bundle0.LoadAsset<GameObject>("assets/prefabs/revolverbullet.prefab");
            //[bundle-0][assets/prefabs/railcannonbeammalicious.prefab]
            maliciousCannonBeam = bundle0.LoadAsset<GameObject>("assets/prefabs/railcannonbeammalicious.prefab");
            //[bundle-0][assets/prefabs/revolverbeam.prefab]
            revolverBeam = bundle0.LoadAsset<GameObject>("assets/prefabs/revolverbeam.prefab");
            //[bundle-1][assets/prefabs/explosionwaveenemy.prefab]
            blastwave = bundle1.LoadAsset<GameObject>("assets/prefabs/explosionwaveenemy.prefab");
            //[bundle-0][assets/prefabs/enemies/minosprime.prefab]
            minosPrime = bundle0.LoadAsset<GameObject>("assets/prefabs/enemies/minosprime.prefab");
            //[unhardened-bundle-1][assets/prefabs/cannonball.prefab]
            cannonBall = uhbundle1.LoadAsset<GameObject>("assets/prefabs/cannonball.prefab");
            //[unhardened-bundle-1][assets/sounds/other weapons/machinepumploop.wav]
            cannonBallChargeAudio = uhbundle1.LoadAsset<AudioClip>("assets/sounds/other weapons/machinepumploop.wav");

            // hideousMassProjectile.AddComponent<HideousMassProjectile>();
        }

        public static bool ultrapainDifficulty = false;
        public static bool realUltrapainDifficulty = false;
        public static GameObject currentDifficultyButton;
        public static GameObject currentDifficultyPanel;
        public void OnSceneChange(Scene before, Scene after)
        {
            StyleIDs.RegisterIDs();
            PatchAll();

            MinosPrimeCharge.CreateDecoy();

            if (SceneManager.GetActiveScene().name == "Main Menu")
            {
                LoadPrefabs();

                //Canvas/Difficulty Select (1)/Violent
                Transform difficultySelect = SceneManager.GetActiveScene().GetRootGameObjects().Where(obj => obj.name == "Canvas").First().transform.Find("Difficulty Select (1)");
                GameObject ultrapainButton = GameObject.Instantiate(difficultySelect.Find("Violent").gameObject, difficultySelect);
                currentDifficultyButton = ultrapainButton;

                ultrapainButton.transform.Find("Name").GetComponent<Text>().text = ConfigManager.pluginName.value;
                ultrapainButton.GetComponent<DifficultySelectButton>().difficulty = 5;
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
                MonoSingleton<StyleHUD>.Instance.RegisterStyleItem(StyleIDs.rocketBoost, ConfigManager.rocketBoostStyleText.value);

                Debug.Log("Registered all style ids");
            }
        }

        public static Harmony harmonyTweaks;
        public static Harmony harmonyBase;
        private static MethodInfo GetMethod<T>(string name)
        {
            return typeof(T).GetMethod(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        }

        private static void PatchAllEnemies()
        {
            if (!ConfigManager.enemyTweakToggle.value)
                return;

            harmonyTweaks.Patch(GetMethod<StatueBoss>("Start"), postfix: new HarmonyMethod(GetMethod<StatueBoss_Start_Patch>("Postfix")));
            if (ConfigManager.cerberusDashToggle.value)
                harmonyTweaks.Patch(GetMethod<StatueBoss>("StopDash"), postfix: new HarmonyMethod(GetMethod<StatueBoss_StopDash_Patch>("Postfix")));

            harmonyTweaks.Patch(GetMethod<Drone>("Start"), postfix: new HarmonyMethod(GetMethod<Drone_Start_Patch>("Postfix")));
            harmonyTweaks.Patch(GetMethod<Drone>("Shoot"), prefix: new HarmonyMethod(GetMethod<Drone_Shoot_Patch>("Prefix")));

            harmonyTweaks.Patch(GetMethod<Ferryman>("Start"), postfix: new HarmonyMethod(GetMethod<FerrymanStart>("Postfix")));
            if(ConfigManager.ferrymanComboToggle.value)
                harmonyTweaks.Patch(GetMethod<Ferryman>("StopMoving"), postfix: new HarmonyMethod(GetMethod<FerrymanStopMoving>("Postfix")));

            if(ConfigManager.filthExplodeToggle.value)
                harmonyTweaks.Patch(GetMethod<SwingCheck2>("CheckCollision"), prefix: new HarmonyMethod(GetMethod<SwingCheck2_CheckCollision_Patch2>("Prefix")));

            if(ConfigManager.fleshPrisonSpinAttackToggle.value)
                harmonyTweaks.Patch(GetMethod<FleshPrison>("HomingProjectileAttack"), postfix: new HarmonyMethod(GetMethod<FleshPrisonShoot>("Postfix")));

            if (ConfigManager.hideousMassInsigniaToggle.value)
            {
                harmonyTweaks.Patch(GetMethod<Projectile>("Explode"), postfix: new HarmonyMethod(GetMethod<Projectile_Explode_Patch>("Postfix")));
                harmonyTweaks.Patch(GetMethod<Mass>("ShootExplosive"), postfix: new HarmonyMethod(GetMethod<HideousMassHoming>("Postfix")), prefix: new HarmonyMethod(GetMethod<HideousMassHoming>("Prefix")));
            }

            if (ConfigManager.maliciousFaceHomingProjectileToggle.value)
            {
                harmonyTweaks.Patch(GetMethod<SpiderBody>("Start"), postfix: new HarmonyMethod(GetMethod<MaliciousFace_Start_Patch>("Postfix")));
                harmonyTweaks.Patch(GetMethod<SpiderBody>("ShootProj"), postfix: new HarmonyMethod(GetMethod<MaliciousFace_ShootProj_Patch>("Postfix")));
            }
            if (ConfigManager.maliciousFaceRadianceOnEnrage.value)
                harmonyTweaks.Patch(GetMethod<SpiderBody>("Enrage"), postfix: new HarmonyMethod(GetMethod<MaliciousFace_Enrage_Patch>("Postfix")));

            harmonyTweaks.Patch(GetMethod<Mindflayer>("Start"), postfix: new HarmonyMethod(GetMethod<Mindflayer_Start_Patch>("Postfix")));
            if (ConfigManager.mindflayerShootTweakToggle.value)
                harmonyTweaks.Patch(GetMethod<Mindflayer>("ShootProjectiles"), prefix: new HarmonyMethod(GetMethod<Mindflayer_ShootProjectiles_Patch>("Prefix")));
            if (ConfigManager.mindflayerTeleportComboToggle.value)
            {
                harmonyTweaks.Patch(GetMethod<SwingCheck2>("CheckCollision"), postfix: new HarmonyMethod(GetMethod<SwingCheck2_CheckCollision_Patch>("Postfix")));
                harmonyTweaks.Patch(GetMethod<Mindflayer>("MeleeTeleport"), prefix: new HarmonyMethod(GetMethod<Mindflayer_MeleeTeleport_Patch>("Prefix")));
                harmonyTweaks.Patch(GetMethod<SwingCheck2>("DamageStop"), postfix: new HarmonyMethod(GetMethod<SwingCheck2_DamageStop_Patch>("Postfix")));
            }

            if (ConfigManager.minosPrimeRandomTeleportToggle.value)
                harmonyTweaks.Patch(GetMethod<MinosPrime>("ProjectileCharge"), postfix: new HarmonyMethod(GetMethod<MinosPrimeCharge>("Postfix")));
            if (ConfigManager.minosPrimeTeleportTrail.value)
                harmonyTweaks.Patch(GetMethod<MinosPrime>("Teleport"), postfix: new HarmonyMethod(GetMethod<MinosPrimeCharge>("TeleportPostfix")));

            if (ConfigManager.schismSpreadAttackToggle.value)
                harmonyTweaks.Patch(GetMethod<ZombieProjectiles>("ShootProjectile"), postfix: new HarmonyMethod(GetMethod<ZombieProjectile_ShootProjectile_Patch>("Postfix")));

            if (ConfigManager.soliderShootTweakToggle.value)
            {
                harmonyTweaks.Patch(GetMethod<ZombieProjectiles>("Start"), postfix: new HarmonyMethod(GetMethod<Solider_Start_Patch>("Postfix")));
            }
            if(ConfigManager.soliderCoinsIgnoreWeakPointToggle.value)
                harmonyTweaks.Patch(GetMethod<ZombieProjectiles>("SpawnProjectile"), postfix: new HarmonyMethod(GetMethod<Solider_SpawnProjectile_Patch>("Postfix")));
            if (ConfigManager.soliderShootGrenadeToggle.value)
            {
                harmonyTweaks.Patch(GetMethod<ZombieProjectiles>("ThrowProjectile"), postfix: new HarmonyMethod(GetMethod<Solider_ThrowProjectile_Patch>("Postfix")));
                harmonyTweaks.Patch(GetMethod<Grenade>("Explode"), postfix: new HarmonyMethod(GetMethod<Grenade_Explode_Patch>("Postfix")), prefix: new HarmonyMethod(GetMethod<Grenade_Explode_Patch>("Prefix")));
            }

            if (ConfigManager.stalkerSurviveExplosion.value)
                harmonyTweaks.Patch(GetMethod<Stalker>("SandExplode"), prefix: new HarmonyMethod(GetMethod<Stalker_SandExplode_Patch>("Prefix")));

            if (ConfigManager.strayCoinsIgnoreWeakPointToggle.value)
                harmonyTweaks.Patch(GetMethod<ZombieProjectiles>("SpawnProjectile"), postfix: new HarmonyMethod(GetMethod<Swing>("Postfix")));
            if (ConfigManager.strayShootToggle.value)
            {
                harmonyTweaks.Patch(GetMethod<ZombieProjectiles>("Start"), postfix: new HarmonyMethod(GetMethod<ZombieProjectile_Start_Patch1>("Postfix")));
                harmonyTweaks.Patch(GetMethod<ZombieProjectiles>("ThrowProjectile"), postfix: new HarmonyMethod(GetMethod<ZombieProjectile_ThrowProjectile_Patch>("Postfix")));
                harmonyTweaks.Patch(GetMethod<ZombieProjectiles>("SwingEnd"), prefix: new HarmonyMethod(GetMethod<SwingEnd>("Prefix")));
                harmonyTweaks.Patch(GetMethod<ZombieProjectiles>("DamageEnd"), prefix: new HarmonyMethod(GetMethod<DamageEnd>("Prefix")));
            }

            if(ConfigManager.streetCleanerCoinsIgnoreWeakPointToggle.value)
                harmonyTweaks.Patch(GetMethod<Streetcleaner>("Start"), postfix: new HarmonyMethod(GetMethod<StreetCleaner_Start_Patch>("Postfix")));
            if(ConfigManager.streetCleanerPredictiveDodgeToggle.value)
                harmonyTweaks.Patch(GetMethod<BulletCheck>("OnTriggerEnter"), postfix: new HarmonyMethod(GetMethod<BulletCheck_OnTriggerEnter_Patch>("Postfix")));

            if(ConfigManager.swordsMachineNoLightKnockbackToggle.value)
                harmonyTweaks.Patch(GetMethod<SwordsMachine>("Knockdown"), prefix: new HarmonyMethod(GetMethod<SwordsMachine_Knockdown_Patch>("Prefix")));
            if (ConfigManager.swordsMachineExplosiveSwordToggle.value)
            {
                harmonyTweaks.Patch(GetMethod<ThrownSword>("Start"), postfix: new HarmonyMethod(GetMethod<ThrownSword_Start_Patch>("Postfix")));
                harmonyTweaks.Patch(GetMethod<ThrownSword>("OnTriggerEnter"), postfix: new HarmonyMethod(GetMethod<ThrownSword_OnTriggerEnter_Patch>("Postfix")));
            }

            harmonyTweaks.Patch(GetMethod<Turret>("Start"), postfix: new HarmonyMethod(GetMethod<TurretStart>("Postfix")));
            if(ConfigManager.turretBurstFireToggle.value)
            {
                harmonyTweaks.Patch(GetMethod<Turret>("Shoot"), prefix: new HarmonyMethod(GetMethod<TurretShoot>("Prefix")));
                harmonyTweaks.Patch(GetMethod<Turret>("StartAiming"), postfix: new HarmonyMethod(GetMethod<TurretAim>("Postfix")));
            }

            harmonyTweaks.Patch(GetMethod<Explosion>("Start"), postfix: new HarmonyMethod(GetMethod<V2CommonExplosion>("Postfix")));

            harmonyTweaks.Patch(GetMethod<V2>("Start"), postfix: new HarmonyMethod(GetMethod<V2FirstStart>("Postfix")));
            harmonyTweaks.Patch(GetMethod<V2>("Update"), prefix: new HarmonyMethod(GetMethod<V2FirstUpdate>("Prefix")));
            harmonyTweaks.Patch(GetMethod<V2>("ShootWeapon"), prefix: new HarmonyMethod(GetMethod<V2FirstShootWeapon>("Prefix")));

            harmonyTweaks.Patch(GetMethod<V2>("Start"), postfix: new HarmonyMethod(GetMethod<V2SecondStart>("Postfix")));
            if(ConfigManager.v2SecondStartEnraged.value)
                harmonyTweaks.Patch(GetMethod<BossHealthBar>("OnEnable"), postfix: new HarmonyMethod(GetMethod<V2SecondEnrage>("Postfix")));
            harmonyTweaks.Patch(GetMethod<V2>("Update"), prefix: new HarmonyMethod(GetMethod<V2SecondUpdate>("Prefix")));
            //harmonyTweaks.Patch(GetMethod<V2>("AltShootWeapon"), postfix: new HarmonyMethod(GetMethod<V2AltShootWeapon>("Postfix")));
            harmonyTweaks.Patch(GetMethod<V2>("SwitchWeapon"), prefix: new HarmonyMethod(GetMethod<V2SecondSwitchWeapon>("Prefix")));
            harmonyTweaks.Patch(GetMethod<V2>("ShootWeapon"), prefix: new HarmonyMethod(GetMethod<V2SecondShootWeapon>("Prefix")), postfix: new HarmonyMethod(GetMethod<V2SecondShootWeapon>("Postfix")));
            if(ConfigManager.v2SecondFastCoinToggle.value)
                harmonyTweaks.Patch(GetMethod<V2>("ThrowCoins"), prefix: new HarmonyMethod(GetMethod<V2SecondFastCoin>("Prefix")));
            harmonyTweaks.Patch(GetMethod<Cannonball>("OnTriggerEnter"), prefix: new HarmonyMethod(GetMethod<V2RocketLauncher>("CannonBallTriggerPrefix")));

            harmonyTweaks.Patch(GetMethod<Drone>("Start"), postfix: new HarmonyMethod(GetMethod<Virtue_Start_Patch>("Postfix")));
            harmonyTweaks.Patch(GetMethod<Drone>("SpawnInsignia"), prefix: new HarmonyMethod(GetMethod<Virtue_SpawnInsignia_Patch>("Prefix")));
        }

        private static void PatchAllPlayers()
        {
            if (!ConfigManager.playerTweakToggle.value)
                return;

            harmonyTweaks.Patch(GetMethod<Punch>("CheckForProjectile"), prefix: new HarmonyMethod(GetMethod<Punch_CheckForProjectile_Patch>("Prefix")));
            harmonyTweaks.Patch(GetMethod<Grenade>("Explode"), prefix: new HarmonyMethod(GetMethod<Grenade_Explode_Patch1>("Prefix")));
            harmonyTweaks.Patch(GetMethod<Grenade>("Collision"), prefix: new HarmonyMethod(GetMethod<Grenade_Collision_Patch>("Prefix")));

            if(ConfigManager.rocketGrabbingToggle.value)
                harmonyTweaks.Patch(GetMethod<HookArm>("FixedUpdate"), prefix: new HarmonyMethod(GetMethod<HookArm_FixedUpdate_Patch>("Prefix")));
        }

        public static void PatchAll()
        {
            harmonyTweaks.UnpatchSelf();

            if (!ultrapainDifficulty)
                return;

            if(realUltrapainDifficulty && ConfigManager.discordRichPresenceToggle.value)
                harmonyTweaks.Patch(GetMethod<DiscordController>("SendActivity"), prefix: new HarmonyMethod(GetMethod<DiscordController_SendActivity_Patch>("Prefix")));
            if (realUltrapainDifficulty && ConfigManager.steamRichPresenceToggle.value)
                harmonyTweaks.Patch(GetMethod<SteamFriends>("SetRichPresence"), prefix: new HarmonyMethod(GetMethod<SteamFriends_SetRichPresence_Patch>("Prefix")));


            PatchAllEnemies();
            PatchAllPlayers();
        }

        public void Awake()
        {
            instance = this;

            // Plugin startup logic 
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            harmonyTweaks = new Harmony(PLUGIN_GUID + "_tweaks");
            harmonyBase = new Harmony(PLUGIN_GUID + "_base");
            harmonyBase.Patch(GetMethod<DifficultySelectButton>("SetDifficulty"), postfix: new HarmonyMethod(GetMethod<DifficultySelectPatch>("Postfix")));
            harmonyBase.Patch(GetMethod<DifficultyTitle>("Check"), postfix: new HarmonyMethod(GetMethod<DifficultyTitle_Check_Patch>("Postfix")));
            harmonyBase.Patch(typeof(PrefsManager).GetConstructor(new Type[0]), postfix: new HarmonyMethod(GetMethod<PrefsManager_Ctor>("Postfix")));
            harmonyBase.Patch(GetMethod<PrefsManager>("EnsureValid"), prefix: new HarmonyMethod(GetMethod<PrefsManager_EnsureValid>("Prefix")));
            ConfigManager.Initialize();

            SceneManager.activeSceneChanged += OnSceneChange;
        }
    }

    // Asset destroyer tracker
    /*[HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Destroy), new Type[] { typeof(UnityEngine.Object) })]
    public class TempClass1
    {
        static void Postfix(UnityEngine.Object __0)
        {
            if (__0 != null && __0 == Plugin.homingProjectile)
            {
                System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
                Debug.LogError("Projectile destroyed");
                Debug.LogError(t.ToString());
                throw new Exception("Attempted to destroy proj");
            }
        }
    }

    [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Destroy), new Type[] { typeof(UnityEngine.Object), typeof(float) })]
    public class TempClass2
    {
        static void Postfix(UnityEngine.Object __0)
        {
            if (__0 != null && __0 == Plugin.homingProjectile)
            {
                System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
                Debug.LogError("Projectile destroyed");
                Debug.LogError(t.ToString());
                throw new Exception("Attempted to destroy proj");
            }
        }
    }

    [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.DestroyImmediate), new Type[] { typeof(UnityEngine.Object) })]
    public class TempClass3
    {
        static void Postfix(UnityEngine.Object __0)
        {
            if (__0 != null && __0 == Plugin.homingProjectile)
            {
                System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
                Debug.LogError("Projectile destroyed");
                Debug.LogError(t.ToString());
                throw new Exception("Attempted to destroy proj");
            }
        }
    }

    [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.DestroyImmediate), new Type[] { typeof(UnityEngine.Object), typeof(bool) })]
    public class TempClass4
    {
        static void Postfix(UnityEngine.Object __0)
        {
            if (__0 != null && __0 == Plugin.homingProjectile)
            {
                System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
                Debug.LogError("Projectile destroyed");
                Debug.LogError(t.ToString());
                throw new Exception("Attempted to destroy proj");
            }
        }
    }*/
}
