using System.IO;
using UnityEngine;

public static class LevelProgressStorage
{
    // keep level save files easy to find and clean up
    private const string FilePrefix = "level-progress-";
    private const string FileExtension = ".bytes";

    public static bool TryLoad(string levelID, int width, int height, out byte[] rawPixels)
    {
        rawPixels = null;

        string path = GetPath(levelID);
        if (!File.Exists(path))
        {
            return false;
        }

        try
        {
            byte[] bytes = File.ReadAllBytes(path);

            // raw rgba32 data should always match texture size
            if (bytes.Length != width * height * 4)
            {
                Debug.LogWarning($"Saved progress for {levelID} has an unexpected size. Ignoring it.");
                return false;
            }

            rawPixels = bytes;
            return true;
        }
        catch (IOException exception)
        {
            Debug.LogWarning($"Could not load saved progress for {levelID}: {exception.Message}");
            return false;
        }
    }

    public static void Save(string levelID, byte[] rawPixels)
    {
        if (rawPixels == null || rawPixels.Length == 0)
        {
            return;
        }

        try
        {
            File.WriteAllBytes(GetPath(levelID), rawPixels);
        }
        catch (IOException exception)
        {
            Debug.LogWarning($"Could not save progress for {levelID}: {exception.Message}");
        }
    }

    public static void DeleteAll()
    {
        if (!Directory.Exists(Application.persistentDataPath))
        {
            return;
        }

        // delete only the files created by this storage helper
        string searchPattern = $"{FilePrefix}*{FileExtension}";
        foreach (string path in Directory.GetFiles(Application.persistentDataPath, searchPattern))
        {
            try
            {
                File.Delete(path);
            }
            catch (IOException exception)
            {
                Debug.LogWarning($"Could not delete saved progress file {path}: {exception.Message}");
            }
        }
    }

    private static string GetPath(string levelID)
    {
        string safeLevelID = Sanitize(levelID);
        return Path.Combine(Application.persistentDataPath, $"{FilePrefix}{safeLevelID}{FileExtension}");
    }

    private static string Sanitize(string levelID)
    {
        if (string.IsNullOrWhiteSpace(levelID))
        {
            return "unknown-level";
        }

        // remove file name characters that are not safe on every platform
        foreach (char invalidCharacter in Path.GetInvalidFileNameChars())
        {
            levelID = levelID.Replace(invalidCharacter, '_');
        }

        return levelID;
    }
}
