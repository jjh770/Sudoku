using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VictoryPopup : MonoBehaviour
{
    public GameObject popupPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI difficultyText;

    public Button changeDifficultyButton;
    public Button newGameButton;

    void Start()
    {
        SetupButtons();
    }

    void SetupButtons()
    {
        if (newGameButton != null)
            newGameButton.onClick.AddListener(NewGame);

        if (changeDifficultyButton != null)
            changeDifficultyButton.onClick.AddListener(ChangeDifficulty);
    }

    public void ShowPopup(float playTime, int difficulty, int mistakes, int hintsUsed)
    {
        if (titleText != null)
        {
            titleText.text = "Ŭ���� �����մϴ�!";
        }

        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(playTime / 60f);
            int seconds = Mathf.FloorToInt(playTime % 60f);
            timeText.text = $"�ð�: {minutes:00}:{seconds:00}";
        }

        if (difficultyText != null)
        {
            string difficultyName = GetDifficultyName(difficulty);
            difficultyText.text = $"���̵�: {difficultyName}";
        }

        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
        }
    }

    string GetDifficultyName(int difficulty)
    {
        switch (difficulty)
        {
            case 30: return "����";
            case 40: return "����";
            case 50: return "�����";
            case 55: return "������";
            case 60: return "����";
            default: return "����";
        }
    }

    void NewGame()
    {
        SoundManager.Instance.PlayButtonClick();
        ClosePopup();
        GameManager.Instance.ShowGameUI();
        GameManager.Instance.GenerateNewPuzzle();
    }

    void ChangeDifficulty()
    {
        SoundManager.Instance.PlayButtonClick();
        ClosePopup();
        GameManager.Instance.ShowDifficultyPopupForChange();
    }

    public void ClosePopup()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
    }
}
