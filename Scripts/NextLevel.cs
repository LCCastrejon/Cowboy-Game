using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevel : MonoBehaviour
{
    public string nextLevel;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) // Access gameObject property
        {
            LoadNextLevel();
        }
    }

    public void LoadNextLevel()
    {
        SceneManager.LoadScene(nextLevel);
    }
}
