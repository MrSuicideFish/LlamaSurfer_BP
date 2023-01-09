using TMPro;
using UnityEngine;

public class UILevelButton : MonoBehaviour
{
    public void SetLevel(int level, bool isActive)
    {
        if (transform.childCount > 1)
        {
            transform.GetChild(0).gameObject.SetActive(isActive);
            TextMeshProUGUI txt = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            if (txt != null)
            {
                txt.text = level.ToString();
            }
        }
    }
}
