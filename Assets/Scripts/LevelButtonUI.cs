using UnityEngine;
using UnityEngine.UI;

public class LevelButtonUI : MonoBehaviour
{
    public string levelID = "Level1";
    public Image buttonImage;

    void Start()
    {
        // tint finished levels so they stand out in the grid
        if (PlayerPrefs.GetInt(levelID, 0) == 1)
        {
            buttonImage.color = Color.green;
        }
    }
}
