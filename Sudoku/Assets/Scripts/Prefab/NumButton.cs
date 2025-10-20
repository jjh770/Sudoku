using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NumButton : MonoBehaviour
{
    public int number;
    public TextMeshProUGUI buttonText;
    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void Initialize(int num)
    {
        number = num;
        buttonText.text = num.ToString();

        button.onClick.AddListener(OnButtonClick);
    }

    void OnButtonClick()
    {
        GameManager.Instance.InputNumber(number);
    }
}
