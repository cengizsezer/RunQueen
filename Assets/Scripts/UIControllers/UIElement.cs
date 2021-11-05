using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIElement : MonoBehaviour
{
    
    protected UIManager UIManager;
    protected GameController GameController;

    public bool InitializeOnStart = false;
    public bool ActivateWithParent = true;

    [SerializeField, UnityEngine.UI.Extensions.ReadOnly] protected bool isInitialized = false;

    

    public virtual void Initialize()
    {

        if (isInitialized) 
        {
            Debug.LogError(gameObject.name + " is already initialized." +
                      " It is being tried to be initialized more than once which may cause unpredictable issue.");
            return;
        }

        isInitialized = true;
        gameObject.SetActive(true);
        UIManager = UIManager.request();
        GameController = GameController.request();


    }

    protected virtual void Start()
    {
        if (InitializeOnStart)
        {
            Initialize();
        }
            

    }

   
}
