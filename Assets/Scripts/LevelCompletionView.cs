using UnityEngine;

public class LevelCompletionView : MonoBehaviour
{
    public string levelID = "Level1";
    public GameObject completedMark;

    void Start()
    {
        // show the completion icon only for finished levels
        bool isCompleted = PlayerPrefs.GetInt(levelID, 0) == 1;
        completedMark.SetActive(isCompleted);
    }
}
