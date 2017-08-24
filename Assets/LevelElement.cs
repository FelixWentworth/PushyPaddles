using UnityEngine;
using UnityEngine.UI;

public class LevelElement : MonoBehaviour
{
    public Button Button;

    public void Setup(string number, string description)
    {
        transform.Find("LevelNumber").GetComponent<Text>().text = number;
        transform.Find("LevelDescription").GetComponent<Text>().text = description;
    }
}
