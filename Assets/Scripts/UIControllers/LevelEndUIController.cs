using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelEndUIController : UIElement
{
    public Button LevelFailButton;
    public Button LevelWinButton;

    public override void Initialize()
    {
        base.Initialize();
        GameController.OnGameWin.AddListener(LevelWin);
        GameController.OnGameLost.AddListener(LevelFailed);

        LevelWinButton.onClick.AddListener(NextLevel);
        LevelFailButton.onClick.AddListener(RestartLevel);
    }

    public void LevelWin()
    {
        //Activate();
        LevelWinButton.transform.parent.gameObject.SetActive(true);
    }

    public void LevelFailed()
    {
        LevelFailButton.transform.parent.gameObject.SetActive(true);
    }

    public void NextLevel()
    {
        LevelWinButton.transform.parent.gameObject.SetActive(false);
        GameController.IncreaseLevel();
    }

    public void RestartLevel()
    {
        LevelFailButton.transform.parent.gameObject.SetActive(false);
        GameController.ReloadLevel();

    }

}
