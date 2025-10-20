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

            // Ÿ�̸� �Ͻ�����
            GameManager.Instance.PauseTimer();

            // ���� �÷��� �ð��� ���̵� ǥ��
            UpdateInfo();
        }
    }

    void UpdateInfo()
    {
        // �÷��� �ð� ǥ��
        if (timeText != null)
        {
            float playTime = GameManager.Instance.GetPlayTime();
            int minutes = Mathf.FloorToInt(playTime / 60f);
            int seconds = Mathf.FloorToInt(playTime % 60f);
            timeText.text = $"{minutes:00}:{seconds:00}";
        }

        // ���̵� ǥ��
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
            case 35: return "�� ��";
            case 42: return "�� ��";
            case 50: return "�����";
            case 55: return "������";
            case 60: return "�� ��";
            default: return "����";
        }
    }

    void Resume()
    {
        SoundManager.Instance.PlayButtonClick();

        ClosePopup();
        GameManager.Instance.ShowGameUI(); // UI �ٽ� ���̱�

        // Ÿ�̸� �簳
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
            Time.timeScale = 1f; // ���� �簳
        }
    }
}
