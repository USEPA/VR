using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;

        public static bool IsInitialized
        {
            get
            {
                return instance != null;
            }
        }

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<T>();

                    if (FindObjectsOfType<T>().Length > 1)
                    {
                        Debug.LogError("[Singleton] Something went really wrong - there should never be more than 1 singleton! Reopening the scene might fix it.");
                        return instance;
                    }

                    if (instance == null)
                    {
                        Debug.LogError("[Singleton] Something went really wrong - specified Singleton does not found!");
                    }
                }
                return instance;
            }
        }
    }
}

