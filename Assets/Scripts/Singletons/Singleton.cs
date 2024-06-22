using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public  static T  Instance
    {
        get
        {
            if (mInstance == null)
            {
                mInstance = FindObjectOfType<T>() as T;

                if (mInstance == null)
                {
                    mInstance = new GameObject(typeof(T).ToString(), typeof(T)) as T;
                }
            }
            return mInstance;
        }
    }
    private static T mInstance;
}
