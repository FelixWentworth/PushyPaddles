using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundButtonController : MonoBehaviour
{
    private Image _image;

    [SerializeField] private Sprite _on;
    [SerializeField] private Sprite _off;

    // Use this for initialization
    void Start ()
    {
        _image = GetComponent<Image>();
    }

    public void Toggle()
    {
        AudioListener.volume = AudioListener.volume == 0f ? 1f : 0f;
    }

	// Update is called once per frame
	void Update ()
	{
	    _image.sprite = AudioListener.volume == 1f ? _on : _off;
	}
}
