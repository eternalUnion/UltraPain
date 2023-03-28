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
        public static Plugin instance;

        const string PLUGIN_GUID = "com.eternalUnion.difficultyTweak";
        const string PLUGIN_NAME = "Difficulty Tweak";
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

        private bool DefaultBoolReturn(GameObject o)
        {
            return true;
        }

        List<Tuple<string, GameObjectReference, Func<GameObject, bool>>> _toLoad;
        List<Tuple<string, GameObjectReference, Func<GameObject, bool>>> toLoad
        {
            get
            {
                if(_toLoad == null)
                {
                    _toLoad = new List<Tuple<string, GameObjectReference, Func<GameObject, bool>>>()
                    {
                        new Tuple<string, GameObjectReference, Func<GameObject, bool>>("ProjectileSpread", projectileSpread, DefaultBoolReturn),
                        new Tuple<string, GameObjectReference, Func<GameObject, bool>>("ProjectileHoming", homingProjectile, DefaultBoolReturn),
                        new Tuple<string, GameObjectReference, Func<GameObject, bool>>("ProjectileDecorative 2", decorativeProjectile2, DefaultBoolReturn),
                        new Tuple<string, GameObjectReference, Func<GameObject, bool>>("Grenade", shotgunGrenade, (GameObject o) =>
                        {
                            /*CapsuleCollider col = o.GetComponent<CapsuleCollider>();
                            col.direction = 1;
                            col.height = 2f;
                            col.radius = 1f;*/

                            return true;
                        }),
                        new Tuple<string, GameObjectReference, Func<GameObject, bool>>("TurretBeam", turretBeam, DefaultBoolReturn),
                        new Tuple<string, GameObjectReference, Func<GameObject, bool>>("MaliciousBeam", beam, DefaultBoolReturn),
                        new Tuple<string, GameObjectReference, Func<GameObject, bool>>("LightningStrikeExplosive", lighningStrikeExplosive, DefaultBoolReturn),
                        new Tuple<string, GameObjectReference, Func<GameObject, bool>>("LightningBoltWindupFollow Variant", lighningStrikeWindup, DefaultBoolReturn),

                        new Tuple<string, GameObjectReference, Func<GameObject, bool>>("Spider", maliciousFace, DefaultBoolReturn),
                        new Tuple<string, GameObjectReference, Func<GameObject, bool>>("Idol", idol, (GameObject o) =>
                        {
                            return o.GetComponent<Idol>() != null;
                        }),
                        new Tuple<string, GameObjectReference, Func<GameObject, bool>>("Ferryman", ferryman, (GameObject o) =>
                        {
                            return o.GetComponent<Ferryman>() != null;
                        }),
                    };
                }

                return _toLoad;
            }
        }

        void OnLoad()
        {
            Logger.LogInfo("All prefabs loaded, processing...");

            // Run when all prefabs are found
        }

        public void LoadPrefabs()
        {
            if (toLoad.Count == 0)
                return;

            if (Universe.CurrentGlobalState != Universe.GlobalState.SetupCompleted)
            {
                Thread.Sleep(100);
                Task.Run(LoadPrefabs);
                return;
            }

            for (int i = 0; i < toLoad.Count; i++)
                Resources.Load<GameObject>(toLoad[i].Item1);

            foreach (GameObject o in RuntimeHelper.FindObjectsOfTypeAll<GameObject>())
            {
                if (!IsPersistent(o))
                    continue;
                for(int i = toLoad.Count - 1; i >= 0; i--)
                {
                    if (o.name == toLoad[i].Item1 && toLoad[i].Item3(o))
                    {
                        Logger.LogInfo($"Loaded new asset: {o.name}");

                        toLoad[i].Item2.gameObject = o;
                        toLoad.RemoveAt(i);
                    }
                }
            }

            if (toLoad.Count == 0)
                OnLoad();
        }

        public void OnSceneChange(Scene before, Scene after)
        {
            Task.Run(LoadPrefabs);
        }

        public void Awake()
        {
            instance = this;

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
