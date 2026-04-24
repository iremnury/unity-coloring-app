using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelButton : MonoBehaviour
{
    public int levelNumber;

    public void OnButtonClick()
    {
        
        GameManager.selectedLevel = levelNumber;
        Debug.Log("Selected level: " + levelNumber);
        SceneManager.LoadScene("ColoringScene"); // open the coloring scene when the level button is pressed
    }
}


