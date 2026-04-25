using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

public class PaintOnClick : MonoBehaviour, IPointerClickHandler
{
    public Color paintColor = Color.white;
    public float completionThreshold = 0.9f;

    private Texture2D originalTexture;
    private Texture2D workingTexture;
    private RectTransform rectTransform;
    private Image image;

    private int totalFillablePixels;
    private int paintedPixels;

    private bool[,] backgroundPixels;
    private bool[,] paintedPixelMap;

    public GameObject completionPanel;
    private bool isCompleted = false;

    public Texture2D[] levelTextures;

    public string levelID = "Level1";

    public RectTransform greatJobText;

    public bool isEraser = false;
    public Color eraserColor = Color.white;


    public GameObject eraserButton;
    public GameObject palettePanel;




    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();

        levelID = "Level" + GameManager.selectedLevel;

        if (PlayerPrefs.HasKey("R"))
        {
            float r = PlayerPrefs.GetFloat("R");
            float g = PlayerPrefs.GetFloat("G");
            float b = PlayerPrefs.GetFloat("B");

            paintColor = new Color(r, g, b);
        }

        Texture2D src = levelTextures[GameManager.selectedLevel - 1];

        // keep the original texture to detect border pixels later
        originalTexture = new Texture2D(src.width, src.height, TextureFormat.RGBA32, false);
        originalTexture.SetPixels(src.GetPixels());
        originalTexture.Apply();

        // make a separate editable texture for painting
        workingTexture = new Texture2D(src.width, src.height, TextureFormat.RGBA32, false);
        workingTexture.SetPixels(src.GetPixels());
        workingTexture.Apply();

        paintedPixelMap = new bool[workingTexture.width, workingTexture.height];

        image.sprite = Sprite.Create(
            workingTexture,
            new Rect(0, 0, workingTexture.width, workingTexture.height),
            new Vector2(0.5f, 0.5f)
        );
        DetectBackgroundPixels();
        CountFillablePixels();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
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

        float textureWidth = workingTexture.width;
        float textureHeight = workingTexture.height;

        float rectWidth = rect.width;
        float rectHeight = rect.height;

        float textureAspect = textureWidth / textureHeight;
        float rectAspect = rectWidth / rectHeight;

        float displayedWidth;
        float displayedHeight;
        float offsetX = 0f;
        float offsetY = 0f;

        if (textureAspect > rectAspect)
        {
            displayedWidth = rectWidth;
            displayedHeight = rectWidth / textureAspect;
            offsetY = (rectHeight - displayedHeight) * 0.5f;
        }
        else
        {
            displayedHeight = rectHeight;
            displayedWidth = rectHeight * textureAspect;
            offsetX = (rectWidth - displayedWidth) * 0.5f;
        }

        // convert the ui click position into texture coordinates
        float adjustedX = localPoint.x - rect.xMin - offsetX;
        float adjustedY = localPoint.y - rect.yMin - offsetY;

        if (adjustedX < 0 || adjustedX > displayedWidth || adjustedY < 0 || adjustedY > displayedHeight)
            return;

        float normalizedX = adjustedX / displayedWidth;
        float normalizedY = adjustedY / displayedHeight;

        int px = Mathf.RoundToInt(normalizedX * (workingTexture.width - 1));
        int py = Mathf.RoundToInt(normalizedY * (workingTexture.height - 1));

        px = Mathf.Clamp(px, 0, workingTexture.width - 1);
        py = Mathf.Clamp(py, 0, workingTexture.height - 1);

        // ignore clicks on the dark outline
        if (IsOutlinePixel(px, py))
            return;

        FloodFill(px, py);

        float percent = (float)paintedPixels / totalFillablePixels;
        Debug.Log("Completion: " + (percent * 100f) + "%");
        if (!isCompleted && percent >= completionThreshold)
        {
            isCompleted = true;


            completionPanel.SetActive(true);

            if (eraserButton != null){
                eraserButton.SetActive(false);
                }
                

            if (palettePanel != null){ 
                palettePanel.SetActive(false);
                }
               

            // set to zero at start
            completionPanel.transform.localScale = Vector3.zero;

            // animation
            completionPanel.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
            
            greatJobText.localScale = Vector3.zero;

            greatJobText.DOScale(1.2f, 0.3f)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    greatJobText.DOScale(1f, 0.2f);
                });

            PlayerPrefs.SetInt(levelID, 1);
            PlayerPrefs.Save();
        }
        workingTexture.Apply();
    }

    void FloodFill(int startX, int startY)
    {
        int width = workingTexture.width;
        int height = workingTexture.height;

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        bool[,] visited = new bool[width, height];

        queue.Enqueue(new Vector2Int(startX, startY));

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            int x = current.x;
            int y = current.y;

            if (x < 0 || x >= width || y < 0 || y >= height)
                continue;

            if (visited[x, y])
                continue;

            visited[x, y] = true;

            if (!IsPaintablePixel(x, y))
                continue;

            Color currentColor = workingTexture.GetPixel(x, y);
            Color targetPaintColor = isEraser ? eraserColor : paintColor;

            if (ApproximatelySameColor(currentColor, targetPaintColor))
                continue;

            workingTexture.SetPixel(x, y, targetPaintColor);

            if (isEraser)
            {
                if (paintedPixelMap[x, y])
                {
                    paintedPixelMap[x, y] = false;
                    paintedPixels--;
                }
            }
            else
            {
                if (!paintedPixelMap[x, y])
                {
                    paintedPixelMap[x, y] = true;
                    paintedPixels++;
                }
            }

            queue.Enqueue(new Vector2Int(x + 1, y));
            queue.Enqueue(new Vector2Int(x - 1, y));
            queue.Enqueue(new Vector2Int(x, y + 1));
            queue.Enqueue(new Vector2Int(x, y - 1));
        }
    }


    bool IsOutlinePixel(int x, int y)
    {
        Color c = originalTexture.GetPixel(x, y);

        // treat very dark pixels as the border
        float brightness = (c.r + c.g + c.b) / 3f;

        return brightness < 0.2f;
    }

    bool IsPaintablePixel(int x, int y)
    {
        if (IsOutlinePixel(x, y))
            return false;

        if (backgroundPixels != null && backgroundPixels[x, y])
            return false;

        return true;
    }
    bool ApproximatelySameColor(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r) < 0.05f &&
               Mathf.Abs(a.g - b.g) < 0.05f &&
               Mathf.Abs(a.b - b.b) < 0.05f &&
               Mathf.Abs(a.a - b.a) < 0.05f;
    }

    void CountFillablePixels()
    {
        totalFillablePixels = 0;

        for (int x = 0; x < originalTexture.width; x++)
        {
            for (int y = 0; y < originalTexture.height; y++)
            {
                if (IsPaintablePixel(x, y))
                {
                    totalFillablePixels++;
                }
            }
        }

        Debug.Log("Total paintable pixel: " + totalFillablePixels);
    }

    void DetectBackgroundPixels()
    {
        int width = originalTexture.width;
        int height = originalTexture.height;

        backgroundPixels = new bool[width, height];
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        for (int x = 0; x < width; x++)
        {
            queue.Enqueue(new Vector2Int(x, 0));
            queue.Enqueue(new Vector2Int(x, height - 1));
        }

        for (int y = 0; y < height; y++)
        {
            queue.Enqueue(new Vector2Int(0, y));
            queue.Enqueue(new Vector2Int(width - 1, y));
        }

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            int x = current.x;
            int y = current.y;

            if (x < 0 || x >= width || y < 0 || y >= height)
                continue;

            if (backgroundPixels[x, y])
                continue;

            if (IsOutlinePixel(x, y))
                continue;

            backgroundPixels[x, y] = true;

            queue.Enqueue(new Vector2Int(x + 1, y));
            queue.Enqueue(new Vector2Int(x - 1, y));
            queue.Enqueue(new Vector2Int(x, y + 1));
            queue.Enqueue(new Vector2Int(x, y - 1));
        }
    }
}
