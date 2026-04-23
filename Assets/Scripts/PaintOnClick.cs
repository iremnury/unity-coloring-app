using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class PaintOnClick : MonoBehaviour, IPointerClickHandler
{
    public Color paintColor = Color.red;

    private Texture2D originalTexture;
    private Texture2D workingTexture;
    private RectTransform rectTransform;
    private Image image;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();

        Texture2D src = image.sprite.texture;

        // keep the original texture to detect border pixels later
        originalTexture = new Texture2D(src.width, src.height, TextureFormat.RGBA32, false);
        originalTexture.SetPixels(src.GetPixels());
        originalTexture.Apply();

        // make a separate editable texture for painting
        workingTexture = new Texture2D(src.width, src.height, TextureFormat.RGBA32, false);
        workingTexture.SetPixels(src.GetPixels());
        workingTexture.Apply();

        image.sprite = Sprite.Create(
            workingTexture,
            new Rect(0, 0, workingTexture.width, workingTexture.height),
            new Vector2(0.5f, 0.5f)
        );
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
        workingTexture.Apply();
    }

    void FloodFill(int startX, int startY)
    {
        int width = workingTexture.width;
        int height = workingTexture.height;

        // fill the connected area without crossing the outline
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

            if (IsOutlinePixel(x, y))
                continue;

            Color currentColor = workingTexture.GetPixel(x, y);

            if (ApproximatelySameColor(currentColor, paintColor))
                continue;

            workingTexture.SetPixel(x, y, paintColor);

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

    bool ApproximatelySameColor(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r) < 0.05f &&
               Mathf.Abs(a.g - b.g) < 0.05f &&
               Mathf.Abs(a.b - b.b) < 0.05f &&
               Mathf.Abs(a.a - b.a) < 0.05f;
    }
}
