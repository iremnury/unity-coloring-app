using UnityEngine;

public class ResetProgress
{
    [UnityEditor.MenuItem("Tools/Reset PlayerPrefs")]
    public static void Reset()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("All progress reset");
    }
}