using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelStartUIController : UIElement
{
    public GameObject TapToStartText;
    public override void Initialize()
    {
        base.Initialize();

        if (!GameController.Instance.AutoStart)
        {
            TapToStartText.SetActive(true);
        }
       
    }
    private void Update()
    {
        if (GameController.IsGameStarted && TapToStartText.activeInHierarchy)
        {
            TapToStartText.SetActive(false);
        }
    }


}
