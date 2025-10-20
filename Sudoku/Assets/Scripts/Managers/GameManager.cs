using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static GameManager Instance { get; private set; }

    [Header("버튼관련")]
    public Button eraseButton;
    public Button hintButton;
    public Button undoButton;
    public Button noteModeButton;
    public Button pauseButton; // 일시정지 버튼 추가
    public Image noteModeButtonImage;
    public GameObject numberButtonPrefab;

    [Header("타이머")]
    public TextMeshProUGUI timerText; // 타이머 텍스트
    private float playTime = 0f;
    private bool isTimerRunning = false;

    [Header("그리드, 셀 관련")]
    public GameObject cellPrefab;
    public Transform gridParent;
    public GameObject DivLine;

    [Header("패널 영역")]
    public Transform numberInputPanel;
    public Transform buttonPanel; // Inspector에서 ButtonPanel 연결

    [Header("난이도")]
    public int difficulty = 40;

    [Header("팝업")]
    public DifficultyPopup difficultyPopup; // Inspector에서 연결
    public PausePopup pausePopup; // Inspector에서 연결
    public RetryConfirmPopup restartConfirmPopup; // Inspector에서 연결
    public VictoryPopup victoryPopup;

    [Header("실수, 힌트")]
    public TextMeshProUGUI mistakeText; // 실수 표시 텍스트
    public TextMeshProUGUI hintText; // 힌트 표시 텍스트
    public int maxMistakes = 3; // 최대 실수 허용 횟수
    public int maxHints = 3; // 최대 힌트 사용 횟수
    private int mistakeCount = 0; // 현재 실수 횟수
    private int hintCount = 0; // 현재 힌트 사용 횟수

    // 셀 생성 관련
    private SudokuCell[,] cells = new SudokuCell[9, 9];
    private SudokuGenerator generator;
    private SudokuCell selectedCell;

    // 완성된 셀 구조
    private int[,] initialPuzzle;
    private int[,] currentPuzzle;
    private int[,] solution;

    // 실행 취소 기록
    private Stack<Move> moveHistory = new Stack<Move>();

    [Header("메모 모드")]
    private bool isNoteMode = false;
    public TextMeshProUGUI noteModeButtonText;

    [Header("Solver")]
    public SudokuSolver solver;

    [Header("힌트 연속 클릭")]
    private int hintClickCount = 0;
    private float lastHintClickTime = 0f;
    private float clickTimeWindow = 2f; // 2초 이내 클릭만 카운트
    private bool isSolvableCellsShown = false;

    void Awake()
    {
        // 싱글톤 패턴 구현
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환시 파괴되지 않음 (선택사항)
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
        public HashSet<int> previousNotes; // 이전 메모 상태 추가

        public Move(int r, int c, int prevNum, int newNum, HashSet<int> prevNotes = null)
        {
            row = r;
            col = c;
            previousNumber = prevNum;
            newNumber = newNum;

            // 이전 메모 복사 (null이면 빈 리스트)
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

        // UI 요소들 숨기기
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

    // New Game 버튼이나 일시정지에서 난이도 변경시 사용
    public void ShowDifficultyPopupForChange()
    {
        ShowDifficultyPopup();
    }

    // 현재 퍼즐 재시작 (처음 상태로)
    public void RestartCurrentPuzzle()
    {
        if (initialPuzzle == null) return;

        // 선택된 셀 초기화
        if (selectedCell != null)
        {
            selectedCell.SetSelected(false);
            selectedCell = null;
        }
        // 풀 수 있는 칸 표시 해제
        if (isSolvableCellsShown)
        {
            HideSolvableCells();
            isSolvableCellsShown = false;
        }
        // 모든 하이라이트 제거
        ClearAllHighlights();
        ShowGameUI();

        // 메모 모드 OFF
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

        // 실수 카운터 초기화
        mistakeCount = 0;
        UpdateMistakeDisplay();

        // 힌트 카운터 초기화
        hintCount = 0;
        UpdateHintDisplay();

        // 초기 퍼즐도 검증 (만약을 위한 안전장치)
        if (!HasSolvableCells(initialPuzzle))
        {
            Debug.LogWarning("저장된 초기 퍼즐이 유효하지 않습니다. 새 퍼즐을 생성합니다.");
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
        // 모든 셀의 메모 초기화 추가
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                cells[row, col].ClearNotes();
            }
        }

        DisplayPuzzle();
        StartTimer(); // 타이머 재시작
    }

    // 재시작 확인 팝업 표시
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
            // UI 요소들 숨기기
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
    // PausePopup에서 사용할 수 있도록 public으로 변경
    public float GetPlayTime()
    {
        return playTime;
    }
    public void GenerateNewPuzzle()
    {
        // 첫 게임이면 그리드 생성
        if (cells[0, 0] == null)
        {
            CreateGrid();
        }

        // 선택된 셀 초기화
        if (selectedCell != null)
        {
            selectedCell.SetSelected(false);
            selectedCell = null;
        }
        // 풀 수 있는 칸 표시 해제
        if (isSolvableCellsShown)
        {
            HideSolvableCells();
            isSolvableCellsShown = false;
        }
        // 메모 모드 OFF
        if (isNoteMode)
        {
            ToggleNoteMode();
        }

        // 모든 하이라이트 제거
        ClearAllHighlights();
        // UI 요소들 보이기
        ShowGameUI();

        moveHistory.Clear();

        // 실수 카운터 초기화
        mistakeCount = 0;
        UpdateMistakeDisplay();

        // 힌트 카운터 초기화
        hintCount = 0;
        UpdateHintDisplay();

        // 유일 해를 유지하면서 제거
        solution = generator.GenerateFullBoard();
        currentPuzzle = generator.RemoveDigitsWithCheck(solution, difficulty, solver);

        // 초기 퍼즐 상태 저장
        initialPuzzle = (int[,])currentPuzzle.Clone();

        // 모든 셀의 메모 초기화 추가
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                cells[row, col].ClearNotes();
            }
        }

        DisplayPuzzle();
        // 새 게임 시작시 타이머 시작
        StartTimer();
    }
    // 풀 수 있는 칸이 하나라도 있는지 체크
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

                // 메모 초기화 추가
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
        // 풀 수 있는 칸 표시 해제
        if (isSolvableCellsShown)
        {
            HideSolvableCells();
            isSolvableCellsShown = false;
            hintClickCount = 0; // 카운트도 초기화
        }
        // 이전 하이라이트 제거
        ClearAllHighlights();

        if (selectedCell != null)
        {
            selectedCell.SetSelected(false);
        }

        selectedCell = cell;
        selectedCell.SetSelected(true);

        // 같은 행/열 하이라이트
        HighlightRowAndColumn(cell.row, cell.col);

        // 같은 숫자 하이라이트
        if (cell.number > 0)
        {
            HighlightSameNumbers(cell.number);
        }
    }

    // 같은 숫자 하이라이트
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

    // 같은 행/열 하이라이트
    void HighlightRowAndColumn(int selectedRow, int selectedCol)
    {
        // 같은 행
        for (int c = 0; c < 9; c++)
        {
            cells[selectedRow, c].SetRowColHighlight(true);
        }

        // 같은 열
        for (int r = 0; r < 9; r++)
        {
            cells[r, selectedCol].SetRowColHighlight(true);
        }
    }

    // 모든 하이라이트 제거
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

        // 풀 수 있는 칸 표시 해제
        if (isSolvableCellsShown)
        {
            HideSolvableCells();
            isSolvableCellsShown = false;
            hintClickCount = 0;
        }

        // 메모 모드
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

        // 일반 입력 모드
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

        // 틀린 답이면 실수 카운트 증가
        if (!isCorrect)
        {
            mistakeCount++;
            UpdateMistakeDisplay();

            // 최대 실수 횟수 도달시 게임 오버
            if (mistakeCount >= maxMistakes)
            {
                GameOver();
                return;
            }
        }

        ClearAllConflicts();
        RecheckAllConflicts();

        // 입력한 숫자 하이라이트 추가
        RefreshHighlights();

        if (CheckPuzzleComplete())
        {
            ShowVictoryPopup();
        }
    }

    // 현재 선택된 셀 기준으로 하이라이트 갱신
    void RefreshHighlights()
    {
        if (selectedCell == null)
            return;

        // 하이라이트 제거
        ClearAllHighlights();

        // 행/열 하이라이트
        HighlightRowAndColumn(selectedCell.row, selectedCell.col);

        // 같은 숫자 하이라이트
        if (selectedCell.number > 0)
        {
            HighlightSameNumbers(selectedCell.number);
        }
    }

    // 같은 행/열/박스의 메모에서 숫자 제거
    void RemoveNotesFromRelatedCells(int row, int col, int number)
    {
        // 같은 행의 모든 셀
        for (int c = 0; c < 9; c++)
        {
            if (c != col && !cells[row, c].isGiven)
            {
                cells[row, c].RemoveNote(number);
            }
        }

        // 같은 열의 모든 셀
        for (int r = 0; r < 9; r++)
        {
            if (r != row && !cells[r, col].isGiven)
            {
                cells[r, col].RemoveNote(number);
            }
        }

        // 같은 3x3 박스의 모든 셀
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
                    noteModeButtonText.text = "메모 ON";
                }
            }
            else
            {
                noteModeButtonImage.color = Color.white;
                if (noteModeButtonText != null)
                {
                    noteModeButtonText.text = "메모";
                }
            }
        }
    }

    void CheckConflicts(int row, int col)
    {
        int number = currentPuzzle[row, col];

        if (number == 0) return;

        // 현재 셀이 정답이면 중복 체크 안함
        if (number == solution[row, col])
            return;

        for (int c = 0; c < 9; c++)
        {
            if (c != col && currentPuzzle[row, c] == number && !cells[row, c].isGiven)
            {
                // 상대방이 정답이 아닐 때만 conflict 표시
                if (currentPuzzle[row, c] != solution[row, c])
                {
                    cells[row, c].SetConflict(true);
                }
                // 현재 셀은 틀렸으므로 conflict 표시
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
            // 풀 수 있는 칸 표시 해제
            if (isSolvableCellsShown)
            {
                HideSolvableCells();
                isSolvableCellsShown = false;
                hintClickCount = 0;
            }

            int row = selectedCell.row;
            int col = selectedCell.col;
            int previousNumber = currentPuzzle[row, col];
            HashSet<int> previousNotes = new HashSet<int>(selectedCell.notes); // HashSet으로 변경

            if (previousNumber == 0 && previousNotes.Count == 0)
                return;

            SoundManager.Instance.PlayErase(); // 지우기 사운드만

            Move move = new Move(row, col, previousNumber, 0, previousNotes);
            moveHistory.Push(move);

            selectedCell.Clear();
            currentPuzzle[row, col] = 0;

            ClearAllConflicts();
            RecheckAllConflicts();

            // 하이라이트 갱신
            RefreshHighlights();
        }
    }

    public void Undo()
    {
        if (moveHistory.Count > 0)
        {
            // 풀 수 있는 칸 표시 해제
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

                // 이전 메모 복원
                if (previousNotes != null && previousNotes.Count > 0)
                {
                    cells[row, col].notes = new HashSet<int>(previousNotes);
                    cells[row, col].UpdateNotesDisplay();
                }
            }

            SoundManager.Instance.PlayButtonClick();

            ClearAllConflicts();
            RecheckAllConflicts();

            // 하이라이트 갱신
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
        // 연속 클릭 카운트
        if (Time.time - lastHintClickTime <= clickTimeWindow)
        {
            hintClickCount++;
        }
        else
        {
            hintClickCount = 1;
        }
        lastHintClickTime = Time.time;

        // 10번 연속 클릭시
        if (hintClickCount >= 10)
        {
            ToggleSolvableCells();
            hintClickCount = 0;
            return;
        }

        // 힌트 횟수 체크
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

            // 힌트 사용 횟수 증가
            hintCount++;
            UpdateHintDisplay();

            SoundManager.Instance.PlayWrite();

            Move move = new Move(row, col, previousNumber, correctNumber);
            moveHistory.Push(move);

            selectedCell.SetNumber(correctNumber, false);
            currentPuzzle[row, col] = correctNumber;
            selectedCell.SetCorrect(true);

            // 같은 행/열/박스의 메모에서 해당 숫자 제거
            RemoveNotesFromRelatedCells(row, col, correctNumber);

            ClearAllConflicts();
            RecheckAllConflicts();

            // 하이라이트 갱신
            RefreshHighlights();

            if (CheckPuzzleComplete())
            {
                ShowVictoryPopup();
            }
        }
    }

    // 풀 수 있는 칸 표시 토글
    void ToggleSolvableCells()
    {
        if (isSolvableCellsShown)
        {
            // 표시 해제
            HideSolvableCells();
        }
        else
        {
            // 표시
            ShowSolvableCells();
        }

        isSolvableCellsShown = !isSolvableCellsShown;
    }
    // 풀 수 있는 칸 표시 해제
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

        Debug.Log($"논리적으로 풀 수 있는 칸: {solvable.Count}개");

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
            mistakeText.text = $"실수 : {mistakeCount}/{maxMistakes}";
        }
    }

    void UpdateHintDisplay()
    {
        if (hintText != null)
        {
            hintText.text = $"힌트 : {hintCount}/{maxHints}";
        }

        // 힌트 버튼 활성화/비활성화
        if (hintButton != null)
        {
            hintButton.interactable = (hintCount < maxHints);
        }
    }

    void GameOver()
    {
        StopTimer();
        HideGameUI();

        // 임시로 재시작 팝업 표시
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
