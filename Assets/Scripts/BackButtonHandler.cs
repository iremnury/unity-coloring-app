using UnityEngine;
using UnityEngine.SceneManagement;

public class BackButtonHandler : MonoBehaviour
{
    public void GoBack()
    {
        // let the player leave the coloring scene at any time
        SceneManager.LoadScene("MainScene");
    }
}
