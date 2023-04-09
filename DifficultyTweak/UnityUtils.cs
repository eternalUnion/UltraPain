using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DifficultyTweak
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
    }
}
