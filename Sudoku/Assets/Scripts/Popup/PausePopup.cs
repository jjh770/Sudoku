using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PausePopup : MonoBehaviour
{
    public GameObject popupPanel;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI difficultyText;
    public Button resumeButton;
    public Button restartButton;
    public Button changeDifficultyButton;

    void Start()
    {
        SetupButtons();
    }

    void SetupButtons()
    {
        if (resumeButton != null)
            resumeButton.onClick.AddListener(Resume);

        if (restartButton != null)
            restartButton.onClick.AddListener(Restart);

        if (changeDifficultyButton != null)
            changeDifficultyButton.onClick.AddListener(ChangeDifficulty);
    }

    public void ShowPopup()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(true);

            // 타이머 일시정지
            GameManager.Instance.PauseTimer();

            // 현재 플레이 시간과 난이도 표시
            UpdateInfo();
        }
    }

    void UpdateInfo()
    {
        // 플레이 시간 표시
        if (timeText != null)
        {
            float playTime = GameManager.Instance.GetPlayTime();
            int minutes = Mathf.FloorToInt(playTime / 60f);
            int seconds = Mathf.FloorToInt(playTime % 60f);
            timeText.text = $"{minutes:00}:{seconds:00}";
        }

        // 난이도 표시
        if (difficultyText != null)
        {
            int difficulty = GameManager.Instance.difficulty;
            string difficultyName = GetDifficultyName(difficulty);
            difficultyText.text = $"{difficultyName}";
        }
    }

    string GetDifficultyName(int difficulty)
    {
        switch (difficulty)
        {
            case 35: return "쉬 움";
            case 42: return "보 통";
            case 50: return "어려움";
            case 55: return "전문가";
            case 60: return "지 옥";
            default: return "보통";
        }
    }

    void Resume()
    {
        SoundManager.Instance.PlayButtonClick();

        ClosePopup();
        GameManager.Instance.ShowGameUI(); // UI 다시 보이기

        // 타이머 재개
        GameManager.Instance.ResumeTimer();
    }

    void Restart()
    {
        SoundManager.Instance.PlayButtonClick();
        ClosePopup();
        GameManager.Instance.ShowRestartConfirmPopup();

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
            Time.timeScale = 1f; // 게임 재개
        }
    }
}
