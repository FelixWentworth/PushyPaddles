using UnityEngine;
using UnityEngine.UI;

public class LevelElement : MonoBehaviour
{
    public Button Button;
    public GameObject StarRating;
    public GameObject Star0;
    public GameObject Star1;
    public GameObject Star2;
    public GameObject Star3;

    public void Setup(string number, string description, string year, string lesson)
    {
        transform.Find("LevelNumber").GetComponent<Text>().text = number;
        transform.Find("LevelDescription").GetComponent<Text>().text = description;
        SetStars(year, lesson);
    }

    private void SetStars(string year, string lesson)
    {
        StarRating.SetActive(SP_Manager.Instance.IsSinglePlayer());
        if (SP_Manager.Instance.IsSinglePlayer())
        {
            Star0.SetActive(false);
            Star1.SetActive(false);
            Star2.SetActive(false);
            Star3.SetActive(false);
            // GetRating
            var rating = SP_Manager.Instance.Get<SP_Levels>().GetRating(year, lesson);
            switch (rating)
            {
                case 1:
                    Star1.SetActive(true);
                    break;
                case 2:
                    Star2.SetActive(true);
                    break;
                case 3:
                    Star3.SetActive(true);
                    break;
                default:
                    Star0.SetActive(true);
                    break;
            }
        }
    }
}
