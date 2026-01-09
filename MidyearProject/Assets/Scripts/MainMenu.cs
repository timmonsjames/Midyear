using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
    public void GoToScene(string sceneName) {
        SceneManager.LoadScene(sceneName);
    }

    public void QuitApp() {
        Debug.Log("Quit game requested");
        Application.Quit();
    }
    public GameObject InstructionsPanel;
    public void ShowInstructions()
    {
        InstructionsPanel.SetActive(true);
    }
    public void HideInstructions()
    {
        InstructionsPanel.SetActive(false);
    }
}
    