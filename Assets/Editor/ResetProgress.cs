using UnityEngine;

public class ResetProgress
{
    [UnityEditor.MenuItem("Tools/Reset PlayerPrefs")]
    public static void Reset()
    {
        // clear prefs and saved paint files together
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        LevelProgressStorage.DeleteAll();

        if (Application.isPlaying)
        {
            PaintOnClick[] activePaintCanvases = Object.FindObjectsByType<PaintOnClick>(FindObjectsSortMode.None);
            foreach (PaintOnClick activePaintCanvas in activePaintCanvases)
            {
                activePaintCanvas.ResetLoadedProgress();
            }
        }

        Debug.Log("All progress reset");
    }
}
