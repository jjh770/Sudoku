using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RetryConfirmPopup : MonoBehaviour
{
    [Header("�ؽ�Ʈ")]
    public TextMeshProUGUI titleText; // ���� �ؽ�Ʈ
    [Header("��ư")]
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
            titleText.text = "������Ͻðڽ��ϱ�?";

        // ��� ��ư ���̱�
        if (cancelButton != null)
            cancelButton.gameObject.SetActive(true);

        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
        }
    }
    // ���� ���� �˾�
    public void ShowGameOverPopup()
    {

        if (titleText != null)
            titleText.text = "���� ����";

        // ��� ��ư ����� (���� ������ ��� �Ұ�)
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
