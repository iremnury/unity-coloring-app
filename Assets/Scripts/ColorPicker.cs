using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class ColorPicker : MonoBehaviour
{
    // reference to the script that applies the selected color
    public PaintOnClick painter;

    // color assigned to this picker
    public Color selectedColor;

    public GameObject selectionRing;


    private static List<ColorPicker> allPickers = new List<ColorPicker>();

    void Awake()
    {
        allPickers.Add(this);
    }

    public void SelectColor()
    {
        painter.paintColor = selectedColor;

        PlayerPrefs.SetFloat("R", selectedColor.r);
        PlayerPrefs.SetFloat("G", selectedColor.g);
        PlayerPrefs.SetFloat("B", selectedColor.b);
        PlayerPrefs.Save();

    
        foreach (var picker in allPickers)
        {
            if (picker.selectionRing != null)
                picker.selectionRing.SetActive(false);

            picker.transform.DOKill();
            picker.transform.DOScale(1f, 0.2f).SetEase(Ease.OutQuad);
        }

        
        if (selectionRing != null)
            selectionRing.SetActive(true);

        // animation 
        transform.DOKill();
        transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack);

        Debug.Log("selected color: " + selectedColor);
    }
}