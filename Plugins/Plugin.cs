using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Plugins
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        const string PLUGIN_GUID = "com.eternalUnion.soundEffectVolumeBar";
        const string PLUGIN_NAME = "Sound Effect Bar";
        const string PLUGIN_VERSION = "1.0.0";

        GameObject sfxBar;
        Slider sfxSlider;

        List<Tuple<float, AudioSource>> sources = new List<Tuple<float, AudioSource>>();

        #region UTILS
        public GameObject FindGameObjectRecursive(Transform t, string name)
        {
            foreach (Transform o in t)
            {
                if (o.gameObject.name == name)
                    return o.gameObject;
                GameObject recursiveResult = FindGameObjectRecursive(o.transform, name);
                if (recursiveResult != null)
                    return recursiveResult;
            }
            return null;
        }

        public T FindComponentRecursive<T>(Transform t)
        {
            T component;
            foreach(Transform tr in t)
            {
                component = tr.gameObject.GetComponent<T>();
                if (component != null)
                    return component;

                T recursiveComponent = FindComponentRecursive<T>(tr);
                if (recursiveComponent != null)
                    return recursiveComponent;
            }
            return default(T);
        }

        public GameObject GetCanvas()
        {
            foreach (GameObject o in SceneManager.GetActiveScene().GetRootGameObjects())
                if (o.name == "Canvas")
                    return o;
            return null;
        }
        #endregion

        private void GenerateSourceListRecursive(Transform t)
        {
            AudioSource audSource;
            foreach(Transform child in t)
            {
                audSource = child.gameObject.GetComponent<AudioSource>();
                if (audSource != null && !audSource.gameObject.name.Contains("Music"))
                {
                    sources.Add(new Tuple<float, AudioSource>(audSource.volume, audSource));
                    Logger.LogInfo(audSource.gameObject.name);
                }
                GenerateSourceListRecursive(child);
            }
        }

        private void GenerateSourceList()
        {
            sources.Clear();
            AudioSource audSource;

            foreach(GameObject rootObj in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                audSource = rootObj.GetComponent<AudioSource>();
                if (audSource != null && !audSource.gameObject.name.Contains("Music"))
                {
                    sources.Add(new Tuple<float, AudioSource>(audSource.volume, audSource));
                    Logger.LogInfo(audSource.gameObject.name);
                }

                GenerateSourceListRecursive(rootObj.transform);
            }
        }

        private void OnSliderChange(float value)
        {
            value /= 100.0f;
            foreach(Tuple<float, AudioSource> pair in sources)
            {
                pair.Item2.volume = pair.Item1 * value;
            }
        }

        private void OnSceneChange(Scene before, Scene after)
        {
            GameObject canvas = GetCanvas();
            if(canvas == null)
            {
                Logger.LogWarning("Could not find root canvas");
                return;
            }

            GameObject musicBar = FindGameObjectRecursive(canvas.transform, "Music Volume");
            if(musicBar == null)
            {
                Logger.LogWarning("Could not find music bar");
                return;
            }

            sfxBar = Instantiate(musicBar, musicBar.transform.parent);
            sfxBar.name = "SFX Volume";
            try
            {
                sfxBar.transform.Find("Text").GetComponent<Text>().text = "SFX VOLUME";
            }
            catch (Exception) { }
            sfxBar.transform.localPosition = new Vector3(0, -160, 0);

            sfxSlider = FindComponentRecursive<Slider>(sfxBar.transform);
            if(sfxSlider == null)
            {
                Logger.LogWarning("Could not find slider component");
                sfxBar = null;
                return;
            }

            GenerateSourceList();
            Logger.LogInfo($"Found {sources.Count} sources");
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.value = 100;
            sfxSlider.onValueChanged.AddListener(OnSliderChange);
        }

        public void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PLUGIN_GUID} is loaded!");

            SceneManager.activeSceneChanged += OnSceneChange;
        }
    }
}
