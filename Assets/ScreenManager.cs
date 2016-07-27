using UnityEngine;
using System.Collections;

public class ScreenManager : MonoBehaviour {

    public GameObject gameScreen;
    public GameObject titleScreen;
    public GameObject resultScreen;
    public GameObject levelScreen;

    public void ShowGame()
    {
        gameScreen.SetActive(true);
        titleScreen.SetActive(false);
        resultScreen.SetActive(false);
        levelScreen.SetActive(false);
    }

    public void ShowTitle()
    {
        titleScreen.SetActive(true);
        gameScreen.SetActive(false);
        resultScreen.SetActive(false);
        levelScreen.SetActive(false);
    }

    public void ShowLevel()
    {
        levelScreen.SetActive(true);
        titleScreen.SetActive(false);
        resultScreen.SetActive(false);
        titleScreen.SetActive(false);
    }

    public void ShowResult()
    {
        resultScreen.SetActive(true);
        gameScreen.SetActive(false);
        titleScreen.SetActive(false);
        levelScreen.SetActive(false);
    }
}
