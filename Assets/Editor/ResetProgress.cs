using UnityEngine;

public class ResetProgress
{
    [UnityEditor.MenuItem("Tools/Reset PlayerPrefs")]
    public static void Reset()
    {
        // clear prefs and saved paint files together
        PlayerPrefs.DeleteAll();
        LevelProgressStorage.DeleteAll();
        Debug.Log("All progress reset");
    }
}
