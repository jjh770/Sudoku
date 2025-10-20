using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{
    // �̱��� �ν��Ͻ�
    public static GameManager Instance { get; private set; }

    [Header("��ư����")]
    public Button eraseButton;
    public Button hintButton;
    public Button undoButton;
    public Button noteModeButton;
    public Button pauseButton; // �Ͻ����� ��ư �߰�
    public Image noteModeButtonImage;
    public GameObject numberButtonPrefab;

    [Header("Ÿ�̸�")]
    public TextMeshProUGUI timerText; // Ÿ�̸� �ؽ�Ʈ
    private float playTime = 0f;
    private bool isTimerRunning = false;

    [Header("�׸���, �� ����")]
    public GameObject cellPrefab;
    public Transform gridParent;
    public GameObject DivLine;

    [Header("�г� ����")]
    public Transform numberInputPanel;
    public Transform buttonPanel; // Inspector���� ButtonPanel ����

    [Header("���̵�")]
    public int difficulty = 40;

    [Header("�˾�")]
    public DifficultyPopup difficultyPopup; // Inspector���� ����
    public PausePopup pausePopup; // Inspector���� ����
    public RetryConfirmPopup restartConfirmPopup; // Inspector���� ����
    public VictoryPopup victoryPopup;

    [Header("�Ǽ�, ��Ʈ")]
    public TextMeshProUGUI mistakeText; // �Ǽ� ǥ�� �ؽ�Ʈ
    public TextMeshProUGUI hintText; // ��Ʈ ǥ�� �ؽ�Ʈ
    public int maxMistakes = 3; // �ִ� �Ǽ� ��� Ƚ��
    public int maxHints = 3; // �ִ� ��Ʈ ��� Ƚ��
    private int mistakeCount = 0; // ���� �Ǽ� Ƚ��
    private int hintCount = 0; // ���� ��Ʈ ��� Ƚ��

    // �� ���� ����
    private SudokuCell[,] cells = new SudokuCell[9, 9];
    private SudokuGenerator generator;
    private SudokuCell selectedCell;

    // �ϼ��� �� ����
    private int[,] initialPuzzle;
    private int[,] currentPuzzle;
    private int[,] solution;

    // ���� ��� ���
    private Stack<Move> moveHistory = new Stack<Move>();

    [Header("�޸� ���")]
    private bool isNoteMode = false;
    public TextMeshProUGUI noteModeButtonText;

    [Header("Solver")]
    public SudokuSolver solver;

    [Header("��Ʈ ���� Ŭ��")]
    private int hintClickCount = 0;
    private float lastHintClickTime = 0f;
    private float clickTimeWindow = 2f; // 2�� �̳� Ŭ���� ī��Ʈ
    private bool isSolvableCellsShown = false;

    void Awake()
    {
        // �̱��� ���� ����
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // �� ��ȯ�� �ı����� ���� (���û���)
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }


    [System.Serializable]
    public class Move
    {
        public int row;
        public int col;
        public int previousNumber;
        public int newNumber;
        public HashSet<int> previousNotes; // ���� �޸� ���� �߰�

        public Move(int r, int c, int prevNum, int newNum, HashSet<int> prevNotes = null)
        {
            row = r;
            col = c;
            previousNumber = prevNum;
            newNumber = newNum;

            // ���� �޸� ���� (null�̸� �� ����Ʈ)
            if (prevNotes != null)
                previousNotes = new HashSet<int>(prevNotes);
            else
                previousNotes = new HashSet<int>();
        }
    }

    void Start()
    {
        generator = gameObject.AddComponent<SudokuGenerator>();
        solver = gameObject.AddComponent<SudokuSolver>();
        //CreateGrid();
        CreateNumberButtons();
        SetupButtons();

        // UI ��ҵ� �����
        HideGameUI();

        ShowDifficultyPopup();
    }

    void Update()
    {
        if (isTimerRunning)
        {
            playTime += Time.deltaTime;
            UpdateTimerDisplay();
        }
    }

    void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(playTime / 60f);
            int seconds = Mathf.FloorToInt(playTime % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    void StartTimer()
    {
        playTime = 0f;
        isTimerRunning = true;
        UpdateTimerDisplay();
    }

    void StopTimer()
    {
        isTimerRunning = false;
    }

    void CreateGrid()
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                GameObject cellObj = Instantiate(cellPrefab, gridParent);
                SudokuCell cell = cellObj.GetComponent<SudokuCell>();
                cell.InitCell(row, col);
                cell.gameObject.name = $"Cell_{row}_{col}";
                cells[row, col] = cell;
            }
        }

        Instantiate(DivLine, gridParent);
    }

    void CreateNumberButtons()
    {
        for (int i = 1; i <= 9; i++)
        {
            GameObject buttonObj = Instantiate(numberButtonPrefab, numberInputPanel);
            NumButton numButton = buttonObj.AddComponent<NumButton>();
            numButton.Initialize(i);
        }
    }

    void SetupButtons()
    {
        if (eraseButton != null)
        {
            eraseButton.onClick.AddListener(EraseSelectedCell);
        }

        if (hintButton != null)
        {
            hintButton.onClick.AddListener(GiveHint);
        }

        if (undoButton != null)
        {
            undoButton.onClick.AddListener(Undo);
        }

        if (noteModeButton != null)
        {
            noteModeButton.onClick.AddListener(ToggleNoteMode);
        }

        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(ShowPausePopup);
        }
    }
    void ShowDifficultyPopup()
    {
        if (difficultyPopup != null)
        {
            if (!difficultyPopup.gameObject.activeInHierarchy)
                difficultyPopup.gameObject.SetActive(true);

            difficultyPopup.ShowPopup();
        }
    }

    // New Game ��ư�̳� �Ͻ��������� ���̵� ����� ���
    public void ShowDifficultyPopupForChange()
    {
        ShowDifficultyPopup();
    }

    // ���� ���� ����� (ó�� ���·�)
    public void RestartCurrentPuzzle()
    {
        if (initialPuzzle == null) return;

        // ���õ� �� �ʱ�ȭ
        if (selectedCell != null)
        {
            selectedCell.SetSelected(false);
            selectedCell = null;
        }
        // Ǯ �� �ִ� ĭ ǥ�� ����
        if (isSolvableCellsShown)
        {
            HideSolvableCells();
            isSolvableCellsShown = false;
        }
        // ��� ���̶���Ʈ ����
        ClearAllHighlights();
        ShowGameUI();

        // �޸� ��� OFF
        if (isNoteMode)
        {
            ToggleNoteMode();
        }

        if (selectedCell != null)
        {
            selectedCell.SetSelected(false);
            selectedCell = null;
        }

        moveHistory.Clear();

        // �Ǽ� ī���� �ʱ�ȭ
        mistakeCount = 0;
        UpdateMistakeDisplay();

        // ��Ʈ ī���� �ʱ�ȭ
        hintCount = 0;
        UpdateHintDisplay();

        // �ʱ� ���� ���� (������ ���� ������ġ)
        if (!HasSolvableCells(initialPuzzle))
        {
            Debug.LogWarning("����� �ʱ� ������ ��ȿ���� �ʽ��ϴ�. �� ������ �����մϴ�.");
            GenerateNewPuzzle();
            return;
        }

        currentPuzzle = (int[,])initialPuzzle.Clone();

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                cells[row, col].ClearNotes();
            }
        }
        // ��� ���� �޸� �ʱ�ȭ �߰�
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                cells[row, col].ClearNotes();
            }
        }

        DisplayPuzzle();
        StartTimer(); // Ÿ�̸� �����
    }

    // ����� Ȯ�� �˾� ǥ��
    public void ShowRestartConfirmPopup()
    {
        if (restartConfirmPopup != null)
        {
            if (!restartConfirmPopup.gameObject.activeInHierarchy)
            {
                restartConfirmPopup.gameObject.SetActive(true);
            }

            restartConfirmPopup.ShowPopup();
        }
    }
    public void ShowPausePopup()
    {
        if (pausePopup != null)
        {
            // UI ��ҵ� �����
            HideGameUI();

            SoundManager.Instance.PlayButtonClick();

            if (!pausePopup.gameObject.activeInHierarchy)
            {
                pausePopup.gameObject.SetActive(true);
            }

            pausePopup.ShowPopup();
        }
    }

    public void PauseTimer()
    {
        isTimerRunning = false;
    }

    public void ResumeTimer()
    {
        isTimerRunning = true;
    }

    public void HideGameUI()
    {
        if (gridParent != null)
            gridParent.gameObject.SetActive(false);

        if (numberInputPanel != null)
            numberInputPanel.gameObject.SetActive(false);

        if (buttonPanel != null)
            buttonPanel.gameObject.SetActive(false);

        if (timerText != null)
            timerText.gameObject.SetActive(false);

        if (mistakeText != null)
            mistakeText.gameObject.SetActive(false);

        if (hintText != null)
            hintText.gameObject.SetActive(false);

        if (pauseButton != null)
            pauseButton.gameObject.SetActive(false);
    }

    public void ShowGameUI()
    {
        if (gridParent != null)
            gridParent.gameObject.SetActive(true);

        if (numberInputPanel != null)
            numberInputPanel.gameObject.SetActive(true);

        if (buttonPanel != null)
            buttonPanel.gameObject.SetActive(true);

        if (timerText != null)
            timerText.gameObject.SetActive(true);

        if (mistakeText != null)
            mistakeText.gameObject.SetActive(true);

        if (hintText != null)
            hintText.gameObject.SetActive(true);

        if (pauseButton != null)
            pauseButton.gameObject.SetActive(true);
    }
    // PausePopup���� ����� �� �ֵ��� public���� ����
    public float GetPlayTime()
    {
        return playTime;
    }
    public void GenerateNewPuzzle()
    {
        // ù �����̸� �׸��� ����
        if (cells[0, 0] == null)
        {
            CreateGrid();
        }

        // ���õ� �� �ʱ�ȭ
        if (selectedCell != null)
        {
            selectedCell.SetSelected(false);
            selectedCell = null;
        }
        // Ǯ �� �ִ� ĭ ǥ�� ����
        if (isSolvableCellsShown)
        {
            HideSolvableCells();
            isSolvableCellsShown = false;
        }
        // �޸� ��� OFF
        if (isNoteMode)
        {
            ToggleNoteMode();
        }

        // ��� ���̶���Ʈ ����
        ClearAllHighlights();
        // UI ��ҵ� ���̱�
        ShowGameUI();

        moveHistory.Clear();

        // �Ǽ� ī���� �ʱ�ȭ
        mistakeCount = 0;
        UpdateMistakeDisplay();

        // ��Ʈ ī���� �ʱ�ȭ
        hintCount = 0;
        UpdateHintDisplay();

        // ���� �ظ� �����ϸ鼭 ����
        solution = generator.GenerateFullBoard();
        currentPuzzle = generator.RemoveDigitsWithCheck(solution, difficulty, solver);

        // �ʱ� ���� ���� ����
        initialPuzzle = (int[,])currentPuzzle.Clone();

        // ��� ���� �޸� �ʱ�ȭ �߰�
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                cells[row, col].ClearNotes();
            }
        }

        DisplayPuzzle();
        // �� ���� ���۽� Ÿ�̸� ����
        StartTimer();
    }
    // Ǯ �� �ִ� ĭ�� �ϳ��� �ִ��� üũ
    bool HasSolvableCells(int[,] puzzle)
    {
        List<SolvableCell> solvable = solver.FindAllSolvableCells(puzzle);
        return solvable.Count > 0;
    }
    void DisplayPuzzle()
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                int number = currentPuzzle[row, col];

                // �޸� �ʱ�ȭ �߰�
                cells[row, col].ClearNotes();

                if (number > 0)
                {
                    cells[row, col].SetNumber(number, true);
                }
                else
                {
                    cells[row, col].SetNumber(0, false);
                }
            }
        }
    }

    public void SelectCell(SudokuCell cell)
    {
        // Ǯ �� �ִ� ĭ ǥ�� ����
        if (isSolvableCellsShown)
        {
            HideSolvableCells();
            isSolvableCellsShown = false;
            hintClickCount = 0; // ī��Ʈ�� �ʱ�ȭ
        }
        // ���� ���̶���Ʈ ����
        ClearAllHighlights();

        if (selectedCell != null)
        {
            selectedCell.SetSelected(false);
        }

        selectedCell = cell;
        selectedCell.SetSelected(true);

        // ���� ��/�� ���̶���Ʈ
        HighlightRowAndColumn(cell.row, cell.col);

        // ���� ���� ���̶���Ʈ
        if (cell.number > 0)
        {
            HighlightSameNumbers(cell.number);
        }
    }

    // ���� ���� ���̶���Ʈ
    void HighlightSameNumbers(int number)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (currentPuzzle[row, col] == number)
                {
                    cells[row, col].SetHighlight(true);
                }
            }
        }
    }

    // ���� ��/�� ���̶���Ʈ
    void HighlightRowAndColumn(int selectedRow, int selectedCol)
    {
        // ���� ��
        for (int c = 0; c < 9; c++)
        {
            cells[selectedRow, c].SetRowColHighlight(true);
        }

        // ���� ��
        for (int r = 0; r < 9; r++)
        {
            cells[r, selectedCol].SetRowColHighlight(true);
        }
    }

    // ��� ���̶���Ʈ ����
    void ClearAllHighlights()
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                cells[row, col].SetHighlight(false);
                cells[row, col].SetRowColHighlight(false);
            }
        }
    }

    public void InputNumber(int number)
    {
        if (selectedCell == null || selectedCell.isGiven)
            return;

        // Ǯ �� �ִ� ĭ ǥ�� ����
        if (isSolvableCellsShown)
        {
            HideSolvableCells();
            isSolvableCellsShown = false;
            hintClickCount = 0;
        }

        // �޸� ���
        if (isNoteMode)
        {
            int row = selectedCell.row;
            int col = selectedCell.col;
            HashSet<int> previousNotes = new HashSet<int>(selectedCell.notes);

            selectedCell.ToggleNote(number);

            SoundManager.Instance.PlayWrite();

            Move noteMove = new Move(row, col, 0, 0, previousNotes);
            moveHistory.Push(noteMove);
            return;
        }

        // �Ϲ� �Է� ���
        int inputRow = selectedCell.row;
        int inputCol = selectedCell.col;
        int previousNumber = currentPuzzle[inputRow, inputCol];
        HashSet<int> previousInputNotes = new HashSet<int>(selectedCell.notes);

        if (previousNumber == number)
            return;

        SoundManager.Instance.PlayWrite();

        Move numberMove = new Move(inputRow, inputCol, previousNumber, number, previousInputNotes);
        moveHistory.Push(numberMove);

        selectedCell.SetNumber(number, false);
        currentPuzzle[inputRow, inputCol] = number;

        bool isCorrect = (number == solution[inputRow, inputCol]);
        selectedCell.SetCorrect(isCorrect);

        RemoveNotesFromRelatedCells(inputRow, inputCol, number);

        // Ʋ�� ���̸� �Ǽ� ī��Ʈ ����
        if (!isCorrect)
        {
            mistakeCount++;
            UpdateMistakeDisplay();

            // �ִ� �Ǽ� Ƚ�� ���޽� ���� ����
            if (mistakeCount >= maxMistakes)
            {
                GameOver();
                return;
            }
        }

        ClearAllConflicts();
        RecheckAllConflicts();

        // �Է��� ���� ���̶���Ʈ �߰�
        RefreshHighlights();

        if (CheckPuzzleComplete())
        {
            ShowVictoryPopup();
        }
    }

    // ���� ���õ� �� �������� ���̶���Ʈ ����
    void RefreshHighlights()
    {
        if (selectedCell == null)
            return;

        // ���̶���Ʈ ����
        ClearAllHighlights();

        // ��/�� ���̶���Ʈ
        HighlightRowAndColumn(selectedCell.row, selectedCell.col);

        // ���� ���� ���̶���Ʈ
        if (selectedCell.number > 0)
        {
            HighlightSameNumbers(selectedCell.number);
        }
    }

    // ���� ��/��/�ڽ��� �޸𿡼� ���� ����
    void RemoveNotesFromRelatedCells(int row, int col, int number)
    {
        // ���� ���� ��� ��
        for (int c = 0; c < 9; c++)
        {
            if (c != col && !cells[row, c].isGiven)
            {
                cells[row, c].RemoveNote(number);
            }
        }

        // ���� ���� ��� ��
        for (int r = 0; r < 9; r++)
        {
            if (r != row && !cells[r, col].isGiven)
            {
                cells[r, col].RemoveNote(number);
            }
        }

        // ���� 3x3 �ڽ��� ��� ��
        int boxRow = (row / 3) * 3;
        int boxCol = (col / 3) * 3;

        for (int r = boxRow; r < boxRow + 3; r++)
        {
            for (int c = boxCol; c < boxCol + 3; c++)
            {
                if ((r != row || c != col) && !cells[r, c].isGiven)
                {
                    cells[r, c].RemoveNote(number);
                }
            }
        }
    }

    void ToggleNoteMode()
    {
        isNoteMode = !isNoteMode;

        SoundManager.Instance.PlayButtonClick();

        if (noteModeButtonImage != null)
        {
            if (isNoteMode)
            {
                noteModeButtonImage.color = new Color(0.6f, 0.9f, 1f);
                if (noteModeButtonText != null)
                {
                    noteModeButtonText.text = "�޸� ON";
                }
            }
            else
            {
                noteModeButtonImage.color = Color.white;
                if (noteModeButtonText != null)
                {
                    noteModeButtonText.text = "�޸�";
                }
            }
        }
    }

    void CheckConflicts(int row, int col)
    {
        int number = currentPuzzle[row, col];

        if (number == 0) return;

        // ���� ���� �����̸� �ߺ� üũ ����
        if (number == solution[row, col])
            return;

        for (int c = 0; c < 9; c++)
        {
            if (c != col && currentPuzzle[row, c] == number && !cells[row, c].isGiven)
            {
                // ������ ������ �ƴ� ���� conflict ǥ��
                if (currentPuzzle[row, c] != solution[row, c])
                {
                    cells[row, c].SetConflict(true);
                }
                // ���� ���� Ʋ�����Ƿ� conflict ǥ��
                cells[row, col].SetConflict(true);
            }
        }

        for (int r = 0; r < 9; r++)
        {
            if (r != row && currentPuzzle[r, col] == number && !cells[r, col].isGiven)
            {
                if (currentPuzzle[r, col] != solution[r, col])
                {
                    cells[r, col].SetConflict(true);
                }
                cells[row, col].SetConflict(true);
            }
        }

        int boxRow = (row / 3) * 3;
        int boxCol = (col / 3) * 3;

        for (int r = boxRow; r < boxRow + 3; r++)
        {
            for (int c = boxCol; c < boxCol + 3; c++)
            {
                if ((r != row || c != col) && currentPuzzle[r, c] == number && !cells[r, c].isGiven)
                {
                    if (currentPuzzle[r, c] != solution[r, c])
                    {
                        cells[r, c].SetConflict(true);
                    }
                    cells[row, col].SetConflict(true);
                }
            }
        }
    }

    public void EraseSelectedCell()
    {
        if (selectedCell != null && !selectedCell.isGiven)
        {
            // Ǯ �� �ִ� ĭ ǥ�� ����
            if (isSolvableCellsShown)
            {
                HideSolvableCells();
                isSolvableCellsShown = false;
                hintClickCount = 0;
            }

            int row = selectedCell.row;
            int col = selectedCell.col;
            int previousNumber = currentPuzzle[row, col];
            HashSet<int> previousNotes = new HashSet<int>(selectedCell.notes); // HashSet���� ����

            if (previousNumber == 0 && previousNotes.Count == 0)
                return;

            SoundManager.Instance.PlayErase(); // ����� ���常

            Move move = new Move(row, col, previousNumber, 0, previousNotes);
            moveHistory.Push(move);

            selectedCell.Clear();
            currentPuzzle[row, col] = 0;

            ClearAllConflicts();
            RecheckAllConflicts();

            // ���̶���Ʈ ����
            RefreshHighlights();
        }
    }

    public void Undo()
    {
        if (moveHistory.Count > 0)
        {
            // Ǯ �� �ִ� ĭ ǥ�� ����
            if (isSolvableCellsShown)
            {
                HideSolvableCells();
                isSolvableCellsShown = false;
                hintClickCount = 0;
            }

            Move lastMove = moveHistory.Pop();

            int row = lastMove.row;
            int col = lastMove.col;
            int previousNumber = lastMove.previousNumber;
            HashSet<int> previousNotes = lastMove.previousNotes;

            currentPuzzle[row, col] = previousNumber;

            if (previousNumber > 0)
            {
                cells[row, col].SetNumber(previousNumber, false);
                bool isCorrect = (previousNumber == solution[row, col]);
                cells[row, col].SetCorrect(isCorrect);
            }
            else
            {
                cells[row, col].Clear();

                // ���� �޸� ����
                if (previousNotes != null && previousNotes.Count > 0)
                {
                    cells[row, col].notes = new HashSet<int>(previousNotes);
                    cells[row, col].UpdateNotesDisplay();
                }
            }

            SoundManager.Instance.PlayButtonClick();

            ClearAllConflicts();
            RecheckAllConflicts();

            // ���̶���Ʈ ����
            RefreshHighlights();
        }
    }

    void ClearAllConflicts()
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                cells[row, col].SetConflict(false);
            }
        }
    }

    void RecheckAllConflicts()
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (currentPuzzle[row, col] != 0 && !cells[row, col].isGiven)
                {
                    CheckConflicts(row, col);
                }
            }
        }
    }

    public void GiveHint()
    {
        // ���� Ŭ�� ī��Ʈ
        if (Time.time - lastHintClickTime <= clickTimeWindow)
        {
            hintClickCount++;
        }
        else
        {
            hintClickCount = 1;
        }
        lastHintClickTime = Time.time;

        // 10�� ���� Ŭ����
        if (hintClickCount >= 10)
        {
            ToggleSolvableCells();
            hintClickCount = 0;
            return;
        }

        // ��Ʈ Ƚ�� üũ
        if (hintCount >= maxHints)
        {
            return;
        }

        if (selectedCell != null && !selectedCell.isGiven)
        {
            int row = selectedCell.row;
            int col = selectedCell.col;
            int previousNumber = currentPuzzle[row, col];
            int correctNumber = solution[row, col];

            if (previousNumber == correctNumber)
                return;

            // ��Ʈ ��� Ƚ�� ����
            hintCount++;
            UpdateHintDisplay();

            SoundManager.Instance.PlayWrite();

            Move move = new Move(row, col, previousNumber, correctNumber);
            moveHistory.Push(move);

            selectedCell.SetNumber(correctNumber, false);
            currentPuzzle[row, col] = correctNumber;
            selectedCell.SetCorrect(true);

            // ���� ��/��/�ڽ��� �޸𿡼� �ش� ���� ����
            RemoveNotesFromRelatedCells(row, col, correctNumber);

            ClearAllConflicts();
            RecheckAllConflicts();

            // ���̶���Ʈ ����
            RefreshHighlights();

            if (CheckPuzzleComplete())
            {
                ShowVictoryPopup();
            }
        }
    }

    // Ǯ �� �ִ� ĭ ǥ�� ���
    void ToggleSolvableCells()
    {
        if (isSolvableCellsShown)
        {
            // ǥ�� ����
            HideSolvableCells();
        }
        else
        {
            // ǥ��
            ShowSolvableCells();
        }

        isSolvableCellsShown = !isSolvableCellsShown;
    }
    // Ǯ �� �ִ� ĭ ǥ�� ����
    void HideSolvableCells()
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                cells[row, col].SetSolvableHighlight(false);
            }
        }
    }
    void ShowSolvableCells()
    {
        List<SolvableCell> solvable = solver.FindAllSolvableCells(currentPuzzle);

        Debug.Log($"�������� Ǯ �� �ִ� ĭ: {solvable.Count}��");

        foreach (var cell in solvable)
        {
            cells[cell.row, cell.col].SetSolvableHighlight(true);
        }
    }

    void ShowVictoryPopup()
    {
        StopTimer();
        HideGameUI();

        if (victoryPopup != null)
        {
            if (!victoryPopup.gameObject.activeInHierarchy)
            {
                victoryPopup.gameObject.SetActive(true);
            }

            victoryPopup.ShowPopup(playTime, difficulty, mistakeCount, hintCount);
        }
    }

    bool CheckPuzzleComplete()
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (currentPuzzle[row, col] != solution[row, col])
                {
                    return false;
                }
            }
        }
        return true;
    }
    void UpdateMistakeDisplay()
    {
        if (mistakeText != null)
        {
            mistakeText.text = $"�Ǽ� : {mistakeCount}/{maxMistakes}";
        }
    }

    void UpdateHintDisplay()
    {
        if (hintText != null)
        {
            hintText.text = $"��Ʈ : {hintCount}/{maxHints}";
        }

        // ��Ʈ ��ư Ȱ��ȭ/��Ȱ��ȭ
        if (hintButton != null)
        {
            hintButton.interactable = (hintCount < maxHints);
        }
    }

    void GameOver()
    {
        StopTimer();
        HideGameUI();

        // �ӽ÷� ����� �˾� ǥ��
        ShowGameOverPopup();
    }

    public void ShowGameOverPopup()
    {
        if (restartConfirmPopup != null)
        {
            if (!restartConfirmPopup.gameObject.activeInHierarchy)
            {
                restartConfirmPopup.gameObject.SetActive(true);
            }

            restartConfirmPopup.ShowGameOverPopup();
        }
    }

    public SudokuCell GetCell(int row, int col)
    {
        if (row >= 0 && row < 9 && col >= 0 && col < 9)
            return cells[row, col];
        return null;
    }
}
