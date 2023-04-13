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

        // PLAYER PANEL
        public static BoolField rocketBoostToggle;
        public static BoolField rocketBoostAlwaysExplodesToggle;
        public static FloatField rocketBoostDamageMultiplierPerHit;
        public static FloatField rocketBoostSizeMultiplierPerHit;
        public static FloatField rocketBoostSpeedMultiplierPerHit;
        public static StringField rocketBoostStyleText;
        public static IntegerField rocketBoostStylePoints;

        public static BoolField rocketGrabbingToggle;

        public static BoolField grenadeBoostToggle;
        public static FloatField grenadeBoostDamageMultiplier;
        public static FloatField grenadeBoostSizeMultiplier;
        public static StringField  grenadeBoostStyleText;
        public static IntegerField grenadeBoostStylePoints;

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

        // CERBERUS
        public static BoolField cerberusDashToggle;
        public static IntegerField cerberusTotalDashCount;

        // DRONE
        public static BoolField droneExplosionBeamToggle;
        public static BoolField droneSentryBeamToggle;
        public static FloatField droneSentryBeamDamage;

        // FILTH
        public static BoolField filthExplodeToggle;
        public static BoolField filthExplodeKills;
        public static IntegerField filthExplosionDamage;
        public static FloatField filthExplosionSize;

        // HIDEOUS MASS
        public static BoolField hideousMassInsigniaToggle;
        public static FloatField hideousMassInsigniaSpeed;
        public static BoolField hideousMassInsigniaYtoggle;
        public static FloatField hideousMassInsigniaYsize;
        public static IntegerField hideousMassInsigniaYdamage;
        public static BoolField hideousMassInsigniaZtoggle;
        public static FloatField hideousMassInsigniaZsize;
        public static IntegerField hideousMassInsigniaZdamage;
        public static BoolField hideousMassInsigniaXtoggle;
        public static FloatField hideousMassInsigniaXsize;
        public static IntegerField hideousMassInsigniaXdamage;

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
        public static IntegerField virtueNormalInsigniaXdamage;
        public static BoolField virtueNormalInsigniaYtoggle;
        public static FloatField virtueNormalInsigniaYsize;
        public static IntegerField virtueNormalInsigniaYdamage;
        public static BoolField virtueNormalInsigniaZtoggle;
        public static FloatField virtueNormalInsigniaZsize;
        public static IntegerField virtueNormalInsigniaZdamage;

        public static FloatField virtueNormalLightningDamage;
        public static FloatField virtueNormalLightningDelay;

        public static BoolField virtueTweakEnragedAttackToggle;
        public static EnumField<VirtueAttackType> virtueEnragedAttackType;

        public static BoolField virtueEnragedInsigniaXtoggle;
        public static FloatField virtueEnragedInsigniaXsize;
        public static IntegerField virtueEnragedInsigniaXdamage;
        public static BoolField virtueEnragedInsigniaYtoggle;
        public static FloatField virtueEnragedInsigniaYsize;
        public static IntegerField virtueEnragedInsigniaYdamage;
        public static BoolField virtueEnragedInsigniaZtoggle;
        public static FloatField virtueEnragedInsigniaZsize;
        public static IntegerField virtueEnragedInsigniaZdamage;

        public static FloatField virtueEnragedLightningDamage;
        public static FloatField virtueEnragedLightningDelay;

        // MALICIOUS FACE
        public static BoolField maliciousFaceRadianceOnEnrage;
        public static IntegerField maliciousFaceRadianceAmount;
        public static BoolField maliciousFaceHomingProjectileToggle;
        public static IntegerField maliciousFaceHomingProjectileDamage;
        public static FloatField maliciousFaceHomingProjectileTurnSpeed;
        public static FloatField maliciousFaceHomingProjectileSpeed;

        // MINDFLAYER
        public static BoolField mindflayerShootTweakToggle;
        public static IntegerField mindflayerShootAmount;
        public static FloatField mindflayerShootDelay;
        public static BoolField mindflayerTeleportComboToggle;

        // SCHISM
        public static BoolField schismSpreadAttackToggle;
        public static FloatField schismSpreadAttackAngle;
        public static IntegerField schismSpreadAttackCount;

        // SOLIDER
        public static BoolField soliderCoinsIgnoreWeakPointToggle;
        public static BoolField soliderShootTweakToggle;
        public static IntegerField soliderShootCount;
        public static BoolField soliderShootGrenadeToggle;
        public static IntegerField soliderGrenadeDamage;
        public static FloatField soliderGrenadeSize;

        // STALKER
        public static BoolField stalkerSurviveExplosion;

        // STRAY
        public static BoolField strayShootToggle;
        public static IntegerField strayShootCount;
        public static FloatField strayShootSpeed;

        // STREET CLEANER
        public static BoolField streetCleanerPredictiveDodgeToggle;
        public static BoolField streetCleanerCoinsIgnoreWeakPointToggle;

        // SWORDS MACHINE
        public static BoolField swordsMachineNoLightKnockbackToggle;
        public static BoolField swordsMachineExplosiveSwordToggle;
        public static IntegerField swordsMachineExplosiveSwordDamage;
        public static FloatField swordsMachineExplosiveSwordSize;

        public static void Initialize()
        {
            if (config != null)
                return;

            config = PluginConfigurator.Create("ULTRAPAIN", Plugin.PLUGIN_GUID);

            // ROOT PANEL
            new ConfigHeader(config.rootPanel, "Enemy Tweaks");
            enemyTweakToggle = new BoolField(config.rootPanel, "Enabled", "enemyTweakToggle", true);
            enemyPanel = new ConfigPanel(config.rootPanel, "Enemy Tweaks", "enemyTweakPanel");
            enemyTweakToggle.onValueChange = (BoolField.BoolValueChangeEvent data) =>
            {
                enemyPanel.interactable = data.value;
            };
            enemyPanel.interactable = enemyTweakToggle.value;

            new ConfigHeader(config.rootPanel, "Player Tweaks");
            playerTweakToggle = new BoolField(config.rootPanel, "Enabled", "playerTweakToggle", true);
            playerPanel = new ConfigPanel(config.rootPanel, "Player Tweaks", "enemyTweakPanel");
            playerTweakToggle.onValueChange = (BoolField.BoolValueChangeEvent data) =>
            {
                playerPanel.interactable = data.value;
            };
            playerPanel.interactable = playerTweakToggle.value;

            new ConfigHeader(config.rootPanel, "Difficulty Rich Presence Override", 20);
            discordRichPresenceToggle = new BoolField(config.rootPanel, "Discord rich presence", "discordRichPresenceToggle", false);
            steamRichPresenceToggle = new BoolField(config.rootPanel, "Steam rich presence", "steamRichPresenceToggle", false);

            new ConfigHeader(config.rootPanel, "Plugin Difficulty Name");
            pluginName = new StringField(config.rootPanel, "Difficulty name", "pluginName", "ULTRAPAIN");
            pluginName.onValueChange = (StringField.StringValueChangeEvent data) =>
            {
                if (Plugin.currentDifficultyButton != null)
                    Plugin.currentDifficultyButton.transform.Find("Name").GetComponent<Text>().text = data.value;

                if(Plugin.currentDifficultyPanel != null)
                    Plugin.currentDifficultyPanel.transform.Find("Title (1)").GetComponent<Text>().text = $"--{data.value}--";
            };

            // PLAYER PANEL
            new ConfigHeader(playerPanel, "Rocket Boosting");
            rocketBoostToggle = new BoolField(playerPanel, "Enabled", "rocketBoostToggle", true);
            rocketBoostAlwaysExplodesToggle = new BoolField(playerPanel, "Always explode", "rocketBoostAlwaysExplodes", true);
            rocketBoostDamageMultiplierPerHit = new FloatField(playerPanel, "Damage multiplier per hit", "rocketBoostDamageMultiplier", 0.35f);
            rocketBoostSizeMultiplierPerHit = new FloatField(playerPanel, "Size multiplier per hit", "rocketBoostSizeMultiplier", 0.35f);
            rocketBoostSpeedMultiplierPerHit = new FloatField(playerPanel, "Speed multiplier per hit", "rocketBoostSpeedMultiplierPerHit", 0.35f);
            rocketBoostStyleText = new StringField(playerPanel, "Style text", "rocketBoostStyleText", "<color=lime>ROCKET BOOST</color>");
            rocketBoostStylePoints = new IntegerField(playerPanel, "Style points", "rocketBoostStylePoints", 10);

            new ConfigHeader(playerPanel, "Rocket Grabbing\r\n<size=16>(Can pull yourself to frozen rockets)</size>");
            rocketGrabbingToggle = new BoolField(playerPanel, "Enabled", "rocketGrabbingTabble", true);

            new ConfigHeader(playerPanel, "Grenade Boosting");
            grenadeBoostToggle = new BoolField(playerPanel, "Enabled", "grenadeBoostToggle", true);
            grenadeBoostDamageMultiplier = new FloatField(playerPanel, "Damage multiplier", "grenadeBoostDamageMultiplier", 1f);
            grenadeBoostSizeMultiplier = new FloatField(playerPanel, "Size multiplier", "grenadeBoostSizeMultiplier", 1f);
            grenadeBoostStyleText = new StringField(playerPanel, "Style text", "grenadeBoostStyleText", "<color=cyan>FISTFUL OF 'NADE</color>");
            grenadeBoostStylePoints = new IntegerField(playerPanel, "Style points", "grenadeBoostStylePoints", 10);

            // ENEMY PANEL
            cerberusPanel = new ConfigPanel(enemyPanel, "Cerberus", "cerberusPanel");
            dronePanel = new ConfigPanel(enemyPanel, "Drone", "dronePanel");
            filthPanel = new ConfigPanel(enemyPanel, "Filth", "filthPanel");
            hideousMassPanel = new ConfigPanel(enemyPanel, "Hideous Mass", "hideousMassPanel");
            maliciousFacePanel = new ConfigPanel(enemyPanel, "Malicious Face", "maliciousFacePanel");
            mindflayerPanel = new ConfigPanel(enemyPanel, "Mindflayer", "mindflayerPanel");
            schismPanel = new ConfigPanel(enemyPanel, "Schism", "schismPanel");
            soliderPanel = new ConfigPanel(enemyPanel, "Soldier", "soliderPanel");
            stalkerPanel = new ConfigPanel(enemyPanel, "Stalker", "stalkerPanel");
            strayPanel = new ConfigPanel(enemyPanel, "Stray", "strayPanel");
            streetCleanerPanel = new ConfigPanel(enemyPanel, "Street Cleaner", "streetCleanerPanel");
            swordsMachinePanel = new ConfigPanel(enemyPanel, "Swords Machine", "swordsMachinePanel");
            virtuePanel = new ConfigPanel(enemyPanel, "Virtue", "virtuePanel");

            // CERBERUS
            cerberusDashToggle = new BoolField(cerberusPanel, "Extra dashes", "cerberusDashToggle", true);
            cerberusTotalDashCount = new IntegerField(cerberusPanel, "Total dash count", "cerberusTotalDashCount", 3);

            // DRONE
            droneExplosionBeamToggle = new BoolField(dronePanel, "Can shoot explosions", "droneExplosionBeamToggle", true);
            droneSentryBeamToggle = new BoolField(dronePanel, "Can shoot sentry beam", "droneSentryBeamToggle", true);
            droneSentryBeamDamage = new FloatField(dronePanel, "Sentry beam damage", "droneSentryBeamDamage", 2f);

            // FILTH
            new ConfigHeader(filthPanel, "Explode On Hit");
            filthExplodeToggle = new BoolField(filthPanel, "Enabled", "filthExplodeOnHit", true);
            filthExplodeKills = new BoolField(filthPanel, "Explosion kills the filth", "filthExplosionKills", false);
            filthExplosionDamage = new IntegerField(filthPanel, "Explosion damage", "filthExplosionDamage", 30);
            filthExplosionSize = new FloatField(filthPanel, "Explosion size", "filthExplosionSize", 0.5f);

            // HIDEOUS MASS
            new ConfigHeader(hideousMassPanel, "Insignia On Projectile Hit");
            hideousMassInsigniaToggle = new BoolField(hideousMassPanel, "Enabled", "hideousMassInsigniaToggle", true);
            hideousMassInsigniaSpeed = new FloatField(hideousMassPanel, "Insignia speed multiplier", "hideousMassInsigniaSpeed", 2.5f);
            new ConfigHeader(hideousMassPanel, "Vertical Insignia", 12);
            hideousMassInsigniaYtoggle = new BoolField(hideousMassPanel, "Enabled", "hideousMassInsigniaYtoggle", true);
            hideousMassInsigniaYdamage = new IntegerField(hideousMassPanel, "Damage", "hideousMassInsigniaYdamage", 20);
            hideousMassInsigniaYsize = new FloatField(hideousMassPanel, "Size", "hideousMassInsigniaYsize", 2f);
            new ConfigHeader(hideousMassPanel, "Forward Insignia", 12);
            hideousMassInsigniaZtoggle = new BoolField(hideousMassPanel, "Enabled", "hideousMassInsigniaZtoggle", false);
            hideousMassInsigniaZdamage = new IntegerField(hideousMassPanel, "Damage", "hideousMassInsigniaZdamage", 20);
            hideousMassInsigniaZsize = new FloatField(hideousMassPanel, "Size", "hideousMassInsigniaZsize", 2f);
            new ConfigHeader(hideousMassPanel, "Side Insignia", 12);
            hideousMassInsigniaXtoggle = new BoolField(hideousMassPanel, "Enabled", "hideousMassInsigniaXtoggle", false);
            hideousMassInsigniaXdamage = new IntegerField(hideousMassPanel, "Damage", "hideousMassInsigniaXdamage", 20);
            hideousMassInsigniaXsize = new FloatField(hideousMassPanel, "Size", "hideousMassInsigniaXsize", 2f);

            // MALICIOUS FACE
            new ConfigHeader(maliciousFacePanel, "Radiance When Enraged");
            maliciousFaceRadianceOnEnrage = new BoolField(maliciousFacePanel, "Enabled", "maliciousFaceRadianceWhenEnraged", true);
            maliciousFaceRadianceAmount = new IntegerField(maliciousFacePanel, "Radiance level", "maliciousFaceRadianceAmount", 1);
            new ConfigHeader(maliciousFacePanel, "Homing Projectile");
            maliciousFaceHomingProjectileToggle = new BoolField(maliciousFacePanel, "Enabled", "maliciousFaceHomingProjectileToggle", true);
            maliciousFaceHomingProjectileDamage = new IntegerField(maliciousFacePanel, "Projectile damage", "maliciousFaceHomingProjectileDamage", 25);
            maliciousFaceHomingProjectileSpeed = new FloatField(maliciousFacePanel, "Projectile speed", "maliciousFaceHomingProjectileSpeed", 20f);
            maliciousFaceHomingProjectileTurnSpeed = new FloatField(maliciousFacePanel, "Projectile turn speed", "maliciousFaceHomingProjectileTurnSpeed", 0.4f);

            // MINDFLAYER
            new ConfigHeader(mindflayerPanel, "Shoot Tweak");
            mindflayerShootTweakToggle = new BoolField(mindflayerPanel, "Enabled", "mindflayerShootTweakToggle", true);
            mindflayerShootAmount = new IntegerField(mindflayerPanel, "Projectile amount", "mindflayerShootProjectileAmount", 20);
            mindflayerShootDelay = new FloatField(mindflayerPanel, "Delay between shots", "mindflayerShootProjectileDelay", 0.02f);
            new ConfigHeader(mindflayerPanel, "Melee Combo");
            mindflayerTeleportComboToggle = new BoolField(mindflayerPanel, "Enabled", "mindflayerMeleeCombo", true);

            // SHCISM
            new ConfigHeader(schismPanel, "Spread Attack");
            schismSpreadAttackToggle = new BoolField(schismPanel, "Enabled", "schismSpreadAttackToggle", true);
            schismSpreadAttackAngle = new FloatField(schismPanel, "Angular spread", "schismSpreadAttackAngle", 15f);
            schismSpreadAttackCount = new IntegerField(schismPanel, "Projectile count per side", "schismSpreadAttackCount", 1);

            // SOLIDER
            new ConfigHeader(soliderPanel, "Coins Ignore Weak Point");
            soliderCoinsIgnoreWeakPointToggle = new BoolField(soliderPanel, "Enabled", "soliderCoinsIgnoreWeakPoint", true);
            new ConfigHeader(soliderPanel, "Shoot Tweak");
            soliderShootTweakToggle = new BoolField(soliderPanel, "Enabled", "soliderShootTweakToggle", true);
            soliderShootCount = new IntegerField(soliderPanel, "Shoot count", "soliderShootCount", 3);
            new ConfigHeader(soliderPanel, "Shoot Grenade");
            soliderShootGrenadeToggle = new BoolField(soliderPanel, "Enabled", "soliderShootGrenade", true);
            soliderGrenadeDamage = new IntegerField(soliderPanel, "Explosion damage", "soliderGrenadeDamage", 10);
            soliderGrenadeSize = new FloatField(soliderPanel, "Explosion size multiplier", "soliderGrenadeSize", 0.75f);

            // STALKER
            new ConfigHeader(stalkerPanel, "Survive Explosion");
            stalkerSurviveExplosion = new BoolField(stalkerPanel, "Enabled", "stalkerSurviveExplosion", true);

            // STRAY
            new ConfigHeader(strayPanel, "Shoot Tweak");
            strayShootToggle = new BoolField(strayPanel, "Enabled", "strayShootToggle", true);
            strayShootCount = new IntegerField(strayPanel, "Extra projectile count", "strayShootCount", 2);
            strayShootSpeed = new FloatField(strayPanel, "Shoot speed", "strayShootSpeed", 20f);

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
            swordsMachineExplosiveSwordDamage = new IntegerField(swordsMachinePanel, "Explosion damage", "swordsMachineExplosiveSwordDamage", 20);
            swordsMachineExplosiveSwordSize = new FloatField(swordsMachinePanel, "Explosion size multiplier", "swordsMachineExplosiveSwordSize", 0.5f);

            // VIRTUE
            new ConfigHeader(virtuePanel, "Tweak Normal Attack");
            virtueTweakNormalAttackToggle = new BoolField(virtuePanel, "Enabled", "virtueTweakNormalAttackToggle", true);
            virtueNormalAttackType = new EnumField<VirtueAttackType>(virtuePanel, "Attack type", "virtueNormalAttackType", VirtueAttackType.Insignia);
            
            ConfigHeader virtueNormalYInsigniaHeader = new ConfigHeader(virtuePanel, "Vertical Insignia", 12);
            virtueNormalInsigniaYtoggle = new BoolField(virtuePanel, "Enabled", "virtueNormalInsigniaYtoggle", true);
            virtueNormalInsigniaYdamage = new IntegerField(virtuePanel, "Damage", "virtueNormalInsigniaYdamage", 30);
            virtueNormalInsigniaYsize = new FloatField(virtuePanel, "Size", "virtueNormalInsigniaYsize", 2f);

            ConfigHeader virtueNormalZInsigniaHeader = new ConfigHeader(virtuePanel, "Forward Insignia", 12);
            virtueNormalInsigniaZtoggle = new BoolField(virtuePanel, "Enabled", "virtueNormalInsigniaZtoggle", false);
            virtueNormalInsigniaZdamage = new IntegerField(virtuePanel, "Damage", "virtueNormalInsigniaZdamage", 15);
            virtueNormalInsigniaZsize = new FloatField(virtuePanel, "Size", "virtueNormalInsigniaZsize", 2f);

            ConfigHeader virtueNormalXInsigniaHeader = new ConfigHeader(virtuePanel, "Side Insignia", 12);
            virtueNormalInsigniaXtoggle = new BoolField(virtuePanel, "Enabled", "virtueNormalInsigniaXtoggle", false);
            virtueNormalInsigniaXdamage = new IntegerField(virtuePanel, "Damage", "virtueNormalInsigniaXdamage", 15);
            virtueNormalInsigniaXsize = new FloatField(virtuePanel, "Size", "virtueNormalInsigniaXsize", 2f);

            virtueNormalLightningDamage = new FloatField(virtuePanel, "Damage multiplier", "virtueNormalLightningDamage", 1f);
            //virtueNormalLightningSize = new FloatField(virtuePanel, "Size multiplier", "virtueNormalLightningSize", 1f);
            virtueNormalLightningDelay = new FloatField(virtuePanel, "Lighning delay", "virtueNormalLightningDelay", 3f);

            void OnVirtueNormalAttackChange(EnumValueChangeEvent<VirtueAttackType> newType)
            {
                if(newType.value == VirtueAttackType.Insignia)
                {
                    virtueNormalLightningDamage.hidden = true;
                    virtueNormalLightningDelay.hidden = true;
                    //virtueNormalLightningSize.hidden = true;

                    virtueNormalYInsigniaHeader.hidden = false;
                    virtueNormalInsigniaYtoggle.hidden = false;
                    virtueNormalInsigniaYdamage.hidden = false;
                    virtueNormalInsigniaYsize.hidden = false;

                    virtueNormalZInsigniaHeader.hidden = false;
                    virtueNormalInsigniaZtoggle.hidden = false;
                    virtueNormalInsigniaZdamage.hidden = false;
                    virtueNormalInsigniaZsize.hidden = false;

                    virtueNormalXInsigniaHeader.hidden = false;
                    virtueNormalInsigniaXtoggle.hidden = false;
                    virtueNormalInsigniaXdamage.hidden = false;
                    virtueNormalInsigniaXsize.hidden = false;
                }
                else
                {
                    virtueNormalLightningDamage.hidden = false;
                    virtueNormalLightningDelay.hidden = false;
                    //virtueNormalLightningSize.hidden = false;

                    virtueNormalYInsigniaHeader.hidden = true;
                    virtueNormalInsigniaYtoggle.hidden = true;
                    virtueNormalInsigniaYdamage.hidden = true;
                    virtueNormalInsigniaYsize.hidden = true;

                    virtueNormalZInsigniaHeader.hidden = true;
                    virtueNormalInsigniaZtoggle.hidden = true;
                    virtueNormalInsigniaZdamage.hidden = true;
                    virtueNormalInsigniaZsize.hidden = true;

                    virtueNormalXInsigniaHeader.hidden = true;
                    virtueNormalInsigniaXtoggle.hidden = true;
                    virtueNormalInsigniaXdamage.hidden = true;
                    virtueNormalInsigniaXsize.hidden = true;
                }
            }
            virtueNormalAttackType.onValueChange = OnVirtueNormalAttackChange;
            OnVirtueNormalAttackChange(new EnumValueChangeEvent<VirtueAttackType>() { value = virtueNormalAttackType.value });

            new ConfigHeader(virtuePanel, "Tweak Enraged Attack");
            virtueTweakEnragedAttackToggle = new BoolField(virtuePanel, "Enabled", "virtueTweakEnragedAttackToggle", true);
            virtueEnragedAttackType = new EnumField<VirtueAttackType>(virtuePanel, "Attack type", "virtueEnragedAttackType", VirtueAttackType.Insignia);

            ConfigHeader virtueEnragedYInsigniaHeader = new ConfigHeader(virtuePanel, "Vertical Insignia", 12);
            virtueEnragedInsigniaYtoggle = new BoolField(virtuePanel, "Enabled", "virtueEnragedInsigniaYtoggle", true);
            virtueEnragedInsigniaYdamage = new IntegerField(virtuePanel, "Damage", "virtueEnragedInsigniaYdamage", 30);
            virtueEnragedInsigniaYsize = new FloatField(virtuePanel, "Size", "virtueEnragedInsigniaYsize", 2f);

            ConfigHeader virtueEnragedZInsigniaHeader = new ConfigHeader(virtuePanel, "Forward Insignia", 12);
            virtueEnragedInsigniaZtoggle = new BoolField(virtuePanel, "Enabled", "virtueEnragedInsigniaZtoggle", true);
            virtueEnragedInsigniaZdamage = new IntegerField(virtuePanel, "Damage", "virtueEnragedInsigniaZdamage", 15);
            virtueEnragedInsigniaZsize = new FloatField(virtuePanel, "Size", "virtueEnragedInsigniaZsize", 2f);

            ConfigHeader virtueEnragedXInsigniaHeader = new ConfigHeader(virtuePanel, "Side Insignia", 12);
            virtueEnragedInsigniaXtoggle = new BoolField(virtuePanel, "Enabled", "virtueEnragedInsigniaXtoggle", true);
            virtueEnragedInsigniaXdamage = new IntegerField(virtuePanel, "Damage", "virtueEnragedInsigniaXdamage", 15);
            virtueEnragedInsigniaXsize = new FloatField(virtuePanel, "Size", "virtueEnragedInsigniaXsize", 2f);

            virtueEnragedLightningDamage = new FloatField(virtuePanel, "Damage multiplier", "virtueEnragedLightningDamage", 1f);
            //virtueEnragedLightningSize = new FloatField(virtuePanel, "Size multiplier", "virtueEnragedLightningSize", 1f);
            virtueEnragedLightningDelay = new FloatField(virtuePanel, "Lighning delay", "virtueEnragedLightningDelay", 2f);

            void OnVirtueEnragedAttackChange(EnumValueChangeEvent<VirtueAttackType> newType)
            {
                if (newType.value == VirtueAttackType.Insignia)
                {
                    virtueEnragedLightningDamage.hidden = true;
                    virtueEnragedLightningDelay.hidden = true;
                    //virtueEnragedLightningSize.hidden = true;

                    virtueEnragedYInsigniaHeader.hidden = false;
                    virtueEnragedInsigniaYtoggle.hidden = false;
                    virtueEnragedInsigniaYdamage.hidden = false;
                    virtueEnragedInsigniaYsize.hidden = false;

                    virtueEnragedZInsigniaHeader.hidden = false;
                    virtueEnragedInsigniaZtoggle.hidden = false;
                    virtueEnragedInsigniaZdamage.hidden = false;
                    virtueEnragedInsigniaZsize.hidden = false;

                    virtueEnragedXInsigniaHeader.hidden = false;
                    virtueEnragedInsigniaXtoggle.hidden = false;
                    virtueEnragedInsigniaXdamage.hidden = false;
                    virtueEnragedInsigniaXsize.hidden = false;
                }
                else
                {
                    virtueEnragedLightningDamage.hidden = false;
                    virtueEnragedLightningDelay.hidden = false;
                    //virtueEnragedLightningSize.hidden = false;

                    virtueEnragedYInsigniaHeader.hidden = true;
                    virtueEnragedInsigniaYtoggle.hidden = true;
                    virtueEnragedInsigniaYdamage.hidden = true;
                    virtueEnragedInsigniaYsize.hidden = true;

                    virtueEnragedZInsigniaHeader.hidden = true;
                    virtueEnragedInsigniaZtoggle.hidden = true;
                    virtueEnragedInsigniaZdamage.hidden = true;
                    virtueEnragedInsigniaZsize.hidden = true;

                    virtueEnragedXInsigniaHeader.hidden = true;
                    virtueEnragedInsigniaXtoggle.hidden = true;
                    virtueEnragedInsigniaXdamage.hidden = true;
                    virtueEnragedInsigniaXsize.hidden = true;
                }
            }
            virtueEnragedAttackType.onValueChange = OnVirtueEnragedAttackChange;
            OnVirtueEnragedAttackChange(new EnumValueChangeEvent<VirtueAttackType>() { value = virtueEnragedAttackType.value });

            config.Flush();
        }
    }
}
