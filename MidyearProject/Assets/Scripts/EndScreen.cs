using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class EndScreen : MonoBehaviour
{
    public void GoToScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void QuitApp()
    {
        Debug.Log("Quit game requested");
        Application.Quit();
    }
}