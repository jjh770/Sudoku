using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

public class SudokuCell : MonoBehaviour, IPointerClickHandler
{
    public int row;
    public int col;
    public int number;
    public bool isGiven;
    public bool isSelected;
    public bool isCorrect = true;
    public bool hasConflict = false;

    public TextMeshProUGUI numberText;
    public Image cellImage;

    public bool isHighlighted = false;
    public bool isRowColHighlighted = false;

    // �޸� ��� - 9���� ���� �ؽ�Ʈ
    public TMP_FontAsset notesFont;
    private TextMeshProUGUI[] noteTexts = new TextMeshProUGUI[9];
    public HashSet<int> notes = new HashSet<int>();

    public bool isSolvableHighlighted = false;

    void Awake()
    {
        numberText = GetComponentInChildren<TextMeshProUGUI>();
        cellImage = GetComponent<Image>();

        // �޸�� �ؽ�Ʈ 9�� ����
        CreateNotesTexts();
    }

    void CreateNotesTexts()
    {
        for (int i = 0; i < 9; i++)
        {
            GameObject noteObj = new GameObject($"Note_{i + 1}");
            noteObj.transform.SetParent(transform);

            RectTransform noteRect = noteObj.AddComponent<RectTransform>();

            // 3x3 �׸��� ��ġ ���
            int gridRow = i / 3;
            int gridCol = i % 3;

            // ���� 3����� ��ġ�� ��ġ
            float cellWidth = 90f;  // Cell ũ��
            float cellHeight = 90f;
            float noteWidth = cellWidth / 3f;
            float noteHeight = cellHeight / 3f;

            noteRect.anchorMin = new Vector2(0, 1); // ���� ��� ����
            noteRect.anchorMax = new Vector2(0, 1);
            noteRect.pivot = new Vector2(0.5f, 0.5f);

            // ��ġ ����
            float xPos = (gridCol * noteWidth) + (noteWidth / 2f);
            float yPos = -(gridRow * noteHeight) - (noteHeight / 2f);

            noteRect.anchoredPosition = new Vector2(xPos, yPos);
            noteRect.sizeDelta = new Vector2(noteWidth, noteHeight);

            TextMeshProUGUI noteText = noteObj.AddComponent<TextMeshProUGUI>();
            noteText.fontSize = 18;
            noteText.alignment = TextAlignmentOptions.Center;
            noteText.color = new Color(0.5f, 0.5f, 0.5f);
            noteText.text = "";

            // ��Ʈ ����
            if (notesFont != null)
            {
                noteText.font = notesFont;
            }

            noteTexts[i] = noteText;
        }
    }

    public void InitCell(int r, int c)
    {
        row = r;
        col = c;
        number = 0;
        isGiven = false;
        isSelected = false;
        notes.Clear();

    }

    public void SetNumber(int num, bool given = false)
    {
        number = num;
        isGiven = given;

        if (num > 0)
        {

            numberText.text = num.ToString();
            numberText.gameObject.SetActive(true);

            // �޸� �����
            for (int i = 0; i < 9; i++)
            {
                noteTexts[i].gameObject.SetActive(false);
            }
            notes.Clear();
        }
        else
        {
            numberText.text = "";
            numberText.gameObject.SetActive(false);
            isCorrect = true;
            hasConflict = false;
            // �� ĭ�� ���� �޸� �ʱ�ȭ �߰�
            notes.Clear();
            for (int i = 0; i < 9; i++)
            {
                noteTexts[i].gameObject.SetActive(false);
            }
            // �޸� ������ ǥ��
            UpdateNotesDisplay();
        }

        UpdateTextColor();
    }

    public void ToggleNote(int noteNumber)
    {
        if (number != 0 || isGiven)
        {
            return;
        }

        if (notes.Contains(noteNumber))
        {
            notes.Remove(noteNumber);
        }
        else
        {
            notes.Add(noteNumber);
        }

        UpdateNotesDisplay();
    }

    public void UpdateNotesDisplay()
    {
        if (number != 0)
        {
            // ���ڰ� ������ �޸� ����
            for (int i = 0; i < noteTexts.Length; i++)
            {
                noteTexts[i].gameObject.SetActive(false);
            }
            return;
        }

        // �޸� ǥ��
        for (int i = 0; i < noteTexts.Length; i++)
        {

            int noteNumber = i + 1;
            if (notes.Contains(noteNumber))
            {
                noteTexts[i].text = noteNumber.ToString();
                noteTexts[i].gameObject.SetActive(true); // GameObject Ȱ��ȭ
            }
            else
            {
                noteTexts[i].text = "";
                noteTexts[i].gameObject.SetActive(false); // GameObject ��Ȱ��ȭ
            }
        }
    }

    public void ClearNotes()
    {
        notes.Clear();
        UpdateNotesDisplay();
    }

    public void SetCorrect(bool correct)
    {
        isCorrect = correct;
        UpdateTextColor();
    }

    public void SetConflict(bool conflict)
    {
        hasConflict = conflict;
        UpdateTextColor();
    }

    void UpdateTextColor()
    {
        if (isGiven)
        {
            numberText.color = Color.black;
        }
        else if (isCorrect)
        {
            numberText.color = new Color(0.2f, 0.4f, 0.8f);
        }
        else
        {
            numberText.color = Color.red;
        }
    }

    public void Clear()
    {
        if (!isGiven)
        {
            SetNumber(0);
            ClearNotes();
        }
    }

    // Ư�� �޸� ���� ���� (�ڵ� ���ſ�)
    public void RemoveNote(int noteNumber)
    {
        if (number != 0 || isGiven)
            return;

        if (notes.Contains(noteNumber))
        {
            notes.Remove(noteNumber);
            UpdateNotesDisplay();
        }
    }

    public void SetHighlight(bool highlight)
    {
        isHighlighted = highlight;
        UpdateCellBackground();
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateCellBackground();
    }

    public void SetSolvableHighlight(bool highlight)
    {
        isSolvableHighlighted = highlight;
        UpdateCellBackground();
    }

    void UpdateCellBackground()
    {
        if (cellImage != null)
        {
            if (isSelected)
            {
                // ���õ� ��: ���� �Ķ���
                cellImage.color = new Color(0.7f, 0.85f, 1f);
            }
            else if (isSolvableHighlighted)
            {
                // Ǯ �� �ִ� ĭ: ���� ������
                cellImage.color = new Color(1f, 0.8f, 0.8f);
            }
            else if (isHighlighted)
            {
                // ���� ����: ȸ��
                cellImage.color = new Color(0.85f, 0.85f, 0.85f);
            }
            else if (isRowColHighlighted)
            {
                // ���� ��/��: ���� ȸ��
                cellImage.color = new Color(0.95f, 0.95f, 0.95f);
            }
            else
            {
                // �⺻: ���
                cellImage.color = Color.white;
            }
        }
    }

    public void SetRowColHighlight(bool highlight)
    {
        isRowColHighlighted = highlight;
        UpdateCellBackground();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        GameManager.Instance.SelectCell(this);
    }
}
