using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelPauseUIController : UIElement
{
    public Button LevelMenuButton;
    public Button LevelResumeButton;
    public Button LevelQuitButton;

    public List<Button> Buttons;


    public override void Initialize()
    {
        base.Initialize();

        GameController.OnGamePaused.AddListener(ActiveAllButton);
        GameController.OnGameResume.AddListener(DeactiveAllButton);

        LevelResumeButton.onClick.AddListener(GameController.Resume);
        
        LevelMenuButton.onClick.AddListener(() =>
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MENU");
        });

        LevelQuitButton.onClick.AddListener(() =>
        {
            Debug.Log("Quitting Game...");
            Application.Quit();
        });

    }
    public void ActiveAllButton()
    {
        for (int i = 0; i < Buttons.Count; i++)
        {
            Buttons[i].gameObject.SetActive(true);
        }
        
    }

    public void DeactiveAllButton()
    {
        for (int i = 0; i < Buttons.Count; i++)
        {
            Buttons[i].gameObject.SetActive(false);
        }
        
    }
    

}
