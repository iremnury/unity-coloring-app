using UnityEngine;
using UnityEngine.SceneManagement;

public class BackButtonHandler : MonoBehaviour
{
    public void GoBack()
    {
        SceneManager.LoadScene("MainScene");
    }
}