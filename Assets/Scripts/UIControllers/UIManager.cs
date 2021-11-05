using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class UIManager : Singleton<UIManager>
{
    [HideInInspector] public Canvas mainCanvas;

    void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            UIElement uiElement = transform.GetChild(i).GetComponent<UIElement>();
            if (uiElement) uiElement.Initialize();
        }


        mainCanvas = GetComponent<Canvas>();

    }



//#if UNITY_EDITOR
//    [Button("Prepare For Build")]
//    public void PrepareForBuild()
//    {
//        for (int i = 0; i < transform.childCount; i++)
//        {
//            UIElement uiElement = transform.GetChild(i).GetComponent<UIElement>();
//            if (uiElement) uiElement.PrepareForBuild();
//        }

//        UnityEditor.EditorUtility.SetDirty(gameObject);

//    }
//#endif
}
