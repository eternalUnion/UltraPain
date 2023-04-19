using PluginConfig.API;
using PluginConfig.API.Fields;
using PluginConfig.API.Decorators;

using System;
using System.Collections.Generic;
using System.Text;
using Steamworks.Data;
using UnityEngine.UI;

namespace DifficultyTweak
{
    public static class ConfigManager
    {
        public static PluginConfigurator config = null;

        // ROOT PANEL
        public static BoolField enemyTweakToggle;
        private static ConfigPanel enemyPanel;
        public static BoolField playerTweakToggle;
        private static ConfigPanel playerPanel;
        public static BoolField discordRichPresenceToggle;
        public static BoolField steamRichPresenceToggle;
        public static StringField pluginName;
        public static BoolField globalDifficultySwitch; // TEMPORARY

        // PLAYER PANEL
        public static BoolField rocketBoostToggle;
        public static BoolField rocketBoostAlwaysExplodesToggle;
        public static FloatField rocketBoostDamageMultiplierPerHit;
        public static FloatField rocketBoostSizeMultiplierPerHit;
        public static FloatField rocketBoostSpeedMultiplierPerHit;
        public static StringField rocketBoostStyleText;
        public static IntField rocketBoostStylePoints;

        public static BoolField rocketGrabbingToggle;

        public static BoolField grenadeBoostToggle;
        public static FloatField grenadeBoostDamageMultiplier;
        public static FloatField grenadeBoostSizeMultiplier;
        public static StringField grenadeBoostStyleText;
        public static IntField grenadeBoostStylePoints;

        // ENEMY PANEL
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

        // CERBERUS
        public static BoolField cerberusDashToggle;
        public static IntField cerberusTotalDashCount;

        // DRONE
        public static BoolField droneExplosionBeamToggle;
        public static BoolField droneSentryBeamToggle;
        public static FloatField droneSentryBeamDamage;

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

        // MINDFLAYER
        public static BoolField mindflayerShootTweakToggle;
        public static IntField mindflayerShootAmount;
        public static FloatField mindflayerShootDelay;
        public static BoolField mindflayerTeleportComboToggle;

        // SCHISM
        public static BoolField schismSpreadAttackToggle;
        public static FloatField schismSpreadAttackAngle;
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

        // STRAY
        public static BoolField strayShootToggle;
        public static IntField strayShootCount;
        public static FloatField strayShootSpeed;
        public static BoolField strayCoinsIgnoreWeakPointToggle;

        // STREET CLEANER
        public static BoolField streetCleanerPredictiveDodgeToggle;
        public static BoolField streetCleanerCoinsIgnoreWeakPointToggle;

        // SWORDS MACHINE
        public static BoolField swordsMachineNoLightKnockbackToggle;
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
        public static BoolField minosPrimeRandomTeleportToggle;
        public static FloatField minosPrimeRandomTeleportMinDistance;
        public static FloatField minosPrimeRandomTeleportMaxDistance;

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

        // V2 - SECOND
        public static BoolField v2SecondStartEnraged;
        public static BoolField v2SecondRocketLauncherToggle;
        public static BoolField v2SecondFastCoin;
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
            new ConfigHeader(config.rootPanel, "Enemy Tweaks");
            enemyTweakToggle = new BoolField(config.rootPanel, "Enabled", "enemyTweakToggle", true);
            enemyPanel = new ConfigPanel(config.rootPanel, "Enemy Tweaks", "enemyTweakPanel");
            enemyTweakToggle.onValueChange += (BoolField.BoolValueChangeEvent data) =>
            {
                enemyPanel.interactable = data.value;
                dirtyField = true;
            };
            enemyTweakToggle.TriggerValueChangeEvent();

            new ConfigHeader(config.rootPanel, "Player Tweaks");
            playerTweakToggle = new BoolField(config.rootPanel, "Enabled", "playerTweakToggle", true);
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

            new ConfigHeader(config.rootPanel, "Plugin Difficulty Name");
            pluginName = new StringField(config.rootPanel, "Difficulty name", "pluginName", "ULTRAPAIN");
            pluginName.onValueChange += (StringField.StringValueChangeEvent data) =>
            {
                if (Plugin.currentDifficultyButton != null)
                    Plugin.currentDifficultyButton.transform.Find("Name").GetComponent<Text>().text = data.value;

                if (Plugin.currentDifficultyPanel != null)
                    Plugin.currentDifficultyPanel.transform.Find("Title (1)").GetComponent<Text>().text = $"--{data.value}--";
            };

            new ConfigHeader(config.rootPanel, "Global Difficulty");
            globalDifficultySwitch = new BoolField(config.rootPanel, "Enabled", "globalDifficultySwitch", false);
            globalDifficultySwitch.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                dirtyField = true;
            };

            // PLAYER PANEL
            new ConfigHeader(playerPanel, "Rocket Boosting");
            ConfigDivision rocketBoostDiv = new ConfigDivision(playerPanel, "rocketBoostDiv");
            rocketBoostToggle = new BoolField(playerPanel, "Enabled", "rocketBoostToggle", true);
            rocketBoostToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                rocketBoostDiv.interactable = e.value;
                dirtyField = true;
            };
            rocketBoostToggle.TriggerValueChangeEvent();
            rocketBoostAlwaysExplodesToggle = new BoolField(rocketBoostDiv, "Always explode", "rocketBoostAlwaysExplodes", true);
            rocketBoostDamageMultiplierPerHit = new FloatField(rocketBoostDiv, "Damage multiplier per hit", "rocketBoostDamageMultiplier", 0.35f);
            rocketBoostSizeMultiplierPerHit = new FloatField(rocketBoostDiv, "Size multiplier per hit", "rocketBoostSizeMultiplier", 0.35f);
            rocketBoostSpeedMultiplierPerHit = new FloatField(rocketBoostDiv, "Speed multiplier per hit", "rocketBoostSpeedMultiplierPerHit", 0.35f);
            rocketBoostStyleText = new StringField(rocketBoostDiv, "Style text", "rocketBoostStyleText", "<color=lime>ROCKET BOOST</color>");
            rocketBoostStylePoints = new IntField(rocketBoostDiv, "Style points", "rocketBoostStylePoints", 10);

            new ConfigHeader(playerPanel, "Rocket Grabbing\r\n<size=16>(Can pull yourself to frozen rockets)</size>");
            rocketGrabbingToggle = new BoolField(playerPanel, "Enabled", "rocketGrabbingTabble", true);
            rocketGrabbingToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                dirtyField = true;
            };

            new ConfigHeader(playerPanel, "Grenade Boosting");
            ConfigDivision grenadeBoostDiv = new ConfigDivision(playerPanel, "grenadeBoostDiv");
            grenadeBoostToggle = new BoolField(playerPanel, "Enabled", "grenadeBoostToggle", true);
            grenadeBoostToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                grenadeBoostDiv.interactable = e.value;
                dirtyField = true;
            };
            grenadeBoostToggle.TriggerValueChangeEvent();
            grenadeBoostDamageMultiplier = new FloatField(grenadeBoostDiv, "Damage multiplier", "grenadeBoostDamageMultiplier", 1f);
            grenadeBoostSizeMultiplier = new FloatField(grenadeBoostDiv, "Size multiplier", "grenadeBoostSizeMultiplier", 1f);
            grenadeBoostStyleText = new StringField(grenadeBoostDiv, "Style text", "grenadeBoostStyleText", "<color=cyan>FISTFUL OF 'NADE</color>");
            grenadeBoostStylePoints = new IntField(grenadeBoostDiv, "Style points", "grenadeBoostStylePoints", 10);

            // ENEMY PANEL
            new ConfigHeader(enemyPanel, "Common Enemies");
            filthPanel = new ConfigPanel(enemyPanel, "Filth", "filthPanel");
            strayPanel = new ConfigPanel(enemyPanel, "Stray", "strayPanel");
            schismPanel = new ConfigPanel(enemyPanel, "Schism", "schismPanel");
            soliderPanel = new ConfigPanel(enemyPanel, "Soldier", "soliderPanel");
            dronePanel = new ConfigPanel(enemyPanel, "Drone", "dronePanel");
            streetCleanerPanel = new ConfigPanel(enemyPanel, "Street Cleaner", "streetCleanerPanel");
            virtuePanel = new ConfigPanel(enemyPanel, "Virtue", "virtuePanel");
            stalkerPanel = new ConfigPanel(enemyPanel, "Stalker", "stalkerPanel");
            new ConfigHeader(enemyPanel, "Mini Bosses");
            cerberusPanel = new ConfigPanel(enemyPanel, "Cerberus", "cerberusPanel");
            maliciousFacePanel = new ConfigPanel(enemyPanel, "Malicious Face", "maliciousFacePanel");
            mindflayerPanel = new ConfigPanel(enemyPanel, "Mindflayer", "mindflayerPanel");
            swordsMachinePanel = new ConfigPanel(enemyPanel, "Swords Machine", "swordsMachinePanel");
            hideousMassPanel = new ConfigPanel(enemyPanel, "Hideous Mass", "hideousMassPanel");
            ferrymanPanel = new ConfigPanel(enemyPanel, "Ferryman", "ferrymanPanel");
            turretPanel = new ConfigPanel(enemyPanel, "Sentry", "turretPanel");
            new ConfigHeader(enemyPanel, "Bosses");
            v2FirstPanel = new ConfigPanel(enemyPanel, "V2 - First", "v2FirstPanel");
            v2SecondPanel = new ConfigPanel(enemyPanel, "V2 - Second", "v2SecondPanel");
            new ConfigHeader(enemyPanel, "Prime Bosses");
            fleshPrisonPanel = new ConfigPanel(enemyPanel, "Flesh Prison", "fleshPrisonPanel");
            minosPrimePanel = new ConfigPanel(enemyPanel, "Minos Prime", "minosPrimePanel");

            // CERBERUS
            new ConfigHeader(cerberusPanel, "Extra Dashes");
            ConfigDivision cerebusExtraDashDiv = new ConfigDivision(cerberusPanel, "cerberusExtraDashDiv");
            cerberusDashToggle = new BoolField(cerberusPanel, "Enabled", "cerberusDashToggle", true);
            cerberusDashToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                cerebusExtraDashDiv.interactable = e.value;
                dirtyField = true;
            };
            cerberusDashToggle.TriggerValueChangeEvent();
            cerberusTotalDashCount = new IntField(cerebusExtraDashDiv, "Total dash count", "cerberusTotalDashCount", 3, 1, int.MaxValue);

            // DRONE
            droneExplosionBeamToggle = new BoolField(dronePanel, "Can shoot explosions", "droneExplosionBeamToggle", true);
            droneExplosionBeamToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                dirtyField = true;
            };
            ConfigDivision droneSentryBeamDiv = new ConfigDivision(dronePanel, "droneSentryBeamDiv");
            droneSentryBeamToggle = new BoolField(dronePanel, "Can shoot sentry beam", "droneSentryBeamToggle", true);
            droneSentryBeamToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                droneSentryBeamDiv.interactable = e.value;
                dirtyField = true;
            };
            droneSentryBeamToggle.TriggerValueChangeEvent();
            droneSentryBeamDamage = new FloatField(droneSentryBeamDiv, "Sentry beam damage", "droneSentryBeamDamage", 2f, 0f, float.MaxValue);

            // FILTH
            new ConfigHeader(filthPanel, "Explode On Hit");
            ConfigDivision filthExplosionDiv = new ConfigDivision(filthPanel, "filthExplosionDiv");
            filthExplodeToggle = new BoolField(filthPanel, "Enabled", "filthExplodeOnHit", true);
            filthExplodeToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                filthExplosionDiv.interactable = e.value;
                dirtyField = true;
            };
            filthExplodeToggle.TriggerValueChangeEvent();
            filthExplodeKills = new BoolField(filthExplosionDiv, "Explosion kills the filth", "filthExplosionKills", false);
            filthExplosionDamage = new IntField(filthExplosionDiv, "Explosion damage", "filthExplosionDamage", 30);
            filthExplosionSize = new FloatField(filthExplosionDiv, "Explosion size", "filthExplosionSize", 0.5f);

            // HIDEOUS MASS
            new ConfigHeader(hideousMassPanel, "Insignia On Projectile Hit");
            ConfigDivision hideousMassInsigniaDiv = new ConfigDivision(hideousMassPanel, "hideousMassInsigniaDiv");
            hideousMassInsigniaToggle = new BoolField(hideousMassPanel, "Enabled", "hideousMassInsigniaToggle", true);
            hideousMassInsigniaToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                hideousMassInsigniaDiv.interactable = e.value;
                dirtyField = true;
            };
            hideousMassInsigniaToggle.TriggerValueChangeEvent();
            hideousMassInsigniaSpeed = new FloatField(hideousMassInsigniaDiv, "Insignia speed multiplier", "hideousMassInsigniaSpeed", 2.5f);
            new ConfigHeader(hideousMassInsigniaDiv, "Vertical Insignia", 12);
            ConfigDivision hideousMassInsigniaYdiv = new ConfigDivision(hideousMassInsigniaDiv, "hideousMassInsigniaYdiv");
            hideousMassInsigniaYtoggle = new BoolField(hideousMassInsigniaDiv, "Enabled", "hideousMassInsigniaYtoggle", true);
            hideousMassInsigniaYtoggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                hideousMassInsigniaYdiv.interactable = e.value;
            };
            hideousMassInsigniaYtoggle.TriggerValueChangeEvent();
            hideousMassInsigniaYdamage = new IntField(hideousMassInsigniaYdiv, "Damage", "hideousMassInsigniaYdamage", 20);
            hideousMassInsigniaYsize = new FloatField(hideousMassInsigniaYdiv, "Size", "hideousMassInsigniaYsize", 2f);
            new ConfigHeader(hideousMassInsigniaDiv, "Forward Insignia", 12);
            ConfigDivision hideousMassInsigniaZdiv = new ConfigDivision(hideousMassInsigniaDiv, "hideousMassInsigniaZdiv");
            hideousMassInsigniaZtoggle = new BoolField(hideousMassInsigniaDiv, "Enabled", "hideousMassInsigniaZtoggle", false);
            hideousMassInsigniaZtoggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                hideousMassInsigniaZdiv.interactable = e.value;
            };
            hideousMassInsigniaZtoggle.TriggerValueChangeEvent();
            hideousMassInsigniaZdamage = new IntField(hideousMassInsigniaZdiv, "Damage", "hideousMassInsigniaZdamage", 20);
            hideousMassInsigniaZsize = new FloatField(hideousMassInsigniaZdiv, "Size", "hideousMassInsigniaZsize", 2f);
            new ConfigHeader(hideousMassInsigniaDiv, "Side Insignia", 12);
            ConfigDivision hideousMassInsigniaXdiv = new ConfigDivision(hideousMassInsigniaDiv, "hideousMassInsigniaXdiv");
            hideousMassInsigniaXtoggle = new BoolField(hideousMassInsigniaDiv, "Enabled", "hideousMassInsigniaXtoggle", false);
            hideousMassInsigniaXtoggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                hideousMassInsigniaXdiv.interactable = e.value;
            };
            hideousMassInsigniaXtoggle.TriggerValueChangeEvent();
            hideousMassInsigniaXdamage = new IntField(hideousMassInsigniaXdiv, "Damage", "hideousMassInsigniaXdamage", 20);
            hideousMassInsigniaXsize = new FloatField(hideousMassInsigniaXdiv, "Size", "hideousMassInsigniaXsize", 2f);

            // MALICIOUS FACE
            new ConfigHeader(maliciousFacePanel, "Radiance When Enraged");
            ConfigDivision maliciousFaceRadianceOnEnrageDiv = new ConfigDivision(maliciousFacePanel, "maliciousFaceRadianceOnEnrageDiv");
            maliciousFaceRadianceOnEnrage = new BoolField(maliciousFacePanel, "Enabled", "maliciousFaceRadianceWhenEnraged", true);
            maliciousFaceRadianceOnEnrage.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                maliciousFaceRadianceOnEnrageDiv.interactable = e.value;
                dirtyField = true;
            };
            maliciousFaceRadianceOnEnrage.TriggerValueChangeEvent();
            maliciousFaceRadianceAmount = new IntField(maliciousFaceRadianceOnEnrageDiv, "Radiance level", "maliciousFaceRadianceAmount", 1);
            new ConfigHeader(maliciousFacePanel, "Homing Projectile");
            ConfigDivision maliciousFaceHomingProjecileDiv = new ConfigDivision(maliciousFacePanel, "maliciousFaceHomingProjecileDiv");
            maliciousFaceHomingProjectileToggle = new BoolField(maliciousFacePanel, "Enabled", "maliciousFaceHomingProjectileToggle", true);
            maliciousFaceHomingProjectileToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                maliciousFaceHomingProjecileDiv.interactable = e.value;
                dirtyField = true;
            };
            maliciousFaceHomingProjectileToggle.TriggerValueChangeEvent();
            maliciousFaceHomingProjectileCount = new IntField(maliciousFaceHomingProjecileDiv, "Projectile count", "maliciousFaceHomingProjectileCount", 5);
            maliciousFaceHomingProjectileDamage = new IntField(maliciousFaceHomingProjecileDiv, "Projectile damage", "maliciousFaceHomingProjectileDamage", 25);
            maliciousFaceHomingProjectileSpeed = new FloatField(maliciousFaceHomingProjecileDiv, "Projectile speed", "maliciousFaceHomingProjectileSpeed", 20f);
            maliciousFaceHomingProjectileTurnSpeed = new FloatField(maliciousFaceHomingProjecileDiv, "Projectile turn speed", "maliciousFaceHomingProjectileTurnSpeed", 0.4f);

            // MINDFLAYER
            new ConfigHeader(mindflayerPanel, "Shoot Tweak");
            ConfigDivision mindflayerShootTweakDiv = new ConfigDivision(mindflayerPanel, "mindflayerShootTweakDiv");
            mindflayerShootTweakToggle = new BoolField(mindflayerPanel, "Enabled", "mindflayerShootTweakToggle", true);
            mindflayerShootTweakToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                mindflayerShootTweakDiv.interactable = e.value;
                dirtyField = true;
            };
            mindflayerShootTweakToggle.TriggerValueChangeEvent();
            mindflayerShootAmount = new IntField(mindflayerShootTweakDiv, "Projectile amount", "mindflayerShootProjectileAmount", 20);
            mindflayerShootDelay = new FloatField(mindflayerShootTweakDiv, "Delay between shots", "mindflayerShootProjectileDelay", 0.02f);
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
            schismSpreadAttackToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                schismSpreadAttackDiv.interactable = e.value;
                dirtyField = true;
            };
            schismSpreadAttackToggle.TriggerValueChangeEvent();
            schismSpreadAttackAngle = new FloatField(schismSpreadAttackDiv, "Angular spread", "schismSpreadAttackAngle", 15f);
            schismSpreadAttackCount = new IntField(schismSpreadAttackDiv, "Projectile count per side", "schismSpreadAttackCount", 1);

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
            soliderShootTweakToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                soliderShootTweakDiv.interactable = e.value;
                dirtyField = true;
            };
            soliderShootTweakToggle.TriggerValueChangeEvent();
            soliderShootCount = new IntField(soliderShootTweakDiv, "Shoot count", "soliderShootCount", 3);
            new ConfigHeader(soliderPanel, "Shoot Grenade");
            ConfigDivision soliderShootGrenadeDiv = new ConfigDivision(soliderPanel, "soliderShootGrenadeDiv");
            soliderShootGrenadeToggle = new BoolField(soliderPanel, "Enabled", "soliderShootGrenade", true);
            soliderShootGrenadeToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                soliderShootGrenadeDiv.interactable = e.value;
                dirtyField = true;
            };
            soliderGrenadeDamage = new IntField(soliderShootGrenadeDiv, "Explosion damage", "soliderGrenadeDamage", 10);
            soliderGrenadeSize = new FloatField(soliderShootGrenadeDiv, "Explosion size multiplier", "soliderGrenadeSize", 0.75f);

            // STALKER
            new ConfigHeader(stalkerPanel, "Survive Explosion");
            stalkerSurviveExplosion = new BoolField(stalkerPanel, "Enabled", "stalkerSurviveExplosion", true);
            stalkerSurviveExplosion.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                dirtyField = true;
            };

            // STRAY
            new ConfigHeader(strayPanel, "Shoot Tweak");
            ConfigDivision strayShootDiv = new ConfigDivision(strayPanel, "strayShootDiv");
            strayShootToggle = new BoolField(strayPanel, "Enabled", "strayShootToggle", true);
            strayShootToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                strayShootDiv.interactable = e.value;
                dirtyField = true;
            };
            strayShootToggle.TriggerValueChangeEvent();
            strayShootCount = new IntField(strayShootDiv, "Extra projectile count", "strayShootCount", 2);
            strayShootSpeed = new FloatField(strayShootDiv, "Shoot speed", "strayShootSpeed", 20f);
            new ConfigHeader(strayPanel, "Coins Ignore Weak Point");
            strayCoinsIgnoreWeakPointToggle = new BoolField(strayPanel, "Enabled", "strayCoinsIgnoreWeakPointToggle", true);
            strayCoinsIgnoreWeakPointToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                dirtyField = true;
            };

            // STREET CLEANER
            new ConfigHeader(streetCleanerPanel, "Predictive Dodge");
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
            new ConfigHeader(swordsMachinePanel, "No Light Knockback");
            swordsMachineNoLightKnockbackToggle = new BoolField(swordsMachinePanel, "Enabled", "swordsMachineNoLightKnockbackToggle", true);
            swordsMachineNoLightKnockbackToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                dirtyField = true;
            };
            new ConfigHeader(swordsMachinePanel, "Explosive Sword Throw");
            ConfigDivision swordsMachineExplosiveSwordDiv = new ConfigDivision(swordsMachinePanel, "swordsMachineExplosiveSwordDiv");
            swordsMachineExplosiveSwordToggle = new BoolField(swordsMachinePanel, "Enabled", "swordsMachineExplosiveSwordToggle", true);
            swordsMachineExplosiveSwordToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                swordsMachineExplosiveSwordDiv.interactable = e.value;
                dirtyField = true;
            };
            swordsMachineExplosiveSwordToggle.TriggerValueChangeEvent();
            swordsMachineExplosiveSwordDamage = new IntField(swordsMachineExplosiveSwordDiv, "Explosion damage", "swordsMachineExplosiveSwordDamage", 20);
            swordsMachineExplosiveSwordSize = new FloatField(swordsMachineExplosiveSwordDiv, "Explosion size multiplier", "swordsMachineExplosiveSwordSize", 0.5f);

            // VIRTUE
            new ConfigHeader(virtuePanel, "Tweak Normal Attack");
            ConfigDivision virtueTweakNormalAttackDiv = new ConfigDivision(virtuePanel, "virtueTweakNormalAttackDiv");
            virtueTweakNormalAttackToggle = new BoolField(virtuePanel, "Enabled", "virtueTweakNormalAttackToggle", true);
            virtueTweakNormalAttackToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                virtueTweakNormalAttackDiv.interactable = e.value;
                dirtyField = true;
            };
            virtueTweakNormalAttackToggle.TriggerValueChangeEvent();
            virtueNormalAttackType = new EnumField<VirtueAttackType>(virtueTweakNormalAttackDiv, "Attack type", "virtueNormalAttackType", VirtueAttackType.Insignia);

            ConfigDivision virtueNormalInsigniaDiv = new ConfigDivision(virtueTweakNormalAttackDiv, "virtueNormalInsigniaDiv");
            ConfigDivision virtueNormalYInsigniaDiv = new ConfigDivision(virtueNormalInsigniaDiv, "virtueNormalYInsigniaDiv");
            new ConfigHeader(virtueNormalInsigniaDiv, "Vertical Insignia", 12);
            virtueNormalInsigniaYtoggle = new BoolField(virtueNormalInsigniaDiv, "Enabled", "virtueNormalInsigniaYtoggle", true);
            virtueNormalInsigniaYtoggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                virtueNormalYInsigniaDiv.interactable = e.value;
            };
            virtueNormalInsigniaYtoggle.TriggerValueChangeEvent();
            virtueNormalInsigniaYdamage = new IntField(virtueNormalYInsigniaDiv, "Damage", "virtueNormalInsigniaYdamage", 30);
            virtueNormalInsigniaYsize = new FloatField(virtueNormalYInsigniaDiv, "Size", "virtueNormalInsigniaYsize", 2f);

            ConfigDivision virtueNormalZInsigniaDiv = new ConfigDivision(virtueNormalInsigniaDiv, "virtueNormalZInsigniaDiv");
            new ConfigHeader(virtueNormalInsigniaDiv, "Forward Insignia", 12);
            virtueNormalInsigniaZtoggle = new BoolField(virtueNormalInsigniaDiv, "Enabled", "virtueNormalInsigniaZtoggle", false);
            virtueNormalInsigniaZtoggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                virtueNormalZInsigniaDiv.interactable = e.value;
            };
            virtueNormalInsigniaZtoggle.TriggerValueChangeEvent();
            virtueNormalInsigniaZdamage = new IntField(virtueNormalZInsigniaDiv, "Damage", "virtueNormalInsigniaZdamage", 15);
            virtueNormalInsigniaZsize = new FloatField(virtueNormalZInsigniaDiv, "Size", "virtueNormalInsigniaZsize", 2f);

            ConfigDivision virtueNormalXInsigniaDiv = new ConfigDivision(virtueNormalInsigniaDiv, "virtueNormalXInsigniaDiv");
            new ConfigHeader(virtueNormalInsigniaDiv, "Side Insignia", 12);
            virtueNormalInsigniaXtoggle = new BoolField(virtueNormalInsigniaDiv, "Enabled", "virtueNormalInsigniaXtoggle", false);
            virtueNormalInsigniaXtoggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                virtueNormalXInsigniaDiv.interactable = e.value;
            };
            virtueNormalInsigniaXtoggle.TriggerValueChangeEvent();
            virtueNormalInsigniaXdamage = new IntField(virtueNormalXInsigniaDiv, "Damage", "virtueNormalInsigniaXdamage", 15);
            virtueNormalInsigniaXsize = new FloatField(virtueNormalXInsigniaDiv, "Size", "virtueNormalInsigniaXsize", 2f);

            ConfigDivision virtueNormalLigthningDiv = new ConfigDivision(virtueTweakNormalAttackDiv, "virtueNormalLigthningDiv");
            virtueNormalLightningDamage = new FloatField(virtueNormalLigthningDiv, "Damage multiplier", "virtueNormalLightningDamage", 1f);
            //virtueNormalLightningSize = new FloatField(virtuePanel, "Size multiplier", "virtueNormalLightningSize", 1f);
            virtueNormalLightningDelay = new FloatField(virtueNormalLigthningDiv, "Lighning delay", "virtueNormalLightningDelay", 3f);

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
            virtueTweakEnragedAttackToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                virtueTweakEnragedAttackDiv.interactable = e.value;
                dirtyField = true;
            };
            virtueTweakEnragedAttackToggle.TriggerValueChangeEvent();
            virtueEnragedAttackType = new EnumField<VirtueAttackType>(virtueTweakEnragedAttackDiv, "Attack type", "virtueEnragedAttackType", VirtueAttackType.Insignia);

            ConfigDivision virtueEnragedInsigniaDiv = new ConfigDivision(virtueTweakEnragedAttackDiv, "virtueEnragedInsigniaDiv");
            ConfigHeader virtueEnragedYInsigniaHeader = new ConfigHeader(virtueEnragedInsigniaDiv, "Vertical Insignia", 12);
            ConfigDivision virtueEnragedYInsigniaDiv = new ConfigDivision(virtueEnragedInsigniaDiv, "virtueEnragedYInsigniaDiv");
            virtueEnragedInsigniaYtoggle = new BoolField(virtueEnragedInsigniaDiv, "Enabled", "virtueEnragedInsigniaYtoggle", true);
            virtueEnragedInsigniaYtoggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                virtueEnragedYInsigniaDiv.interactable = e.value;
                dirtyField = true;
            };
            virtueEnragedInsigniaYtoggle.TriggerValueChangeEvent();
            virtueEnragedInsigniaYdamage = new IntField(virtueEnragedYInsigniaDiv, "Damage", "virtueEnragedInsigniaYdamage", 30);
            virtueEnragedInsigniaYsize = new FloatField(virtueEnragedYInsigniaDiv, "Size", "virtueEnragedInsigniaYsize", 2f);

            ConfigHeader virtueEnragedZInsigniaHeader = new ConfigHeader(virtueEnragedInsigniaDiv, "Forward Insignia", 12);
            ConfigDivision virtueEnragedZInsigniaDiv = new ConfigDivision(virtueEnragedInsigniaDiv, "virtueEnragedZInsigniaDiv");
            virtueEnragedInsigniaZtoggle = new BoolField(virtueEnragedInsigniaDiv, "Enabled", "virtueEnragedInsigniaZtoggle", true);
            virtueEnragedInsigniaZtoggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                virtueEnragedZInsigniaDiv.interactable = e.value;
                dirtyField = true;
            };
            virtueEnragedInsigniaZtoggle.TriggerValueChangeEvent();
            virtueEnragedInsigniaZdamage = new IntField(virtueEnragedZInsigniaDiv, "Damage", "virtueEnragedInsigniaZdamage", 15);
            virtueEnragedInsigniaZsize = new FloatField(virtueEnragedZInsigniaDiv, "Size", "virtueEnragedInsigniaZsize", 2f);

            ConfigHeader virtueEnragedXInsigniaHeader = new ConfigHeader(virtueEnragedInsigniaDiv, "Side Insignia", 12);
            ConfigDivision virtueEnragedXInsigniaDiv = new ConfigDivision(virtueEnragedInsigniaDiv, "virtueEnragedXInsigniaDiv");
            virtueEnragedInsigniaXtoggle = new BoolField(virtueEnragedInsigniaDiv, "Enabled", "virtueEnragedInsigniaXtoggle", true);
            virtueEnragedInsigniaXtoggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                virtueEnragedXInsigniaDiv.interactable = e.value;
                dirtyField = true;
            };
            virtueEnragedInsigniaXtoggle.TriggerValueChangeEvent();
            virtueEnragedInsigniaXdamage = new IntField(virtueEnragedXInsigniaDiv, "Damage", "virtueEnragedInsigniaXdamage", 15);
            virtueEnragedInsigniaXsize = new FloatField(virtueEnragedXInsigniaDiv, "Size", "virtueEnragedInsigniaXsize", 2f);

            ConfigDivision virtueEnragedLigthningDiv = new ConfigDivision(virtueTweakEnragedAttackDiv, "virtueEnragedLigthningDiv");
            virtueEnragedLightningDamage = new FloatField(virtueEnragedLigthningDiv, "Damage multiplier", "virtueEnragedLightningDamage", 1f);
            //virtueEnragedLightningSize = new FloatField(virtuePanel, "Size multiplier", "virtueEnragedLightningSize", 1f);
            virtueEnragedLightningDelay = new FloatField(virtueEnragedLigthningDiv, "Lighning delay", "virtueEnragedLightningDelay", 2f);

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
            ferrymanComboToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                ferrymanComboDiv.interactable = e.value;
                dirtyField = true;
            };
            ferrymanComboToggle.TriggerValueChangeEvent();
            ferrymanComboCount = new IntField(ferrymanComboDiv, "Count", "ferrymanComboCount", 3);
            ferrymanAttackDelay = new FloatField(ferrymanComboDiv, "Delay (0-1)", "ferrymanAttackDelay", 0.1f);

            // SENTRY
            new ConfigHeader(turretPanel, "Burst Fire");
            ConfigDivision turretBurstFireDiv = new ConfigDivision(turretPanel, "turretBurstFireDiv");
            turretBurstFireToggle = new BoolField(turretPanel, "Enabled", "turretBurstFireToggle", true);
            turretBurstFireToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                turretBurstFireDiv.interactable = e.value;
                dirtyField = true;
            };
            turretBurstFireToggle.TriggerValueChangeEvent();
            turretBurstFireCount = new IntField(turretBurstFireDiv, "Extra shots", "turretBurstFireCount", 1);
            turretBurstFireDelay = new FloatField(turretBurstFireDiv, "Delay between shots", "turretBurstFireDelay", 1f);

            // FLESH PRISON
            new ConfigHeader(fleshPrisonPanel, "Spin Insignia");
            ConfigDivision fleshPrionSpinAttackDiv = new ConfigDivision(fleshPrisonPanel, "fleshPrionSpinAttackDiv");
            fleshPrisonSpinAttackToggle = new BoolField(fleshPrisonPanel, "Enabled", "fleshPrisonSpinAttackToggle", true);
            fleshPrisonSpinAttackToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                fleshPrionSpinAttackDiv.interactable = e.value;
                dirtyField = true;
            };
            fleshPrisonSpinAttackToggle.TriggerValueChangeEvent();
            fleshPrisonSpinAttackCount = new IntField(fleshPrionSpinAttackDiv, "Insignia count", "fleshPrisonSpinAttackCount", 5);
            fleshPrisonSpinAttackDamage = new IntField(fleshPrionSpinAttackDiv, "Insignia damage", "fleshPrisonSpinAttackDamage", 10);
            fleshPrisonSpinAttackSize = new FloatField(fleshPrionSpinAttackDiv, "Insignia size", "fleshPrisonSpinAttackSize", 2f);
            fleshPrisonSpinAttackDistance = new FloatField(fleshPrionSpinAttackDiv, "Circle radius", "fleshPrisonSpinAttackDistance", 30f);
            fleshPrisonSpinAttackTurnSpeed = new FloatField(fleshPrionSpinAttackDiv, "Turn speed", "fleshPrisonSpinAttackTurnSpeed", 30f);
            fleshPrisonSpinAttackActivateSpeed = new FloatField(fleshPrionSpinAttackDiv, "Activasion speed", "fleshPrisonSpinAttackActivateSpeed", 0.5f);

            // MINOS PRIME
            new ConfigHeader(minosPrimePanel, "Random Teleport Before Shoot");
            ConfigDivision minosPrimeRandomTeleportDiv = new ConfigDivision(minosPrimePanel, "minosPrimeRandomTeleportDiv");
            minosPrimeRandomTeleportToggle = new BoolField(minosPrimePanel, "Enabled", "minosPrimeRandomTeleportToggle", true);
            minosPrimeRandomTeleportToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                minosPrimeRandomTeleportDiv.interactable = e.value;
                dirtyField = true;
            };
            minosPrimeRandomTeleportToggle.TriggerValueChangeEvent();
            minosPrimeRandomTeleportMinDistance = new FloatField(minosPrimeRandomTeleportDiv, "Minimum distance", "minosPrimeRandomTeleportMinDistance", 20f);
            minosPrimeRandomTeleportMaxDistance = new FloatField(minosPrimeRandomTeleportDiv, "Maximum distance", "minosPrimeRandomTeleportMaxDistance", 50f);

            // V2 - FIRST
            new ConfigHeader(v2FirstPanel, "Knuckleblaster");
            ConfigDivision v2FirstKnuckleBlasterDiv = new ConfigDivision(v2FirstPanel, "v2FirstKnuckleBlasterDiv");
            v2FirstKnuckleBlasterToggle = new BoolField(v2FirstPanel, "Enabled", "v2FirstKnuckleBlasterToggle", true);
            v2FirstKnuckleBlasterToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                v2FirstKnuckleBlasterDiv.interactable = e.value;
                dirtyField = true;
            };
            v2FirstKnuckleBlasterToggle.TriggerValueChangeEvent();
            v2FirstKnuckleBlasterCooldown = new FloatField(v2FirstKnuckleBlasterDiv, "Cooldown", "v2FirstKnuckleBlasterCooldown", 3f, 0f, float.MaxValue);
            ConfigDivision v2FirstKnuckleBlasterHitPlayerDiv = new ConfigDivision(v2FirstKnuckleBlasterDiv, "v2FirstKnuckleBlasterHitPlayerDiv");
            v2FirstKnuckleBlasterHitPlayerToggle = new BoolField(v2FirstKnuckleBlasterDiv, "Hit player", "v2FirstKnuckleBlasterHitPlayerToggle", true);
            v2FirstKnuckleBlasterHitPlayerToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                v2FirstKnuckleBlasterHitPlayerDiv.interactable = e.value;
                dirtyField = true;
            };
            v2FirstKnuckleBlasterHitPlayerToggle.TriggerValueChangeEvent();
            v2FirstKnuckleBlasterHitPlayerMinDistance = new FloatField(v2FirstKnuckleBlasterHitPlayerDiv, "Minimum distance to player", "v2FirstKnuckleBlasterHitPlayerMinDistance", 5f);
            v2FirstKnuckleBlasterHitDamage = new IntField(v2FirstKnuckleBlasterHitPlayerDiv, "Hit damage", "v2FirstKnuckleBlasterHitDamage", 5);
            v2FirstKnuckleBlasterDeflectShotgunToggle = new BoolField(v2FirstKnuckleBlasterDiv, "Deflect shotgun pellets", "v2FirstKnuckleBlasterDeflectShotgunToggle", true);
            v2FirstKnuckleBlasterDeflectShotgunToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                dirtyField = true;
            };
            v2FirstKnuckleBlasterExplosionDamage = new IntField(v2FirstKnuckleBlasterDiv, "Explosion damage", "v2FirstKnuckleBlasterExplosionDamage", 10);
            v2FirstKnuckleBlasterSize = new FloatField(v2FirstKnuckleBlasterDiv, "Explosion size", "v2FirstKnuckleBlasterSize", 15);
            v2FirstKnuckleBlasterSpeed = new FloatField(v2FirstKnuckleBlasterDiv, "Explosion speed", "v2FirstKnuckleBlasterSpeed", 15f / 2);

            new ConfigHeader(v2FirstPanel, "Grenade Snipe");
            ConfigDivision v2FirstCoreSnipeDiv = new ConfigDivision(v2FirstPanel, "v2FirstCoreSnipeDiv");
            v2FirstCoreSnipeToggle = new BoolField(v2FirstPanel, "Enabled", "v2FirstCoreSnipeToggle", true);
            v2FirstCoreSnipeToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                v2FirstCoreSnipeDiv.interactable = e.value;
                dirtyField = true;
            };
            v2FirstCoreSnipeToggle.TriggerValueChangeEvent();
            v2FirstCoreSnipeMaxDistanceToPlayer = new FloatField(v2FirstCoreSnipeDiv, "Max distance to player", "v2FirstCoreSnipeMaxDistanceToPlayer", 10f);
            v2FirstCoreSnipeMinDistanceToV2 = new FloatField(v2FirstCoreSnipeDiv, "Min distance to V2", "v2FirstCoreSnipeMinDistanceToV2", 20f);

            // V2 - SECOND
            v2SecondStartEnraged = new BoolField(v2SecondPanel, "Start enraged", "v2SecondStartEnraged", true);
            v2SecondRocketLauncherToggle = new BoolField(v2SecondPanel, "Use rocket launcher", "v2SecondRocketLauncherToggle", true);
            v2SecondFastCoin = new BoolField(v2SecondPanel, "Shoot coins separately", "v2SecondFastCoin", true);
            v2SecondCoinRailcannon = new BoolField(v2SecondPanel, "Electric railcannon chargeback", "v2SecondCoinRailcannon", true);
            v2SecondCoinRailcannon.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                v2SecondCoinRailcannonCooldown.interactable = e.value;
            };
            v2SecondCoinRailcannonCooldown = new FloatField(v2SecondPanel, "Electric railcannon cooldown", "v2SecondCoinRailcannonCooldown", 15f);
            v2SecondCoinRailcannon.TriggerValueChangeEvent();
            ConfigDivision v2SecondMalCannonDiv = new ConfigDivision(v2SecondPanel, "v2SecondMalCannonDiv");
            new ConfigHeader(v2SecondPanel, "Malicious Cannon Snipe");
            v2SecondMalCannonSnipeToggle = new BoolField(v2SecondPanel, "Enabled", "v2SecondMalCannonSnipeToggle", true);
            v2SecondMalCannonSnipeToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                v2SecondMalCannonDiv.interactable = e.value;
            };
            v2SecondMalCannonSnipeToggle.TriggerValueChangeEvent();
            v2SecondMalCannonSnipeCooldown = new FloatField(v2SecondMalCannonDiv, "Cooldown", "v2SecondMalCannonSnipeCooldown", 15f);
            v2SecondMalCannonSnipeReactTime = new FloatField(v2SecondMalCannonDiv, "React time", "v2SecondMalCannonSnipeReactTime", 0.2f);
            v2SecondMalCannonSnipeMaxDistanceToPlayer = new FloatField(v2SecondMalCannonDiv, "Max distance to player", "v2SecondMalCannonSnipeMaxDistanceToPlayer", 20f);
            v2SecondMalCannonSnipeMinDistanceToV2 = new FloatField(v2SecondMalCannonDiv, "Min distance to V2", "v2SecondMalCannonSnipeMinDistanceToV2", 30f);
            ConfigDivision v2SecondSnipeDiv = new ConfigDivision(v2SecondPanel, "v2SecondSnipeDiv");
            new ConfigHeader(v2SecondPanel, "Grenade Snipe");
            v2SecondCoreSnipeToggle = new BoolField(v2SecondPanel, "Enabled", "v2SecondCoreSnipeToggle", true);
            v2SecondCoreSnipeToggle.onValueChange += (BoolField.BoolValueChangeEvent e) =>
            {
                v2SecondSnipeDiv.interactable = e.value;
            };
            v2SecondCoreSnipeToggle.TriggerValueChangeEvent();
            v2SecondCoreSnipeMaxDistanceToPlayer = new FloatField(v2SecondSnipeDiv, "Max distance to player", "v2SecondCoreSnipeMaxDistanceToPlayer", 10f);
            v2SecondCoreSnipeMinDistanceToV2 = new FloatField(v2SecondSnipeDiv, "Min distance to V2", "v2SecondCoreSnipeMinDistanceToV2", 20f);

            config.Flush();
        }
    }
}
