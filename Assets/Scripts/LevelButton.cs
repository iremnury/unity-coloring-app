using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelButton : MonoBehaviour
{
    public void OnButtonClick()
    {
        // open the coloring scene when the level button is pressed
        SceneManager.LoadScene("ColoringScene");
    }
}
