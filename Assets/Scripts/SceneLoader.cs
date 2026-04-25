using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadMainScene()
    {
        // return from the result panel to the level grid
        SceneManager.LoadScene("MainScene");
    }
}
