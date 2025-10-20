using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RetryConfirmPopup : MonoBehaviour
{
    [Header("텍스트")]
    public TextMeshProUGUI titleText; // 제목 텍스트
    [Header("버튼")]
    public GameObject popupPanel;
    public Button restartCurrentButton;
    public Button newGameButton;
    public Button cancelButton;

    void Start()
    {
        SetupButtons();
    }

    void SetupButtons()
    {
        if (restartCurrentButton != null)
            restartCurrentButton.onClick.AddListener(RestartCurrent);

        if (newGameButton != null)
            newGameButton.onClick.AddListener(NewGame);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(Cancel);
    }

    public void ShowPopup()
    {

        if (titleText != null)
            titleText.text = "재시작하시겠습니까?";

        // 취소 버튼 보이기
        if (cancelButton != null)
            cancelButton.gameObject.SetActive(true);

        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
        }
    }
    // 게임 오버 팝업
    public void ShowGameOverPopup()
    {

        if (titleText != null)
            titleText.text = "게임 오버";

        // 취소 버튼 숨기기 (게임 오버시 취소 불가)
        if (cancelButton != null)
            cancelButton.gameObject.SetActive(false);

        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
        }
    }
    void RestartCurrent()
    {
        SoundManager.Instance.PlayButtonClick();
        ClosePopup();
        GameManager.Instance.RestartCurrentPuzzle();
    }

    void NewGame()
    {
        SoundManager.Instance.PlayButtonClick();
        ClosePopup();
        GameManager.Instance.ShowDifficultyPopupForChange();
    }

    void Cancel()
    {
        SoundManager.Instance.PlayButtonClick();
        ClosePopup();
        GameManager.Instance.ShowPausePopup();
    }


    public void ClosePopup()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
    }
}
