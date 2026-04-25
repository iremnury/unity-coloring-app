using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class ColorPicker : MonoBehaviour
{
    // reference to the script that applies the selected color
    public PaintOnClick painter;

    // color assigned to this picker
    public Color selectedColor;

    public ColorPaletteData paletteData;
    public int colorIndex;

    public GameObject selectionRing;


    private static List<ColorPicker> allPickers = new List<ColorPicker>();

    void Awake()
    {
        // keep a shared list so only one swatch looks active at a time
        allPickers.Add(this);

        if (paletteData != null && paletteData.colors.Length > colorIndex)
        {
            selectedColor = paletteData.colors[colorIndex];
        }
    }

    void OnDestroy()
    {
        allPickers.Remove(this);
    }

    public void SelectColor()
    {
        // switch the painter back from eraser mode to normal color mode
        painter.paintColor = selectedColor;
        painter.isEraser = false;

        if (EraserButton.activeEraser != null)
        {
            if (EraserButton.activeEraser.selectionRing != null)
                EraserButton.activeEraser.selectionRing.SetActive(false);

            EraserButton.activeEraser.transform.DOKill();
            EraserButton.activeEraser.transform.DOScale(1f, 0.2f).SetEase(Ease.OutQuad);
        }

        PlayerPrefs.SetFloat("R", selectedColor.r);
        PlayerPrefs.SetFloat("G", selectedColor.g);
        PlayerPrefs.SetFloat("B", selectedColor.b);
        PlayerPrefs.Save();

        Debug.Log("Paint color: " + selectedColor);

        // reset all other swatches before showing the new active one
        foreach (var picker in allPickers)
        {
            if (picker == null)
                continue;

            if (picker.selectionRing != null)
                picker.selectionRing.SetActive(false);

            picker.transform.DOKill();
            picker.transform.DOScale(1f, 0.2f).SetEase(Ease.OutQuad);
        }

        if (selectionRing != null)
            selectionRing.SetActive(true);

        // add a small bounce to the selected swatch
        transform.DOKill();
        transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack);

        Debug.Log("selected color: " + selectedColor);
    }
}
