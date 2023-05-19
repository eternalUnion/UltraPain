using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

namespace Ultrapain
{
    public static class UnityUtils
    {
        public static LayerMask envLayer = new LayerMask() { value = (1 << 8) | (1 << 24) };

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

        public static List<Tuple<EnemyIdentifier, float>> GetClosestEnemies(Vector3 sourcePosition, int enemyCount, Func<EnemyIdentifier, bool> validator)
        {
            List<Tuple<EnemyIdentifier, float>> targetEnemies = new List<Tuple<EnemyIdentifier, float>>();

            foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
            {
                float sqrMagnitude = (enemy.transform.position - sourcePosition).sqrMagnitude;
                if (targetEnemies.Count < enemyCount || sqrMagnitude < targetEnemies.Last().Item2)
                {
                    EnemyIdentifier eid = enemy.GetComponent<EnemyIdentifier>();
                    if (eid == null || eid.dead || eid.blessed)
                        continue;

                    if (Physics.Raycast(sourcePosition, enemy.transform.position - sourcePosition, out RaycastHit hit, Vector3.Distance(sourcePosition, enemy.transform.position) - 0.5f, envLayer))
                        continue;

                    if (!validator(eid))
                        continue;

                    if (targetEnemies.Count == 0)
                    {
                        targetEnemies.Add(new Tuple<EnemyIdentifier, float>(eid, sqrMagnitude));
                        continue;
                    }

                    int insertionPoint = targetEnemies.Count;
                    while (insertionPoint != 0 && targetEnemies[insertionPoint - 1].Item2 > sqrMagnitude)
                        insertionPoint -= 1;

                    targetEnemies.Insert(insertionPoint, new Tuple<EnemyIdentifier, float>(eid, sqrMagnitude));
                    if (targetEnemies.Count > enemyCount)
                        targetEnemies.RemoveAt(enemyCount);
                }
            }

            return targetEnemies;
        }
    }
}
