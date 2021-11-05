using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton<T>: MonoBehaviour where T :Component
{
    //FİELD
    private static T instance;

    //PROPERTY
    public static T Instance
    {
        get
        {
            if(instance == null)
            {
                instance = FindObjectOfType<T>();
            }
            return instance;
        }
    }
    //METHODS
    protected virtual void Awake()
    {
        if(instance == null)
        {
            instance = this as T;
        }
        else
        {
            Destroy(gameObject);
        }
        
    }

    public static T request()
    {
        if(!instance)
        {
            Debug.Log("There is no instance of " + typeof(T).Name + " in the scene");
        }
        return instance;
    }

    public static T forceRequest()
    {
        if(instance == null)
        {
            GameObject ownerObject = new GameObject();
            instance = ownerObject.AddComponent<T>();
        }
        return instance;
    }

}
