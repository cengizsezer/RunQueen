using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameUIController : UIElement
{
    public Button MenuButton;

    public override void Initialize()
    {
        base.Initialize();

        if (GameController.IsGameStarted)
        {
            gameObject.SetActive(true);
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(true);
                
            }
        }

        if(GameController.IsGameStarted == false)
        MenuButton.onClick.AddListener(GameController.Pause);

    }
}
