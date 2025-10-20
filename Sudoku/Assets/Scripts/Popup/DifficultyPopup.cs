using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DifficultyPopup : MonoBehaviour
{
    public GameObject popupPanel;
    public Button easyButton;
    public Button mediumButton;
    public Button hardButton;
    public Button expertButton;
    public Button hellButton;

    void Start()
    {
        // �˾� ǥ��
        ShowPopup();

        // ��ư �̺�Ʈ ����
        if (easyButton != null)
            easyButton.onClick.AddListener(() => SelectDifficulty(35));

        if (mediumButton != null)
            mediumButton.onClick.AddListener(() => SelectDifficulty(42));

        if (hardButton != null)
            hardButton.onClick.AddListener(() => SelectDifficulty(50));

        if (expertButton != null)
            expertButton.onClick.AddListener(() => SelectDifficulty(55));

        if (hellButton != null)
            hellButton.onClick.AddListener(() => SelectDifficulty(60));
    }

    public void ShowPopup()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
        }
    }

    void SelectDifficulty(int difficulty)
    {
        SoundManager.Instance.PlayButtonClick();

        // GameManager�� ���̵� ����
        GameManager.Instance.difficulty = difficulty;

        // �˾� �ݰ� ���� ����
        ClosePopup();

        // ���� ����
        GameManager.Instance.GenerateNewPuzzle();
    }

    void ClosePopup()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
    }
}
