using UnityEngine;
using UnityEngine.UI;

using TMPro;
using System;

public class ColorPreview : MonoBehaviour
{
    public Graphic previewGraphic;
    public Color currentColor;
    public Color hexColor;

    public ColorPicker colorPicker;
    public Slider alphaPicker;

    public TMP_InputField hex;

    public event Action<Color> onColorUpdated;

    private void Start()
    {
        var color = colorPicker.color;
        color.a = alphaPicker.normalizedValue;
        previewGraphic.color = color;

        colorPicker.onColorChanged += OnColorChanged;
        alphaPicker.onValueChanged.AddListener(OnAlphaChanged);
    }

    public void ChangeColor(Color c)
    {
        currentColor = c;
        alphaPicker.value = c.a;
        hex.text = Color2Hex(c);
        c.a = 1;
        colorPicker.color = c;
    }

    public void OnColorChanged(Color c)
    {
        // use previous alpha as to not overwrite it; c will always have alpha 1 here.
        c.a = currentColor.a;
        currentColor = c;
        previewGraphic.color = c;
        hex.text = Color2Hex(c);

        onColorUpdated.Invoke(currentColor);
    }

    private void OnAlphaChanged(float _)
    {
        currentColor.a = alphaPicker.normalizedValue;
        previewGraphic.color = currentColor;
        hex.text = Color2Hex(currentColor);

        onColorUpdated.Invoke(currentColor);
    }

    public void SetColorFromHex(string hex)
    {
        hexColor = Hex2RGB(hex);
        colorPicker.color = hexColor;
        alphaPicker.value = hexColor.a;
    }

    private void OnDestroy()
    {
        if (colorPicker != null)
            colorPicker.onColorChanged -= OnColorChanged;
    }

    public void SetColorFromTMP()
    {
        SetColorFromHex(hex.text);
    }

    static string Color2Hex(Color color)
    {
        Color32 col = (Color32)color;
        string hex = col.r.ToString("X2") + col.g.ToString("X2") + col.b.ToString("X2") + col.a.ToString("X2");
        return hex;
    }

    /// <summary>
    /// Converts a Hexadecimal Color to an RGB Color.
    /// </summary>
    static Color Hex2RGB(string hex)
    {
        if (hex is null or "") return Color.black;
        try
        {
            hex = hex.Replace("0x", "");//in case the string is formatted 0xFFFFFF
            hex = hex.Replace("#", "");//in case the string is formatted #FFFFFF
            byte a = 255;//assume fully visible unless specified in hex
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            //Only use alpha if the string has enough characters
            if (hex.Length >= 8)
            {
                a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
            }
            return new Color32(r, g, b, a);
        }
        catch
        {
            return Color.black;
        }
    }
}