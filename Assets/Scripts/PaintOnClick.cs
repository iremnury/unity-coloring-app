using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PaintOnClick : MonoBehaviour, IPointerClickHandler
{
    private const int PixelsPerFrame = 2048;
    private const float OutlineBrightnessThreshold = 0.2f;
    private const float ColorTolerance = 0.05f;

    public Color paintColor = Color.white;
    public float completionThreshold = 0.9f;

    private Texture2D originalTexture;
    private Texture2D workingTexture;
    private RectTransform rectTransform;
    private Image image;

    private int textureWidth;
    private int textureHeight;
    private int totalFillablePixels;
    private int paintedPixels;

    private Color32[] originalPixels;
    private Color32[] workingPixels;
    private bool[] backgroundPixels;
    private bool[] paintedPixelMap;
    private int[] fillVisitMarks;
    private int fillVisitToken;

    public GameObject completionPanel;
    private bool isCompleted;
    private bool isFilling;
    private Coroutine activeFillRoutine;

    public Texture2D[] levelTextures;

    public string levelID = "Level1";

    public RectTransform greatJobText;

    public bool isEraser = false;
    public Color eraserColor = Color.white;

    public GameObject eraserButton;
    public GameObject palettePanel;

    private void Start()
    {
        // cache ui references used for click conversion and texture display
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();

        levelID = "Level" + GameManager.selectedLevel;
        isCompleted = PlayerPrefs.GetInt(levelID, 0) == 1;

        LoadPersistedPaintColor();

        if (!TryInitializeTextures())
        {
            enabled = false;
            return;
        }

        image.sprite = Sprite.Create(
            workingTexture,
            new Rect(0, 0, textureWidth, textureHeight),
            new Vector2(0.5f, 0.5f)
        );

        // mark the outer empty area so it does not count as paintable space
        DetectBackgroundPixels();

        // rebuild progress counters from the loaded texture
        RebuildPaintedState();
    }

    private void OnDisable()
    {
        // keep the latest painted state before leaving the scene
        SaveCurrentProgress();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // save when the app goes into the background
            SaveCurrentProgress();
        }
    }

    private void OnApplicationQuit()
    {
        // save one last time before the app closes
        SaveCurrentProgress();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isCompleted || isFilling || workingTexture == null)
        {
            return;
        }

        Vector2 localPoint;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out localPoint))
        {
            return;
        }

        Rect rect = rectTransform.rect;

        float textureAspect = (float)textureWidth / textureHeight;
        float rectAspect = rect.width / rect.height;

        float displayedWidth;
        float displayedHeight;
        float offsetX = 0f;
        float offsetY = 0f;

        if (textureAspect > rectAspect)
        {
            displayedWidth = rect.width;
            displayedHeight = rect.width / textureAspect;
            offsetY = (rect.height - displayedHeight) * 0.5f;
        }
        else
        {
            displayedHeight = rect.height;
            displayedWidth = rect.height * textureAspect;
            offsetX = (rect.width - displayedWidth) * 0.5f;
        }

        float adjustedX = localPoint.x - rect.xMin - offsetX;
        float adjustedY = localPoint.y - rect.yMin - offsetY;

        if (adjustedX < 0 || adjustedX > displayedWidth || adjustedY < 0 || adjustedY > displayedHeight)
        {
            return;
        }

        float normalizedX = adjustedX / displayedWidth;
        float normalizedY = adjustedY / displayedHeight;

        // map the ui click point back to the source texture pixel
        int px = Mathf.RoundToInt(normalizedX * (textureWidth - 1));
        int py = Mathf.RoundToInt(normalizedY * (textureHeight - 1));

        px = Mathf.Clamp(px, 0, textureWidth - 1);
        py = Mathf.Clamp(py, 0, textureHeight - 1);

        int startIndex = GetIndex(px, py);
        if (!IsPaintableIndex(startIndex))
        {
            return;
        }

        Color32 sourceColor = workingPixels[startIndex];
        if (isEraser)
        {
            if (ApproximatelySameColor(sourceColor, originalPixels[startIndex]))
            {
                return;
            }
        }
        else if (ApproximatelySameColor(sourceColor, (Color32)paintColor))
        {
            // skip fills that would not change the tapped region
            return;
        }

        activeFillRoutine = StartCoroutine(FloodFillRoutine(px, py));
    }

    private IEnumerator FloodFillRoutine(int startX, int startY)
    {
        isFilling = true;

        // reuse the same visit array with a new token for each fill
        fillVisitToken++;
        if (fillVisitToken == int.MaxValue)
        {
            System.Array.Clear(fillVisitMarks, 0, fillVisitMarks.Length);
            fillVisitToken = 1;
        }

        Queue<int> queue = new Queue<int>();
        int startIndex = GetIndex(startX, startY);

        // remember the tapped region color so the fill stays inside it
        Color32 sourceColor = workingPixels[startIndex];
        Color32 selectedColor = (Color32)paintColor;
        bool fillUsesEraser = isEraser;

        queue.Enqueue(startIndex);

        int processedThisFrame = 0;
        bool didChangeAnyPixel = false;

        while (queue.Count > 0)
        {
            int index = queue.Dequeue();
            if (fillVisitMarks[index] == fillVisitToken)
            {
                continue;
            }

            fillVisitMarks[index] = fillVisitToken;

            if (!IsPaintableIndex(index))
            {
                continue;
            }

            if (!ApproximatelySameColor(workingPixels[index], sourceColor))
            {
                continue;
            }

            Color32 newColor = fillUsesEraser ? originalPixels[index] : selectedColor;
            if (ApproximatelySameColor(workingPixels[index], newColor))
            {
                continue;
            }

            bool wasPainted = paintedPixelMap[index];
            workingPixels[index] = newColor;

            bool isPaintedNow = !ApproximatelySameColor(workingPixels[index], originalPixels[index]);
            paintedPixelMap[index] = isPaintedNow;

            if (wasPainted != isPaintedNow)
            {
                paintedPixels += isPaintedNow ? 1 : -1;
            }

            didChangeAnyPixel = true;

            int x = index % textureWidth;
            int y = index / textureWidth;

            if (x > 0)
            {
                queue.Enqueue(index - 1);
            }

            if (x < textureWidth - 1)
            {
                queue.Enqueue(index + 1);
            }

            if (y > 0)
            {
                queue.Enqueue(index - textureWidth);
            }

            if (y < textureHeight - 1)
            {
                queue.Enqueue(index + textureWidth);
            }

            processedThisFrame++;
            if (processedThisFrame >= PixelsPerFrame)
            {
                processedThisFrame = 0;

                // pause here so large fills do not block the whole frame
                yield return null;
            }
        }

        if (didChangeAnyPixel)
        {
            // push the updated pixel buffer back to the texture once per fill
            ApplyWorkingPixels();
            CheckCompletion();
        }

        activeFillRoutine = null;
        isFilling = false;
    }

    private void LoadPersistedPaintColor()
    {
        if (!PlayerPrefs.HasKey("R"))
        {
            return;
        }

        // restore the last selected palette color from prefs
        float r = PlayerPrefs.GetFloat("R");
        float g = PlayerPrefs.GetFloat("G");
        float b = PlayerPrefs.GetFloat("B");

        paintColor = new Color(r, g, b);
    }

    private bool TryInitializeTextures()
    {
        if (levelTextures == null || levelTextures.Length == 0)
        {
            Debug.LogError("No level textures assigned.");
            return false;
        }

        int levelIndex = GameManager.selectedLevel - 1;
        if (levelIndex < 0 || levelIndex >= levelTextures.Length || levelTextures[levelIndex] == null)
        {
            Debug.LogError("Selected level texture is missing.");
            return false;
        }

        Texture2D sourceTexture = levelTextures[levelIndex];
        textureWidth = sourceTexture.width;
        textureHeight = sourceTexture.height;

        // keep the source texture untouched for outline checks and erasing
        originalTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        originalTexture.SetPixels32(sourceTexture.GetPixels32());
        originalTexture.Apply(false);

        workingTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);

        // load the last saved canvas for this level if it exists
        if (LevelProgressStorage.TryLoad(levelID, textureWidth, textureHeight, out byte[] savedPixels))
        {
            workingTexture.LoadRawTextureData(savedPixels);
            workingTexture.Apply(false);
        }
        else
        {
            workingTexture.SetPixels32(sourceTexture.GetPixels32());
            workingTexture.Apply(false);
        }

        originalPixels = originalTexture.GetPixels32();
        workingPixels = workingTexture.GetPixels32();
        backgroundPixels = new bool[workingPixels.Length];
        paintedPixelMap = new bool[workingPixels.Length];
        fillVisitMarks = new int[workingPixels.Length];

        return true;
    }

    private void CheckCompletion()
    {
        if (totalFillablePixels <= 0)
        {
            return;
        }

        float percent = (float)paintedPixels / totalFillablePixels;
        Debug.Log("Completion: " + (percent * 100f) + "%");

        if (isCompleted || percent < completionThreshold)
        {
            return;
        }

        // stop normal interaction after the level is done
        isCompleted = true;

        if (completionPanel != null)
        {
            completionPanel.SetActive(true);
            completionPanel.transform.localScale = Vector3.zero;
            completionPanel.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
        }

        if (eraserButton != null)
        {
            eraserButton.SetActive(false);
        }

        if (palettePanel != null)
        {
            palettePanel.SetActive(false);
        }

        if (greatJobText != null)
        {
            greatJobText.localScale = Vector3.zero;
            greatJobText.DOScale(1.2f, 0.3f)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    greatJobText.DOScale(1f, 0.2f);
                });
        }

        PlayerPrefs.SetInt(levelID, 1);
        PlayerPrefs.Save();
    }

    private void ApplyWorkingPixels()
    {
        // copy the edited color array back into the visible texture
        workingTexture.SetPixels32(workingPixels);
        workingTexture.Apply(false);
    }

    public void ResetLoadedProgress()
    {
        if (originalPixels == null || workingPixels == null || workingTexture == null)
        {
            return;
        }

        if (activeFillRoutine != null)
        {
            StopCoroutine(activeFillRoutine);
            activeFillRoutine = null;
        }

        isFilling = false;
        isCompleted = false;

        // restore the current canvas to the untouched source art
        System.Array.Copy(originalPixels, workingPixels, originalPixels.Length);
        ApplyWorkingPixels();
        RebuildPaintedState();

        if (completionPanel != null)
        {
            completionPanel.SetActive(false);
        }

        if (eraserButton != null)
        {
            eraserButton.SetActive(true);
        }

        if (palettePanel != null)
        {
            palettePanel.SetActive(true);
        }
    }

    private void SaveCurrentProgress()
    {
        if (workingTexture == null || workingPixels == null)
        {
            return;
        }

        // write the current canvas into a small raw file in persistent data
        ApplyWorkingPixels();
        byte[] rawPixels = workingTexture.GetRawTextureData<byte>().ToArray();
        LevelProgressStorage.Save(levelID, rawPixels);
    }

    private void RebuildPaintedState()
    {
        // count how much of the valid paint area is already filled
        totalFillablePixels = 0;
        paintedPixels = 0;

        for (int index = 0; index < workingPixels.Length; index++)
        {
            if (!IsPaintableIndex(index))
            {
                paintedPixelMap[index] = false;
                continue;
            }

            totalFillablePixels++;

            bool isPainted = !ApproximatelySameColor(workingPixels[index], originalPixels[index]);
            paintedPixelMap[index] = isPainted;

            if (isPainted)
            {
                paintedPixels++;
            }
        }

        Debug.Log("Total paintable pixel: " + totalFillablePixels);
    }

    private void DetectBackgroundPixels()
    {
        // flood from the edges to find pixels outside the drawing borders
        Queue<int> queue = new Queue<int>();

        for (int x = 0; x < textureWidth; x++)
        {
            queue.Enqueue(GetIndex(x, 0));
            queue.Enqueue(GetIndex(x, textureHeight - 1));
        }

        for (int y = 0; y < textureHeight; y++)
        {
            queue.Enqueue(GetIndex(0, y));
            queue.Enqueue(GetIndex(textureWidth - 1, y));
        }

        while (queue.Count > 0)
        {
            int index = queue.Dequeue();
            if (backgroundPixels[index] || IsOutlinePixel(index))
            {
                continue;
            }

            backgroundPixels[index] = true;

            int x = index % textureWidth;
            int y = index / textureWidth;

            if (x > 0)
            {
                queue.Enqueue(index - 1);
            }

            if (x < textureWidth - 1)
            {
                queue.Enqueue(index + 1);
            }

            if (y > 0)
            {
                queue.Enqueue(index - textureWidth);
            }

            if (y < textureHeight - 1)
            {
                queue.Enqueue(index + textureWidth);
            }
        }
    }

    private bool IsOutlinePixel(int index)
    {
        Color32 color = originalPixels[index];
        float brightness = (color.r + color.g + color.b) / (3f * 255f);

        // dark pixels from the source image are treated as borders
        return brightness < OutlineBrightnessThreshold;
    }

    private bool IsPaintableIndex(int index)
    {
        // ignore border pixels and the outer background area
        if (IsOutlinePixel(index))
        {
            return false;
        }

        if (backgroundPixels[index])
        {
            return false;
        }

        return true;
    }

    private bool ApproximatelySameColor(Color32 a, Color32 b)
    {
        // allow a small tolerance so tiny color differences do not break fills
        float tolerance = ColorTolerance * 255f;
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance &&
               Mathf.Abs(a.a - b.a) < tolerance;
    }

    private int GetIndex(int x, int y)
    {
        return y * textureWidth + x;
    }
}
