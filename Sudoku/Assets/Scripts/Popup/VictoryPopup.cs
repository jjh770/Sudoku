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
            titleText.text = "클리어 축하합니다!";
        }

        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(playTime / 60f);
            int seconds = Mathf.FloorToInt(playTime % 60f);
            timeText.text = $"시간: {minutes:00}:{seconds:00}";
        }

        if (difficultyText != null)
        {
            string difficultyName = GetDifficultyName(difficulty);
            difficultyText.text = $"난이도: {difficultyName}";
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
            case 30: return "쉬움";
            case 40: return "보통";
            case 50: return "어려움";
            case 55: return "전문가";
            case 60: return "지옥";
            default: return "보통";
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
