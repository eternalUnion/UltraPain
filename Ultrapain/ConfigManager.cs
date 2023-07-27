using PluginConfig.API;
using PluginConfig.API.Fields;
using PluginConfig.API.Decorators;
using UnityEngine.UI;
using Ultrapain.Patches;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System;
using BepInEx.Configuration;
using System.IO;
using PluginConfig.API.Functionals;
using System.Reflection;
using System.Text;
using static Ultrapain.ConfigManager;

namespace Ultrapain
{
    // Adds an image next to the given input field
    public class ImageInputField : CustomConfigField
    {
        public Sprite sprite;
        public Color color;
        public ConfigField field;
        private GameObject currentUI;
        private Image currentImage;

        private static FieldInfo f_IntField_currentUi = typeof(IntField).GetField("currentUi", UnityUtils.instanceFlag);
        private static FieldInfo f_FloatField_currentUi = typeof(FloatField).GetField("currentUi", UnityUtils.instanceFlag);
        private static FieldInfo f_StringField_currentUi = typeof(StringField).GetField("currentUi", UnityUtils.instanceFlag);

        private const float textAnchorX = 40f;
        private const float fieldAnchorX = 230f;
        private const float fieldAnchorY = -30f;
        private const float fieldSizeX = 270f;

        public ImageInputField(ConfigField field, Sprite sprite, Color color) : base(field.parentPanel, 0, 0)
        {
            this.field = field;
            this.sprite = sprite;
            this.color = color;

            if (currentImage != null)
            {
                currentImage.sprite = sprite;
                currentImage.color = color;

                SetSize();
            }
        }

        private void SetSize()
        {
            RectTransform imgRect = currentImage.GetComponent<RectTransform>();

            float horizontalLength = sprite == null ? 100f : sprite.rect.width * (40f / sprite.rect.height);
            imgRect.sizeDelta = new Vector2(horizontalLength, 40f);
            imgRect.anchoredPosition = new Vector2(10f, 0f);

            Text txt = currentUI.GetComponentInChildren<Text>();
            InputField input = currentUI.GetComponentInChildren<InputField>();
            RectTransform inputRect = input.GetComponent<RectTransform>();

            float deltaPos = horizontalLength - 20f;
            if (deltaPos > 0)
            {
                txt.GetComponent<RectTransform>().anchoredPosition = new Vector2(textAnchorX + deltaPos, 0);
                inputRect.anchoredPosition = new Vector2(fieldAnchorX + deltaPos, fieldAnchorY);
                inputRect.sizeDelta = new Vector2(fieldSizeX - deltaPos, inputRect.sizeDelta.y);
            }
            else
            {
                txt.GetComponent<RectTransform>().anchoredPosition = new Vector2(textAnchorX, 0);
                inputRect.anchoredPosition = new Vector2(fieldAnchorX, fieldAnchorY);
                inputRect.sizeDelta = new Vector2(fieldSizeX, inputRect.sizeDelta.y);
            }
        }

        protected override void OnCreateUI(RectTransform fieldUI)
        {
            GameObject.Destroy(fieldUI.gameObject);

            GameObject ui = null;
            if (field is IntField intField)
                ui = (GameObject)f_IntField_currentUi.GetValue(intField);
            else if (field is FloatField floatField)
                ui = (GameObject)f_FloatField_currentUi.GetValue(floatField);
            else if (field is StringField stringField)
                ui = (GameObject)f_StringField_currentUi.GetValue(stringField);
            else
                throw new Exception("Image field expected given field to be int, float or string field");

            currentUI = ui;

            if (ui == null)
            {
                Debug.LogWarning("Could not find float field ui");
                return;
            }

            GameObject img = new GameObject();
            RectTransform imgRect = img.AddComponent<RectTransform>();
            imgRect.SetParent(ui.transform);
            imgRect.localScale = Vector3.one;
            imgRect.pivot = new Vector2(0f, 0.5f);
            imgRect.anchorMax = imgRect.anchorMin = new Vector2(0f, 0.5f);
            
            Image imgComp = currentImage = img.AddComponent<Image>();
            imgComp.sprite = sprite;
            imgComp.color = color;

            SetSize();
        }
    }

    // Separates fields by a small space
    public class SpaceField : CustomConfigField
    {
        public const float defaultSpace = 10f;

        public float space = defaultSpace;
        public RectTransform currentUI;

        public SpaceField(ConfigPanel panel, float space) : base(panel)
        {
            this.space = space;

            if (currentUI != null)
                currentUI.sizeDelta = new Vector2(currentUI.sizeDelta.x, space);
        }

        public SpaceField(ConfigPanel panel) : this(panel, defaultSpace) { }

        protected override void OnCreateUI(RectTransform fieldUI)
        {
            currentUI = fieldUI;

            fieldUI.sizeDelta = new Vector2(fieldUI.sizeDelta.x, space);
        }
    }

    public static class ConfigManager
    {
        public static PluginConfigurator config = null;

        public static void AddMissingPresets()
        {
            string presetFolder = Path.Combine(Plugin.workingDir, "defaultpresets");
            if (!Directory.Exists(presetFolder))
            {
                Debug.LogWarning("UltraPain misses the default preset folder at " + presetFolder);
                return;
            }

            foreach (string filePath in Directory.GetFiles(presetFolder))
            {
                if (!filePath.EndsWith(".config"))
                {
                    Debug.LogWarning($"Incorrect file format at {filePath}. Extension must be .config");
                    continue;
                }

                string fileName = Path.GetFileName(filePath);
                fileName = fileName.Substring(0, fileName.Length - 7);
                if (string.IsNullOrWhiteSpace(fileName))
                    continue;

                config.TryAddPreset(fileName, fileName, filePath);
            }
        }

        // ROOT PANEL
        public static BoolField enemyTweakToggle;
        private static ConfigPanel enemyPanel;
        public static BoolField playerTweakToggle;
        private static ConfigPanel playerPanel;
        public static BoolField discordRichPresenceToggle;
        public static BoolField steamRichPresenceToggle;
        public static StringField pluginName;
        public static StringMultilineField pluginInfo;
        public static BoolField globalDifficultySwitch;
        public static ConfigPanel memePanel;

        // MEME PANEL
        public static BoolField enrageSfxToggle;
        public static BoolField funnyDruidKnightSFXToggle;
        public static BoolField fleshObamiumToggle;
        public static StringField fleshObamiumName;
        public static BoolField obamapticonToggle;
        public static StringField obamapticonName;

        // PLAYER PANEL
        
        // PLAYER STAT EDITOR
        // MOVEMENT
        public static ConfigPanel playerStatEditorPanel;
        public static FloatField staminaRegSpeedMulti;

        // REVOLVER
        public static FloatField revolverDamage;
        public static FloatField revolverAltDamage;
        public static FloatField revolverFireRate;
        public static FloatField revolverAltFireRate;

        public static FloatField coinRegSpeedMulti;

        public static FloatField chargedRevRegSpeedMulti;
        public static FloatField chargedRevDamage;
        public static FloatField chargedAltRevDamage;
        public static IntField chargedRevTotalHits;
        public static IntField chargedAltRevTotalHits;
        public static IntField chargedRevMaxHitsPerTarget;
        public static IntField chargedAltRevMaxHitsPerTarget;

        public static FloatField sharpshooterRegSpeedMulti;
        public static FloatField sharpshooterDamage;
        public static FloatField sharpshooterAltDamage;
        public static IntField sharpshooterMaxHitsPerTarget;
        public static IntField sharpshooterAltMaxHitsPerTarget;

        // SHOTGUN
        public static IntField shotgunBluePelletCount;
        public static FloatField shotgunBlueDamagePerPellet;
        public static FloatField shotgunBlueSpreadAngle;

        public static FloatField shotgunCoreExplosionSize;
        public static FloatField shotgunCoreExplosionSpeed;
        public static IntField shotgunCoreExplosionDamage;
        public static IntField shotgunCoreExplosionPlayerDamage;

        public static IntField shotgunGreenPump1Count;
        public static FloatField shotgunGreenPump1Damage;
        public static FloatField shotgunGreenPump1Spread;
        public static IntField shotgunGreenPump2Count;
        public static FloatField shotgunGreenPump2Damage;
        public static FloatField shotgunGreenPump2Spread;
        public static IntField shotgunGreenPump3Count;
        public static FloatField shotgunGreenPump3Damage;
        public static FloatField shotgunGreenPump3Spread;

        public static FloatField shotgunGreenExplosionSize;
        public static FloatField shotgunGreenExplosionSpeed;
        public static IntField shotgunGreenExplosionDamage;
        public static IntField shotgunGreenExplosionPlayerDamage;

        // NAILGUN/SAW LAUNCHER
        public static FloatField nailgunBlueDamage;
        public static FloatField nailgunGreenDamage;
        public static FloatField nailgunGreenBurningDamage;

        public static FloatField sawBlueDamage;
        public static FloatField sawBlueHitAmount;
        public static FloatField sawGreenDamage;
        public static FloatField sawGreenHitAmount;
        public static FloatField sawGreenBurningDamage;
        public static FloatField sawGreenBurningHitAmount;

        public static FloatField nailgunAmmoRegSpeedMulti;
        public static FloatField sawAmmoRegSpeedMulti;
        public static FloatField nailgunHeatsinkRegSpeedMulti;
        public static FloatField sawHeatsinkRegSpeedMulti;

        // RAILCANNON
        public static FloatField railcannonRegSpeedMulti;
        public static FloatField electricCannonDamageMulti;
        public static FloatField malCannonExpSizeMulti;
        public static FloatField malCannonExpDamageMulti;
        public static FloatField screwdriverDamageMulti;
        public static FloatField screwdriverLengthMulti;

        // ROCKET LAUNCHER
        public static FloatField rocketExpSizeMulti;
        public static FloatField rocketExpDamageMulti;
        public static FloatField rocketExpSuperSizeMulti;
        public static FloatField rocketExpSuperDamageMulti;

        public static FloatField rocketFreezeRegSpeedMulti;
        public static FloatField rocketCannonballRegSpeedMulti;

        // PLAYER HEALTH
        public static IntField maxPlayerHp;
        public static IntField playerHpSupercharge;
        public static FloatSliderField hardDamagePercent;
        public static IntField whiplashHardDamageCap;
        public static FloatField whiplashHardDamageSpeed;
        public static BoolField playerHpDeltaToggle;
        public static IntField playerHpDeltaAmount;
        public static IntField playerHpDeltaLimit;
        public static FloatField playerHpDeltaDelay;
        public static BoolField playerHpDeltaHurtAudio;
        public static BoolField playerHpDeltaCalm;
        public static BoolField playerHpDeltaCombat;
        public static BoolField playerHpDeltaCybergrind;
        public static BoolField playerHpDeltaSandbox;
      
        public static BoolField rocketBoostToggle;
        public static BoolField rocketBoostAlwaysExplodesToggle;
        public static FloatField rocketBoostDamageMultiplierPerHit;
        public static FloatField rocketBoostSizeMultiplierPerHit;
        public static FloatField rocketBoostSpeedMultiplierPerHit;
        public static FormattedStringField rocketBoostStyleText;
        public static IntField rocketBoostStylePoints;

        public static BoolField rocketGrabbingToggle;

        public static BoolField grenadeBoostToggle;
        public static FloatField grenadeBoostDamageMultiplier;
        public static FloatField grenadeBoostSizeMultiplier;
        public static FormattedStringField grenadeBoostStyleText;
        public static IntField grenadeBoostStylePoints;

        public static BoolField orbStrikeToggle;
        public static FloatField orbStrikeMinDistance;

        // REVOLVER BEAM ORBITAL
        public static FormattedStringField orbStrikeRevolverStyleText;
        public static IntField orbStrikeRevolverStylePoint;
        public static BoolField orbStrikeRevolverGrenade;
        public static FloatField orbStrikeRevolverGrenadeExtraSize;
        public static FloatField orbStrikeRevolverGrenadeExtraDamage;
        public static BoolField orbStrikeRevolverExplosion;
        public static FloatField orbStrikeRevolverExplosionDamage;
        public static FloatField orbStrikeRevolverExplosionSize;

        // CHARGED BEAM ORBITAL
        public static FormattedStringField orbStrikeRevolverChargedStyleText;
        public static IntField orbStrikeRevolverChargedStylePoint;
        public static BoolField orbStrikeRevolverChargedGrenade;
        public static FloatField orbStrikeRevolverChargedGrenadeExtraSize;
        public static FloatField orbStrikeRevolverChargedGrenadeExtraDamage;
        public static BoolField orbStrikeRevolverChargedInsignia;
        public static FloatField orbStrikeRevolverChargedInsigniaDelayBoost;
        public static IntField orbStrikeRevolverChargedInsigniaDamage;
        public static FloatField orbStrikeRevolverChargedInsigniaSize;

        // ELECTRIC RAIL CANNON
        public static FormattedStringField orbStrikeElectricCannonStyleText;
        public static IntField orbStrikeElectricCannonStylePoint;
        public static BoolField orbStrikeElectricCannonGrenade;
        public static FloatField orbStrikeElectricCannonGrenadeExtraSize;
        public static FloatField orbStrikeElectricCannonGrenadeExtraDamage;
        public static BoolField orbStrikeElectricCannonExplosion;
        public static FloatField orbStrikeElectricCannonExplosionDamage;
        public static FloatField orbStrikeElectricCannonExplosionSize;

        // MALICIOUS CANNON
        public static FormattedStringField orbStrikeMaliciousCannonStyleText;
        public static IntField orbStrikeMaliciousCannonStylePoint;
        public static BoolField orbStrikeMaliciousCannonGrenade;
        public static FloatField orbStrikeMaliciousCannonGrenadeExtraSize;
        public static FloatField orbStrikeMaliciousCannonGrenadeExtraDamage;
        public static BoolField orbStrikeMaliciousCannonExplosion;
        public static FloatField orbStrikeMaliciousCannonExplosionDamageMultiplier;
        public static FloatField orbStrikeMaliciousCannonExplosionSizeMultiplier;
        
        // MALICIOUS CHARGEBACK
        public static FormattedStringField maliciousChargebackStyleText;
        public static IntField maliciousChargebackStylePoint;
        public static BoolField maliciousChargebackExplosionToggle;
        public static FloatField maliciousChargebackExplosionSizeMultiplier;
        public static FloatField maliciousChargebackExplosionDamageMultiplier;

        // SENTRY CHARGEBACK
        public static FormattedStringField sentryChargebackStyleText;
        public static IntField sentryChargebackStylePoint;
        public static IntField sentryChargebackExtraBeamCount;

        // ENEMY PANEL
        public static ConfigPanel globalEnemyPanel;
        public static ConfigPanel cerberusPanel;
        public static ConfigPanel dronePanel;
        public static ConfigPanel filthPanel;
        public static ConfigPanel hideousMassPanel;
        public static ConfigPanel maliciousFacePanel;
        public static ConfigPanel mindflayerPanel;
        public static ConfigPanel schismPanel;
        public static ConfigPanel soliderPanel;
        public static ConfigPanel stalkerPanel;
        public static ConfigPanel strayPanel;
        public static ConfigPanel streetCleanerPanel;
        public static ConfigPanel swordsMachinePanel;
        public static ConfigPanel virtuePanel;
        public static ConfigPanel ferrymanPanel;
        public static ConfigPanel turretPanel;
        public static ConfigPanel fleshPrisonPanel;
        public static ConfigPanel minosPrimePanel;
        public static ConfigPanel v2FirstPanel;
        public static ConfigPanel v2SecondPanel;
        public static ConfigPanel sisyInstPanel;
        public static ConfigPanel leviathanPanel;
        public static ConfigPanel panopticonPanel;
        public static ConfigPanel idolPanel;

        // GLOBAL ENEMY CONFIG
        public static BoolField friendlyFireDamageOverrideToggle;
        public static FloatSliderField friendlyFireDamageOverrideExplosion;
        public static FloatSliderField friendlyFireDamageOverrideProjectile;
        public static FloatSliderField friendlyFireDamageOverrideFire;
        public static FloatSliderField friendlyFireDamageOverrideMelee;

        // ENEMY STAT CONFIG
        public static char resistanceSeparator = (char)1;
        public struct EidStatContainer
        {
            public FloatField health;
            public FloatField damage;
            public FloatField speed;
            public StringField resistanceStr;
            public Dictionary<string, float> resistanceDict;

            public void SetHidden(bool hidden)
            {
                health.hidden = damage.hidden = speed.hidden = hidden;
            }
        }
        
        public static Dictionary<EnemyType, float> defaultEnemyHealth = new Dictionary<EnemyType, float>()
        {
            { EnemyType.MinosPrime, 2f }
        };
        public static Dictionary<EnemyType, float> defaultEnemySpeed = new Dictionary<EnemyType, float>()
        {
            { EnemyType.MinosPrime, 1.2f }
        };
        public static Dictionary<EnemyType, float> defaultEnemyDamage = new Dictionary<EnemyType, float>()
        {

        };
        
        public enum HitterType
        {
            revolver,
            coin,
            shotgun,
            shotgunzone,
            nail,
            harpoon,
            sawblade,
            railcannon,
            drill,
            cannonball,
            punch,
            heavypunch,
            hook,
            ground_slam,

            explosion,
            fire,
            acid,
            environment,
            projectile
        }
        public static Dictionary<HitterType, string> hitterDisplayNames = new Dictionary<HitterType, string>()
        {
            { HitterType.revolver, "Revolver" },
            { HitterType.coin, "Fistful of dollar" },
            { HitterType.shotgun, "Shotgun pellet" },
            { HitterType.shotgunzone, "Shotgun close" },
            { HitterType.nail, "Nail" },
            { HitterType.harpoon, "Magnet" },
            { HitterType.sawblade, "Sawblade" },
            { HitterType.railcannon, "Electric railcannon" },
            { HitterType.drill, "Drill" },
            { HitterType.cannonball, "Cannonball" },
            { HitterType.punch, "Feedbacker" },
            { HitterType.heavypunch, "Knuckleblaster" },
            { HitterType.hook, "Whiplash hook" },
            { HitterType.ground_slam, "Ground slam" },
            { HitterType.explosion, "Explosion" },
            { HitterType.fire, "Fire" },
            { HitterType.acid, "Acid" },
            { HitterType.environment, "Environment" },
            { HitterType.projectile, "Projectile" },
        };

        public static ConfigPanel eidStatEditorPanel;
        public static EnumField<EnemyType> eidStatEditorSelector;
        public static Dictionary<EnemyType, EidStatContainer> enemyStats = new Dictionary<EnemyType, EidStatContainer>();
        public static EnumField<HitterType> eidResistanceSelector;
        public static BoolField eidResistanceEnabled;
        public static FloatField eidResistanceField;
        public static ConfigHeader eidResistanceInfo;

        // SCREWDRIVER
        public static BoolField screwDriverSplitToggle;
        public static IntField screwDriverCoinSplitCount;
        public static IntField screwDriverGrenadeSplitCount;

        public static BoolField screwDriverHomeToggle;
        public static BoolField screwDriverHomeDestroyMagnets;
        public static FloatField screwDriverHomePierceDamage;

        // CERBERUS
        public static BoolField cerberusDashToggle;
        public static IntField cerberusTotalDashCount;
        public static BoolField cerberusParryable;
        public static FloatField cerberusParryableDuration;
        public static IntField cerberusParryDamage;

        // DRONE
        public static BoolField droneProjectileToggle;
        public static FloatField droneProjectileDelay;
        public static FloatSliderField droneProjectileChance;
        public static BoolField droneExplosionBeamToggle;
        public static FloatField droneExplosionBeamDelay;
        public static FloatSliderField droneExplosionBeamChance;
        public static BoolField droneSentryBeamToggle;
        public static FloatField droneSentryBeamDelay;
        public static FloatSliderField droneSentryBeamChance;
        public static FloatField droneSentryBeamDamage;
        public static BoolField droneDrawSentryBeamLine;
        public static ColorField droneSentryBeamLineNormalColor;
        public static ColorField droneSentryBeamLineWarningColor;
        public static FloatField droneSentryBeamLineIndicatorDelay;
        public static BoolField droneHomeToggle;
        public static FloatField droneHomeTurnSpeed;

        // FILTH
        public static BoolField filthExplodeToggle;
        public static BoolField filthExplodeKills;
        public static IntField filthExplosionDamage;
        public static FloatField filthExplosionSize;

        // HIDEOUS MASS
        public static BoolField hideousMassInsigniaToggle;
        public static FloatField hideousMassInsigniaSpeed;
        public static BoolField hideousMassInsigniaYtoggle;
        public static FloatField hideousMassInsigniaYsize;
        public static IntField hideousMassInsigniaYdamage;
        public static BoolField hideousMassInsigniaZtoggle;
        public static FloatField hideousMassInsigniaZsize;
        public static IntField hideousMassInsigniaZdamage;
        public static BoolField hideousMassInsigniaXtoggle;
        public static FloatField hideousMassInsigniaXsize;
        public static IntField hideousMassInsigniaXdamage;

        // VIRTUE
        public enum VirtueAttackType
        {
            Insignia,
            LighningBolt
        }
        public static BoolField virtueTweakNormalAttackToggle;
        public static EnumField<VirtueAttackType> virtueNormalAttackType;

        public static FloatField virtueNormalInsigniaLastMulti;
        public static BoolField virtueNormalInsigniaXtoggle;
        public static FloatField virtueNormalInsigniaXsize;
        public static IntField virtueNormalInsigniaXdamage;
        public static BoolField virtueNormalInsigniaYtoggle;
        public static FloatField virtueNormalInsigniaYsize;
        public static IntField virtueNormalInsigniaYdamage;
        public static BoolField virtueNormalInsigniaZtoggle;
        public static FloatField virtueNormalInsigniaZsize;
        public static IntField virtueNormalInsigniaZdamage;

        public static FloatField virtueNormalLightningDamage;
        public static FloatField virtueNormalLightningDelay;

        public static BoolField virtueTweakEnragedAttackToggle;
        public static EnumField<VirtueAttackType> virtueEnragedAttackType;

        public static FloatField virtueEnragedInsigniaLastMulti;
        public static BoolField virtueEnragedInsigniaXtoggle;
        public static FloatField virtueEnragedInsigniaXsize;
        public static IntField virtueEnragedInsigniaXdamage;
        public static BoolField virtueEnragedInsigniaYtoggle;
        public static FloatField virtueEnragedInsigniaYsize;
        public static IntField virtueEnragedInsigniaYdamage;
        public static BoolField virtueEnragedInsigniaZtoggle;
        public static FloatField virtueEnragedInsigniaZsize;
        public static IntField virtueEnragedInsigniaZdamage;

        public static FloatField virtueEnragedLightningDamage;
        public static FloatField virtueEnragedLightningDelay;

        // MALICIOUS FACE
        public static BoolField maliciousFaceRadianceOnEnrage;
        public static IntField maliciousFaceRadianceAmount;
        public static BoolField maliciousFaceHomingProjectileToggle;
        public static IntField maliciousFaceHomingProjectileCount;
        public static IntField maliciousFaceHomingProjectileDamage;
        public static FloatField maliciousFaceHomingProjectileTurnSpeed;
        public static FloatField maliciousFaceHomingProjectileSpeed;
        public static IntField maliciousFaceBeamCountNormal;
        public static IntField maliciousFaceBeamCountEnraged;

        // MINDFLAYER
        public static BoolField mindflayerShootTweakToggle;
        public static IntField mindflayerShootAmount;
        public static FloatField mindflayerShootDelay;
        public static FloatField mindflayerShootInitialSpeed;
        public static FloatField mindflayerShootTurnSpeed;
        public static FloatSliderField mindflayerProjectileSelfDamageMultiplier;
        public static BoolField mindflayerTeleportComboToggle;

        // SCHISM
        public static BoolField schismSpreadAttackToggle;
        public static FloatSliderField schismSpreadAttackAngle;
        public static IntField schismSpreadAttackCount;

        // SOLIDER
        public static BoolField soliderCoinsIgnoreWeakPointToggle;
        public static BoolField soliderShootTweakToggle;
        public static IntField soliderShootCount;
        public static BoolField soliderShootGrenadeToggle;
        public static IntField soliderGrenadeDamage;
        public static FloatField soliderGrenadeSize;

        // STALKER
        public static BoolField stalkerSurviveExplosion;
        public static BoolField stalkerSpreadHealthRad;
        public static IntField stalkerSpreadHealthAddition;
        public static BoolField stalkerSpreadDamageRad;
        public static IntField stalkerSpreadDamageAddition;
        public static BoolField stalkerSpreadSpeedRad;
        public static IntField stalkerSpreadSpeedAddition;

        // STRAY
        public static BoolField strayShootToggle;
        public static IntField strayShootCount;
        public static FloatField strayShootSpeed;
        public static BoolField strayCoinsIgnoreWeakPointToggle;

        // STREET CLEANER
        public static BoolField streetCleanerPredictiveDodgeToggle;
        public static BoolField streetCleanerCoinsIgnoreWeakPointToggle;

        // SWORDS MACHINE
        public enum SwordsMachineSecondPhase
        {
            None,
            SpeedUp,
            Skip
        }
        public static BoolField swordsMachineNoLightKnockbackToggle;
        public static EnumField<SwordsMachineSecondPhase> swordsMachineSecondPhaseMode;
        public static FloatField swordsMachineSecondPhaseSpeed;
        public static BoolField swordsMachineExplosiveSwordToggle;
        public static IntField swordsMachineExplosiveSwordDamage;
        public static FloatField swordsMachineExplosiveSwordSize;

        // FERRYMAN
        public static BoolField ferrymanComboToggle;
        public static IntField ferrymanComboCount;
        public static FloatField ferrymanAttackDelay;

        // TURRET
        public static BoolField turretBurstFireToggle;
        public static IntField turretBurstFireCount;
        public static FloatField turretBurstFireDelay;

        // FLESH PRISON
        public static BoolField fleshPrisonSpinAttackToggle;
        public static IntField fleshPrisonSpinAttackCount;
        public static FloatField fleshPrisonSpinAttackTurnSpeed;
        public static FloatField fleshPrisonSpinAttackActivateSpeed;
        public static FloatField fleshPrisonSpinAttackSize;
        public static IntField fleshPrisonSpinAttackDamage;
        public static FloatField fleshPrisonSpinAttackDistance;

        // MINOS PRIME
        public static BoolField minosPrimeEarlyPhaseToggle;
        public static BoolField minosPrimeComboToggle;
        public static BoolField minosPrimeRandomTeleportToggle;
        public static FloatField minosPrimeRandomTeleportMinDistance;
        public static FloatField minosPrimeRandomTeleportMaxDistance;
        public static BoolField minosPrimeTeleportTrail;
        public static FloatField minosPrimeTeleportTrailDuration;

        public static BoolField minosPrimeExplosionToggle;
        public static FloatSliderField minosPrimeExplosionChance;
        public static FloatField minosPrimeExplosionWindupSpeed;
        public static FloatField minosPrimeExplosionSize;
        public static FloatField minosPrimeExplosionDamage;

        public static BoolField minosPrimeCrushAttackToggle;
        public static FloatSliderField minosPrimeCrushAttackChance;

        public static BoolField minosPrimeComboExplosiveEndToggle;
        public static FloatSliderField minosPrimeComboExplosiveEndChance;

        public static BoolField minosPrimeComboExplosionToggle;
        public static FloatField minosPrimeComboExplosionSize;
        public static FloatField minosPrimeComboExplosionDamage;

        // V2 - FIRST
        public static BoolField v2FirstKnuckleBlasterToggle;
        public static BoolField v2FirstKnuckleBlasterHitPlayerToggle;
        public static FloatField v2FirstKnuckleBlasterHitPlayerMinDistance;
        public static IntField v2FirstKnuckleBlasterHitDamage;
        public static BoolField v2FirstKnuckleBlasterDeflectShotgunToggle;
        public static FloatField v2FirstKnuckleBlasterCooldown;
        public static IntField v2FirstKnuckleBlasterExplosionDamage;
        public static FloatField v2FirstKnuckleBlasterSize;
        public static FloatField v2FirstKnuckleBlasterSpeed;

        public static BoolField v2FirstCoreSnipeToggle;
        public static FloatField v2FirstCoreSnipeMaxDistanceToPlayer;
        public static FloatField v2FirstCoreSnipeMinDistanceToV2;
        public static FloatField v2FirstCoreSnipeReactionTime;

        public static BoolField v2FirstSharpshooterToggle;
        public static FloatSliderField v2FirstSharpshooterChance;
        public static FloatSliderField v2FirstSharpshooterAutoaimAngle;
        public static IntField v2FirstSharpshooterReflections;
        public static FloatField v2FirstSharpshooterDamage;
        public static FloatField v2FirstSharpshooterSpeed;

        // V2 - SECOND
        public static BoolField v2SecondStartEnraged;
        public static BoolField v2SecondRocketLauncherToggle;
        public static BoolField v2SecondFastCoinToggle;
        public static FloatField v2SecondFastCoinShootDelay;
        public static FloatField v2SecondFastCoinThrowDelay;
        public static BoolField v2SecondCoinRailcannon;
        public static FloatField v2SecondCoinRailcannonCooldown;

        public static BoolField v2SecondMalCannonSnipeToggle;
        public static FloatField v2SecondMalCannonSnipeCooldown;
        public static FloatField v2SecondMalCannonSnipeReactTime;
        public static FloatField v2SecondMalCannonSnipeMaxDistanceToPlayer;
        public static FloatField v2SecondMalCannonSnipeMinDistanceToV2;

        public static BoolField v2SecondCoreSnipeToggle;
        public static FloatField v2SecondCoreSnipeMaxDistanceToPlayer;
        public static FloatField v2SecondCoreSnipeMinDistanceToV2;
        public static FloatField v2SecondCoreSnipeReactionTime;

        public static BoolField v2SecondSharpshooterToggle;
        public static FloatSliderField v2SecondSharpshooterChance;
        public static FloatSliderField v2SecondSharpshooterAutoaimAngle;
        public static IntField v2SecondSharpshooterReflections;
        public static FloatField v2SecondSharpshooterDamage;
        public static FloatField v2SecondSharpshooterSpeed;

        // SISYPHIUS INSTRUCTIONIST
        public static BoolField sisyInstBoulderShockwave;
        public static FloatField sisyInstBoulderShockwaveSize;
        public static FloatField sisyInstBoulderShockwaveSpeed;
        public static IntField sisyInstBoulderShockwaveDamage;

        public static BoolField sisyInstJumpShockwave;
        public static FloatField sisyInstJumpShockwaveSize;
        public static FloatField sisyInstJumpShockwaveSpeed;
        public static IntField sisyInstJumpShockwaveDamage;

        public static BoolField sisyInstStrongerExplosion;
        public static FloatField sisyInstStrongerExplosionSizeMulti;
        public static FloatField sisyInstStrongerExplosionDamageMulti;

        // ADD ME
        // LEVIATHAN
        public static BoolField leviathanSecondPhaseBegin;

        public static BoolField leviathanProjectileMixToggle;
        public static FloatSliderField leviathanProjectileBlueChance;
        public static FloatSliderField leviathanProjectileYellowChance;

        public static IntField leviathanProjectileCount;
        public static FloatField leviathanProjectileDensity;
        public static FloatSliderField leviathanProjectileFriendlyFireDamageMultiplier;

        public static BoolField leviathanChargeAttack;
        public static FloatSliderField leviathanChargeChance;
        public static FloatField leviathanChargeSizeMulti;
        public static FloatField leviathanChargeDamageMulti;
        public static IntField leviathanChargeCount;
        public static FloatField leviathanChargeDelay;
        public static BoolField leviathanChargeHauntRocketRiding;

        public static IntField leviathanTailComboCount;

        // PANOPTICON
        public static BoolField panopticonFullPhase;
        public static BoolField panopticonBalanceEyes;
        public static BoolField panopticonBlackholeProj;
        public static BoolField panopticonAxisBeam;
        public static FloatField panopticonAxisBeamSizeMulti;

        public static BoolField panopticonSpinAttackToggle;
        public static IntField panopticonSpinAttackCount;
        public static FloatField panopticonSpinAttackTurnSpeed;
        public static FloatField panopticonSpinAttackActivateSpeed;
        public static FloatField panopticonSpinAttackSize;
        public static IntField panopticonSpinAttackDamage;
        public static FloatField panopticonSpinAttackDistance;

        public static BoolField panopticonBlueProjToggle;
        public static IntField panopticonBlueProjCount;
        public static FloatField panopticonBlueProjDamage;
        public static FloatField panopticonBlueProjTurnSpeed;
        public static FloatField panopticonBlueProjInitialSpeed;
      
        // IDOL
        public static BoolField idolExplosionToggle;
        public enum IdolExplosionType
        {
            Normal,
            Big,
            Ligthning,
            Sisyphean,
            Sand
        }
        public static EnumField<IdolExplosionType> idolExplodionType;
        public static FloatField idolExplosionSizeMultiplier;
        public static FloatField idolExplosionDamageMultiplier;
        public static FloatSliderField idolExplosionEnemyDamagePercent;

        /////////// ADD MEEEE
        // GABRIEL SECOND
        public static BoolField gabriSecondP1Chaos;
        public static IntField gabriSecondP1ChaosCount;

        private static bool dirtyField = false;
        public static void Initialize()
        {
            if (config != null)
                return;

            config = PluginConfigurator.Create("ULTRAPAIN", Plugin.PLUGIN_GUID);
            config.postConfigChange += () =>
            {
                if (dirtyField)
                {
                    Plugin.PatchAll();
                    dirtyField = false;
                }
            };

            // ROOT PANEL
            ButtonArrayField buttons = new ButtonArrayField(config.rootPanel, "buttons", 2, new float[] { 0.5f, 0.5f }, new string[] { "Bug Report", "Feature Request" });
            buttons.OnClickEventHandler(0).onClick += () => Application.OpenURL("https://github.com/eternalUnion/UltraPain/issues/new?assignees=eternalUnion&labels=bug&projects=&template=bug-report.md&title=%5BBUG%5D+Bug+name+here");
            buttons.OnClickEventHandler(1).onClick += () => Application.OpenURL("https://github.com/eternalUnion/UltraPain/issues/new?assignees=eternalUnion&labels=feature+request&projects=&template=feature-request.md&title=%5BFEATURE%5D+Your+idea+goes+here");

            new ConfigHeader(config.rootPanel, "Enemy Tweaks");
            enemyTweakToggle = new BoolField(config.rootPanel, "Enabled", "enemyTweakToggle", true);
            enemyTweakToggle.presetLoadPriority = 1;
            enemyPanel = new ConfigPanel(config.rootPanel, "Enemy Tweaks", "enemyTweakPanel");
            enemyTweakToggle.onValueChange += (BoolField.BoolValueChangeEvent data) =>
            {
                enemyPanel.interactable = data.value;
                dirtyField = true;
            };
            enemyTweakToggle.TriggerValueChangeEvent();

            new ConfigHeader(config.rootPanel, "Player Tweaks");
            playerTweakToggle = new BoolField(config.rootPanel, "Enabled", "playerTweakToggle", true);
            playerTweakToggle.presetLoadPriority = 1;
            playerPanel = new ConfigPanel(config.rootPanel, "Player Tweaks", "enemyTweakPanel");
            playerTweakToggle.onValueChange += (BoolField.BoolValueChangeEvent data) =>
            {
                playerPanel.interactable = data.value;
                dirtyField = true;
            };
            playerTweakToggle.TriggerValueChangeEvent();

            new ConfigHeader(config.rootPanel, "Difficulty Rich Presence Override", 20);
            discordRichPresenceToggle = new BoolField(config.rootPanel, "Discord rich presence", "discordRichPresenceToggle", false);
            discordRichPresenceToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                dirtyField = true;
            };
            steamRichPresenceToggle = new BoolField(config.rootPanel, "Steam rich presence", "steamRichPresenceToggle", false);
            steamRichPresenceToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                dirtyField = true;
            };

            new ConfigHeader(config.rootPanel, "Plugin Difficulty Info");
            pluginName = new StringField(config.rootPanel, "Difficulty name", "pluginName", "ULTRAPAIN");
            pluginName.onValueChange += (StringField.StringValueChangeEvent data) =>
            {
                if (Plugin.currentDifficultyButton != null)
                    Plugin.currentDifficultyButton.transform.Find("Name").GetComponent<Text>().text = data.value;

                if (Plugin.currentDifficultyPanel != null)
                    Plugin.currentDifficultyPanel.transform.Find("Title (1)").GetComponent<Text>().text = $"--{data.value}--";
            };
            pluginInfo = new StringMultilineField(config.rootPanel, "Difficulty info", "pluginInfo",
                """
                    Fast and aggressive enemies with unique attack patterns.

                    Quick thinking, mobility options, and a decent understanding of the vanilla game are essential.

                    <color=red>Recommended for players who have gotten used to VIOLENT's changes and are looking to freshen up their gameplay with unique enemy mechanics.</color>
                    
                    <color=orange>This difficulty uses UKMD difficulty and slot. To use the mod on another difficulty, enable global difficulty from settings.</color>
                    """);
            pluginInfo.onValueChange += (StringMultilineField.StringValueChangeEvent e) =>
            {
                if (Plugin.currentDifficultyInfoText != null)
                    Plugin.currentDifficultyInfoText.text = e.value;
            };
            

            // GLOBAL STATE
            new ConfigHeader(config.rootPanel, "Global Difficulty");
            new ConfigHeader(config.rootPanel, "(Enable tweaks on all difficulties)", 12);
            globalDifficultySwitch = new BoolField(config.rootPanel, "Enabled", "globalDifficultySwitch", false);
            globalDifficultySwitch.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                dirtyField = true;
            };

            new ConfigHeader(config.rootPanel, "Extras");
            memePanel = new ConfigPanel(config.rootPanel, "Memes", "memePanel");

            new ConfigHeader(config.rootPanel, "Danger Zone");
            ButtonField addMissingDefaultPresets = new ButtonField(config.rootPanel, "Add missing default presets", "addMissingDefaultPresets");
            addMissingDefaultPresets.onClick += AddMissingPresets;

            // MEME PANEL
            enrageSfxToggle = new BoolField(memePanel, "Enrage SFX\n(volume warning)", "enrageSfxToggle", false);
            enrageSfxToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                dirtyField = true;
            };
            funnyDruidKnightSFXToggle = new BoolField(memePanel, "Funny druid knight sfx", "funnyDruidKnightSFXToggle", false);
            funnyDruidKnightSFXToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                dirtyField = true;
            };
            fleshObamiumToggle = new BoolField(memePanel, "FLESH OBAMIUM", "fleshObamiumToggle", false);
            fleshObamiumToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                dirtyField = true;
            };
            fleshObamiumName = new StringField(memePanel, "FLESH OBAMIUM name", "fleshObamiumName", "FLESH OBAMIUM");
            obamapticonToggle = new BoolField(memePanel, "OBAMAPTICON", "obamapticonToggle", false);
            obamapticonToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                dirtyField = true;
            };
            obamapticonName = new StringField(memePanel, "OBAMAPTICON name", "obamapticonName", "OBAMAPTICON");

            // PLAYER PANEL

            // PLAYER STAT EDITOR
            playerStatEditorPanel = new ConfigPanel(playerPanel, "Player stat editor", "playerStatEditorPanel");
            
            // MOVEMENT
            new ConfigHeader(playerStatEditorPanel, "Movement");
            staminaRegSpeedMulti = new FloatField(playerStatEditorPanel, "Stamina regen speed", "staminaRegSpeedMulti", 1f, 0.01f, float.MaxValue);
            
            // HEALTH
            new ConfigHeader(playerStatEditorPanel, "Health");
            maxPlayerHp = new IntField(playerStatEditorPanel, "Max HP", "maxPlayerHp", 100, 1, int.MaxValue);
            playerHpSupercharge = new IntField(playerStatEditorPanel, "Max overcharge HP", "playerHpSupercharge", 200, 1, int.MaxValue);

            new SpaceField(playerStatEditorPanel);
            
            hardDamagePercent = new FloatSliderField(playerStatEditorPanel, "Hard damage taken percent", "hardDamagePercent", new Tuple<float, float>(0, 100), 100);
            whiplashHardDamageCap = new IntField(playerStatEditorPanel, "Whiplash hard damage cap", "whiplashHardDamageCap", 50, 0, int.MaxValue);
            whiplashHardDamageSpeed = new FloatField(playerStatEditorPanel, "Whiplash hard damage speed multiplier", "whiplashHardDamageSpeed", 1f, 0.01f, float.MaxValue);

            new SpaceField(playerStatEditorPanel);

            ConfigDivision deltaHpDiv = new ConfigDivision(playerStatEditorPanel, "deltaHpDiv");
            playerHpDeltaToggle = new BoolField(playerStatEditorPanel, "HP regen/decay", "playerHpDeltaToggle", false);
            playerHpDeltaToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                if (NewMovement_DeltaHpComp.instance != null)
                    NewMovement_DeltaHpComp.instance.UpdateEnabled();

                deltaHpDiv.interactable = e.value;
                dirtyField = true;
            };
            playerHpDeltaToggle.TriggerValueChangeEvent();
            playerHpDeltaAmount = new IntField(deltaHpDiv, "HP regen/decay amount", "playerHpDeltaAmount", -1);
            playerHpDeltaDelay = new FloatField(deltaHpDiv, "Delay", "playerHpDeltaDelay", 2f, 0.01f, float.MaxValue);
            playerHpDeltaLimit = new IntField(deltaHpDiv, "Max regen/min decay limit", "playerHpDeltaLimit", 50);
            playerHpDeltaHurtAudio = new BoolField(deltaHpDiv, "Hurt audio on decay", "playerHpDeltaHurtAudio", false);
            playerHpDeltaCalm = new BoolField(deltaHpDiv, "Active during calm phase", "playerHpDeltaCalm", false);
            playerHpDeltaCombat = new BoolField(deltaHpDiv, "Active during combat", "playerHpDeltaCombat", true);
            playerHpDeltaCybergrind = new BoolField(deltaHpDiv, "Active in cybergrind", "playerHpDeltaCybergrind", true);
            playerHpDeltaCybergrind.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                if (NewMovement_DeltaHpComp.instance != null)
                    NewMovement_DeltaHpComp.instance.UpdateEnabled();
            };
            playerHpDeltaSandbox = new BoolField(deltaHpDiv, "Active in sandbox", "playerHpDeltaSandbox", false);
            playerHpDeltaSandbox.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                if (NewMovement_DeltaHpComp.instance != null)
                    NewMovement_DeltaHpComp.instance.UpdateEnabled();
            };

            // REVOLVER
            new ConfigHeader(playerStatEditorPanel, "Revolver");
            revolverDamage = new FloatField(playerStatEditorPanel, "Revolver damage", "revolverDamageMulti", 1f, 0.01f, float.MaxValue);
                new ImageInputField(revolverDamage, Plugin.blueRevolverSprite, Color.white);
            revolverAltDamage = new FloatField(playerStatEditorPanel, "Alt revolver damage", "revolverAltDamageMulti", 1.25f, 0.01f, float.MaxValue);
                new ImageInputField(revolverAltDamage, Plugin.blueRevolverSprite, Color.white);
            
            chargedRevDamage = new FloatField(playerStatEditorPanel, "Charged revolver damage", "chargedRevDamage", 1f, 0f, float.MaxValue);
                new ImageInputField(chargedRevDamage, Plugin.blueRevolverSprite, Color.cyan);
            chargedAltRevDamage = new FloatField(playerStatEditorPanel, "Charged alt revolver damage", "chargedAltRevDamage", 1.25f, 0f, float.MaxValue);
                new ImageInputField(chargedAltRevDamage, Plugin.blueRevolverSprite, Color.cyan);
            chargedRevTotalHits = new IntField(playerStatEditorPanel, "Charged revolver total hits", "chargedRevTotalHits", 3, 1, int.MaxValue);
                new ImageInputField(chargedRevTotalHits, Plugin.blueRevolverSprite, Color.cyan);
            chargedAltRevTotalHits = new IntField(playerStatEditorPanel, "Charged alt revolver total hits", "chargedAltRevTotalHits", 6, 1, int.MaxValue);
                new ImageInputField(chargedAltRevTotalHits, Plugin.blueRevolverSprite, Color.cyan);
            chargedRevMaxHitsPerTarget = new IntField(playerStatEditorPanel, "Charged revolver max hits per target", "chargedRevMaxHitsPerTarget", 3, 1, int.MaxValue);
                new ImageInputField(chargedRevMaxHitsPerTarget, Plugin.blueRevolverSprite, Color.cyan);
            chargedAltRevMaxHitsPerTarget = new IntField(playerStatEditorPanel, "Charged alt revolver max hits per target", "chargedAltRevMaxHitsPerTarget", 4, 1, int.MaxValue);
                new ImageInputField(chargedAltRevMaxHitsPerTarget, Plugin.blueRevolverSprite, Color.cyan);
            
            sharpshooterDamage = new FloatField(playerStatEditorPanel, "Sharpshooter damage", "sharpshooterDamage", 1f, 0f, float.MaxValue);
                new ImageInputField(sharpshooterDamage, Plugin.redRevolverSprite, Color.red);
            sharpshooterAltDamage = new FloatField(playerStatEditorPanel, "Sharpshooter alt damage", "sharpshooterAltDamage", 1.25f, 0f, float.MaxValue);
                new ImageInputField(sharpshooterAltDamage, Plugin.redRevolverSprite, Color.red);
            sharpshooterMaxHitsPerTarget = new IntField(playerStatEditorPanel, "Sharpshooter max hits per target", "sharpshooterMaxHitsPerTarget", 1, 1, int.MaxValue);
                new ImageInputField(sharpshooterMaxHitsPerTarget, Plugin.redRevolverSprite, Color.red);
            sharpshooterAltMaxHitsPerTarget = new IntField(playerStatEditorPanel, "Sharpshooter alt max hits per target", "sharpshooterAltMaxHitsPerTarget", 2, 1, int.MaxValue);
                new ImageInputField(sharpshooterAltMaxHitsPerTarget, Plugin.redRevolverSprite, Color.red);
            
            chargedRevRegSpeedMulti = new FloatField(playerStatEditorPanel, "Charged revolver regen speed", "chargedRevRegSpeedMulti", 1f, 0.01f, float.MaxValue);
                new ImageInputField(chargedRevRegSpeedMulti, Plugin.blueRevolverSprite, Color.cyan);
            coinRegSpeedMulti = new FloatField(playerStatEditorPanel, "Coin regen speed", "coinRegSpeedMulti", 1f, 0.01f, float.MaxValue);
                new ImageInputField(coinRegSpeedMulti, Plugin.greenRevolverSprite, Color.green);
            sharpshooterRegSpeedMulti = new FloatField(playerStatEditorPanel, "Sharpshooter regen speed", "sharpshooterRegSpeedMulti", 1f, 0.01f, float.MaxValue);
                new ImageInputField(sharpshooterRegSpeedMulti, Plugin.redRevolverSprite, Color.red);

            // SHOTGUN
            new ConfigHeader(playerStatEditorPanel, "Shotgun");
            shotgunBluePelletCount = new IntField(playerStatEditorPanel, "Pellet count", "shotgunBluePelletCount", 12, 1, int.MaxValue);
                new ImageInputField(shotgunBluePelletCount, Plugin.blueShotgunSprite, Color.cyan);
            shotgunBlueSpreadAngle = new FloatField(playerStatEditorPanel, "Pellet spread angle", "shotgunBlueSpreadAngle", 10f, 0f, 180f);
                new ImageInputField(shotgunBlueSpreadAngle, Plugin.blueShotgunSprite, Color.cyan);
            shotgunBlueDamagePerPellet = new FloatField(playerStatEditorPanel, "Damage per pellet", "shotgunBlueDamagePerPellet", 1, 0, float.MaxValue);
                new ImageInputField(shotgunBlueDamagePerPellet, Plugin.blueShotgunSprite, Color.cyan);

            new SpaceField(playerStatEditorPanel);

            shotgunCoreExplosionSize = new FloatField(playerStatEditorPanel, "Core explosion size", "shotgunCoreExplosionSize", 6, 0, float.MaxValue);
                new ImageInputField(shotgunCoreExplosionSize, Plugin.blueShotgunSprite, Color.cyan);
            shotgunCoreExplosionSpeed = new FloatField(playerStatEditorPanel, "Core explosion fade speed", "shotgunCoreExplosionSpeed", 1, 0.01f, float.MaxValue);
                new ImageInputField(shotgunCoreExplosionSpeed, Plugin.blueShotgunSprite, Color.cyan);
            shotgunCoreExplosionDamage = new IntField(playerStatEditorPanel, "Core explosion enemy damage", "shotgunCoreExplosionDamage", 35, 0, int.MaxValue);
                new ImageInputField(shotgunCoreExplosionDamage, Plugin.blueShotgunSprite, Color.cyan);
            shotgunCoreExplosionPlayerDamage = new IntField(playerStatEditorPanel, "Core explosion player damage", "shotgunCoreExplosionPlayerDamage", 35, 0, int.MaxValue);
                new ImageInputField(shotgunCoreExplosionPlayerDamage, Plugin.blueShotgunSprite, Color.cyan);

            new SpaceField(playerStatEditorPanel);

            shotgunGreenPump1Count = new IntField(playerStatEditorPanel, "Pump 1 pellet count", "shotgunGreenPump1Count", 10, 1, int.MaxValue);
                new ImageInputField(shotgunGreenPump1Count, Plugin.greenShotgunSprite, Color.green);
            shotgunGreenPump1Spread = new FloatField(playerStatEditorPanel, "Pump 1 spread angle", "shotgunGreenPump1Spread", 10f / 1.5f, 0f, 180f);
                new ImageInputField(shotgunGreenPump1Spread, Plugin.greenShotgunSprite, Color.green);
            shotgunGreenPump1Damage = new FloatField(playerStatEditorPanel, "Pump 1 damage per pellet", "shotgunGreenPump1Damage", 1f, 0f, float.MaxValue);
                new ImageInputField(shotgunGreenPump1Damage, Plugin.greenShotgunSprite, Color.green);

            new SpaceField(playerStatEditorPanel);

            shotgunGreenPump2Count = new IntField(playerStatEditorPanel, "Pump 2 pellet count", "shotgunGreenPump2Count", 16, 1, int.MaxValue);
                new ImageInputField(shotgunGreenPump2Count, Plugin.greenShotgunSprite, Color.green);
            shotgunGreenPump2Spread = new FloatField(playerStatEditorPanel, "Pump 2 spread angle", "shotgunGreenPump2Spread", 10f, 0f, 180f);
                new ImageInputField(shotgunGreenPump2Spread, Plugin.greenShotgunSprite, Color.green);
            shotgunGreenPump2Damage = new FloatField(playerStatEditorPanel, "Pump 2 damage per pellet", "shotgunGreenPump2Damage", 1f, 0f, float.MaxValue);
                new ImageInputField(shotgunGreenPump2Damage, Plugin.greenShotgunSprite, Color.green);

            new SpaceField(playerStatEditorPanel);

            shotgunGreenPump3Count = new IntField(playerStatEditorPanel, "Pump 3 pellet count", "shotgunGreenPump3Count", 24, 1, int.MaxValue);
                new ImageInputField(shotgunGreenPump3Count, Plugin.greenShotgunSprite, Color.green);
            shotgunGreenPump3Spread = new FloatField(playerStatEditorPanel, "Pump 3 spread angle", "shotgunGreenPump3Spread", 20f, 0f, 180f);
                new ImageInputField(shotgunGreenPump3Spread, Plugin.greenShotgunSprite, Color.green);
            shotgunGreenPump3Damage = new FloatField(playerStatEditorPanel, "Pump 3 damage per pellet", "shotgunGreenPump3Damage", 1f, 0f, float.MaxValue);
                new ImageInputField(shotgunGreenPump3Damage, Plugin.greenShotgunSprite, Color.green);

            new SpaceField(playerStatEditorPanel);

            shotgunGreenExplosionSize = new FloatField(playerStatEditorPanel, "Explosion size", "shotgunGreenExplosionSize", 9, 0, float.MaxValue);
                new ImageInputField(shotgunGreenExplosionSize, Plugin.greenShotgunSprite, Color.green);
            shotgunGreenExplosionSpeed = new FloatField(playerStatEditorPanel, "Explosion fade speed", "shotgunGreenExplosionSpeed", 1, 0.01f, float.MaxValue);
                new ImageInputField(shotgunGreenExplosionSpeed, Plugin.greenShotgunSprite, Color.green);
            shotgunGreenExplosionDamage = new IntField(playerStatEditorPanel, "Explosion enemy damage", "shotgunGreenExplosionDamage", 50, 0, int.MaxValue);
                new ImageInputField(shotgunGreenExplosionDamage, Plugin.greenShotgunSprite, Color.green);
            shotgunGreenExplosionPlayerDamage = new IntField(playerStatEditorPanel, "Explosion player damage", "shotgunGreenExplosionPlayerDamage", 50, 0, int.MaxValue);
                new ImageInputField(shotgunGreenExplosionPlayerDamage, Plugin.greenShotgunSprite, Color.green);

            // NAILGUN/SAW LAUNCHER
            new ConfigHeader(playerStatEditorPanel, "Nailgun/Saw Launcher");
            nailgunBlueDamage = new FloatField(playerStatEditorPanel, "Bullet damage", "nailgunBlueDamage", 0.205f, 0f, float.MaxValue);
                new ImageInputField(nailgunBlueDamage, Plugin.blueNailgunSprite, Color.cyan);
            nailgunGreenDamage = new FloatField(playerStatEditorPanel, "Bullet damage", "nailgunGreenDamage", 0.185f, 0f, float.MaxValue);
                new ImageInputField(nailgunGreenDamage, Plugin.greenNailgunSprite, Color.green);
            nailgunGreenBurningDamage = new FloatField(playerStatEditorPanel, "Burning bullet damage", "nailgunGreenBurningDamage", 0.185f, 0f, float.MaxValue);
                new ImageInputField(nailgunGreenBurningDamage, Plugin.greenNailgunSprite, Color.green);

            new SpaceField(playerStatEditorPanel);

            sawBlueDamage = new FloatField(playerStatEditorPanel, "Saw damage", "sawBlueDamage", 0.75f, 0f, float.MaxValue);
                new ImageInputField(sawBlueDamage, Plugin.blueSawLauncherSprite, Color.cyan);
            sawBlueHitAmount = new FloatField(playerStatEditorPanel, "Saw durability", "sawBlueHitAmount", 3.9f, 0f, float.MaxValue);
                new ImageInputField(sawBlueHitAmount, Plugin.blueSawLauncherSprite, Color.cyan);
            sawGreenDamage = new FloatField(playerStatEditorPanel, "Weak saw damage", "sawGreenDamage", 0.6f, 0f, float.MaxValue);
                new ImageInputField(sawGreenDamage, Plugin.greenSawLauncherSprite, Color.green);
            sawGreenHitAmount = new FloatField(playerStatEditorPanel, "Weak saw durability", "sawGreenHitAmount", 3f, 0f, float.MaxValue);
                new ImageInputField(sawGreenHitAmount, Plugin.greenSawLauncherSprite, Color.green);
            sawGreenBurningDamage = new FloatField(playerStatEditorPanel, "Super saw damage", "sawGreenBurningDamage", 1f, 0f, float.MaxValue);
                new ImageInputField(sawGreenBurningDamage, Plugin.greenSawLauncherSprite, Color.green);
            sawGreenBurningHitAmount = new FloatField(playerStatEditorPanel, "Super saw durability", "sawGreenBurningHitAmount", 20.9f, 0f, float.MaxValue);
                new ImageInputField(sawGreenBurningHitAmount, Plugin.greenSawLauncherSprite, Color.green);

            new SpaceField(playerStatEditorPanel);

            nailgunAmmoRegSpeedMulti = new FloatField(playerStatEditorPanel, "Nailgun ammo regen speed", "nailgunAmmoRegSpeedMulti", 1f, 0.01f, float.MaxValue);
                new ImageInputField(nailgunAmmoRegSpeedMulti, Plugin.blueNailgunSprite, Color.cyan);
            nailgunHeatsinkRegSpeedMulti = new FloatField(playerStatEditorPanel, "Nailgun heatsink regen speed", "nailgunHeatsinkRegSpeedMulti", 1f, 0.01f, float.MaxValue);
                new ImageInputField(nailgunHeatsinkRegSpeedMulti, Plugin.greenNailgunSprite, Color.green);
            sawAmmoRegSpeedMulti = new FloatField(playerStatEditorPanel, "Saw ammo regen speed", "sawAmmoRegSpeedMulti", 1f, 0.01f, float.MaxValue);
                new ImageInputField(sawAmmoRegSpeedMulti, Plugin.blueSawLauncherSprite, Color.cyan);
            sawHeatsinkRegSpeedMulti = new FloatField(playerStatEditorPanel, "Saw heatsink regen speed", "sawHeatsinkRegSpeedMulti", 1f, 0.01f, float.MaxValue);
                new ImageInputField(sawHeatsinkRegSpeedMulti, Plugin.greenSawLauncherSprite, Color.green);
            
            // RAILCANNON
            new ConfigHeader(playerStatEditorPanel, "Railcannon");
            railcannonRegSpeedMulti = new FloatField(playerStatEditorPanel, "Railcannon charge speed", "railcannonRegSpeedMulti", 1f, 0.01f, float.MaxValue);
            new ConfigHeader(playerStatEditorPanel, "Rocket Freeze/Cannonball");
            rocketFreezeRegSpeedMulti = new FloatField(playerStatEditorPanel, "Rocket freeze regen speed", "rocketFreezeRegSpeedMulti", 1f, 0.01f, float.MaxValue);
            rocketCannonballRegSpeedMulti = new FloatField(playerStatEditorPanel, "Cannonball regen speed", "rocketCannonballRegSpeedMulti", 1f, 0.01f, float.MaxValue);

            void AddDirtyFlagToFloatFieldValueChange(FloatField field)
            {
                field.onValueChange += (FloatField.FloatValueChangeEvent e) =>
                {
                    dirtyField = true;
                };
            }
            void AddDirtyFlagToFloatSliderFieldValueChange(FloatSliderField field)
            {
                field.onValueChange += (FloatSliderField.FloatSliderValueChangeEvent e) =>
                {
                    dirtyField = true;
                };
            }
            void AddDirtyFlagToIntFieldValueChange(IntField field)
            {
                field.onValueChange += (IntField.IntValueChangeEvent e) =>
                {
                    dirtyField = true;
                };
            }

            AddDirtyFlagToFloatFieldValueChange(staminaRegSpeedMulti);

            AddDirtyFlagToIntFieldValueChange(maxPlayerHp);
            AddDirtyFlagToIntFieldValueChange(playerHpSupercharge);
            AddDirtyFlagToFloatSliderFieldValueChange(hardDamagePercent);
            AddDirtyFlagToIntFieldValueChange(whiplashHardDamageCap);
            AddDirtyFlagToFloatFieldValueChange(whiplashHardDamageSpeed);

            AddDirtyFlagToFloatFieldValueChange(revolverDamage);
            AddDirtyFlagToFloatFieldValueChange(revolverAltDamage);

            AddDirtyFlagToFloatFieldValueChange(chargedRevDamage);
            AddDirtyFlagToFloatFieldValueChange(chargedAltRevDamage);
            AddDirtyFlagToIntFieldValueChange(chargedRevTotalHits);
            AddDirtyFlagToIntFieldValueChange(chargedAltRevTotalHits);
            AddDirtyFlagToIntFieldValueChange(chargedRevMaxHitsPerTarget);
            AddDirtyFlagToIntFieldValueChange(chargedAltRevMaxHitsPerTarget);

            AddDirtyFlagToFloatFieldValueChange(sharpshooterDamage);
            AddDirtyFlagToFloatFieldValueChange(sharpshooterAltDamage);
            AddDirtyFlagToIntFieldValueChange(sharpshooterMaxHitsPerTarget);
            AddDirtyFlagToIntFieldValueChange(sharpshooterAltMaxHitsPerTarget);

            AddDirtyFlagToFloatFieldValueChange(chargedRevRegSpeedMulti);
            AddDirtyFlagToFloatFieldValueChange(coinRegSpeedMulti);
            AddDirtyFlagToFloatFieldValueChange(sharpshooterRegSpeedMulti);

            AddDirtyFlagToIntFieldValueChange(shotgunBluePelletCount);
            AddDirtyFlagToFloatFieldValueChange(shotgunBlueSpreadAngle);
            AddDirtyFlagToFloatFieldValueChange(shotgunBlueDamagePerPellet);

            AddDirtyFlagToFloatFieldValueChange(shotgunCoreExplosionSize);
            AddDirtyFlagToFloatFieldValueChange(shotgunCoreExplosionSpeed);
            AddDirtyFlagToIntFieldValueChange(shotgunCoreExplosionDamage);
            AddDirtyFlagToIntFieldValueChange(shotgunCoreExplosionPlayerDamage);

            AddDirtyFlagToIntFieldValueChange(shotgunGreenPump1Count);
            AddDirtyFlagToFloatFieldValueChange(shotgunGreenPump1Spread);
            AddDirtyFlagToFloatFieldValueChange(shotgunGreenPump1Damage);

            AddDirtyFlagToIntFieldValueChange(shotgunGreenPump2Count);
            AddDirtyFlagToFloatFieldValueChange(shotgunGreenPump2Spread);
            AddDirtyFlagToFloatFieldValueChange(shotgunGreenPump2Damage);

            AddDirtyFlagToIntFieldValueChange(shotgunGreenPump3Count);
            AddDirtyFlagToFloatFieldValueChange(shotgunGreenPump3Spread);
            AddDirtyFlagToFloatFieldValueChange(shotgunGreenPump3Damage);

            AddDirtyFlagToFloatFieldValueChange(shotgunGreenExplosionSize);
            AddDirtyFlagToFloatFieldValueChange(shotgunGreenExplosionSpeed);
            AddDirtyFlagToIntFieldValueChange(shotgunGreenExplosionDamage);
            AddDirtyFlagToIntFieldValueChange(shotgunGreenExplosionPlayerDamage);

            AddDirtyFlagToFloatFieldValueChange(nailgunBlueDamage);
            AddDirtyFlagToFloatFieldValueChange(nailgunGreenDamage);
            AddDirtyFlagToFloatFieldValueChange(nailgunGreenBurningDamage);

            AddDirtyFlagToFloatFieldValueChange(sawBlueDamage);
            AddDirtyFlagToFloatFieldValueChange(sawBlueHitAmount);
            AddDirtyFlagToFloatFieldValueChange(sawGreenDamage);
            AddDirtyFlagToFloatFieldValueChange(sawGreenHitAmount);
            AddDirtyFlagToFloatFieldValueChange(sawGreenBurningDamage);
            AddDirtyFlagToFloatFieldValueChange(sawGreenBurningHitAmount);

            AddDirtyFlagToFloatFieldValueChange(nailgunAmmoRegSpeedMulti);
            AddDirtyFlagToFloatFieldValueChange(nailgunHeatsinkRegSpeedMulti);

            AddDirtyFlagToFloatFieldValueChange(sawAmmoRegSpeedMulti);
            AddDirtyFlagToFloatFieldValueChange(sawHeatsinkRegSpeedMulti);

            AddDirtyFlagToFloatFieldValueChange(railcannonRegSpeedMulti);

            AddDirtyFlagToFloatFieldValueChange(rocketFreezeRegSpeedMulti);
            AddDirtyFlagToFloatFieldValueChange(rocketCannonballRegSpeedMulti);

            new ConfigHeader(playerPanel, "Rocket Boosting");
            ConfigDivision rocketBoostDiv = new ConfigDivision(playerPanel, "rocketBoostDiv");
            rocketBoostToggle = new BoolField(playerPanel, "Enabled", "rocketBoostToggle", true);
            rocketBoostToggle.presetLoadPriority = 1;
            rocketBoostToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                rocketBoostDiv.interactable = e.value;
                dirtyField = true;
            };
            rocketBoostToggle.TriggerValueChangeEvent();
            rocketBoostAlwaysExplodesToggle = new BoolField(rocketBoostDiv, "Always explode", "rocketBoostAlwaysExplodes", true);
            rocketBoostDamageMultiplierPerHit = new FloatField(rocketBoostDiv, "Damage multiplier per hit", "rocketBoostDamageMultiplier", 0.35f, 0f, float.MaxValue);
            rocketBoostSizeMultiplierPerHit = new FloatField(rocketBoostDiv, "Size multiplier per hit", "rocketBoostSizeMultiplier", 0.35f, 0f, float.MaxValue);
            rocketBoostSpeedMultiplierPerHit = new FloatField(rocketBoostDiv, "Speed multiplier per hit", "rocketBoostSpeedMultiplierPerHit", 0.35f, 0f, float.MaxValue);
            FormattedStringBuilder rocketBoostStyleBuilder = new FormattedStringBuilder();
            rocketBoostStyleBuilder.currentFormat = new PluginConfig.API.Fields.CharacterInfo() { color = Color.green };
            rocketBoostStyleBuilder += "ROCKET BOOST";
            rocketBoostStyleText = new FormattedStringField(rocketBoostDiv, "Style text", "rocketBoostStyleText", rocketBoostStyleBuilder.Build());
            rocketBoostStyleText.onValueChange += (FormattedStringField.FormattedStringValueChangeEvent e) =>
            {
                Plugin.StyleIDs.UpdateID(rocketBoostStyleText.guid, e.formattedString.formattedString);
            };
            rocketBoostStylePoints = new IntField(rocketBoostDiv, "Style points", "rocketBoostStylePoints", 10, 0, int.MaxValue);

            new ConfigHeader(playerPanel, "Rocket Grabbing\r\n<size=16>(Can pull yourself to frozen rockets)</size>");
            rocketGrabbingToggle = new BoolField(playerPanel, "Enabled", "rocketGrabbingTabble", true);
            rocketGrabbingToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                dirtyField = true;
            };

            new ConfigHeader(playerPanel, "Grenade Boosting");
            ConfigDivision grenadeBoostDiv = new ConfigDivision(playerPanel, "grenadeBoostDiv");
            grenadeBoostToggle = new BoolField(playerPanel, "Enabled", "grenadeBoostToggle", true);
            grenadeBoostToggle.presetLoadPriority = 1;
            grenadeBoostToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                grenadeBoostDiv.interactable = e.value;
                dirtyField = true;
            };
            grenadeBoostToggle.TriggerValueChangeEvent();
            grenadeBoostDamageMultiplier = new FloatField(grenadeBoostDiv, "Damage multiplier", "grenadeBoostDamageMultiplier", 1f, 0f, float.MaxValue);
            grenadeBoostSizeMultiplier = new FloatField(grenadeBoostDiv, "Size multiplier", "grenadeBoostSizeMultiplier", 1f, 0f, float.MaxValue);
            FormattedStringBuilder grenadeBoostStyleBuilder = new FormattedStringBuilder();
            grenadeBoostStyleBuilder.currentFormat = new PluginConfig.API.Fields.CharacterInfo() { color = Color.cyan };
            grenadeBoostStyleBuilder += "FISTFUL OF 'NADE";
            grenadeBoostStyleText = new FormattedStringField(grenadeBoostDiv, "Style text", "grenadeBoostStyleText", grenadeBoostStyleBuilder.Build());
            grenadeBoostStyleText.onValueChange += (FormattedStringField.FormattedStringValueChangeEvent e) =>
            {
                Plugin.StyleIDs.UpdateID(grenadeBoostStyleText.guid, e.formattedString.formattedString);
            };
            grenadeBoostStylePoints = new IntField(grenadeBoostDiv, "Style points", "grenadeBoostStylePoints", 10, 0, int.MaxValue);

            // SCREWDRIVER
            new ConfigHeader(playerPanel, "Screw Driver Splitting");
            ConfigDivision screwDriverSplitDiv = new ConfigDivision(playerPanel, "screwDriverSplitDiv");
            screwDriverSplitToggle = new BoolField(playerPanel, "Enabled", "screwDriverSplitToggle", true);
            screwDriverSplitToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                screwDriverSplitDiv.interactable = e.value;
                dirtyField = true;
            };
            screwDriverSplitToggle.TriggerValueChangeEvent();
            screwDriverCoinSplitCount = new IntField(screwDriverSplitDiv, "Coin split count", "screwDriverCoinSplitCount", 5, 2, int.MaxValue);
            screwDriverGrenadeSplitCount = new IntField(screwDriverSplitDiv, "Grenade split count", "screwDriverGrenadeSplitCount", 5);

            new ConfigHeader(playerPanel, "Screw Driver Home To Magnet Enemies");
            ConfigDivision screwDriverHomeDiv = new ConfigDivision(playerPanel, "screwDriverHomeDiv");
            screwDriverHomeToggle = new BoolField(playerPanel, "Enabled", "screwDriverHomeToggle", true);
            screwDriverHomeToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                screwDriverHomeDiv.interactable = e.value;
                dirtyField = true;
            };
            screwDriverHomeToggle.TriggerValueChangeEvent();
            screwDriverHomeDestroyMagnets = new BoolField(screwDriverHomeDiv, "Destroy magnets on attach", "screwDriverHomeDestroyMagnets", true);
            screwDriverHomePierceDamage = new FloatField(screwDriverHomeDiv, "Pierce damage", "screwDriverHomePierceDamage", 2f, 0f, float.MaxValue);
                                                  
            new ConfigHeader(playerPanel, "Orbital Strike", 26);
            new ConfigHeader(playerPanel, "(Tweaks for coin-knuckleblaster)", 16);
            ConfigDivision orbStrikeDiv = new ConfigDivision(playerPanel, "orbStrikeDiv");
            orbStrikeToggle = new BoolField(playerPanel, "Enabled", "orbStrikeToggle", true);
            orbStrikeToggle.presetLoadPriority = 1;
            orbStrikeToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                orbStrikeDiv.interactable = e.value;
                dirtyField = true;
            };
            orbStrikeToggle.TriggerValueChangeEvent();
            orbStrikeMinDistance = new FloatField(orbStrikeDiv, "Min trigger distance", "orbStrikeMinDistance", 50f, 0f, float.MaxValue);
            
            new ConfigHeader(orbStrikeDiv, "Revolver Beam", 22);
            FormattedStringBuilder orbStrikeRevolverBuilder = new FormattedStringBuilder();
            orbStrikeRevolverBuilder.currentFormat = new PluginConfig.API.Fields.CharacterInfo() { color = Color.red };
            orbStrikeRevolverBuilder += "ORBITALSTRIKE";
            orbStrikeRevolverStyleText = new FormattedStringField(orbStrikeDiv, "Style text", "orbStrikeRevolverStyleText", orbStrikeRevolverBuilder.Build());
            orbStrikeRevolverStyleText.onValueChange += (FormattedStringField.FormattedStringValueChangeEvent e) =>
            {
                Plugin.StyleIDs.UpdateID(orbStrikeRevolverStyleText.guid, e.formattedString.formattedString);
            };
            orbStrikeRevolverStylePoint = new IntField(orbStrikeDiv, "Style point", "orbStrikeRevolverStylePoint", 20, 0, int.MaxValue);
            new ConfigHeader(orbStrikeDiv, "--Grenade Explosion Boost--", 12);
            ConfigDivision orbStrikeRevolverGrenadeDiv = new ConfigDivision(orbStrikeDiv, "orbStrikeRevolverGrenadeDiv");
            orbStrikeRevolverGrenade = new BoolField(orbStrikeDiv, "Enabled", "orbStrikeRevolverGrenade", true);
            orbStrikeRevolverGrenade.presetLoadPriority = 1;
            orbStrikeRevolverGrenade.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                orbStrikeRevolverGrenadeDiv.interactable = e.value;
            };
            orbStrikeRevolverGrenade.TriggerValueChangeEvent();
            orbStrikeRevolverGrenadeExtraSize = new FloatField(orbStrikeRevolverGrenadeDiv, "Size bonus percent", "orbStrikeRevolverExtraSize", 0.2f, 0f, float.MaxValue);
            orbStrikeRevolverGrenadeExtraDamage = new FloatField(orbStrikeRevolverGrenadeDiv, "Damage bonus percent", "orbStrikeRevolverGrenadeExtraDamage", 0f, 0f, float.MaxValue);
            new ConfigHeader(orbStrikeDiv, "--Explosion On Enemy Hit--", 12);
            ConfigDivision orbStrikeRevolverExplosionDiv = new ConfigDivision(orbStrikeDiv, "orbStrikeRevolverExplosionDiv");
            orbStrikeRevolverExplosion = new BoolField(orbStrikeDiv, "Enabled", "orbStrikeRevolverExplosion", true);
            orbStrikeRevolverExplosion.presetLoadPriority = 1;
            orbStrikeRevolverExplosion.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                orbStrikeRevolverExplosionDiv.interactable = e.value;
            };
            orbStrikeRevolverExplosion.TriggerValueChangeEvent();
            orbStrikeRevolverExplosionDamage = new FloatField(orbStrikeRevolverExplosionDiv, "Damage multiplier", "orbStrikeRevolverExplosionDamage", 1f, 0f, float.MaxValue);
            orbStrikeRevolverExplosionSize = new FloatField(orbStrikeRevolverExplosionDiv, "Size multiplier", "orbStrikeRevolverExplosionSize", 1f, 0f, float.MaxValue);

            new ConfigHeader(orbStrikeDiv, "Charged Revolver Beam", 22);
            FormattedStringBuilder orbStrikeRevolverChargedBuilder = new FormattedStringBuilder();
            orbStrikeRevolverChargedBuilder.currentFormat = new PluginConfig.API.Fields.CharacterInfo() { color = Color.red };
            orbStrikeRevolverChargedBuilder += "ORBITAL";
            orbStrikeRevolverChargedBuilder.currentFormat.color = Color.cyan;
            orbStrikeRevolverChargedBuilder += "ZAP";
            orbStrikeRevolverChargedStyleText = new FormattedStringField(orbStrikeDiv, "Style text", "orbStrikeRevolverChargedStyleText", orbStrikeRevolverChargedBuilder.Build());
            orbStrikeRevolverChargedStyleText.onValueChange += (FormattedStringField.FormattedStringValueChangeEvent e) =>
            {
                Plugin.StyleIDs.UpdateID(orbStrikeRevolverChargedStyleText.guid, e.formattedString.formattedString);
            };
            orbStrikeRevolverChargedStylePoint = new IntField(orbStrikeDiv, "Style point", "orbStrikeRevolverChargedStylePoint", 30, 0, int.MaxValue);
            new ConfigHeader(orbStrikeDiv, "--Grenade Explosion Boost--", 12);
            ConfigDivision orbStrikeRevolverChargedGrenadeDiv = new ConfigDivision(orbStrikeDiv, "orbStrikeRevolverGrenadeDiv");
            orbStrikeRevolverChargedGrenade = new BoolField(orbStrikeDiv, "Enabled", "orbStrikeRevolverChargedGrenade", true);
            orbStrikeRevolverChargedGrenade.presetLoadPriority = 1;
            orbStrikeRevolverChargedGrenade.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                orbStrikeRevolverChargedGrenadeDiv.interactable = e.value;
            };
            orbStrikeRevolverChargedGrenade.TriggerValueChangeEvent();
            orbStrikeRevolverChargedGrenadeExtraSize = new FloatField(orbStrikeRevolverChargedGrenadeDiv, "Size bonus percent", "orbStrikeRevolverChargedGrenadeExtraSize", 0.25f, 0f, float.MaxValue);
            orbStrikeRevolverChargedGrenadeExtraDamage = new FloatField(orbStrikeRevolverChargedGrenadeDiv, "Damage bonus percent", "orbStrikeRevolverChargedGrenadeExtraDamage", 0f, 0f, float.MaxValue);
            new ConfigHeader(orbStrikeDiv, "--Insignia On Enemy Hit--", 12);
            ConfigDivision orbStrikeRevolverChargedInsigniaDiv = new ConfigDivision(orbStrikeDiv, "orbStrikeRevolverChargedInsigniaDiv");
            orbStrikeRevolverChargedInsignia = new BoolField(orbStrikeDiv, "Enabled", "orbStrikeRevolverChargedInsignia", true);
            orbStrikeRevolverChargedInsignia.presetLoadPriority = 1;
            orbStrikeRevolverChargedInsignia.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                orbStrikeRevolverChargedInsigniaDiv.interactable = e.value;
            };
            orbStrikeRevolverChargedInsignia.TriggerValueChangeEvent();
            orbStrikeRevolverChargedInsigniaDamage = new IntField(orbStrikeRevolverChargedInsigniaDiv, "Damage", "orbStrikeRevolverChargedInsigniaDamage", 10, 0, int.MaxValue);
            orbStrikeRevolverChargedInsigniaSize = new FloatField(orbStrikeRevolverChargedInsigniaDiv, "Size", "orbStrikeRevolverChargedInsigniaSize", 2f, 0f, float.MaxValue);
            orbStrikeRevolverChargedInsigniaDelayBoost = new FloatField(orbStrikeRevolverChargedInsigniaDiv, "Windup speed multiplier", "orbStrikeRevolverChargedInsigniaDelayBoost", 2f, 0f, float.MaxValue);

            new ConfigHeader(orbStrikeDiv, "Electric Cannon", 22);
            FormattedStringBuilder orbStrikeElectricCannonBuilder = new FormattedStringBuilder();
            orbStrikeElectricCannonBuilder.currentFormat.color = new Color(0xff / 255f, 0xa5 / 255f, 0);
            orbStrikeElectricCannonBuilder += "ULTRA";
            orbStrikeElectricCannonBuilder.currentFormat.color = Color.red;
            orbStrikeElectricCannonBuilder += "ORBITAL";
            orbStrikeElectricCannonBuilder.currentFormat.color = Color.cyan;
            orbStrikeElectricCannonBuilder += "ZAP";
            orbStrikeElectricCannonStyleText = new FormattedStringField(orbStrikeDiv, "Style text", "orbStrikeElectricCannonStyleText", orbStrikeElectricCannonBuilder.Build());
            orbStrikeElectricCannonStyleText.onValueChange += (FormattedStringField.FormattedStringValueChangeEvent e) =>
            {
                Plugin.StyleIDs.UpdateID(orbStrikeElectricCannonStyleText.guid, e.formattedString.formattedString);
            };
            orbStrikeElectricCannonStylePoint = new IntField(orbStrikeDiv, "Style point", "orbStrikeElectricCannonStylePoint", 50, 0, int.MaxValue);
            new ConfigHeader(orbStrikeDiv, "--Grenade Explosion Boost--", 12);
            ConfigDivision orbStrikeElectricCannonGrenadeDiv = new ConfigDivision(orbStrikeDiv, "orbStrikeRevolverGrenadeDiv");
            orbStrikeElectricCannonGrenade = new BoolField(orbStrikeDiv, "Enabled", "orbStrikeElectricCannonGrenade", true);
            orbStrikeElectricCannonGrenade.presetLoadPriority = 1;
            orbStrikeElectricCannonGrenade.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                orbStrikeElectricCannonGrenadeDiv.interactable = e.value;
            };
            orbStrikeElectricCannonGrenade.TriggerValueChangeEvent();
            orbStrikeElectricCannonGrenadeExtraSize = new FloatField(orbStrikeElectricCannonGrenadeDiv, "Size bonus percent", "orbStrikeElectricCannonGrenadeExtraSize", 0.3f, 0f, float.MaxValue);
            orbStrikeElectricCannonGrenadeExtraDamage = new FloatField(orbStrikeElectricCannonGrenadeDiv, "Damage bonus percent", "orbStrikeElectricCannonGrenadeExtraDamage", 0f, 0f, float.MaxValue);
            new ConfigHeader(orbStrikeDiv, "--Lightning Bolt On Enemy Hit--", 12);
            ConfigDivision orbStrikeElectricCannonExplosionDiv = new ConfigDivision(orbStrikeDiv, "orbStrikeRevolverGrenadeDiv");
            orbStrikeElectricCannonExplosion = new BoolField(orbStrikeDiv, "Enabled", "orbStrikeElectricCannonExplosion", true);
            orbStrikeElectricCannonExplosion.presetLoadPriority = 1;
            orbStrikeElectricCannonExplosion.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                orbStrikeElectricCannonExplosionDiv.interactable = e.value;
            };
            orbStrikeElectricCannonExplosion.TriggerValueChangeEvent();
            orbStrikeElectricCannonExplosionDamage = new FloatField(orbStrikeElectricCannonExplosionDiv, "Damage multiplier", "orbStrikeElectricCannonExplosionDamage", 1f, 0f, float.MaxValue);
            orbStrikeElectricCannonExplosionSize = new FloatField(orbStrikeElectricCannonExplosionDiv, "Size multiplier", "orbStrikeElectricCannonExplosionSize", 1f, 0f, float.MaxValue);


            new ConfigHeader(orbStrikeDiv, "Malicious Cannon", 22);
            FormattedStringBuilder orbStrikeMaliciousBuilder = new FormattedStringBuilder();
            orbStrikeMaliciousBuilder.currentFormat.color = Color.red;
            orbStrikeMaliciousBuilder += "ORBITAL";
            orbStrikeMaliciousBuilder.currentFormat.color = new Color(0x80 / 255f, 0, 0);
            orbStrikeMaliciousBuilder += "NUKE";
            orbStrikeMaliciousCannonStyleText = new FormattedStringField(orbStrikeDiv, "Style text", "orbStrikeMaliciousCannonStyleText", orbStrikeMaliciousBuilder.Build());
            orbStrikeMaliciousCannonStyleText.onValueChange += (FormattedStringField.FormattedStringValueChangeEvent e) =>
            {
                Plugin.StyleIDs.UpdateID(orbStrikeMaliciousCannonStyleText.guid, e.formattedString.formattedString);
            };
            orbStrikeMaliciousCannonStylePoint = new IntField(orbStrikeDiv, "Style point", "orbStrikeMaliciousCannonStylePoint", 70, 0, int.MaxValue);
            new ConfigHeader(orbStrikeDiv, "--Grenade Explosion Boost--", 12);
            ConfigDivision orbStrikeMaliciousCannonGrenadeDiv = new ConfigDivision(orbStrikeDiv, "orbStrikeMaliciousCannonGrenadeDiv");
            orbStrikeMaliciousCannonGrenade = new BoolField(orbStrikeDiv, "Enabled", "orbStrikeMaliciousCannonGrenade", true);
            orbStrikeMaliciousCannonGrenade.presetLoadPriority = 1;
            orbStrikeMaliciousCannonGrenade.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                orbStrikeMaliciousCannonGrenadeDiv.interactable = e.value;
            };
            orbStrikeMaliciousCannonGrenade.TriggerValueChangeEvent();
            orbStrikeMaliciousCannonGrenadeExtraSize = new FloatField(orbStrikeMaliciousCannonGrenadeDiv, "Size bonus percent", "orbStrikeMaliciousCannonGrenadeExtraSize", 0.4f, 0f, float.MaxValue);
            orbStrikeMaliciousCannonGrenadeExtraDamage = new FloatField(orbStrikeMaliciousCannonGrenadeDiv, "Damage bonus percent", "orbStrikeMaliciousCannonGrenadeExtraDamage", 0f, 0f, float.MaxValue);
            new ConfigHeader(orbStrikeDiv, "--Stronger Malicious Explosion--", 12);
            ConfigDivision orbStrikeMaliciousCannonExplosionDiv = new ConfigDivision(orbStrikeDiv, "orbStrikeMaliciousCannonExplosionDiv");
            orbStrikeMaliciousCannonExplosion = new BoolField(orbStrikeDiv, "Enabled", "orbStrikeMaliciousCannonExplosion", true);
            orbStrikeMaliciousCannonExplosion.presetLoadPriority = 1;
            orbStrikeMaliciousCannonExplosion.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                orbStrikeMaliciousCannonExplosionDiv.interactable = e.value;
            };
            orbStrikeMaliciousCannonExplosion.TriggerValueChangeEvent();
            orbStrikeMaliciousCannonExplosionSizeMultiplier = new FloatField(orbStrikeMaliciousCannonExplosionDiv, "Size multiplier", "orbStrikeMaliciousCannonExplosionSizeMultiplier", 1.3f, 0f, float.MaxValue);
            orbStrikeMaliciousCannonExplosionDamageMultiplier = new FloatField(orbStrikeMaliciousCannonExplosionDiv, "Damage multiplier", "orbStrikeMaliciousCannonExplosionDamageMultiplier", 1f, 0f, float.MaxValue);
            
            new ConfigHeader(orbStrikeDiv, "Malicious Beam Chargeback");
            FormattedStringBuilder maliciousChargebackBuilder = new FormattedStringBuilder();
            maliciousChargebackBuilder.currentFormat.color = new Color(0xff / 255f, 0xa5 / 255f, 0);
            maliciousChargebackBuilder += "ULTRA";
            maliciousChargebackBuilder.currentFormat.color = Color.red;
            maliciousChargebackBuilder += "ORBITAL";
            maliciousChargebackBuilder.currentFormat.color = Color.cyan;
            maliciousChargebackBuilder += "CHARGEBACK";
            maliciousChargebackStyleText = new FormattedStringField(orbStrikeDiv, "Style text", "maliciousChargebackStyleText", maliciousChargebackBuilder.Build());
            maliciousChargebackStyleText.onValueChange += (FormattedStringField.FormattedStringValueChangeEvent e) =>
            {
                Plugin.StyleIDs.UpdateID(maliciousChargebackStyleText.guid, e.formattedString.formattedString);
            };
            maliciousChargebackStylePoint = new IntField(orbStrikeDiv, "Style point", "maliciousChargebackStylePoint", 100, 0, int.MaxValue);
            ConfigDivision maliciousChargebackExplosionDiv = new ConfigDivision(orbStrikeDiv, "maliciousChargebackExplosionDiv");
            maliciousChargebackExplosionToggle = new BoolField(orbStrikeDiv, "Explosion on hit", "maliciousChargebackExplosionToggle", true);
            maliciousChargebackExplosionToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                maliciousChargebackExplosionDiv.interactable = e.value;
            };
            maliciousChargebackExplosionToggle.TriggerValueChangeEvent();
            maliciousChargebackExplosionSizeMultiplier = new FloatField(maliciousChargebackExplosionDiv, "Explosion size multiplier", "maliciousChargebackExplosionSizeMultiplier", 0.6f, 0.1f, float.MaxValue);
            maliciousChargebackExplosionDamageMultiplier = new FloatField(maliciousChargebackExplosionDiv, "Explosion damage multiplier", "maliciousChargebackExplosionDamageMultiplier", 0.5f, 0.1f, float.MaxValue);

            // SENTRY CHARGEBACK
            new ConfigHeader(orbStrikeDiv, "Sentry Beam Chargeback");
            sentryChargebackStyleText = new FormattedStringField(orbStrikeDiv, "Style text", "sentryChargebackStyleText", maliciousChargebackBuilder.Build());
            sentryChargebackStyleText.onValueChange += (FormattedStringField.FormattedStringValueChangeEvent e) =>
            {
                Plugin.StyleIDs.UpdateID(sentryChargebackStyleText.guid, e.formattedString.formattedString);
            };
            sentryChargebackStylePoint = new IntField(orbStrikeDiv, "Style point", "sentryChargebackStylePoint", 100, 0, int.MaxValue);
            sentryChargebackExtraBeamCount = new IntField(orbStrikeDiv, "Beam split count", "sentryChargebackExtraBeamCount", 5, 0, int.MaxValue);
            
            // ENEMY PANEL
            globalEnemyPanel = new ConfigPanel(enemyPanel, "Global enemy tweaks", "globalEnemyPanel");
            new ConfigHeader(enemyPanel, "Common Enemies");
            filthPanel = new ConfigPanel(enemyPanel, "Filth", "filthPanel");
            strayPanel = new ConfigPanel(enemyPanel, "Stray", "strayPanel");
            schismPanel = new ConfigPanel(enemyPanel, "Schism", "schismPanel");
            soliderPanel = new ConfigPanel(enemyPanel, "Soldier", "soliderPanel");
            dronePanel = new ConfigPanel(enemyPanel, "Drone", "dronePanel");
            idolPanel = new ConfigPanel(enemyPanel, "Idol", "idolPanel");
            streetCleanerPanel = new ConfigPanel(enemyPanel, "Streetcleaner", "streetCleanerPanel");
            virtuePanel = new ConfigPanel(enemyPanel, "Virtue", "virtuePanel");
            stalkerPanel = new ConfigPanel(enemyPanel, "Stalker", "stalkerPanel");
            new ConfigHeader(enemyPanel, "Mini Bosses");
            cerberusPanel = new ConfigPanel(enemyPanel, "Cerberus", "cerberusPanel");
            ferrymanPanel = new ConfigPanel(enemyPanel, "Ferryman", "ferrymanPanel");
            hideousMassPanel = new ConfigPanel(enemyPanel, "Hideous Mass", "hideousMassPanel");
            maliciousFacePanel = new ConfigPanel(enemyPanel, "Malicious Face", "maliciousFacePanel");
            mindflayerPanel = new ConfigPanel(enemyPanel, "Mindflayer", "mindflayerPanel");
            turretPanel = new ConfigPanel(enemyPanel, "Sentry", "turretPanel");
            sisyInstPanel = new ConfigPanel(enemyPanel, "Sisyphean Insurrectionist", "sisyInstPanel");
            swordsMachinePanel = new ConfigPanel(enemyPanel, "Swordsmachine", "swordsMachinePanel");
            new ConfigHeader(enemyPanel, "Bosses");
            v2FirstPanel = new ConfigPanel(enemyPanel, "V2 - First", "v2FirstPanel");
            v2SecondPanel = new ConfigPanel(enemyPanel, "V2 - Second", "v2SecondPanel");
            leviathanPanel = new ConfigPanel(enemyPanel, "Leviathan", "leviathanPanel");
            new ConfigHeader(enemyPanel, "Prime Bosses");
            fleshPrisonPanel = new ConfigPanel(enemyPanel, "Flesh Prison", "fleshPrisonPanel");
            minosPrimePanel = new ConfigPanel(enemyPanel, "Minos Prime", "minosPrimePanel");
            panopticonPanel = new ConfigPanel(enemyPanel, "Flesh Panopticon", "panopticonPanel");

            // GLOBAL ENEMY TWEAKS
            eidStatEditorPanel = new ConfigPanel(globalEnemyPanel, "Enemy stat editor", "eidStatEditorPanel");

            eidStatEditorSelector = new EnumField<EnemyType>(eidStatEditorPanel, "Selected enemy", "eidStatEditorSelector", EnemyType.Filth);
            eidStatEditorSelector.SetEnumDisplayName(EnemyType.V2Second, "V2 Second");
            eidStatEditorSelector.SetEnumDisplayName(EnemyType.Sisyphus, "Sisyphean Ins.");
            eidStatEditorSelector.SetEnumDisplayName(EnemyType.SisyphusPrime, "Sisyphus Prime");
            eidStatEditorSelector.SetEnumDisplayName(EnemyType.CancerousRodent, "Cancerous Rodent");
            eidStatEditorSelector.SetEnumDisplayName(EnemyType.FleshPanopticon, "Flesh Panopticon");
            eidStatEditorSelector.SetEnumDisplayName(EnemyType.FleshPrison, "Flesh Prison");
            eidStatEditorSelector.SetEnumDisplayName(EnemyType.GabrielSecond, "Gabriel Second");
            eidStatEditorSelector.SetEnumDisplayName(EnemyType.HideousMass, "Hideous Mass");
            eidStatEditorSelector.SetEnumDisplayName(EnemyType.MaliciousFace, "Malicious Face");
            eidStatEditorSelector.SetEnumDisplayName(EnemyType.Mandalore, "Druid Knight");
            eidStatEditorSelector.SetEnumDisplayName(EnemyType.Minos, "Minos Corpse");
            eidStatEditorSelector.SetEnumDisplayName(EnemyType.MinosPrime, "Minos Prime");
            eidStatEditorSelector.SetEnumDisplayName(EnemyType.VeryCancerousRodent, "Very Cancerous Rodent");
            eidStatEditorSelector.SetEnumDisplayName(EnemyType.Wicked, "Something Wicked");
            eidStatEditorSelector.SetEnumDisplayName(EnemyType.Turret, "Sentry");
            
            new ConfigHeader(eidStatEditorPanel, "Base Stat Editor");
            
            foreach(EnemyType eid in Enum.GetValues(typeof(EnemyType)))
            {
                EidStatContainer container = new EidStatContainer();
                
                container.health = new FloatField(eidStatEditorPanel, "Health multiplier", $"eid_{eid}_health", defaultEnemyHealth.ContainsKey(eid) ? defaultEnemyHealth[eid] : 1f , 0.01f, float.MaxValue);
                container.damage = new FloatField(eidStatEditorPanel, "Damage multiplier", $"eid_{eid}_damage", defaultEnemyDamage.ContainsKey(eid) ? defaultEnemyDamage[eid] : 1f, 0.01f, float.MaxValue);
                container.speed = new FloatField(eidStatEditorPanel, "Speed multiplier", $"eid_{eid}_speed", defaultEnemySpeed.ContainsKey(eid) ? defaultEnemySpeed[eid] : 1f, 0.01f, float.MaxValue);
                
                container.resistanceStr = new StringField(eidStatEditorPanel, "Resistance string", $"eid_{eid}_resistance", "", true);
                container.resistanceStr.interactable = false;
                container.resistanceStr.hidden = true;
                container.resistanceDict = new Dictionary<string, float>();

                string[] resistanceArr = container.resistanceStr.value.Split(resistanceSeparator);
                if(resistanceArr.Length > 0 && !string.IsNullOrEmpty(resistanceArr[0]))
                for(int i = 0; i < resistanceArr.Length; i++)
                {
                    string resistance = resistanceArr[i];
                    i += 1;
                    if(i >= resistanceArr.Length)
                    {
                        Debug.LogWarning($"Missing value for resistance {resistance} for enemy {eid}");
                        break;
                    }
                    float resistanceCoefficiance = 1f;
                    if (!float.TryParse(resistanceArr[i], out resistanceCoefficiance))
                    {
                        Debug.LogWarning($"Invalid value for resistance {resistance} for enemy {eid}");
                        continue;
                    }

                    container.resistanceDict[resistance] = resistanceCoefficiance;
                }
                
                enemyStats.Add(eid, container);
            }

            new ConfigHeader(eidStatEditorPanel, "Resistance Editor");
            eidResistanceSelector = new EnumField<HitterType>(eidStatEditorPanel, "Damage type", "eidResistanceSelector", HitterType.revolver);
            foreach(KeyValuePair<HitterType, string> pair in hitterDisplayNames)
            {
                eidResistanceSelector.SetEnumDisplayName(pair.Key, pair.Value);
            }
            eidResistanceEnabled = new BoolField(eidStatEditorPanel, "Enable resistance", "eidResistanceEnabler", false);
            eidResistanceField = new FloatField(eidStatEditorPanel, "Resistance multiplier", "eidResistanceMultiplier", 1f, 0.1f, float.MaxValue);
            eidResistanceInfo = new ConfigHeader(eidStatEditorPanel, "Current resistance:", 24, TextAnchor.UpperLeft);
            
            ButtonField statResetButton = new ButtonField(eidStatEditorPanel, "Reset All Stats", "statResetButton");
            statResetButton.onClick += () =>
            {
                foreach (EidStatContainer stat in enemyStats.Values)
                {
                    stat.health.value = stat.damage.value = stat.speed.value = 1f;
                    stat.resistanceDict.Clear();
                    stat.resistanceStr.value = "";
                }
                eidResistanceEnabled.value = false;
                eidResistanceField.interactable = false;
                eidResistanceField.value = 1f;
                SetResistanceInfoText(enemyStats[eidStatEditorSelector.value]);
            };

            void SetResistanceInfoText(EidStatContainer container)
            {
                string header = "Current resistance:";
                foreach (KeyValuePair<string, float> pair in container.resistanceDict)
                {
                    if (Enum.TryParse(pair.Key.Replace(' ', '_'), out HitterType type))
                    {
                        header += $"\n{hitterDisplayNames[type]}: {pair.Value}";
                    }
                    else
                    {
                        header += $"\n{pair.Key}: {pair.Value}";
                    }
                }
                eidResistanceInfo.text = header;
            }

            void SetResistanceString(EidStatContainer container)
            {
                StringBuilder str = new StringBuilder();
                foreach(KeyValuePair<string, float> pair in container.resistanceDict)
                {
                    if (str.Length != 0)
                        str.Append(resistanceSeparator);
                    str.Append(pair.Key);
                    str.Append(resistanceSeparator);
                    str.Append(pair.Value.ToString());
                }
                container.resistanceStr.value = str.ToString();
            }

            eidStatEditorSelector.presetLoadPriority = 3;
            eidStatEditorSelector.onValueChange += (EnumField<EnemyType>.EnumValueChangeEvent e) =>
            {
                foreach (KeyValuePair<EnemyType, EidStatContainer> stats in enemyStats)
                    stats.Value.SetHidden(stats.Key != e.value);
                EidStatContainer container = enemyStats[e.value];

                if (container.resistanceDict.TryGetValue(eidResistanceSelector.value.ToString().Replace('_', ' '), out float val))
                {
                    eidResistanceEnabled.value = true;
                    eidResistanceField.interactable = true;
                    eidResistanceField.value = val;
                }
                else
                {
                    eidResistanceEnabled.value = false;
                    eidResistanceField.value = 1f;
                    eidResistanceField.interactable = false;
                }

                SetResistanceInfoText(container);
            };
            eidStatEditorSelector.TriggerValueChangeEvent();
            eidResistanceSelector.presetLoadPriority = 2;
            eidResistanceSelector.onValueChange += (EnumField<HitterType>.EnumValueChangeEvent e) =>
            {
                EidStatContainer container = enemyStats[eidStatEditorSelector.value];
                if (container.resistanceDict.TryGetValue(e.value.ToString().Replace('_', ' '), out float val))
                {
                    eidResistanceEnabled.value = true;
                    eidResistanceField.interactable = true;
                    eidResistanceField.value = val;
                }
                else
                {
                    eidResistanceEnabled.value = false;
                    eidResistanceField.interactable = false;
                    eidResistanceField.value = 1f;
                }
            };
            eidResistanceSelector.TriggerValueChangeEvent();
            eidResistanceEnabled.presetLoadPriority = 1;
            eidResistanceEnabled.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                EidStatContainer container = enemyStats[eidStatEditorSelector.value];
                string currentHitter = eidResistanceSelector.value.ToString().Replace('_', ' ');

                if (e.value)
                {
                    container.resistanceDict[currentHitter] = 1f;
                    eidResistanceField.interactable = true;
                    eidResistanceField.value = 1f;
                }
                else
                {
                    if (container.resistanceDict.ContainsKey(currentHitter))
                        container.resistanceDict.Remove(currentHitter);
                    eidResistanceField.interactable = false;
                    eidResistanceField.value = 1f;
                }

                SetResistanceInfoText(container);
                SetResistanceString(container);
            };
            eidResistanceField.onValueChange += (FloatField.FloatValueChangeEvent e) =>
            {
                if (!eidResistanceEnabled.value)
                    return;

                EidStatContainer container = enemyStats[eidStatEditorSelector.value];
                string currentHitter = eidResistanceSelector.value.ToString().Replace('_', ' ');

                container.resistanceDict[currentHitter] = e.value;
                SetResistanceInfoText(container);
                SetResistanceString(container);
            };

            new ConfigHeader(globalEnemyPanel, "Friendly Fire Damage Override");
            ConfigDivision ffDiv = new ConfigDivision(globalEnemyPanel, "ffDiv");
            friendlyFireDamageOverrideToggle = new BoolField(globalEnemyPanel, "Enabled", "friendlyFireDamageOverrideToggle", true);
            friendlyFireDamageOverrideToggle.presetLoadPriority = 1;
            friendlyFireDamageOverrideToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                ffDiv.interactable = e.value;
                dirtyField = true;
            };
            friendlyFireDamageOverrideToggle.TriggerValueChangeEvent();
            friendlyFireDamageOverrideExplosion = new FloatSliderField(ffDiv, "Friendly-fire:\nExplosion damage %", "friendlyFireDamageOverrideExplosion", new System.Tuple<float, float>(0, 100), 50, 1);
            friendlyFireDamageOverrideProjectile = new FloatSliderField(ffDiv, "Friendly-fire:\nProjectile damage %", "friendlyFireDamageOverrideProjectile", new System.Tuple<float, float>(0, 100), 50, 1);
            friendlyFireDamageOverrideFire = new FloatSliderField(ffDiv, "Friendly-fire:\nFire damage %", "friendlyFireDamageOverrideFire", new System.Tuple<float, float>(0, 100), 50, 1);
            friendlyFireDamageOverrideMelee = new FloatSliderField(ffDiv, "Friendly-fire:\nMelee damage %", "friendlyFireDamageOverrideMelee", new System.Tuple<float, float>(0, 100), 50, 1);
            
            // CERBERUS
            new ConfigHeader(cerberusPanel, "Extra Dashes");
            ConfigDivision cerebusExtraDashDiv = new ConfigDivision(cerberusPanel, "cerberusExtraDashDiv");
            cerberusDashToggle = new BoolField(cerberusPanel, "Enabled", "cerberusDashToggle", true);
            cerberusDashToggle.presetLoadPriority = 1;
            cerberusDashToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                cerebusExtraDashDiv.interactable = e.value;
                dirtyField = true;
            };
            cerberusDashToggle.TriggerValueChangeEvent();
            cerberusTotalDashCount = new IntField(cerebusExtraDashDiv, "Total dash count", "cerberusTotalDashCount", 3, 1, int.MaxValue);
            new ConfigHeader(cerberusPanel, "Parryable");
            ConfigDivision cerberusParryableDiv = new ConfigDivision(cerberusPanel, "cerberusParryableDiv");
            cerberusParryable = new BoolField(cerberusPanel, "Enabled", "cerberusParryable", true);
            cerberusParryable.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                cerberusParryableDiv.interactable = e.value;
                dirtyField = true;
            };
            cerberusParryable.TriggerValueChangeEvent();
            cerberusParryableDuration = new FloatField(cerberusParryableDiv, "Duration", "cerberusParryableDuration", 0.5f, 0f, float.MaxValue);
            cerberusParryDamage = new IntField(cerberusParryableDiv, "Parry damage", "cerberusParryDamage", 5, 0, int.MaxValue);

            // DRONE
            ConfigDivision droneProjectileDiv = new ConfigDivision(dronePanel, "droneProjectileDiv");
            droneProjectileToggle = new BoolField(dronePanel, "Can shoot projectiles", "droneProjectileToggle", true);
            droneProjectileToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                dirtyField = true;
                droneProjectileDiv.interactable = e.value;
            };
            droneProjectileToggle.TriggerValueChangeEvent();
            droneProjectileDelay = new FloatField(droneProjectileDiv, "Projectile shoot delay", "droneProjectileDelay", 0.75f, 0f, float.MaxValue);
            droneProjectileChance = new FloatSliderField(droneProjectileDiv, "Projectile shoot chance", "droneProjectileChance", new Tuple<float, float>(0, 100), 50f, 1);

            new ConfigHeader(dronePanel, "-------");
            ConfigDivision droneExplosionBeamDiv = new ConfigDivision(dronePanel, "droneExplosionBeamDiv");
            droneExplosionBeamToggle = new BoolField(dronePanel, "Can shoot explosions", "droneExplosionBeamToggle", true);
            droneExplosionBeamToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                dirtyField = true;
                droneExplosionBeamDiv.interactable = e.value;
            };
            droneExplosionBeamToggle.TriggerValueChangeEvent();
            droneExplosionBeamDelay = new FloatField(droneExplosionBeamDiv, "Explosion beam delay", "droneExplosionBeamDelay", 0.75f, 0f, float.MaxValue);
            droneExplosionBeamChance = new FloatSliderField(droneExplosionBeamDiv, "Explosion beam chance", "droneExplosionBeamChance", new Tuple<float, float>(0, 100), 30f, 1);

            new ConfigHeader(dronePanel, "-------");
            ConfigDivision droneSentryBeamDiv = new ConfigDivision(dronePanel, "droneSentryBeamDiv");
            droneSentryBeamToggle = new BoolField(dronePanel, "Can shoot sentry beam", "droneSentryBeamToggle", true);
            droneSentryBeamToggle.presetLoadPriority = 1;
            droneSentryBeamToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                droneSentryBeamDiv.interactable = e.value;
                dirtyField = true;
            };
            droneSentryBeamToggle.TriggerValueChangeEvent();
            droneSentryBeamDamage = new FloatField(droneSentryBeamDiv, "Sentry beam damage", "droneSentryBeamDamage", 2f, 0f, float.MaxValue);
            droneSentryBeamDelay = new FloatField(droneSentryBeamDiv, "Sentry beam delay", "droneSentryBeamDelay", 0.75f, 0f, float.MaxValue);
            droneSentryBeamChance = new FloatSliderField(droneSentryBeamDiv, "Sentry beam chance", "droneSentryBeamChance", new Tuple<float, float>(0, 100), 20f, 1);
            ConfigDivision droneSentryBeamLineDiv = new ConfigDivision(droneSentryBeamDiv, "droneSentryBeamLineDiv");
            droneDrawSentryBeamLine = new BoolField(droneSentryBeamDiv, "Draw sentry beam line", "droneDrawSentryBeamLine", true);
            droneDrawSentryBeamLine.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                droneSentryBeamLineDiv.interactable = e.value;
            };
            droneSentryBeamLineNormalColor = new ColorField(droneSentryBeamLineDiv, "Normal color", "droneSentryBeamLineNormalColor", Drone_PlaySound_Patch.defaultLineColor);
            droneSentryBeamLineWarningColor = new ColorField(droneSentryBeamLineDiv, "Warning color", "droneSentryBeamLineWarningColor", new Color(1, 1, 1));
            droneSentryBeamLineIndicatorDelay = new FloatField(droneSentryBeamLineDiv, "Sentry beam warning delay", "droneSentryBeamLineIndicatorDelay", 0.5f, 0f, float.MaxValue);
            
            new ConfigHeader(dronePanel, "Homing Drone After Death");
            droneHomeToggle = new BoolField(dronePanel, "Enabled", "droneHomeToggle", true);
            droneHomeToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                droneHomeTurnSpeed.interactable = e.value;
                dirtyField = true;
            };
            droneHomeTurnSpeed = new FloatField(dronePanel, "Turn speed", "droneHomeTurnSpeed", 360f, 0f, float.MaxValue);
            droneHomeToggle.TriggerValueChangeEvent();
            
            // FILTH
            new ConfigHeader(filthPanel, "Explode On Hit");
            ConfigDivision filthExplosionDiv = new ConfigDivision(filthPanel, "filthExplosionDiv");
            filthExplodeToggle = new BoolField(filthPanel, "Enabled", "filthExplodeOnHit", true);
            filthExplodeToggle.presetLoadPriority = 1;
            filthExplodeToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                filthExplosionDiv.interactable = e.value;
                dirtyField = true;
            };
            filthExplodeToggle.TriggerValueChangeEvent();
            filthExplodeKills = new BoolField(filthExplosionDiv, "Explosion kills the filth", "filthExplosionKills", false);
            filthExplosionDamage = new IntField(filthExplosionDiv, "Explosion damage", "filthExplosionDamage", 30, 0, int.MaxValue);
            filthExplosionSize = new FloatField(filthExplosionDiv, "Explosion size", "filthExplosionSize", 0.5f, 0f, float.MaxValue);

            // HIDEOUS MASS
            new ConfigHeader(hideousMassPanel, "Insignia On Projectile Hit");
            ConfigDivision hideousMassInsigniaDiv = new ConfigDivision(hideousMassPanel, "hideousMassInsigniaDiv");
            hideousMassInsigniaToggle = new BoolField(hideousMassPanel, "Enabled", "hideousMassInsigniaToggle", true);
            hideousMassInsigniaToggle.presetLoadPriority = 1;
            hideousMassInsigniaToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                hideousMassInsigniaDiv.interactable = e.value;
                dirtyField = true;
            };
            hideousMassInsigniaToggle.TriggerValueChangeEvent();
            hideousMassInsigniaSpeed = new FloatField(hideousMassInsigniaDiv, "Insignia speed multiplier", "hideousMassInsigniaSpeed", 2.5f, 0f, float.MaxValue);
            new ConfigHeader(hideousMassInsigniaDiv, "Vertical Insignia", 12);
            ConfigDivision hideousMassInsigniaYdiv = new ConfigDivision(hideousMassInsigniaDiv, "hideousMassInsigniaYdiv");
            hideousMassInsigniaYtoggle = new BoolField(hideousMassInsigniaDiv, "Enabled", "hideousMassInsigniaYtoggle", true);
            hideousMassInsigniaYtoggle.presetLoadPriority = 1;
            hideousMassInsigniaYtoggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                hideousMassInsigniaYdiv.interactable = e.value;
            };
            hideousMassInsigniaYtoggle.TriggerValueChangeEvent();
            hideousMassInsigniaYdamage = new IntField(hideousMassInsigniaYdiv, "Damage", "hideousMassInsigniaYdamage", 20, 0, int.MaxValue);
            hideousMassInsigniaYsize = new FloatField(hideousMassInsigniaYdiv, "Size", "hideousMassInsigniaYsize", 2f, 0f, float.MaxValue);
            new ConfigHeader(hideousMassInsigniaDiv, "Forward Insignia", 12);
            ConfigDivision hideousMassInsigniaZdiv = new ConfigDivision(hideousMassInsigniaDiv, "hideousMassInsigniaZdiv");
            hideousMassInsigniaZtoggle = new BoolField(hideousMassInsigniaDiv, "Enabled", "hideousMassInsigniaZtoggle", false);
            hideousMassInsigniaZtoggle.presetLoadPriority = 1;
            hideousMassInsigniaZtoggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                hideousMassInsigniaZdiv.interactable = e.value;
            };
            hideousMassInsigniaZtoggle.TriggerValueChangeEvent();
            hideousMassInsigniaZdamage = new IntField(hideousMassInsigniaZdiv, "Damage", "hideousMassInsigniaZdamage", 20, 0, int.MaxValue);
            hideousMassInsigniaZsize = new FloatField(hideousMassInsigniaZdiv, "Size", "hideousMassInsigniaZsize", 2f, 0f, float.MaxValue);
            new ConfigHeader(hideousMassInsigniaDiv, "Side Insignia", 12);
            ConfigDivision hideousMassInsigniaXdiv = new ConfigDivision(hideousMassInsigniaDiv, "hideousMassInsigniaXdiv");
            hideousMassInsigniaXtoggle = new BoolField(hideousMassInsigniaDiv, "Enabled", "hideousMassInsigniaXtoggle", false);
            hideousMassInsigniaXtoggle.presetLoadPriority = 1;
            hideousMassInsigniaXtoggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                hideousMassInsigniaXdiv.interactable = e.value;
            };
            hideousMassInsigniaXtoggle.TriggerValueChangeEvent();
            hideousMassInsigniaXdamage = new IntField(hideousMassInsigniaXdiv, "Damage", "hideousMassInsigniaXdamage", 20, 0, int.MaxValue);
            hideousMassInsigniaXsize = new FloatField(hideousMassInsigniaXdiv, "Size", "hideousMassInsigniaXsize", 2f, 0f, float.MaxValue);

            // MALICIOUS FACE
            new ConfigHeader(maliciousFacePanel, "Radiance When Enraged");
            ConfigDivision maliciousFaceRadianceOnEnrageDiv = new ConfigDivision(maliciousFacePanel, "maliciousFaceRadianceOnEnrageDiv");
            maliciousFaceRadianceOnEnrage = new BoolField(maliciousFacePanel, "Enabled", "maliciousFaceRadianceWhenEnraged", true);
            maliciousFaceRadianceOnEnrage.presetLoadPriority = 1;
            maliciousFaceRadianceOnEnrage.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                maliciousFaceRadianceOnEnrageDiv.interactable = e.value;
                dirtyField = true;
            };
            maliciousFaceRadianceOnEnrage.TriggerValueChangeEvent();
            maliciousFaceRadianceAmount = new IntField(maliciousFaceRadianceOnEnrageDiv, "Radiance level", "maliciousFaceRadianceAmount", 1, 0, int.MaxValue);
            new ConfigHeader(maliciousFacePanel, "Homing Projectile");
            ConfigDivision maliciousFaceHomingProjecileDiv = new ConfigDivision(maliciousFacePanel, "maliciousFaceHomingProjecileDiv");
            maliciousFaceHomingProjectileToggle = new BoolField(maliciousFacePanel, "Enabled", "maliciousFaceHomingProjectileToggle", true);
            maliciousFaceHomingProjectileToggle.presetLoadPriority = 1;
            maliciousFaceHomingProjectileToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                maliciousFaceHomingProjecileDiv.interactable = e.value;
                dirtyField = true;
            };
            maliciousFaceHomingProjectileToggle.TriggerValueChangeEvent();
            maliciousFaceHomingProjectileCount = new IntField(maliciousFaceHomingProjecileDiv, "Projectile count", "maliciousFaceHomingProjectileCount", 5, 0, int.MaxValue);
            maliciousFaceHomingProjectileDamage = new IntField(maliciousFaceHomingProjecileDiv, "Projectile damage", "maliciousFaceHomingProjectileDamage", 25, 0, int.MaxValue);
            maliciousFaceHomingProjectileSpeed = new FloatField(maliciousFaceHomingProjecileDiv, "Projectile speed", "maliciousFaceHomingProjectileSpeed", 20f, 0f, float.MaxValue);
            maliciousFaceHomingProjectileTurnSpeed = new FloatField(maliciousFaceHomingProjecileDiv, "Projectile turn speed", "maliciousFaceHomingProjectileTurnSpeed", 0.4f, 0f, float.MaxValue);
            new ConfigHeader(maliciousFacePanel, "Beam Count");
            maliciousFaceBeamCountNormal = new IntField(maliciousFacePanel, "Normal state", "maliciousFaceBeamCountNormal", 1, 0, int.MaxValue);
            maliciousFaceBeamCountEnraged = new IntField(maliciousFacePanel, "Enraged state", "maliciousFaceBeamCountEnraged", 2, 0, int.MaxValue);

            // MINDFLAYER
            new ConfigHeader(mindflayerPanel, "Shoot Tweak");
            ConfigDivision mindflayerShootTweakDiv = new ConfigDivision(mindflayerPanel, "mindflayerShootTweakDiv");
            mindflayerShootTweakToggle = new BoolField(mindflayerPanel, "Enabled", "mindflayerShootTweakToggle", true);
            mindflayerShootTweakToggle.presetLoadPriority = 1;
            mindflayerShootTweakToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                mindflayerShootTweakDiv.interactable = e.value;
                dirtyField = true;
            };
            mindflayerShootTweakToggle.TriggerValueChangeEvent();
            mindflayerShootAmount = new IntField(mindflayerShootTweakDiv, "Projectile amount", "mindflayerShootProjectileAmount", 20, 0, int.MaxValue);
            mindflayerShootDelay = new FloatField(mindflayerShootTweakDiv, "Delay between shots", "mindflayerShootProjectileDelay", 0.02f, 0f, float.MaxValue);
            mindflayerShootInitialSpeed = new FloatField(mindflayerShootTweakDiv, "Initial speed", "mindflayerShootInitialSpeed", 10f, 0f, float.MaxValue);
            mindflayerShootTurnSpeed = new FloatField(mindflayerShootTweakDiv, "Turn speed", "mindflayerShootTurnSpeed", 1f, 0f, float.MaxValue);
            mindflayerProjectileSelfDamageMultiplier = new FloatSliderField(mindflayerShootTweakDiv, "Friendly fire damage percent", "mindflayerProjectileSelfDamageMultiplier", new Tuple<float, float>(0f, 100f), 25f, 1);
            new ConfigHeader(mindflayerPanel, "Melee Combo");
            ConfigDivision mindflayerMeleeDiv = new ConfigDivision(mindflayerPanel, "mindflayerMeleeDiv");
            mindflayerTeleportComboToggle = new BoolField(mindflayerPanel, "Enabled", "mindflayerMeleeCombo", true);
            mindflayerTeleportComboToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                dirtyField = true;
            };

            // SHCISM
            new ConfigHeader(schismPanel, "Spread Attack");
            ConfigDivision schismSpreadAttackDiv = new ConfigDivision(schismPanel, "schismSpreadAttackDiv");
            schismSpreadAttackToggle = new BoolField(schismPanel, "Enabled", "schismSpreadAttackToggle", true);
            schismSpreadAttackToggle.presetLoadPriority = 1;
            schismSpreadAttackToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                schismSpreadAttackDiv.interactable = e.value;
                dirtyField = true;
            };
            schismSpreadAttackToggle.TriggerValueChangeEvent();
            schismSpreadAttackAngle = new FloatSliderField(schismSpreadAttackDiv, "Angular spread", "schismSpreadAttackAngle", new System.Tuple<float, float>(0, 360), 15, 1);
            schismSpreadAttackCount = new IntField(schismSpreadAttackDiv, "Projectile count per side", "schismSpreadAttackCount", 1, 0, int.MaxValue);

            // SOLIDER
            new ConfigHeader(soliderPanel, "Coins Ignore Weak Point");
            soliderCoinsIgnoreWeakPointToggle = new BoolField(soliderPanel, "Enabled", "soliderCoinsIgnoreWeakPoint", true);
            soliderCoinsIgnoreWeakPointToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                dirtyField = true;
            };
            new ConfigHeader(soliderPanel, "Shoot Tweak");
            ConfigDivision soliderShootTweakDiv = new ConfigDivision(soliderPanel, "soliderShootTweakDiv");
            soliderShootTweakToggle = new BoolField(soliderPanel, "Enabled", "soliderShootTweakToggle", true);
            soliderShootTweakToggle.presetLoadPriority = 1;
            soliderShootTweakToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                soliderShootTweakDiv.interactable = e.value;
                dirtyField = true;
            };
            soliderShootTweakToggle.TriggerValueChangeEvent();
            soliderShootCount = new IntField(soliderShootTweakDiv, "Shoot count", "soliderShootCount", 3, 1, int.MaxValue);
            new ConfigHeader(soliderPanel, "Shoot Grenade");
            ConfigDivision soliderShootGrenadeDiv = new ConfigDivision(soliderPanel, "soliderShootGrenadeDiv");
            soliderShootGrenadeToggle = new BoolField(soliderPanel, "Enabled", "soliderShootGrenade", true);
            soliderShootGrenadeToggle.presetLoadPriority = 1;
            soliderShootGrenadeToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                soliderShootGrenadeDiv.interactable = e.value;
                dirtyField = true;
            };
            soliderGrenadeDamage = new IntField(soliderShootGrenadeDiv, "Explosion damage", "soliderGrenadeDamage", 10, 0, int.MaxValue);
            soliderGrenadeSize = new FloatField(soliderShootGrenadeDiv, "Explosion size multiplier", "soliderGrenadeSize", 0.75f, 0f, float.MaxValue);

            // STALKER
            new ConfigHeader(stalkerPanel, "Survive Explosion");
            stalkerSurviveExplosion = new BoolField(stalkerPanel, "Enabled", "stalkerSurviveExplosion", true);
            new ConfigHeader(stalkerPanel, "Spread Radiance");
            stalkerSpreadHealthRad = new BoolField(stalkerPanel, "Spread health buff", "stalkerSpreadHealthRad", true);
            stalkerSpreadHealthAddition = new IntField(stalkerPanel, "Spread health additional level", "stalkerSpreadHealthAddition", 1, 0, int.MaxValue);
            stalkerSpreadDamageRad = new BoolField(stalkerPanel, "Spread damage buff", "stalkerSpreadDamageRad", true);
            stalkerSpreadDamageAddition = new IntField(stalkerPanel, "Spread damage additional level", "stalkerSpreadDamageAddition", 1, 0, int.MaxValue);
            stalkerSpreadSpeedRad = new BoolField(stalkerPanel, "Spread health buff", "stalkerSpreadSpeedRad", true);
            stalkerSpreadSpeedAddition = new IntField(stalkerPanel, "Spread health additional level", "stalkerSpreadSpeedAddition", 1, 0, int.MaxValue);

            // STRAY
            new ConfigHeader(strayPanel, "Shoot Tweak");
            ConfigDivision strayShootDiv = new ConfigDivision(strayPanel, "strayShootDiv");
            strayShootToggle = new BoolField(strayPanel, "Enabled", "strayShootToggle", true);
            strayShootToggle.presetLoadPriority = 1;
            strayShootToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                strayShootDiv.interactable = e.value;
                dirtyField = true;
            };
            strayShootToggle.TriggerValueChangeEvent();
            strayShootCount = new IntField(strayShootDiv, "Extra projectile count", "strayShootCount", 2, 1, int.MaxValue);
            strayShootSpeed = new FloatField(strayShootDiv, "Shoot speed", "strayShootSpeed", 20f, 0f, float.MaxValue);
            new ConfigHeader(strayPanel, "Coins Ignore Weak Point");
            strayCoinsIgnoreWeakPointToggle = new BoolField(strayPanel, "Enabled", "strayCoinsIgnoreWeakPointToggle", true);
            strayCoinsIgnoreWeakPointToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                dirtyField = true;
            };

            // STREET CLEANER
            new ConfigHeader(streetCleanerPanel, "Predictive Parry");
            streetCleanerPredictiveDodgeToggle = new BoolField(streetCleanerPanel, "Enabled", "streetCleanerPredictiveDodgeToggle", true);
            streetCleanerPredictiveDodgeToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                dirtyField = true;
            };
            new ConfigHeader(streetCleanerPanel, "Coins Ignore Weak Point");
            streetCleanerCoinsIgnoreWeakPointToggle = new BoolField(streetCleanerPanel, "Enabled", "streetCleanerCoinsIgnoreWeakPointToggle", true);
            streetCleanerCoinsIgnoreWeakPointToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                dirtyField = true;
            };

            // SWORDS MACHINE
            new ConfigHeader(swordsMachinePanel, "Knockback Modifier");
            swordsMachineNoLightKnockbackToggle = new BoolField(swordsMachinePanel, "No light knockback", "swordsMachineNoLightKnockbackToggle", true);
            swordsMachineNoLightKnockbackToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                dirtyField = true;
            };
            swordsMachineSecondPhaseMode = new EnumField<SwordsMachineSecondPhase>(swordsMachinePanel, "Second phase", "swordsMachineSecondPhaseMode", SwordsMachineSecondPhase.SpeedUp);
            swordsMachineSecondPhaseSpeed = new FloatField(swordsMachinePanel, "Speed multiplier", "swordsMachineSecondPhaseSpeed", 2.5f, 1f, float.MaxValue);
            swordsMachineSecondPhaseMode.onValueChange += (EnumField<SwordsMachineSecondPhase>.EnumValueChangeEvent e) =>
            {
                dirtyField = true;
                swordsMachineSecondPhaseSpeed.hidden = e.value != SwordsMachineSecondPhase.SpeedUp;
            };
            swordsMachineSecondPhaseMode.TriggerValueChangeEvent();

            new ConfigHeader(swordsMachinePanel, "Explosive Sword Throw");
            ConfigDivision swordsMachineExplosiveSwordDiv = new ConfigDivision(swordsMachinePanel, "swordsMachineExplosiveSwordDiv");
            swordsMachineExplosiveSwordToggle = new BoolField(swordsMachinePanel, "Enabled", "swordsMachineExplosiveSwordToggle", true);
            swordsMachineExplosiveSwordToggle.presetLoadPriority = 1;
            swordsMachineExplosiveSwordToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                swordsMachineExplosiveSwordDiv.interactable = e.value;
                dirtyField = true;
            };
            swordsMachineExplosiveSwordToggle.TriggerValueChangeEvent();
            swordsMachineExplosiveSwordDamage = new IntField(swordsMachineExplosiveSwordDiv, "Explosion damage", "swordsMachineExplosiveSwordDamage", 20, 0, int.MaxValue);
            swordsMachineExplosiveSwordSize = new FloatField(swordsMachineExplosiveSwordDiv, "Explosion size multiplier", "swordsMachineExplosiveSwordSize", 0.5f, 0f, float.MaxValue);

            // VIRTUE
            new ConfigHeader(virtuePanel, "Tweak Normal Attack");
            ConfigDivision virtueTweakNormalAttackDiv = new ConfigDivision(virtuePanel, "virtueTweakNormalAttackDiv");
            virtueTweakNormalAttackToggle = new BoolField(virtuePanel, "Enabled", "virtueTweakNormalAttackToggle", true);
            virtueTweakNormalAttackToggle.presetLoadPriority = 1;
            virtueTweakNormalAttackToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                virtueTweakNormalAttackDiv.interactable = e.value;
                dirtyField = true;
            };
            virtueTweakNormalAttackToggle.TriggerValueChangeEvent();
            virtueNormalAttackType = new EnumField<VirtueAttackType>(virtueTweakNormalAttackDiv, "Attack type", "virtueNormalAttackType", VirtueAttackType.Insignia);

            ConfigDivision virtueNormalInsigniaDiv = new ConfigDivision(virtueTweakNormalAttackDiv, "virtueNormalInsigniaDiv");
            virtueNormalInsigniaLastMulti = new FloatField(virtueNormalInsigniaDiv, "Insignia last multiplier", "virtueNormalInsigniaLastMulti", 1f, 0.1f, float.MaxValue);
            ConfigDivision virtueNormalYInsigniaDiv = new ConfigDivision(virtueNormalInsigniaDiv, "virtueNormalYInsigniaDiv");
            new ConfigHeader(virtueNormalInsigniaDiv, "Vertical Insignia", 12);
            virtueNormalInsigniaYtoggle = new BoolField(virtueNormalInsigniaDiv, "Enabled", "virtueNormalInsigniaYtoggle", true);
            virtueNormalInsigniaYtoggle.presetLoadPriority = 1;
            virtueNormalInsigniaYtoggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                virtueNormalYInsigniaDiv.interactable = e.value;
            };
            virtueNormalInsigniaYtoggle.TriggerValueChangeEvent();
            virtueNormalInsigniaYdamage = new IntField(virtueNormalYInsigniaDiv, "Damage", "virtueNormalInsigniaYdamage", 30, 0, int.MaxValue);
            virtueNormalInsigniaYsize = new FloatField(virtueNormalYInsigniaDiv, "Size", "virtueNormalInsigniaYsize", 2f, 0f, float.MaxValue);

            ConfigDivision virtueNormalZInsigniaDiv = new ConfigDivision(virtueNormalInsigniaDiv, "virtueNormalZInsigniaDiv");
            new ConfigHeader(virtueNormalInsigniaDiv, "Forward Insignia", 12);
            virtueNormalInsigniaZtoggle = new BoolField(virtueNormalInsigniaDiv, "Enabled", "virtueNormalInsigniaZtoggle", false);
            virtueNormalInsigniaZtoggle.presetLoadPriority = 1;
            virtueNormalInsigniaZtoggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                virtueNormalZInsigniaDiv.interactable = e.value;
            };
            virtueNormalInsigniaZtoggle.TriggerValueChangeEvent();
            virtueNormalInsigniaZdamage = new IntField(virtueNormalZInsigniaDiv, "Damage", "virtueNormalInsigniaZdamage", 15, 0, int.MaxValue);
            virtueNormalInsigniaZsize = new FloatField(virtueNormalZInsigniaDiv, "Size", "virtueNormalInsigniaZsize", 2f, 0f, float.MaxValue);

            ConfigDivision virtueNormalXInsigniaDiv = new ConfigDivision(virtueNormalInsigniaDiv, "virtueNormalXInsigniaDiv");
            new ConfigHeader(virtueNormalInsigniaDiv, "Side Insignia", 12);
            virtueNormalInsigniaXtoggle = new BoolField(virtueNormalInsigniaDiv, "Enabled", "virtueNormalInsigniaXtoggle", false);
            virtueNormalInsigniaXtoggle.presetLoadPriority = 1;
            virtueNormalInsigniaXtoggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                virtueNormalXInsigniaDiv.interactable = e.value;
            };
            virtueNormalInsigniaXtoggle.TriggerValueChangeEvent();
            virtueNormalInsigniaXdamage = new IntField(virtueNormalXInsigniaDiv, "Damage", "virtueNormalInsigniaXdamage", 15, 0, int.MaxValue);
            virtueNormalInsigniaXsize = new FloatField(virtueNormalXInsigniaDiv, "Size", "virtueNormalInsigniaXsize", 2f, 0f, float.MaxValue);

            ConfigDivision virtueNormalLigthningDiv = new ConfigDivision(virtueTweakNormalAttackDiv, "virtueNormalLigthningDiv");
            virtueNormalLightningDamage = new FloatField(virtueNormalLigthningDiv, "Damage multiplier", "virtueNormalLightningDamage", 1f, 0f, float.MaxValue);
            //virtueNormalLightningSize = new FloatField(virtuePanel, "Size multiplier", "virtueNormalLightningSize", 1f);
            virtueNormalLightningDelay = new FloatField(virtueNormalLigthningDiv, "Lighning delay", "virtueNormalLightningDelay", 3f, 0f, float.MaxValue);

            virtueNormalAttackType.presetLoadPriority = 1;
            virtueNormalAttackType.onValueChange += (EnumField<VirtueAttackType>.EnumValueChangeEvent newType) =>
            {
                if (newType.value == VirtueAttackType.Insignia)
                {
                    virtueNormalInsigniaDiv.hidden = false;
                    virtueNormalLigthningDiv.hidden = true;
                }
                else
                {
                    virtueNormalInsigniaDiv.hidden = true;
                    virtueNormalLigthningDiv.hidden = false;
                }
            };
            virtueNormalAttackType.TriggerValueChangeEvent();

            new ConfigHeader(virtuePanel, "Tweak Enraged Attack");
            ConfigDivision virtueTweakEnragedAttackDiv = new ConfigDivision(virtuePanel, "virtueTweakEnragedAttackDiv");
            virtueTweakEnragedAttackToggle = new BoolField(virtuePanel, "Enabled", "virtueTweakEnragedAttackToggle", true);
            virtueTweakEnragedAttackToggle.presetLoadPriority = 1;
            virtueTweakEnragedAttackToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                virtueTweakEnragedAttackDiv.interactable = e.value;
                dirtyField = true;
            };
            virtueTweakEnragedAttackToggle.TriggerValueChangeEvent();
            virtueEnragedAttackType = new EnumField<VirtueAttackType>(virtueTweakEnragedAttackDiv, "Attack type", "virtueEnragedAttackType", VirtueAttackType.Insignia);

            ConfigDivision virtueEnragedInsigniaDiv = new ConfigDivision(virtueTweakEnragedAttackDiv, "virtueEnragedInsigniaDiv");
            virtueEnragedInsigniaLastMulti = new FloatField(virtueEnragedInsigniaDiv, "Insignia last multiplier", "virtueEnragedInsigniaLastMulti", 1f, 0.1f, float.MaxValue);
            ConfigHeader virtueEnragedYInsigniaHeader = new ConfigHeader(virtueEnragedInsigniaDiv, "Vertical Insignia", 12);
            ConfigDivision virtueEnragedYInsigniaDiv = new ConfigDivision(virtueEnragedInsigniaDiv, "virtueEnragedYInsigniaDiv");
            virtueEnragedInsigniaYtoggle = new BoolField(virtueEnragedInsigniaDiv, "Enabled", "virtueEnragedInsigniaYtoggle", true);
            virtueEnragedInsigniaYtoggle.presetLoadPriority = 1;
            virtueEnragedInsigniaYtoggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                virtueEnragedYInsigniaDiv.interactable = e.value;
                dirtyField = true;
            };
            virtueEnragedInsigniaYtoggle.TriggerValueChangeEvent();
            virtueEnragedInsigniaYdamage = new IntField(virtueEnragedYInsigniaDiv, "Damage", "virtueEnragedInsigniaYdamage", 30, 0, int.MaxValue);
            virtueEnragedInsigniaYsize = new FloatField(virtueEnragedYInsigniaDiv, "Size", "virtueEnragedInsigniaYsize", 2f, 0f, float.MaxValue);

            ConfigHeader virtueEnragedZInsigniaHeader = new ConfigHeader(virtueEnragedInsigniaDiv, "Forward Insignia", 12);
            ConfigDivision virtueEnragedZInsigniaDiv = new ConfigDivision(virtueEnragedInsigniaDiv, "virtueEnragedZInsigniaDiv");
            virtueEnragedInsigniaZtoggle = new BoolField(virtueEnragedInsigniaDiv, "Enabled", "virtueEnragedInsigniaZtoggle", true);
            virtueEnragedInsigniaZtoggle.presetLoadPriority = 1;
            virtueEnragedInsigniaZtoggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                virtueEnragedZInsigniaDiv.interactable = e.value;
                dirtyField = true;
            };
            virtueEnragedInsigniaZtoggle.TriggerValueChangeEvent();
            virtueEnragedInsigniaZdamage = new IntField(virtueEnragedZInsigniaDiv, "Damage", "virtueEnragedInsigniaZdamage", 15, 0, int.MaxValue);
            virtueEnragedInsigniaZsize = new FloatField(virtueEnragedZInsigniaDiv, "Size", "virtueEnragedInsigniaZsize", 2f, 0f, float.MaxValue);

            ConfigHeader virtueEnragedXInsigniaHeader = new ConfigHeader(virtueEnragedInsigniaDiv, "Side Insignia", 12);
            ConfigDivision virtueEnragedXInsigniaDiv = new ConfigDivision(virtueEnragedInsigniaDiv, "virtueEnragedXInsigniaDiv");
            virtueEnragedInsigniaXtoggle = new BoolField(virtueEnragedInsigniaDiv, "Enabled", "virtueEnragedInsigniaXtoggle", true);
            virtueEnragedInsigniaXtoggle.presetLoadPriority = 1;
            virtueEnragedInsigniaXtoggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                virtueEnragedXInsigniaDiv.interactable = e.value;
                dirtyField = true;
            };
            virtueEnragedInsigniaXtoggle.TriggerValueChangeEvent();
            virtueEnragedInsigniaXdamage = new IntField(virtueEnragedXInsigniaDiv, "Damage", "virtueEnragedInsigniaXdamage", 15, 0, int.MaxValue);
            virtueEnragedInsigniaXsize = new FloatField(virtueEnragedXInsigniaDiv, "Size", "virtueEnragedInsigniaXsize", 2f, 0f, float.MaxValue);

            ConfigDivision virtueEnragedLigthningDiv = new ConfigDivision(virtueTweakEnragedAttackDiv, "virtueEnragedLigthningDiv");
            virtueEnragedLightningDamage = new FloatField(virtueEnragedLigthningDiv, "Damage multiplier", "virtueEnragedLightningDamage", 1f, 0f, float.MaxValue);
            //virtueEnragedLightningSize = new FloatField(virtuePanel, "Size multiplier", "virtueEnragedLightningSize", 1f);
            virtueEnragedLightningDelay = new FloatField(virtueEnragedLigthningDiv, "Lighning delay", "virtueEnragedLightningDelay", 2f, 0f, float.MaxValue);

            virtueEnragedAttackType.presetLoadPriority = 1;
            virtueEnragedAttackType.onValueChange += (EnumField<VirtueAttackType>.EnumValueChangeEvent newType) =>
            {
                if (newType.value == VirtueAttackType.Insignia)
                {
                    virtueEnragedInsigniaDiv.hidden = false;
                    virtueEnragedLigthningDiv.hidden = true;
                }
                else
                {
                    virtueEnragedInsigniaDiv.hidden = true;
                    virtueEnragedLigthningDiv.hidden = false;
                }
            };
            virtueEnragedAttackType.TriggerValueChangeEvent();

            // FERRYMAN
            new ConfigHeader(ferrymanPanel, "Melee Combo");
            ConfigDivision ferrymanComboDiv = new ConfigDivision(ferrymanPanel, "ferrymanComboDiv");
            ferrymanComboToggle = new BoolField(ferrymanPanel, "Enabled", "ferrymanComboToggle", true);
            ferrymanComboToggle.presetLoadPriority = 1;
            ferrymanComboToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                ferrymanComboDiv.interactable = e.value;
                dirtyField = true;
            };
            ferrymanComboToggle.TriggerValueChangeEvent();
            ferrymanComboCount = new IntField(ferrymanComboDiv, "Count", "ferrymanComboCount", 3, 1, int.MaxValue);
            ferrymanAttackDelay = new FloatField(ferrymanComboDiv, "Delay (0-1)", "ferrymanAttackDelay", 0.1f, 0f, float.MaxValue);

            // SENTRY
            new ConfigHeader(turretPanel, "Burst Fire");
            ConfigDivision turretBurstFireDiv = new ConfigDivision(turretPanel, "turretBurstFireDiv");
            turretBurstFireToggle = new BoolField(turretPanel, "Enabled", "turretBurstFireToggle", true);
            turretBurstFireToggle.presetLoadPriority = 1;
            turretBurstFireToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                turretBurstFireDiv.interactable = e.value;
                dirtyField = true;
            };
            turretBurstFireToggle.TriggerValueChangeEvent();
            turretBurstFireCount = new IntField(turretBurstFireDiv, "Extra shots", "turretBurstFireCount", 1, 0, int.MaxValue);
            turretBurstFireDelay = new FloatField(turretBurstFireDiv, "Delay between shots", "turretBurstFireDelay", 1f, 0f, float.MaxValue);

            // FLESH PRISON
            new ConfigHeader(fleshPrisonPanel, "Spin Insignia");
            ConfigDivision fleshPrionSpinAttackDiv = new ConfigDivision(fleshPrisonPanel, "fleshPrionSpinAttackDiv");
            fleshPrisonSpinAttackToggle = new BoolField(fleshPrisonPanel, "Enabled", "fleshPrisonSpinAttackToggle", true);
            fleshPrisonSpinAttackToggle.presetLoadPriority = 1;
            fleshPrisonSpinAttackToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                fleshPrionSpinAttackDiv.interactable = e.value;
                dirtyField = true;
            };
            fleshPrisonSpinAttackToggle.TriggerValueChangeEvent();
            fleshPrisonSpinAttackCount = new IntField(fleshPrionSpinAttackDiv, "Insignia count", "fleshPrisonSpinAttackCount", 5, 1, int.MaxValue);
            fleshPrisonSpinAttackDamage = new IntField(fleshPrionSpinAttackDiv, "Insignia damage", "fleshPrisonSpinAttackDamage", 10, 0, int.MaxValue);
            fleshPrisonSpinAttackSize = new FloatField(fleshPrionSpinAttackDiv, "Insignia size", "fleshPrisonSpinAttackSize", 2f, 0f, float.MaxValue);
            fleshPrisonSpinAttackDistance = new FloatField(fleshPrionSpinAttackDiv, "Circle radius", "fleshPrisonSpinAttackDistance", 30f, 0f, float.MaxValue);
            fleshPrisonSpinAttackTurnSpeed = new FloatField(fleshPrionSpinAttackDiv, "Turn speed", "fleshPrisonSpinAttackTurnSpeed", 30f, 0f, float.MaxValue);
            fleshPrisonSpinAttackActivateSpeed = new FloatField(fleshPrionSpinAttackDiv, "Activasion speed", "fleshPrisonSpinAttackActivateSpeed", 0.5f, 0f, float.MaxValue);

            // MINOS PRIME
            minosPrimeEarlyPhaseToggle = new BoolField(minosPrimePanel, "Start with phase 2", "minosPrimeEarlyPhaseToggle", true);
            minosPrimeComboToggle = new BoolField(minosPrimePanel, "Faster combos", "minosPrimeComboToggle", true);
            new ConfigHeader(minosPrimePanel, "Random Teleport Before Shoot");
            ConfigDivision minosPrimeRandomTeleportDiv = new ConfigDivision(minosPrimePanel, "minosPrimeRandomTeleportDiv");
            minosPrimeRandomTeleportToggle = new BoolField(minosPrimePanel, "Enabled", "minosPrimeRandomTeleportToggle", true);
            minosPrimeRandomTeleportToggle.presetLoadPriority = 1;
            minosPrimeRandomTeleportToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                minosPrimeRandomTeleportDiv.interactable = e.value;
                dirtyField = true;
            };
            minosPrimeRandomTeleportToggle.TriggerValueChangeEvent();
            minosPrimeRandomTeleportMinDistance = new FloatField(minosPrimeRandomTeleportDiv, "Minimum distance", "minosPrimeRandomTeleportMinDistance", 20f, 0f, float.MaxValue);
            minosPrimeRandomTeleportMaxDistance = new FloatField(minosPrimeRandomTeleportDiv, "Maximum distance", "minosPrimeRandomTeleportMaxDistance", 50f, 0f, float.MaxValue);
            new ConfigHeader(minosPrimePanel, "Teleport Trail");
            minosPrimeTeleportTrail = new BoolField(minosPrimePanel, "Enabled", "minosPrimeTeleportTrail", true);
            minosPrimeTeleportTrail.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                minosPrimeTeleportTrailDuration.hidden = e.value;
                dirtyField = true;
            };
            minosPrimeTeleportTrailDuration = new FloatField(minosPrimePanel, "Duration", "minosPrimeTeleportTrailDuration", 1f, 0, float.PositiveInfinity);

            new ConfigHeader(minosPrimePanel, "Combo Explosive Finish");
            minosPrimeComboExplosionToggle = new BoolField(minosPrimePanel, "Enabled", "minosPrimeComboExplosionToggle", true);
            minosPrimeComboExplosionSize = new FloatField(minosPrimePanel, "Explosion size multiplier", "minosPrimeComboExplosionSize", 1f, 0f, float.MaxValue);
            minosPrimeComboExplosionDamage = new FloatField(minosPrimePanel, "Explosion damage multiplier", "minosPrimeComboExplosionDamage", 1f, 0f, float.MaxValue);
            minosPrimeComboExplosionToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                minosPrimeComboExplosionSize.interactable = e.value;
                minosPrimeComboExplosionDamage.interactable = e.value;
            };
            minosPrimeComboExplosionToggle.TriggerValueChangeEvent();

            new ConfigHeader(minosPrimePanel, "Big Explosion Attack");
            minosPrimeExplosionToggle = new BoolField(minosPrimePanel, "Enabled", "minosPrimeExplosionToggle", true);
            minosPrimeExplosionChance = new FloatSliderField(minosPrimePanel, "Percent chance", "minosPrimeExplosionChance", new Tuple<float, float>(0, 100), 50, 1);
            minosPrimeExplosionWindupSpeed = new FloatField(minosPrimePanel, "Windup speed", "minosPrimeExplosionWindupSpeed", 5f, 1f, float.MaxValue);
            minosPrimeExplosionSize = new FloatField(minosPrimePanel, "Explosion size multiplier", "minosPrimeExplosionSize", 2f, 0f, float.MaxValue);
            minosPrimeExplosionDamage = new FloatField(minosPrimePanel, "Explosion damage multiplier", "minosPrimeExplosionDamage", 1f, 0f, float.MaxValue);
            minosPrimeExplosionToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                minosPrimeExplosionChance.interactable = e.value;
                minosPrimeExplosionSize.interactable = e.value;
                minosPrimeExplosionDamage.interactable = e.value;
            };
            minosPrimeExplosionToggle.TriggerValueChangeEvent();

            new ConfigHeader(minosPrimePanel, "Crush Attack");
            minosPrimeCrushAttackToggle = new BoolField(minosPrimePanel, "Enabled", "minosPrimeCrushAttackToggle", true);
            minosPrimeCrushAttackToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                dirtyField = true;
                minosPrimeCrushAttackChance.interactable = e.value;
            };
            minosPrimeCrushAttackChance = new FloatSliderField(minosPrimePanel, "Chance", "minosPrimeCrushAttackChance", new Tuple<float, float>(0, 100), 50);
            minosPrimeCrushAttackToggle.TriggerValueChangeEvent();

            new ConfigHeader(minosPrimePanel, "End Combo With Dropkick");
            minosPrimeComboExplosiveEndToggle = new BoolField(minosPrimePanel, "Enabled", "minosPrimeComboExplosiveEndToggle", true);
            minosPrimeComboExplosiveEndToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                dirtyField = true;
                minosPrimeComboExplosiveEndChance.interactable = e.value;
            };
            minosPrimeComboExplosiveEndChance = new FloatSliderField(minosPrimePanel, "Chance", "minosPrimeComboExplosiveEndChance", new Tuple<float, float>(0, 100), 50);
            minosPrimeComboExplosiveEndToggle.TriggerValueChangeEvent();

            // V2 - FIRST
            new ConfigHeader(v2FirstPanel, "Knuckleblaster");
            ConfigDivision v2FirstKnuckleBlasterDiv = new ConfigDivision(v2FirstPanel, "v2FirstKnuckleBlasterDiv");
            v2FirstKnuckleBlasterToggle = new BoolField(v2FirstPanel, "Enabled", "v2FirstKnuckleBlasterToggle", true);
            v2FirstKnuckleBlasterToggle.presetLoadPriority = 1;
            v2FirstKnuckleBlasterToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                v2FirstKnuckleBlasterDiv.interactable = e.value;
                dirtyField = true;
            };
            v2FirstKnuckleBlasterToggle.TriggerValueChangeEvent();
            v2FirstKnuckleBlasterCooldown = new FloatField(v2FirstKnuckleBlasterDiv, "Cooldown", "v2FirstKnuckleBlasterCooldown", 3f, 0f, float.MaxValue);
            ConfigDivision v2FirstKnuckleBlasterHitPlayerDiv = new ConfigDivision(v2FirstKnuckleBlasterDiv, "v2FirstKnuckleBlasterHitPlayerDiv");
            v2FirstKnuckleBlasterHitPlayerToggle = new BoolField(v2FirstKnuckleBlasterDiv, "Hit player", "v2FirstKnuckleBlasterHitPlayerToggle", true);
            v2FirstKnuckleBlasterHitPlayerToggle.presetLoadPriority = 1;
            v2FirstKnuckleBlasterHitPlayerToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                v2FirstKnuckleBlasterHitPlayerDiv.interactable = e.value;
                dirtyField = true;
            };
            v2FirstKnuckleBlasterHitPlayerToggle.TriggerValueChangeEvent();
            v2FirstKnuckleBlasterHitPlayerMinDistance = new FloatField(v2FirstKnuckleBlasterHitPlayerDiv, "Minimum distance to player", "v2FirstKnuckleBlasterHitPlayerMinDistance", 5f, 0f, float.MaxValue);
            v2FirstKnuckleBlasterHitDamage = new IntField(v2FirstKnuckleBlasterHitPlayerDiv, "Hit damage", "v2FirstKnuckleBlasterHitDamage", 5, 0, int.MaxValue);
            v2FirstKnuckleBlasterDeflectShotgunToggle = new BoolField(v2FirstKnuckleBlasterDiv, "Deflect shotgun pellets", "v2FirstKnuckleBlasterDeflectShotgunToggle", false);
            v2FirstKnuckleBlasterDeflectShotgunToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                dirtyField = true;
            };
            v2FirstKnuckleBlasterExplosionDamage = new IntField(v2FirstKnuckleBlasterDiv, "Explosion damage", "v2FirstKnuckleBlasterExplosionDamage", 10, 0, int.MaxValue);
            v2FirstKnuckleBlasterSize = new FloatField(v2FirstKnuckleBlasterDiv, "Explosion size", "v2FirstKnuckleBlasterSize", 15, 0f, float.MaxValue);
            v2FirstKnuckleBlasterSpeed = new FloatField(v2FirstKnuckleBlasterDiv, "Explosion speed", "v2FirstKnuckleBlasterSpeed", 15f / 2, 0f, float.MaxValue);

            new ConfigHeader(v2FirstPanel, "Grenade Snipe");
            ConfigDivision v2FirstCoreSnipeDiv = new ConfigDivision(v2FirstPanel, "v2FirstCoreSnipeDiv");
            v2FirstCoreSnipeToggle = new BoolField(v2FirstPanel, "Enabled", "v2FirstCoreSnipeToggle", true);
            v2FirstCoreSnipeToggle.presetLoadPriority = 1;
            v2FirstCoreSnipeToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                v2FirstCoreSnipeDiv.interactable = e.value;
                dirtyField = true;
            };
            v2FirstCoreSnipeToggle.TriggerValueChangeEvent();
            v2FirstCoreSnipeMaxDistanceToPlayer = new FloatField(v2FirstCoreSnipeDiv, "Max distance to player", "v2FirstCoreSnipeMaxDistanceToPlayer", 15f, 0f, float.MaxValue);
            v2FirstCoreSnipeMinDistanceToV2 = new FloatField(v2FirstCoreSnipeDiv, "Min distance to V2", "v2FirstCoreSnipeMinDistanceToV2", 0f, 0f, float.MaxValue);
            v2FirstCoreSnipeReactionTime = new FloatField(v2FirstCoreSnipeDiv, "Reaction time", "v2FirstCoreSnipeReactionTime", 0.2f, 0f, 5f);

            new ConfigHeader(v2FirstPanel, "Sharpshooter");
            ConfigDivision v2FirstSharpShooterDiv = new ConfigDivision(v2FirstPanel, "v2FirstSharpShooterDiv");
            v2FirstSharpshooterToggle = new BoolField(v2FirstPanel, "Enabled", "v2FirstSharpshooterToggle", true);
            v2FirstSharpshooterToggle.presetLoadPriority = 1;
            v2FirstSharpshooterToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                v2FirstSharpShooterDiv.interactable = e.value;
                dirtyField = true;
            };
            v2FirstSharpshooterChance = new FloatSliderField(v2FirstSharpShooterDiv, "Chance", "v2FirstSharpshooterChance", new System.Tuple<float, float>(0, 100), 50, 1);
            v2FirstSharpshooterAutoaimAngle = new FloatSliderField(v2FirstSharpShooterDiv, "Autoaim angle maximum", "v2FirstSharpshooterAutoaimAngle", new System.Tuple<float, float>(0, 180), 30, 1);
            v2FirstSharpshooterReflections = new IntField(v2FirstSharpShooterDiv, "Ricochet count", "v2FirstSharpshooterReflections", 2, 0, int.MaxValue);
            v2FirstSharpshooterDamage = new FloatField(v2FirstSharpShooterDiv, "Damage multiplier", "v2FirstSharpshooterDamage", 1f, 0f, float.MaxValue);
            v2FirstSharpshooterSpeed = new FloatField(v2FirstSharpShooterDiv, "Speed multiplier", "v2FirstSharpshooterSpeed", 1f, 0f, float.MaxValue);
            v2FirstSharpshooterToggle.TriggerValueChangeEvent();

            // V2 - SECOND
            /*v2SecondStartEnraged = new BoolField(v2SecondPanel, "Start enraged", "v2SecondStartEnraged", true);
            v2SecondStartEnraged.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                dirtyField = true;
            };*/
            v2SecondRocketLauncherToggle = new BoolField(v2SecondPanel, "Use rocket launcher", "v2SecondRocketLauncherToggle", true);
            
            /*v2SecondCoinRailcannon = new BoolField(v2SecondPanel, "Electric railcannon chargeback", "v2SecondCoinRailcannon", true);
            v2SecondCoinRailcannon.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                v2SecondCoinRailcannonCooldown.interactable = e.value;
            };
            v2SecondCoinRailcannonCooldown = new FloatField(v2SecondPanel, "Electric railcannon cooldown", "v2SecondCoinRailcannonCooldown", 15f);
            v2SecondCoinRailcannon.TriggerValueChangeEvent();*/

            ConfigDivision v2SecondFastCoinDiv = new ConfigDivision(v2SecondPanel, "v2SecondFastCoinDiv");
            new ConfigHeader(v2SecondPanel, "Shoot Coins Separately");
            v2SecondFastCoinToggle = new BoolField(v2SecondPanel, "Enabled", "v2SecondFastCoin", true);
            v2SecondFastCoinToggle.presetLoadPriority = 1;
            v2SecondFastCoinToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                v2SecondFastCoinDiv.interactable = e.value;
                dirtyField = true;
            };
            v2SecondFastCoinToggle.TriggerValueChangeEvent();
            v2SecondFastCoinThrowDelay = new FloatField(v2SecondFastCoinDiv, "Throw next coin delay", "v2SecondFastCoinThrowDelay", 0.6f, 0f, float.MaxValue);
            v2SecondFastCoinShootDelay = new FloatField(v2SecondFastCoinDiv, "Shoot coin delay", "v2SecondFastCoinShootDelay", 0.3f, 0f, float.MaxValue);

            ConfigDivision v2SecondMalCannonDiv = new ConfigDivision(v2SecondPanel, "v2SecondMalCannonDiv");
            new ConfigHeader(v2SecondPanel, "Malicious Cannon Snipe");
            v2SecondMalCannonSnipeToggle = new BoolField(v2SecondPanel, "Enabled", "v2SecondMalCannonSnipeToggle", true);
            v2SecondMalCannonSnipeToggle.presetLoadPriority = 1;
            v2SecondMalCannonSnipeToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                v2SecondMalCannonDiv.interactable = e.value;
            };
            v2SecondMalCannonSnipeToggle.TriggerValueChangeEvent();
            v2SecondMalCannonSnipeCooldown = new FloatField(v2SecondMalCannonDiv, "Cooldown", "v2SecondMalCannonSnipeCooldown", 15f, 0f, float.MaxValue);
            v2SecondMalCannonSnipeReactTime = new FloatField(v2SecondMalCannonDiv, "React time", "v2SecondMalCannonSnipeReactTime", 0.2f, 0f, float.MaxValue);
            v2SecondMalCannonSnipeMaxDistanceToPlayer = new FloatField(v2SecondMalCannonDiv, "Max distance to player", "v2SecondMalCannonSnipeMaxDistanceToPlayer", 20f, 0f, float.MaxValue);
            v2SecondMalCannonSnipeMinDistanceToV2 = new FloatField(v2SecondMalCannonDiv, "Min distance to V2", "v2SecondMalCannonSnipeMinDistanceToV2", 0f, 0f, float.MaxValue);
            ConfigDivision v2SecondSnipeDiv = new ConfigDivision(v2SecondPanel, "v2SecondSnipeDiv");
            new ConfigHeader(v2SecondPanel, "Grenade Snipe");
            v2SecondCoreSnipeToggle = new BoolField(v2SecondPanel, "Enabled", "v2SecondCoreSnipeToggle", true);
            v2SecondCoreSnipeToggle.presetLoadPriority = 1;
            v2SecondCoreSnipeToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                v2SecondSnipeDiv.interactable = e.value;
            };
            v2SecondCoreSnipeToggle.TriggerValueChangeEvent();
            v2SecondCoreSnipeMaxDistanceToPlayer = new FloatField(v2SecondSnipeDiv, "Max distance to player", "v2SecondCoreSnipeMaxDistanceToPlayer", 15f, 0f, float.MaxValue);
            v2SecondCoreSnipeMinDistanceToV2 = new FloatField(v2SecondSnipeDiv, "Min distance to V2", "v2SecondCoreSnipeMinDistanceToV2", 0f, 0f, float.MaxValue);
            v2SecondCoreSnipeReactionTime = new FloatField(v2SecondSnipeDiv, "Reaction time", "v2SecondCoreSnipeReactionTime", 0.2f, 0f, 5f);

            new ConfigHeader(v2SecondPanel, "Sharpshooter");
            ConfigDivision v2SecondSharpShooterDiv = new ConfigDivision(v2SecondPanel, "v2SecondSharpShooterDiv");
            v2SecondSharpshooterToggle = new BoolField(v2SecondPanel, "Enabled", "v2SecondSharpshooterToggle", true);
            v2SecondSharpshooterToggle.presetLoadPriority = 1;
            v2SecondSharpshooterToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                v2SecondSharpShooterDiv.interactable = e.value;
                dirtyField = true;
            };
            v2SecondSharpshooterChance = new FloatSliderField(v2SecondSharpShooterDiv, "Chance", "v2SecondSharpshooterChance", new System.Tuple<float, float>(0, 100), 50, 1);
            v2SecondSharpshooterAutoaimAngle = new FloatSliderField(v2SecondSharpShooterDiv, "Autoaim angle maximum", "v2SecondSharpshooterAutoaimAngle", new System.Tuple<float, float>(0, 180), 30, 1);
            v2SecondSharpshooterReflections = new IntField(v2SecondSharpShooterDiv, "Ricochet count", "v2SecondSharpshooterReflections", 2, 0, int.MaxValue);
            v2SecondSharpshooterDamage = new FloatField(v2SecondSharpShooterDiv, "Damage multiplier", "v2SecondSharpshooterDamage", 1f, 0f, float.MaxValue);
            v2SecondSharpshooterSpeed = new FloatField(v2SecondSharpShooterDiv, "Speed multiplier", "v2SecondSharpshooterSpeed", 1f, 0f, float.MaxValue);
            v2SecondSharpshooterToggle.TriggerValueChangeEvent();

            // Sisyphean Insurrectionist (yeah don't judge)
            new ConfigHeader(sisyInstPanel, "Boulder Creates Shockwaves");
            ConfigDivision sisyInstShockwaveDiv = new ConfigDivision(sisyInstPanel, "sisyInstShockwaveDiv");
            sisyInstBoulderShockwave = new BoolField(sisyInstPanel, "Enabled", "sisyInstBoulderShockwave", true);
            sisyInstBoulderShockwave.presetLoadPriority = 1;
            sisyInstBoulderShockwave.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                sisyInstShockwaveDiv.interactable = e.value;
                dirtyField = true;
            };
            sisyInstBoulderShockwave.TriggerValueChangeEvent();
            sisyInstBoulderShockwaveSize = new FloatField(sisyInstShockwaveDiv, "Shockwave size", "sisyInstBoulderShockwaveSize", 1f, 0f, float.MaxValue);
            sisyInstBoulderShockwaveSpeed = new FloatField(sisyInstShockwaveDiv, "Shockwave speed", "sisyInstBoulderShockwaveSpeed", 35f, 0f, float.MaxValue);
            sisyInstBoulderShockwaveDamage = new IntField(sisyInstShockwaveDiv, "Shockwave damage", "sisyInstBoulderShockwaveDamage", 10, 0, int.MaxValue);
            new ConfigHeader(sisyInstPanel, "Jump Shockwave Tweak");
            ConfigDivision sisyInstJumpShockwaveDiv = new ConfigDivision(sisyInstPanel, "sisyInstJumpShockwaveDiv");
            sisyInstJumpShockwave = new BoolField(sisyInstPanel, "Enabled", "sisyInstJumpShockwave", true);
            sisyInstJumpShockwave.presetLoadPriority = 1;
            sisyInstJumpShockwave.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                sisyInstJumpShockwaveDiv.interactable = e.value;
                dirtyField = true;
            };
            sisyInstJumpShockwave.TriggerValueChangeEvent();
            sisyInstJumpShockwaveSize = new FloatField(sisyInstJumpShockwaveDiv, "Shockwave size", "sisyInstJumpShockwaveSize", 2f, 0f, float.MaxValue);
            sisyInstJumpShockwaveSize.presetLoadPriority = 1;
            sisyInstJumpShockwaveSize.onValueChange += (FloatField.FloatValueChangeEvent e) =>
            {
                GameObject shockwave = SisyphusInstructionist_Start.shockwave;
                shockwave.transform.localScale = new Vector3(shockwave.transform.localScale.x, 20 * ConfigManager.sisyInstBoulderShockwaveSize.value, shockwave.transform.localScale.z);
            };
            sisyInstJumpShockwaveSpeed = new FloatField(sisyInstJumpShockwaveDiv, "Shockwave speed", "sisyInstJumpShockwaveSpeed", 35f, 0f, float.MaxValue);
            sisyInstJumpShockwaveSpeed.presetLoadPriority = 1;
            sisyInstJumpShockwaveSpeed.onValueChange += (FloatField.FloatValueChangeEvent e) =>
            {
                GameObject shockwave = SisyphusInstructionist_Start.shockwave;
                PhysicalShockwave comp = shockwave.GetComponent<PhysicalShockwave>();
                comp.speed = e.value;
            };
            sisyInstJumpShockwaveDamage = new IntField(sisyInstJumpShockwaveDiv, "Shockwave damage", "sisyInstJumpShockwaveDamage", 15, 0, int.MaxValue);
            sisyInstJumpShockwaveDamage.presetLoadPriority = 1;
            sisyInstJumpShockwaveDamage.onValueChange += (IntField.IntValueChangeEvent e) =>
            {
                GameObject shockwave = SisyphusInstructionist_Start.shockwave;
                PhysicalShockwave comp = shockwave.GetComponent<PhysicalShockwave>();
                comp.damage = e.value;
            };
            new ConfigHeader(sisyInstPanel, "Stronger Stomp");
            ConfigDivision sisyInstExplosionDiv = new ConfigDivision(sisyInstPanel, "sisyInstExplosionDiv");
            sisyInstStrongerExplosion = new BoolField(sisyInstPanel, "Enabled", "sisyInstStrongerExplosion", true);
            sisyInstStrongerExplosion.presetLoadPriority = 1;
            sisyInstStrongerExplosion.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                sisyInstExplosionDiv.interactable = e.value;
                dirtyField = true;
            };
            sisyInstStrongerExplosion.TriggerValueChangeEvent();
            sisyInstStrongerExplosionSizeMulti = new FloatField(sisyInstExplosionDiv, "Size multiplier", "sisyInstStrongerExplosionSizeMulti", 0.5f, 0f, float.MaxValue);
            sisyInstStrongerExplosionDamageMulti = new FloatField(sisyInstExplosionDiv, "Damage multiplier", "sisyInstStrongerExplosionDamageMulti", 0.5f, 0f, float.MaxValue);

            leviathanSecondPhaseBegin = new BoolField(leviathanPanel, "Start at the second phase", "leviathanSecondPhaseBegin", true); ;
            new ConfigHeader(leviathanPanel, "Mixed Projectile Burst");
            ConfigDivision leviathanMixProjectileDiv = new ConfigDivision(leviathanPanel, "leviathanMixProjectileDiv");
            leviathanProjectileMixToggle = new BoolField(leviathanPanel, "Enabled", "leviathanProjectileMixToggle", true);
            leviathanProjectileMixToggle.presetLoadPriority = 1;
            leviathanProjectileMixToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                leviathanMixProjectileDiv.interactable = e.value;
                dirtyField = true;
            };
            leviathanProjectileMixToggle.TriggerValueChangeEvent();
            leviathanProjectileBlueChance = new FloatSliderField(leviathanMixProjectileDiv, "Blue projectile", "leviathanProjectileBlueChance", new System.Tuple<float, float>(0, 100), 40f);
            leviathanProjectileYellowChance = new FloatSliderField(leviathanMixProjectileDiv, "Yellow projectile", "leviathanProjectileYellowChance", new System.Tuple<float, float>(0, 100), 10f);
            new ConfigHeader(leviathanPanel, "Projectile Burst Tweaker");
            leviathanProjectileCount = new IntField(leviathanPanel, "Projectile count", "leviathanProjectileCount", 80, 1, int.MaxValue);
            leviathanProjectileDensity = new FloatField(leviathanPanel, "Projectiles per second", "leviathanProjectileDensity", 25, 1f, float.MaxValue);
            leviathanProjectileFriendlyFireDamageMultiplier = new FloatSliderField(leviathanPanel, "Projectile friendly fire damage%", "leviathanProjectileFriendlyFireDamageMultiplier", new System.Tuple<float, float>(0, 100), 5f);
            new ConfigHeader(leviathanPanel, "Charged Attack");
            ConfigDivision leviathanChargedDiv = new ConfigDivision(leviathanPanel, "leviathanChargedDiv");
            leviathanChargeAttack = new BoolField(leviathanPanel, "Enabled", "leviathanChargeAttack", true);
            leviathanChargeAttack.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                leviathanChargedDiv.interactable = e.value;
                dirtyField = true;
            };
            leviathanChargeAttack.TriggerValueChangeEvent();
            leviathanChargeChance = new FloatSliderField(leviathanChargedDiv, "Chance", "leviathanChargeChance", new System.Tuple<float, float>(0, 100), 25);
            leviathanChargeCount = new IntField(leviathanChargedDiv, "Number of charges shot", "leviathanChargeCount", 3, 1, int.MaxValue);
            leviathanChargeDelay = new FloatField(leviathanChargedDiv, "Delay between shots", "leviathanChargeDelay", 0.75f, 0f, float.MaxValue);
            leviathanChargeSizeMulti = new FloatField(leviathanChargedDiv, "Explosion size multiplier", "leviathanChargeSizeMulti", 1.5f, 0f, float.MaxValue);
            leviathanChargeDamageMulti = new FloatField(leviathanChargedDiv, "Explosion damage multiplier", "leviathanChargeDamageMulti", 1f, 0f, float.MaxValue);
            leviathanChargeHauntRocketRiding = new BoolField(leviathanChargedDiv, "Target ridden rockets", "leviathanChargeHauntRocketRiding", true);
            new ConfigHeader(leviathanPanel, "Tail Swing Combo");
            leviathanTailComboCount = new IntField(leviathanPanel, "Tail swing count", "leviathanTailComboCount", 3, 1, int.MaxValue);

            // PANOPTICON
            panopticonFullPhase = new BoolField(panopticonPanel, "Full fight", "panopticonFullPhase", true);
            panopticonFullPhase.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                dirtyField = true;
            };
            panopticonBalanceEyes = new BoolField(panopticonPanel, "Balance eyes", "panopticonBalanceEyes", true);
            panopticonBalanceEyes.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                dirtyField = true;
            };
            panopticonBlackholeProj = new BoolField(panopticonPanel, "Throw projectile after blackhole", "panopticonBlackholeProj", true);
            panopticonBlackholeProj.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                dirtyField = true;
            };
            panopticonAxisBeam = new BoolField(panopticonPanel, "3 axis insignia", "panopticonAxisBeam", true);
            panopticonAxisBeam.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                dirtyField = true;
                panopticonAxisBeamSizeMulti.interactable = e.value;
            };
            panopticonAxisBeamSizeMulti = new FloatField(panopticonPanel, "Axis beam size multiplier", "panopticonAxisBeamSizeMulti", 0.5f, 0.1f, float.MaxValue);
            panopticonAxisBeam.TriggerValueChangeEvent();

            new ConfigHeader(panopticonPanel, "Spin Insignia");
            ConfigDivision panopticonSpinInsigniaDiv = new ConfigDivision(panopticonPanel, "panopticonSpinInsigniaDiv");
            panopticonSpinAttackToggle = new BoolField(panopticonPanel, "Enabled", "panopticonSpinAttackToggle", true);
            panopticonSpinAttackToggle.presetLoadPriority = 1;
            panopticonSpinAttackToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                panopticonSpinInsigniaDiv.interactable = e.value;
                dirtyField = true;
            };
            panopticonSpinAttackToggle.TriggerValueChangeEvent();
            panopticonSpinAttackCount = new IntField(panopticonSpinInsigniaDiv, "Insignia count", "panopticonSpinAttackCount", 5, 1, int.MaxValue);
            panopticonSpinAttackDamage = new IntField(panopticonSpinInsigniaDiv, "Insignia damage", "panopticonSpinAttackDamage", 10, 0, int.MaxValue);
            panopticonSpinAttackSize = new FloatField(panopticonSpinInsigniaDiv, "Insignia size", "panopticonSpinAttackSize", 5f, 0f, float.MaxValue);
            panopticonSpinAttackDistance = new FloatField(panopticonSpinInsigniaDiv, "Circle radius", "panopticonSpinAttackDistance", 60f, 0f, float.MaxValue);
            panopticonSpinAttackTurnSpeed = new FloatField(panopticonSpinInsigniaDiv, "Turn speed", "panopticonSpinAttackTurnSpeed", 60f, 0f, float.MaxValue);
            panopticonSpinAttackActivateSpeed = new FloatField(panopticonSpinInsigniaDiv, "Activasion speed", "panopticonSpinAttackActivateSpeed", 0.5f, 0f, float.MaxValue);

            new ConfigHeader(panopticonPanel, "Blue Orb Attack");
            ConfigDivision panopticonBlueProjDiv = new ConfigDivision(panopticonPanel, "panopticonBlueProjDiv");
            panopticonBlueProjToggle = new BoolField(panopticonPanel, "Enabled", "panopticonBlueProjToggle", true);
            panopticonBlueProjToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                panopticonBlueProjDiv.interactable = e.value;
                dirtyField = true;
            };
            panopticonBlueProjToggle.TriggerValueChangeEvent();
            panopticonBlueProjCount = new IntField(panopticonBlueProjDiv, "Projectile count", "panopticonBlueProjCount", 3, 1, int.MaxValue);
            panopticonBlueProjDamage = new FloatField(panopticonBlueProjDiv, "Projectile damage", "panopticonBlueProjDiv", 5, 0, float.MaxValue);
            panopticonBlueProjTurnSpeed = new FloatField(panopticonBlueProjDiv, "Projectile turn speed multiplier", "panopticonBlueProjTurnSpeed", 1f, 0f, float.MaxValue);
            panopticonBlueProjInitialSpeed = new FloatField(panopticonBlueProjDiv, "Projectile initial speed", "panopticonBlueProjInitialSpeed", 0f, 0f, float.MaxValue);

            // IDOL
            new ConfigHeader(idolPanel, "Explode on break");
            ConfigDivision idolExplosionDiv = new ConfigDivision(idolPanel, "idolExplosionDiv");
            idolExplosionToggle = new BoolField(idolPanel, "Enabled", "idolExplosionToggle", true);
            idolExplosionToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                idolExplosionDiv.interactable = e.value;
                dirtyField = true;
            };
            idolExplosionToggle.TriggerValueChangeEvent();
            idolExplodionType = new EnumField<IdolExplosionType>(idolExplosionDiv, "Explosion type", "idolExplosionType", IdolExplosionType.Ligthning);
            idolExplodionType.SetEnumDisplayName(IdolExplosionType.Normal, "Normal explosion");
            idolExplodionType.SetEnumDisplayName(IdolExplosionType.Big, "Big explosion");
            idolExplodionType.SetEnumDisplayName(IdolExplosionType.Ligthning, "Lightning bolt");
            idolExplodionType.SetEnumDisplayName(IdolExplosionType.Sisyphean, "Sisyphus prime explosion");
            idolExplodionType.SetEnumDisplayName(IdolExplosionType.Sand, "Sand explosion");
            idolExplosionSizeMultiplier = new FloatField(idolExplosionDiv, "Explosion size multiplier", "idolExplosionSizeMultiplier", 1f, 0f, float.MaxValue);
            idolExplosionDamageMultiplier = new FloatField(idolExplosionDiv, "Explosion damage multiplier", "idolExplosionDamageMultiplier", 1f, 0f, float.MaxValue);
            idolExplodionType.onValueChange += (EnumField<IdolExplosionType>.EnumValueChangeEvent e) =>
            {
                idolExplosionSizeMultiplier.interactable = e.value != IdolExplosionType.Sand;
                idolExplosionDamageMultiplier.interactable = e.value != IdolExplosionType.Sand;
            };
            idolExplodionType.TriggerValueChangeEvent();
            idolExplosionEnemyDamagePercent = new FloatSliderField(idolExplosionDiv, "Enemy damage percent", "idolExplosionEnemyDamagePercent", new Tuple<float, float>(0f, 100f), 0f, 1);

            config.Flush();
            //config.LogDuplicateGUID();
            Plugin.PatchAll();
            dirtyField = false;

            if (config.firstTime)
                AddMissingPresets();
        }

        private static void SwordsMachineSecondPhaseMode_onValueChange(EnumField<SwordsMachineSecondPhase>.EnumValueChangeEvent data)
        {
            throw new NotImplementedException();
        }
    }
}
