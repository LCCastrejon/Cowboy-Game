using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class LevelController : MonoBehaviour
{
    public string nextLevel;

    public void LoadNextLevel()
    {
        SceneManager.LoadScene(nextLevel);
    }

    public void SwitchLevels(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
