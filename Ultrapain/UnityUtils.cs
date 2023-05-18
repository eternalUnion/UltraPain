using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

namespace Ultrapain
{
    public static class UnityUtils
    {
        public static List<T> InsertFill<T>(this List<T> list, int index, T obj)
        {
            if (index > list.Count)
            {
                int itemsToAdd = index - list.Count;
                for (int i = 0; i < itemsToAdd; i++)
                    list.Add(default(T));
                list.Add(obj);
            }
            else
                list.Insert(index, obj);

            return list;
        }

        public static void PrintGameobject(GameObject o, int iters = 0)
        {
            string logMessage = "";
            for (int i = 0; i < iters; i++)
                logMessage += '|';
            logMessage += o.name;

            Debug.Log(logMessage);
            foreach (Transform t in o.transform)
                PrintGameobject(t.gameObject, iters + 1);
        }

        public static IEnumerable<T> GetComponentsInChildrenRecursively<T>(Transform obj)
        {
            T component;
            foreach (Transform child in obj)
            {
                component = child.gameObject.GetComponent<T>();
                if (component != null)
                    yield return component;
                foreach (T childComp in GetComponentsInChildrenRecursively<T>(child))
                    yield return childComp;
            }

            yield break;
        }

        public static T GetComponentInChildrenRecursively<T>(Transform obj)
        {
            T component;
            foreach (Transform child in obj)
            {
                component = child.gameObject.GetComponent<T>();
                if (component != null)
                    return component;
                component = GetComponentInChildrenRecursively<T>(child);
                if (component != null)
                    return component;
            }

            return default(T);
        }

        public static Transform GetChildByNameRecursively(Transform parent, string name)
        {
            foreach(Transform t in parent)
            {
                if (t.name == name)
                    return t;
                Transform child = GetChildByNameRecursively(t, name);
                if (child != null)
                    return child;
            }

            return null;
        }

        public static Transform GetChildByTagRecursively(Transform parent, string tag)
        {
            foreach (Transform t in parent)
            {
                if (t.tag == tag)
                    return t;
                Transform child = GetChildByTagRecursively(t, tag);
                if (child != null)
                    return child;
            }

            return null;
        }

        public static T GetRandomIntWeightedItem<T>(IEnumerable<T> itemsEnumerable, Func<T, int> weightKey)
        {
            var items = itemsEnumerable.ToList();

            var totalWeight = items.Sum(x => weightKey(x));
            var randomWeightedIndex = UnityEngine.Random.RandomRangeInt(0, totalWeight);
            var itemWeightedIndex = 0;
            foreach (var item in items)
            {
                itemWeightedIndex += weightKey(item);
                if (randomWeightedIndex < itemWeightedIndex)
                    return item;
            }
            throw new ArgumentException("Collection count and weights must be greater than 0");
        }

        public static T GetRandomFloatWeightedItem<T>(IEnumerable<T> itemsEnumerable, Func<T, float> weightKey)
        {
            var items = itemsEnumerable.ToList();

            var totalWeight = items.Sum(x => weightKey(x));
            var randomWeightedIndex = UnityEngine.Random.Range(0, totalWeight);
            var itemWeightedIndex = 0f;
            foreach (var item in items)
            {
                itemWeightedIndex += weightKey(item);
                if (randomWeightedIndex < itemWeightedIndex)
                    return item;
            }
            throw new ArgumentException("Collection count and weights must be greater than 0");
        }
    }
}
