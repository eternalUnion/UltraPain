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
        public static StringField  grenadeBoostStyleText;
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

        public static void Initialize()
        {
            if (config != null)
                return;

            config = PluginConfigurator.Create("ULTRAPAIN", Plugin.PLUGIN_GUID);
            config.postConfigChange += () =>
            {
                Plugin.PatchAll();
            };

            // ROOT PANEL
            new ConfigHeader(config.rootPanel, "Enemy Tweaks");
            enemyTweakToggle = new BoolField(config.rootPanel, "Enabled", "enemyTweakToggle", true);
            enemyPanel = new ConfigPanel(config.rootPanel, "Enemy Tweaks", "enemyTweakPanel");
            enemyTweakToggle.onValueChange += (BoolField.BoolValueChangeEvent data) =>
            {
                enemyPanel.interactable = data.value;
            };
            enemyPanel.interactable = enemyTweakToggle.value;

            new ConfigHeader(config.rootPanel, "Player Tweaks");
            playerTweakToggle = new BoolField(config.rootPanel, "Enabled", "playerTweakToggle", true);
            playerPanel = new ConfigPanel(config.rootPanel, "Player Tweaks", "enemyTweakPanel");
            playerTweakToggle.onValueChange += (BoolField.BoolValueChangeEvent data) =>
            {
                playerPanel.interactable = data.value;
            };
            playerPanel.interactable = playerTweakToggle.value;

            new ConfigHeader(config.rootPanel, "Difficulty Rich Presence Override", 20);
            discordRichPresenceToggle = new BoolField(config.rootPanel, "Discord rich presence", "discordRichPresenceToggle", false);
            steamRichPresenceToggle = new BoolField(config.rootPanel, "Steam rich presence", "steamRichPresenceToggle", false);

            new ConfigHeader(config.rootPanel, "Plugin Difficulty Name");
            pluginName = new StringField(config.rootPanel, "Difficulty name", "pluginName", "ULTRAPAIN");
            pluginName.onValueChange += (StringField.StringValueChangeEvent data) =>
            {
                if (Plugin.currentDifficultyButton != null)
                    Plugin.currentDifficultyButton.transform.Find("Name").GetComponent<Text>().text = data.value;

                if(Plugin.currentDifficultyPanel != null)
                    Plugin.currentDifficultyPanel.transform.Find("Title (1)").GetComponent<Text>().text = $"--{data.value}--";
            };

            new ConfigHeader(config.rootPanel, "Global Difficulty");
            globalDifficultySwitch = new BoolField(config.rootPanel, "Enabled", "globalDifficultySwitch", false);

            // PLAYER PANEL
            new ConfigHeader(playerPanel, "Rocket Boosting");
            rocketBoostToggle = new BoolField(playerPanel, "Enabled", "rocketBoostToggle", true);
            rocketBoostAlwaysExplodesToggle = new BoolField(playerPanel, "Always explode", "rocketBoostAlwaysExplodes", true);
            rocketBoostDamageMultiplierPerHit = new FloatField(playerPanel, "Damage multiplier per hit", "rocketBoostDamageMultiplier", 0.35f);
            rocketBoostSizeMultiplierPerHit = new FloatField(playerPanel, "Size multiplier per hit", "rocketBoostSizeMultiplier", 0.35f);
            rocketBoostSpeedMultiplierPerHit = new FloatField(playerPanel, "Speed multiplier per hit", "rocketBoostSpeedMultiplierPerHit", 0.35f);
            rocketBoostStyleText = new StringField(playerPanel, "Style text", "rocketBoostStyleText", "<color=lime>ROCKET BOOST</color>");
            rocketBoostStylePoints = new IntField(playerPanel, "Style points", "rocketBoostStylePoints", 10);

            new ConfigHeader(playerPanel, "Rocket Grabbing\r\n<size=16>(Can pull yourself to frozen rockets)</size>");
            rocketGrabbingToggle = new BoolField(playerPanel, "Enabled", "rocketGrabbingTabble", true);

            new ConfigHeader(playerPanel, "Grenade Boosting");
            grenadeBoostToggle = new BoolField(playerPanel, "Enabled", "grenadeBoostToggle", true);
            grenadeBoostDamageMultiplier = new FloatField(playerPanel, "Damage multiplier", "grenadeBoostDamageMultiplier", 1f);
            grenadeBoostSizeMultiplier = new FloatField(playerPanel, "Size multiplier", "grenadeBoostSizeMultiplier", 1f);
            grenadeBoostStyleText = new StringField(playerPanel, "Style text", "grenadeBoostStyleText", "<color=cyan>FISTFUL OF 'NADE</color>");
            grenadeBoostStylePoints = new IntField(playerPanel, "Style points", "grenadeBoostStylePoints", 10);

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
            new ConfigHeader(enemyPanel, "Prime Bosses");
            fleshPrisonPanel = new ConfigPanel(enemyPanel, "Flesh Prison", "fleshPrisonPanel");
            minosPrimePanel = new ConfigPanel(enemyPanel, "Minos Prime", "minosPrimePanel");

            // CERBERUS
            cerberusDashToggle = new BoolField(cerberusPanel, "Extra dashes", "cerberusDashToggle", true);
            cerberusTotalDashCount = new IntField(cerberusPanel, "Total dash count", "cerberusTotalDashCount", 3);

            // DRONE
            droneExplosionBeamToggle = new BoolField(dronePanel, "Can shoot explosions", "droneExplosionBeamToggle", true);
            droneSentryBeamToggle = new BoolField(dronePanel, "Can shoot sentry beam", "droneSentryBeamToggle", true);
            droneSentryBeamDamage = new FloatField(dronePanel, "Sentry beam damage", "droneSentryBeamDamage", 2f);

            // FILTH
            new ConfigHeader(filthPanel, "Explode On Hit");
            filthExplodeToggle = new BoolField(filthPanel, "Enabled", "filthExplodeOnHit", true);
            filthExplodeKills = new BoolField(filthPanel, "Explosion kills the filth", "filthExplosionKills", false);
            filthExplosionDamage = new IntField(filthPanel, "Explosion damage", "filthExplosionDamage", 30);
            filthExplosionSize = new FloatField(filthPanel, "Explosion size", "filthExplosionSize", 0.5f);

            // HIDEOUS MASS
            new ConfigHeader(hideousMassPanel, "Insignia On Projectile Hit");
            hideousMassInsigniaToggle = new BoolField(hideousMassPanel, "Enabled", "hideousMassInsigniaToggle", true);
            hideousMassInsigniaSpeed = new FloatField(hideousMassPanel, "Insignia speed multiplier", "hideousMassInsigniaSpeed", 2.5f);
            new ConfigHeader(hideousMassPanel, "Vertical Insignia", 12);
            hideousMassInsigniaYtoggle = new BoolField(hideousMassPanel, "Enabled", "hideousMassInsigniaYtoggle", true);
            hideousMassInsigniaYdamage = new IntField(hideousMassPanel, "Damage", "hideousMassInsigniaYdamage", 20);
            hideousMassInsigniaYsize = new FloatField(hideousMassPanel, "Size", "hideousMassInsigniaYsize", 2f);
            new ConfigHeader(hideousMassPanel, "Forward Insignia", 12);
            hideousMassInsigniaZtoggle = new BoolField(hideousMassPanel, "Enabled", "hideousMassInsigniaZtoggle", false);
            hideousMassInsigniaZdamage = new IntField(hideousMassPanel, "Damage", "hideousMassInsigniaZdamage", 20);
            hideousMassInsigniaZsize = new FloatField(hideousMassPanel, "Size", "hideousMassInsigniaZsize", 2f);
            new ConfigHeader(hideousMassPanel, "Side Insignia", 12);
            hideousMassInsigniaXtoggle = new BoolField(hideousMassPanel, "Enabled", "hideousMassInsigniaXtoggle", false);
            hideousMassInsigniaXdamage = new IntField(hideousMassPanel, "Damage", "hideousMassInsigniaXdamage", 20);
            hideousMassInsigniaXsize = new FloatField(hideousMassPanel, "Size", "hideousMassInsigniaXsize", 2f);

            // MALICIOUS FACE
            new ConfigHeader(maliciousFacePanel, "Radiance When Enraged");
            maliciousFaceRadianceOnEnrage = new BoolField(maliciousFacePanel, "Enabled", "maliciousFaceRadianceWhenEnraged", true);
            maliciousFaceRadianceAmount = new IntField(maliciousFacePanel, "Radiance level", "maliciousFaceRadianceAmount", 1);
            new ConfigHeader(maliciousFacePanel, "Homing Projectile");
            maliciousFaceHomingProjectileCount = new IntField(maliciousFacePanel, "Projectile count", "maliciousFaceHomingProjectileCount", 5);
            maliciousFaceHomingProjectileToggle = new BoolField(maliciousFacePanel, "Enabled", "maliciousFaceHomingProjectileToggle", true);
            maliciousFaceHomingProjectileDamage = new IntField(maliciousFacePanel, "Projectile damage", "maliciousFaceHomingProjectileDamage", 25);
            maliciousFaceHomingProjectileSpeed = new FloatField(maliciousFacePanel, "Projectile speed", "maliciousFaceHomingProjectileSpeed", 20f);
            maliciousFaceHomingProjectileTurnSpeed = new FloatField(maliciousFacePanel, "Projectile turn speed", "maliciousFaceHomingProjectileTurnSpeed", 0.4f);

            // MINDFLAYER
            new ConfigHeader(mindflayerPanel, "Shoot Tweak");
            mindflayerShootTweakToggle = new BoolField(mindflayerPanel, "Enabled", "mindflayerShootTweakToggle", true);
            mindflayerShootAmount = new IntField(mindflayerPanel, "Projectile amount", "mindflayerShootProjectileAmount", 20);
            mindflayerShootDelay = new FloatField(mindflayerPanel, "Delay between shots", "mindflayerShootProjectileDelay", 0.02f);
            new ConfigHeader(mindflayerPanel, "Melee Combo");
            mindflayerTeleportComboToggle = new BoolField(mindflayerPanel, "Enabled", "mindflayerMeleeCombo", true);

            // SHCISM
            new ConfigHeader(schismPanel, "Spread Attack");
            schismSpreadAttackToggle = new BoolField(schismPanel, "Enabled", "schismSpreadAttackToggle", true);
            schismSpreadAttackAngle = new FloatField(schismPanel, "Angular spread", "schismSpreadAttackAngle", 15f);
            schismSpreadAttackCount = new IntField(schismPanel, "Projectile count per side", "schismSpreadAttackCount", 1);

            // SOLIDER
            new ConfigHeader(soliderPanel, "Coins Ignore Weak Point");
            soliderCoinsIgnoreWeakPointToggle = new BoolField(soliderPanel, "Enabled", "soliderCoinsIgnoreWeakPoint", true);
            new ConfigHeader(soliderPanel, "Shoot Tweak");
            soliderShootTweakToggle = new BoolField(soliderPanel, "Enabled", "soliderShootTweakToggle", true);
            soliderShootCount = new IntField(soliderPanel, "Shoot count", "soliderShootCount", 3);
            new ConfigHeader(soliderPanel, "Shoot Grenade");
            soliderShootGrenadeToggle = new BoolField(soliderPanel, "Enabled", "soliderShootGrenade", true);
            soliderGrenadeDamage = new IntField(soliderPanel, "Explosion damage", "soliderGrenadeDamage", 10);
            soliderGrenadeSize = new FloatField(soliderPanel, "Explosion size multiplier", "soliderGrenadeSize", 0.75f);

            // STALKER
            new ConfigHeader(stalkerPanel, "Survive Explosion");
            stalkerSurviveExplosion = new BoolField(stalkerPanel, "Enabled", "stalkerSurviveExplosion", true);

            // STRAY
            new ConfigHeader(strayPanel, "Shoot Tweak");
            strayShootToggle = new BoolField(strayPanel, "Enabled", "strayShootToggle", true);
            strayShootCount = new IntField(strayPanel, "Extra projectile count", "strayShootCount", 2);
            strayShootSpeed = new FloatField(strayPanel, "Shoot speed", "strayShootSpeed", 20f);
            new ConfigHeader(strayPanel, "Coins Ignore Weak Point");
            strayCoinsIgnoreWeakPointToggle = new BoolField(strayPanel, "Enabled", "strayCoinsIgnoreWeakPointToggle", true);

            // STREET CLEANER
            new ConfigHeader(streetCleanerPanel, "Predictive Dodge");
            streetCleanerPredictiveDodgeToggle = new BoolField(streetCleanerPanel, "Enabled", "streetCleanerPredictiveDodgeToggle", true);
            new ConfigHeader(streetCleanerPanel, "Coins Ignore Weak Point");
            streetCleanerCoinsIgnoreWeakPointToggle = new BoolField(streetCleanerPanel, "Enabled", "streetCleanerCoinsIgnoreWeakPointToggle", true);

            // SWORDS MACHINE
            new ConfigHeader(swordsMachinePanel, "No Light Knockback");
            swordsMachineNoLightKnockbackToggle = new BoolField(swordsMachinePanel, "Enabled", "swordsMachineNoLightKnockbackToggle", true);
            new ConfigHeader(swordsMachinePanel, "Explosive Sword Throw");
            swordsMachineExplosiveSwordToggle = new BoolField(swordsMachinePanel, "Enabled", "swordsMachineExplosiveSwordToggle", true);
            swordsMachineExplosiveSwordDamage = new IntField(swordsMachinePanel, "Explosion damage", "swordsMachineExplosiveSwordDamage", 20);
            swordsMachineExplosiveSwordSize = new FloatField(swordsMachinePanel, "Explosion size multiplier", "swordsMachineExplosiveSwordSize", 0.5f);

            // VIRTUE
            new ConfigHeader(virtuePanel, "Tweak Normal Attack");
            virtueTweakNormalAttackToggle = new BoolField(virtuePanel, "Enabled", "virtueTweakNormalAttackToggle", true);
            virtueNormalAttackType = new EnumField<VirtueAttackType>(virtuePanel, "Attack type", "virtueNormalAttackType", VirtueAttackType.Insignia);

            ConfigDivision virtueNormalInsigniaDiv = new ConfigDivision(virtuePanel, "virtueNormalInsigniaDiv");
            ConfigHeader virtueNormalYInsigniaHeader = new ConfigHeader(virtueNormalInsigniaDiv, "Vertical Insignia", 12);
            virtueNormalInsigniaYtoggle = new BoolField(virtueNormalInsigniaDiv, "Enabled", "virtueNormalInsigniaYtoggle", true);
            virtueNormalInsigniaYdamage = new IntField(virtueNormalInsigniaDiv, "Damage", "virtueNormalInsigniaYdamage", 30);
            virtueNormalInsigniaYsize = new FloatField(virtueNormalInsigniaDiv, "Size", "virtueNormalInsigniaYsize", 2f);

            ConfigHeader virtueNormalZInsigniaHeader = new ConfigHeader(virtueNormalInsigniaDiv, "Forward Insignia", 12);
            virtueNormalInsigniaZtoggle = new BoolField(virtueNormalInsigniaDiv, "Enabled", "virtueNormalInsigniaZtoggle", false);
            virtueNormalInsigniaZdamage = new IntField(virtueNormalInsigniaDiv, "Damage", "virtueNormalInsigniaZdamage", 15);
            virtueNormalInsigniaZsize = new FloatField(virtueNormalInsigniaDiv, "Size", "virtueNormalInsigniaZsize", 2f);

            ConfigHeader virtueNormalXInsigniaHeader = new ConfigHeader(virtueNormalInsigniaDiv, "Side Insignia", 12);
            virtueNormalInsigniaXtoggle = new BoolField(virtueNormalInsigniaDiv, "Enabled", "virtueNormalInsigniaXtoggle", false);
            virtueNormalInsigniaXdamage = new IntField(virtueNormalInsigniaDiv, "Damage", "virtueNormalInsigniaXdamage", 15);
            virtueNormalInsigniaXsize = new FloatField(virtueNormalInsigniaDiv, "Size", "virtueNormalInsigniaXsize", 2f);

            ConfigDivision virtueNormalLigthningDiv = new ConfigDivision(virtuePanel, "virtueNormalLigthningDiv");
            virtueNormalLightningDamage = new FloatField(virtueNormalLigthningDiv, "Damage multiplier", "virtueNormalLightningDamage", 1f);
            //virtueNormalLightningSize = new FloatField(virtuePanel, "Size multiplier", "virtueNormalLightningSize", 1f);
            virtueNormalLightningDelay = new FloatField(virtueNormalLigthningDiv, "Lighning delay", "virtueNormalLightningDelay", 3f);

            void OnVirtueNormalAttackChange(EnumField<VirtueAttackType>.EnumValueChangeEvent newType)
            {
                if(newType.value == VirtueAttackType.Insignia)
                {
                    virtueNormalInsigniaDiv.hidden = false;
                    virtueNormalLigthningDiv.hidden = true;
                }
                else
                {
                    virtueNormalInsigniaDiv.hidden = true;
                    virtueNormalLigthningDiv.hidden = false;
                }
            }
            virtueNormalAttackType.onValueChange += OnVirtueNormalAttackChange;
            OnVirtueNormalAttackChange(new EnumField<VirtueAttackType>.EnumValueChangeEvent() { value = virtueNormalAttackType.value });

            new ConfigHeader(virtuePanel, "Tweak Enraged Attack");
            virtueTweakEnragedAttackToggle = new BoolField(virtuePanel, "Enabled", "virtueTweakEnragedAttackToggle", true);
            virtueEnragedAttackType = new EnumField<VirtueAttackType>(virtuePanel, "Attack type", "virtueEnragedAttackType", VirtueAttackType.Insignia);

            ConfigDivision virtueEnragedInsigniaDiv = new ConfigDivision(virtuePanel, "virtueEnragedInsigniaDiv");
            ConfigHeader virtueEnragedYInsigniaHeader = new ConfigHeader(virtueEnragedInsigniaDiv, "Vertical Insignia", 12);
            virtueEnragedInsigniaYtoggle = new BoolField(virtueEnragedInsigniaDiv, "Enabled", "virtueEnragedInsigniaYtoggle", true);
            virtueEnragedInsigniaYdamage = new IntField(virtueEnragedInsigniaDiv, "Damage", "virtueEnragedInsigniaYdamage", 30);
            virtueEnragedInsigniaYsize = new FloatField(virtueEnragedInsigniaDiv, "Size", "virtueEnragedInsigniaYsize", 2f);

            ConfigHeader virtueEnragedZInsigniaHeader = new ConfigHeader(virtueEnragedInsigniaDiv, "Forward Insignia", 12);
            virtueEnragedInsigniaZtoggle = new BoolField(virtueEnragedInsigniaDiv, "Enabled", "virtueEnragedInsigniaZtoggle", true);
            virtueEnragedInsigniaZdamage = new IntField(virtueEnragedInsigniaDiv, "Damage", "virtueEnragedInsigniaZdamage", 15);
            virtueEnragedInsigniaZsize = new FloatField(virtueEnragedInsigniaDiv, "Size", "virtueEnragedInsigniaZsize", 2f);

            ConfigHeader virtueEnragedXInsigniaHeader = new ConfigHeader(virtueEnragedInsigniaDiv, "Side Insignia", 12);
            virtueEnragedInsigniaXtoggle = new BoolField(virtueEnragedInsigniaDiv, "Enabled", "virtueEnragedInsigniaXtoggle", true);
            virtueEnragedInsigniaXdamage = new IntField(virtueEnragedInsigniaDiv, "Damage", "virtueEnragedInsigniaXdamage", 15);
            virtueEnragedInsigniaXsize = new FloatField(virtueEnragedInsigniaDiv, "Size", "virtueEnragedInsigniaXsize", 2f);

            ConfigDivision virtueEnragedLigthningDiv = new ConfigDivision(virtuePanel, "virtueEnragedLigthningDiv");
            virtueEnragedLightningDamage = new FloatField(virtueEnragedLigthningDiv, "Damage multiplier", "virtueEnragedLightningDamage", 1f);
            //virtueEnragedLightningSize = new FloatField(virtuePanel, "Size multiplier", "virtueEnragedLightningSize", 1f);
            virtueEnragedLightningDelay = new FloatField(virtueEnragedLigthningDiv, "Lighning delay", "virtueEnragedLightningDelay", 2f);

            void OnVirtueEnragedAttackChange(EnumField<VirtueAttackType>.EnumValueChangeEvent newType)
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
            }
            virtueEnragedAttackType.onValueChange += OnVirtueEnragedAttackChange;
            OnVirtueEnragedAttackChange(new EnumField<VirtueAttackType>.EnumValueChangeEvent() { value = virtueEnragedAttackType.value });

            new ConfigHeader(ferrymanPanel, "Melee Combo");
            ferrymanComboToggle = new BoolField(ferrymanPanel, "Enabled", "ferrymanComboToggle", true);
            ferrymanComboCount = new IntField(ferrymanPanel, "Count", "ferrymanComboCount", 3);
            ferrymanAttackDelay = new FloatField(ferrymanPanel, "Delay (0-1)", "ferrymanAttackDelay", 0.1f);

            new ConfigHeader(turretPanel, "Burst Fire");
            turretBurstFireToggle = new BoolField(turretPanel, "Enabled", "turretBurstFireToggle", true);
            turretBurstFireCount = new IntField(turretPanel, "Extra shots", "turretBurstFireCount", 1);
            turretBurstFireDelay = new FloatField(turretPanel, "Delay between shots", "turretBurstFireDelay", 1f);

            new ConfigHeader(fleshPrisonPanel, "Spin Insignia");
            fleshPrisonSpinAttackToggle = new BoolField(fleshPrisonPanel, "Enabled", "fleshPrisonSpinAttackToggle", true);
            fleshPrisonSpinAttackCount = new IntField(fleshPrisonPanel, "Insignia count", "fleshPrisonSpinAttackCount", 5);
            fleshPrisonSpinAttackDamage = new IntField(fleshPrisonPanel, "Insignia damage", "fleshPrisonSpinAttackDamage", 10);
            fleshPrisonSpinAttackSize = new FloatField(fleshPrisonPanel, "Insignia size", "fleshPrisonSpinAttackSize", 2f);
            fleshPrisonSpinAttackDistance = new FloatField(fleshPrisonPanel, "Circle radius", "fleshPrisonSpinAttackDistance", 30f);
            fleshPrisonSpinAttackTurnSpeed = new FloatField(fleshPrisonPanel, "Turn speed", "fleshPrisonSpinAttackTurnSpeed", 30f);
            fleshPrisonSpinAttackActivateSpeed = new FloatField(fleshPrisonPanel, "Activasion speed", "fleshPrisonSpinAttackActivateSpeed", 0.5f);

            new ConfigHeader(minosPrimePanel, "Random Teleport Before Shoot");
            minosPrimeRandomTeleportToggle = new BoolField(minosPrimePanel, "Enabled", "minosPrimeRandomTeleportToggle", true);
            minosPrimeRandomTeleportMinDistance = new FloatField(minosPrimePanel, "Minimum distance", "minosPrimeRandomTeleportMinDistance", 20f);
            minosPrimeRandomTeleportMaxDistance = new FloatField(minosPrimePanel, "Maximum distance", "minosPrimeRandomTeleportMaxDistance", 50f);

            config.Flush();
        }
    }
}
