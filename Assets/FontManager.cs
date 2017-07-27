using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FontManager : MonoBehaviour
{
    public enum FontType
    {
        Title,
        Button,
        Body
    };

    public Font Titles;
    public Font Buttons;
    public Font Body;

    public Font GetFont(FontType type)
    {
        switch (type)
        {
            case FontType.Title:
                return Titles;
            case FontType.Button:
                return Buttons;
            default:
                return Body;
        }
    }
}
