using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelButton : MonoBehaviour
{
    // this number decides which texture the coloring scene should load
    public int levelNumber;

    public void OnButtonClick()
    {
        // store the selected level before opening the coloring scene
        GameManager.selectedLevel = levelNumber;
        Debug.Log("Selected level: " + levelNumber);
        SceneManager.LoadScene("ColoringScene");
    }
}

