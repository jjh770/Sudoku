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
        // 팝업 표시
        ShowPopup();

        // 버튼 이벤트 설정
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

        // GameManager에 난이도 설정
        GameManager.Instance.difficulty = difficulty;

        // 팝업 닫고 게임 시작
        ClosePopup();

        // 게임 생성
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
