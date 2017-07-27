using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class AutoFont : MonoBehaviour
{

    public FontManager.FontType Style;

	// Use this for initialization
	void Start ()
	{
	    var text = this.GetComponent<Text>();
	    if (text == null)
	    {
            // Try and see if this is a text mesh object
	        this.GetComponent<TextMesh>();
	    }
	    if (text != null)
	    {
	        text.font = GameObject.Find("FontManager").GetComponent<FontManager>().GetFont(Style);
        }
    }
}
