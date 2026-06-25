using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class MainMenu : MonoBehaviour
{
    public GameObject menuPanel;
    public GameManager gameManager;

void Start()
{
    ShowMenu();

    AudioManager.Instance?.PlayMenuMusic();
}

void ShowMenu()
{
    menuPanel.SetActive(true);

    gameManager.paused = true;
}

    public void Play()
    {
        menuPanel.SetActive(false);

        // снять паузу → таймер начинает идти
        gameManager.paused = false;

        AudioManager.Instance?.PlayGameMusic();

        Debug.Log("ИГРА НАЧАЛАСЬ");
    }

    public void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
