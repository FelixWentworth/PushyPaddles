using UnityEngine;
using UnityEngine.UI;

public class Reward : MonoBehaviour
{
    public bool Highlighted;

    [SerializeField] private GameObject _highlightObject;
    [SerializeField] private Text _name;

    public void SetHighlight(bool highlight)
    {
        _highlightObject.SetActive(highlight);
        Highlighted = highlight;
    }

    public void SetName(string username)
    {
        _name.text = username;
    }
}
