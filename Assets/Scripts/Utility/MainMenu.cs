using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        FindObjectOfType<AudioManager>().Play("Theme");
    }
    public void QuitGame()
    {
        Debug.Log("Quited Game");
        Application.Quit();
    }
    public void GoToMainMenu()
    {
        var n = FindObjectOfType<Canvas>().transform.GetChild(6).gameObject;
        n.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
        FindObjectOfType<AudioManager>().Stop("Theme");
        Time.timeScale = 1f;
    }
}
