using BepInEx;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using HarmonyLib;
using System.IO;
using Ultrapain.Patches;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Reflection;
using Steamworks;
using Unity.Audio;
using System.Text;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace Ultrapain
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInDependency("com.eternalUnion.pluginConfigurator", "1.1.0")]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.eternalUnion.ultraPain";
        public const string PLUGIN_NAME = "Ultra Pain";
        public const string PLUGIN_VERSION = "1.0.0";

        public static Plugin instance;

        public static ResourceLocationMap resourceMap = null;
        public static T LoadObject<T>(string path)
        {
            if (resourceMap == null)
            {
                Addressables.InitializeAsync().WaitForCompletion();
                resourceMap = Addressables.ResourceLocators.First() as ResourceLocationMap;
            }

            Debug.Log($"Loading {path}");
            KeyValuePair<object, IList<IResourceLocation>> obj;

            try
            {
                obj = resourceMap.Locations.Where(
                    (KeyValuePair<object, IList<IResourceLocation>> pair) =>
                    {
                        return (pair.Key as string) == path;
                        //return (pair.Key as string).Equals(path, StringComparison.OrdinalIgnoreCase);
                    }).First();
            }
            catch (Exception) { return default(T); }
            
            return Addressables.LoadAsset<T>(obj.Value.First()).WaitForCompletion();
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
        public static GameObject lightningStrikeExplosiveSetup;
        public static GameObject lightningStrikeExplosive;
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
        public static GameObject shockwave;
        public static GameObject sisyphiusExplosion;
        public static GameObject sisyphiusPrimeExplosion;
        public static GameObject explosionWaveKnuckleblaster;

        //public static GameObject idol;
        public static GameObject ferryman;
        public static GameObject minosPrime;

        public static GameObject enrageEffect;
        public static GameObject v2flashUnparryable;

        public static AudioClip cannonBallChargeAudio;

        public static GameObject rocketLauncherAlt;
        public static GameObject maliciousRailcannon;

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

            // Assets/Prefabs/Attacks and Projectiles/Projectile Spread.prefab
            projectileSpread = LoadObject<GameObject>("Assets/Prefabs/Attacks and Projectiles/Projectile Spread.prefab");
            // Assets/Prefabs/Attacks and Projectiles/Projectile Homing.prefab
            homingProjectile = LoadObject<GameObject>("Assets/Prefabs/Attacks and Projectiles/Projectile Homing.prefab");
            // Assets/Prefabs/Attacks and Projectiles/Projectile Decorative 2.prefab
            decorativeProjectile2 = LoadObject<GameObject>("Assets/Prefabs/Attacks and Projectiles/Projectile Decorative 2.prefab");
            // Assets/Prefabs/Attacks and Projectiles/Grenade.prefab
            shotgunGrenade = LoadObject<GameObject>("Assets/Prefabs/Attacks and Projectiles/Grenade.prefab");
            // Assets/Prefabs/Attacks and Projectiles/Hitscan Beams/Turret Beam.prefab
            turretBeam = LoadObject<GameObject>("Assets/Prefabs/Attacks and Projectiles/Hitscan Beams/Turret Beam.prefab");
            // Assets/Prefabs/Attacks and Projectiles/Hitscan Beams/Malicious Beam.prefab
            beam = LoadObject<GameObject>("Assets/Prefabs/Attacks and Projectiles/Hitscan Beams/Malicious Beam.prefab");
            // Assets/Prefabs/Attacks and Projectiles/Explosions/Lightning Strike Explosive.prefab
            lightningStrikeExplosiveSetup = LoadObject<GameObject>("Assets/Prefabs/Attacks and Projectiles/Explosions/Lightning Strike Explosive.prefab");
            // Assets/Particles/Environment/LightningBoltWindupFollow Variant.prefab
            lighningStrikeWindup = LoadObject<GameObject>("Assets/Particles/Environment/LightningBoltWindupFollow Variant.prefab");
            //[bundle-0][assets/prefabs/enemies/idol.prefab]
            //idol = LoadObject<GameObject>("assets/prefabs/enemies/idol.prefab");
            // Assets/Prefabs/Enemies/Ferryman.prefab
            ferryman = LoadObject<GameObject>("Assets/Prefabs/Enemies/Ferryman.prefab");
            // Assets/Prefabs/Attacks and Projectiles/Explosions/Explosion.prefab
            explosion = LoadObject<GameObject>("Assets/Prefabs/Attacks and Projectiles/Explosions/Explosion.prefab");
            // Assets/Prefabs/Attacks and Projectiles/Virtue Insignia.prefab
            virtueInsignia = LoadObject<GameObject>("Assets/Prefabs/Attacks and Projectiles/Virtue Insignia.prefab");
            // Assets/Prefabs/Attacks and Projectiles/Projectile Explosive HH.prefab
            hideousMassProjectile = LoadObject<GameObject>("Assets/Prefabs/Attacks and Projectiles/Projectile Explosive HH.prefab");
            // Assets/Particles/Enemies/RageEffect.prefab
            enrageEffect = LoadObject<GameObject>("Assets/Particles/Enemies/RageEffect.prefab");
            // Assets/Particles/Flashes/V2FlashUnparriable.prefab
            v2flashUnparryable = LoadObject<GameObject>("Assets/Particles/Flashes/V2FlashUnparriable.prefab");
            // Assets/Prefabs/Attacks and Projectiles/Rocket.prefab
            rocket = LoadObject<GameObject>("Assets/Prefabs/Attacks and Projectiles/Rocket.prefab");
            // Assets/Prefabs/Attacks and Projectiles/RevolverBullet.prefab
            revolverBullet = LoadObject<GameObject>("assets/prefabs/revolverbullet.prefab");
            // Assets/Prefabs/Attacks and Projectiles/Hitscan Beams/Railcannon Beam Malicious.prefab
            maliciousCannonBeam = LoadObject<GameObject>("Assets/Prefabs/Attacks and Projectiles/Hitscan Beams/Railcannon Beam Malicious.prefab");
            // Assets/Prefabs/Attacks and Projectiles/Hitscan Beams/Revolver Beam.prefab
            revolverBeam = LoadObject<GameObject>("Assets/Prefabs/Attacks and Projectiles/Hitscan Beams/Revolver Beam.prefab");
            // Assets/Prefabs/Attacks and Projectiles/Explosions/Explosion Wave Enemy.prefab
            blastwave = LoadObject<GameObject>("Assets/Prefabs/Attacks and Projectiles/Explosions/Explosion Wave Enemy.prefab");
            // Assets/Prefabs/Enemies/MinosPrime.prefab
            minosPrime = LoadObject<GameObject>("Assets/Prefabs/Enemies/MinosPrime.prefab");
            // Assets/Prefabs/Attacks and Projectiles/Cannonball.prefab
            cannonBall = LoadObject<GameObject>("Assets/Prefabs/Attacks and Projectiles/Cannonball.prefab");
            // get from Assets/Prefabs/Weapons/Rocket Launcher Cannonball.prefab
            cannonBallChargeAudio = LoadObject<GameObject>("Assets/Prefabs/Weapons/Rocket Launcher Cannonball.prefab").transform.Find("RocketLauncher/Armature/Body_Bone/HologramDisplay").GetComponent<AudioSource>().clip;
            // Assets/Prefabs/Attacks and Projectiles/PhysicalShockwave.prefab
            shockwave = LoadObject<GameObject>("Assets/Prefabs/Attacks and Projectiles/PhysicalShockwave.prefab");
            // Assets/Prefabs/Attacks and Projectiles/Explosions/Explosion Wave Sisyphus.prefab
            sisyphiusExplosion = LoadObject<GameObject>("Assets/Prefabs/Attacks and Projectiles/Explosions/Explosion Wave Sisyphus.prefab");
            // Assets/Prefabs/Attacks and Projectiles/Explosions/Explosion Sisyphus Prime.prefab
            sisyphiusPrimeExplosion = LoadObject<GameObject>("Assets/Prefabs/Attacks and Projectiles/Explosions/Explosion Sisyphus Prime.prefab");
            // Assets/Prefabs/Attacks and Projectiles/Explosions/Explosion Wave.prefab
            explosionWaveKnuckleblaster = LoadObject<GameObject>("Assets/Prefabs/Attacks and Projectiles/Explosions/Explosion Wave.prefab");
            // Assets/Prefabs/Attacks and Projectiles/Explosions/Explosion Lightning.prefab - [bundle-0][assets/prefabs/explosionlightning variant.prefab]
            lightningStrikeExplosive = LoadObject<GameObject>("Assets/Prefabs/Attacks and Projectiles/Explosions/Explosion Lightning.prefab");
            // Assets/Prefabs/Weapons/Rocket Launcher Cannonball.prefab
            rocketLauncherAlt = LoadObject<GameObject>("Assets/Prefabs/Weapons/Rocket Launcher Cannonball.prefab");
            // Assets/Prefabs/Weapons/Railcannon Malicious.prefab
            maliciousRailcannon = LoadObject<GameObject>("Assets/Prefabs/Weapons/Railcannon Malicious.prefab");

            // hideousMassProjectile.AddComponent<HideousMassProjectile>();
        }

        public static bool ultrapainDifficulty = false;
        public static bool realUltrapainDifficulty = false;
        public static GameObject currentDifficultyButton;
        public static GameObject currentDifficultyPanel;
        public void OnSceneChange(Scene before, Scene after)
        {
            StyleIDs.RegisterIDs();
            ScenePatchCheck();

            string mainMenuSceneName = "b3e7f2f8052488a45b35549efb98d902";
            if (SceneManager.GetActiveScene().name == mainMenuSceneName)
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
                    
                    <color=orange>This difficulty uses UKMD difficulty and slot. To use the mod on another difficulty, enable global difficulty from settings.</color>
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

            // LOAD CUSTOM PREFABS HERE TO AVOID MID GAME LAG
            MinosPrimeCharge.CreateDecoy();
            GameObject shockwaveSisyphus = SisyphusInstructionist_Start.shockwave;
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

                MonoSingleton<StyleHUD>.Instance.RegisterStyleItem(ConfigManager.orbStrikeRevolverStyleText.guid, ConfigManager.orbStrikeRevolverStyleText.value);
                MonoSingleton<StyleHUD>.Instance.RegisterStyleItem(ConfigManager.orbStrikeRevolverChargedStyleText.guid, ConfigManager.orbStrikeRevolverChargedStyleText.value);
                MonoSingleton<StyleHUD>.Instance.RegisterStyleItem(ConfigManager.orbStrikeElectricCannonStyleText.guid, ConfigManager.orbStrikeElectricCannonStyleText.value);
                MonoSingleton<StyleHUD>.Instance.RegisterStyleItem(ConfigManager.orbStrikeMaliciousCannonStyleText.guid, ConfigManager.orbStrikeMaliciousCannonStyleText.value);

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

            if(ConfigManager.sisyInstJumpShockwave.value)
                harmonyTweaks.Patch(GetMethod<Sisyphus>("Start"), postfix: new HarmonyMethod(GetMethod<SisyphusInstructionist_Start>("Postfix")));
            if(ConfigManager.sisyInstBoulderShockwave.value)
                harmonyTweaks.Patch(GetMethod<Sisyphus>("SetupExplosion"), postfix: new HarmonyMethod(GetMethod<SisyphusInstructionist_SetupExplosion>("Postfix")));
            if(ConfigManager.sisyInstStrongerExplosion.value)
                harmonyTweaks.Patch(GetMethod<Sisyphus>("StompExplosion"), prefix: new HarmonyMethod(GetMethod<SisyphusInstructionist_StompExplosion>("Prefix")));

            // ADDME
            harmonyTweaks.Patch(GetMethod<Mandalore>("FullBurst"), postfix: new HarmonyMethod(GetMethod<DruidKnight_FullBurst>("Postfix")), prefix: new HarmonyMethod(GetMethod<DruidKnight_FullBurst>("Prefix")));
            harmonyTweaks.Patch(GetMethod<Mandalore>("FullerBurst"), prefix: new HarmonyMethod(GetMethod<DruidKnight_FullerBurst>("Prefix")));
            harmonyTweaks.Patch(GetMethod<Drone>("Explode"), prefix: new HarmonyMethod(GetMethod<Drone_Explode>("Prefix")));
        }

        private static void PatchAllPlayers()
        {
            if (!ConfigManager.playerTweakToggle.value)
                return;

            harmonyTweaks.Patch(GetMethod<Punch>("CheckForProjectile"), prefix: new HarmonyMethod(GetMethod<Punch_CheckForProjectile_Patch>("Prefix")));
            harmonyTweaks.Patch(GetMethod<Grenade>("Explode"), prefix: new HarmonyMethod(GetMethod<Grenade_Explode_Patch1>("Prefix")));
            harmonyTweaks.Patch(GetMethod<Grenade>("Collision"), prefix: new HarmonyMethod(GetMethod<Grenade_Collision_Patch>("Prefix")));
            if (ConfigManager.rocketBoostToggle.value)
                harmonyTweaks.Patch(GetMethod<Explosion>("Collide"), prefix: new HarmonyMethod(GetMethod<Explosion_Collide_Patch>("Prefix")));

            if (ConfigManager.rocketGrabbingToggle.value)
                harmonyTweaks.Patch(GetMethod<HookArm>("FixedUpdate"), prefix: new HarmonyMethod(GetMethod<HookArm_FixedUpdate_Patch>("Prefix")));

            if (ConfigManager.orbStrikeToggle.value)
            {
                harmonyTweaks.Patch(GetMethod<Punch>("BlastCheck"), prefix: new HarmonyMethod(GetMethod<Punch_BlastCheck>("Prefix")), postfix: new HarmonyMethod(GetMethod<Punch_BlastCheck>("Postfix")));
                harmonyTweaks.Patch(GetMethod<Explosion>("Collide"), prefix: new HarmonyMethod(GetMethod<Explosion_Collide>("Prefix")));
                harmonyTweaks.Patch(GetMethod<Coin>("DelayedReflectRevolver"), postfix: new HarmonyMethod(GetMethod<Coin_DelayedReflectRevolver>("Postfix")));
                harmonyTweaks.Patch(GetMethod<Coin>("ReflectRevolver"), postfix: new HarmonyMethod(GetMethod<Coin_ReflectRevolver>("Postfix")), prefix: new HarmonyMethod(GetMethod<Coin_ReflectRevolver>("Prefix")));
                harmonyTweaks.Patch(GetMethod<Grenade>("Explode"), prefix: new HarmonyMethod(GetMethod<Grenade_Explode>("Prefix")), postfix: new HarmonyMethod(GetMethod<Grenade_Explode>("Postfix")));
                
                harmonyTweaks.Patch(GetMethod<EnemyIdentifier>("DeliverDamage"), prefix: new HarmonyMethod(GetMethod<EnemyIdentifier_DeliverDamage>("Prefix")), postfix: new HarmonyMethod(GetMethod<EnemyIdentifier_DeliverDamage>("Postfix")));
                harmonyTweaks.Patch(GetMethod<RevolverBeam>("ExecuteHits"), postfix: new HarmonyMethod(GetMethod<RevolverBeam_ExecuteHits>("Postfix")), prefix: new HarmonyMethod(GetMethod<RevolverBeam_ExecuteHits>("Prefix")));
                harmonyTweaks.Patch(GetMethod<RevolverBeam>("HitSomething"), postfix: new HarmonyMethod(GetMethod<RevolverBeam_HitSomething>("Postfix")), prefix: new HarmonyMethod(GetMethod<RevolverBeam_HitSomething>("Prefix")));
                harmonyTweaks.Patch(GetMethod<RevolverBeam>("Start"), prefix: new HarmonyMethod(GetMethod<RevolverBeam_Start>("Prefix")));
                harmonyTweaks.Patch(GetMethod<Cannonball>("Explode"), prefix: new HarmonyMethod(GetMethod<Cannonball_Explode>("Prefix")));

                harmonyTweaks.Patch(GetMethod<Explosion>("Collide"), prefix: new HarmonyMethod(GetMethod<Explosion_CollideOrbital>("Prefix")));
            }
        }

        private static void PatchAllMemes()
        {
            if (ConfigManager.enrageSfxToggle.value)
                harmonyTweaks.Patch(GetMethod<EnrageEffect>("Start"), postfix: new HarmonyMethod(GetMethod<EnrageEffect_Start>("Postfix")));
        }

        public static bool methodsPatched = false;
        
        public static void ScenePatchCheck()
        {
            if(methodsPatched && !ultrapainDifficulty)
            {
                harmonyTweaks.UnpatchSelf();
                methodsPatched = false;
            }
            else if(!methodsPatched && ultrapainDifficulty)
            {
                PatchAll();
            }
        }
        
        public static void PatchAll()
        {
            harmonyTweaks.UnpatchSelf();
            methodsPatched = false;

            if (!ultrapainDifficulty)
                return;

            if(realUltrapainDifficulty && ConfigManager.discordRichPresenceToggle.value)
                harmonyTweaks.Patch(GetMethod<DiscordController>("SendActivity"), prefix: new HarmonyMethod(GetMethod<DiscordController_SendActivity_Patch>("Prefix")));
            if (realUltrapainDifficulty && ConfigManager.steamRichPresenceToggle.value)
                harmonyTweaks.Patch(GetMethod<SteamFriends>("SetRichPresence"), prefix: new HarmonyMethod(GetMethod<SteamFriends_SetRichPresence_Patch>("Prefix")));

            PatchAllEnemies();
            PatchAllPlayers();
            PatchAllMemes();
            methodsPatched = true;
        }

        public static string workingPath;
        public static string workingDir;

        public static AssetBundle bundle;
        public static AudioClip druidKnightFullAutoAud;
        public static AudioClip druidKnightFullerAutoAud;
        public static AudioClip druidKnightDeathAud;
        public static AudioClip enrageAudioCustom;

        public void Awake()
        {
            instance = this;
            workingPath = Assembly.GetExecutingAssembly().Location;
            workingDir = Path.GetDirectoryName(workingPath);

            Logger.LogInfo($"Working path: {workingPath}, Working dir: {workingDir}");
            try
            {
                bundle = AssetBundle.LoadFromFile(Path.Combine(workingDir, "ultrapain"));
                druidKnightFullAutoAud = bundle.LoadAsset<AudioClip>("assets/ultrapain/druidknight/fullauto.wav");
                druidKnightFullerAutoAud = bundle.LoadAsset<AudioClip>("assets/ultrapain/druidknight/fullerauto.wav");
                druidKnightDeathAud = bundle.LoadAsset<AudioClip>("assets/ultrapain/druidknight/death.wav");
                enrageAudioCustom = bundle.LoadAsset<AudioClip>("assets/ultrapain/sfx/enraged.wav");
            }
            catch (Exception e)
            {
                Logger.LogError($"Could not load the asset bundle:\n{e}");
            }

            // DEBUG
            /*string logPath = Path.Combine(Environment.CurrentDirectory, "log.txt");
            Logger.LogInfo($"Saving to {logPath}");
            List<string> assetPaths = new List<string>()
            {
                "fonts.bundle",
                "videos.bundle",
                "shaders.bundle",
                "particles.bundle",
                "materials.bundle",
                "animations.bundle",
                "prefabs.bundle",
                "physicsmaterials.bundle",
                "models.bundle",
                "textures.bundle",
            };

            //using (FileStream log = File.Open(logPath, FileMode.OpenOrCreate, FileAccess.Write))
            //{
                foreach(string assetPath in assetPaths)
                {
                    Logger.LogInfo($"Attempting to load {assetPath}");
                    AssetBundle bundle = AssetBundle.LoadFromFile(Path.Combine(bundlePath, assetPath));
                    bundles.Add(bundle);
                    //foreach (string name in bundle.GetAllAssetNames())
                    //{
                    //    string line = $"[{bundle.name}][{name}]\n";
                    //    log.Write(Encoding.ASCII.GetBytes(line), 0, line.Length);
                    //}
                    bundle.LoadAllAssets();
                }
            //}
            */

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
