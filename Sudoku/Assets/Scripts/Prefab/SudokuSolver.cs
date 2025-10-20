using UnityEngine;
using System.Collections.Generic;

public class SudokuSolver : MonoBehaviour
{
    // 유일 해 검증
    public bool HasUniqueSolution(int[,] puzzle)
    {
        int[,] testPuzzle = (int[,])puzzle.Clone();
        int solutionCount = 0;
        CountSolutions(testPuzzle, 0, 0, ref solutionCount);
        return solutionCount == 1;
    }

    void CountSolutions(int[,] puzzle, int row, int col, ref int count)
    {
        // 2개 이상 찾으면 조기 종료
        if (count > 1) return;

        // 다음 칸으로 이동
        if (col == 9)
        {
            col = 0;
            row++;
        }

        // 모든 칸 완성 = 해 +1
        if (row == 9)
        {
            count++;
            return;
        }

        // 이미 채워진 칸 건너뛰기
        if (puzzle[row, col] != 0)
        {
            CountSolutions(puzzle, row, col + 1, ref count);
            return;
        }

        // 1~9 시도
        for (int num = 1; num <= 9; num++)
        {
            if (IsValidPlacement(puzzle, row, col, num))
            {
                puzzle[row, col] = num;
                CountSolutions(puzzle, row, col + 1, ref count);
                puzzle[row, col] = 0; // 백트래킹
            }
        }
    }

    bool IsValidPlacement(int[,] puzzle, int row, int col, int num)
    {
        // 행 체크
        for (int c = 0; c < 9; c++)
        {
            if (puzzle[row, c] == num)
                return false;
        }

        // 열 체크
        for (int r = 0; r < 9; r++)
        {
            if (puzzle[r, col] == num)
                return false;
        }

        // 3x3 박스 체크
        int boxRow = (row / 3) * 3;
        int boxCol = (col / 3) * 3;
        for (int r = boxRow; r < boxRow + 3; r++)
        {
            for (int c = boxCol; c < boxCol + 3; c++)
            {
                if (puzzle[r, c] == num)
                    return false;
            }
        }

        return true;
    }

    // 특정 칸에 들어갈 수 있는 후보 숫자들
    public HashSet<int> GetCandidates(int[,] puzzle, int row, int col)
    {
        HashSet<int> candidates = new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        if (puzzle[row, col] != 0)
            return new HashSet<int>();

        // 같은 행의 숫자 제거
        for (int c = 0; c < 9; c++)
        {
            candidates.Remove(puzzle[row, c]);
        }

        // 같은 열의 숫자 제거
        for (int r = 0; r < 9; r++)
        {
            candidates.Remove(puzzle[r, col]);
        }

        // 같은 박스의 숫자 제거
        int boxRow = (row / 3) * 3;
        int boxCol = (col / 3) * 3;
        for (int r = boxRow; r < boxRow + 3; r++)
        {
            for (int c = boxCol; c < boxCol + 3; c++)
            {
                candidates.Remove(puzzle[r, c]);
            }
        }

        return candidates;
    }

    // Naked Single 찾기
    public List<SolvableCell> FindNakedSingles(int[,] puzzle)
    {
        List<SolvableCell> solvable = new List<SolvableCell>();

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (puzzle[row, col] == 0)
                {
                    HashSet<int> candidates = GetCandidates(puzzle, row, col);

                    if (candidates.Count == 1)
                    {
                        int answer = 0;
                        foreach (int num in candidates)
                        {
                            answer = num;
                        }

                        solvable.Add(new SolvableCell(row, col, answer, "Naked Single"));
                    }
                }
            }
        }

        return solvable;
    }

    // Hidden Single 찾기
    public List<SolvableCell> FindHiddenSingles(int[,] puzzle)
    {
        List<SolvableCell> solvable = new List<SolvableCell>();

        for (int row = 0; row < 9; row++)
        {
            solvable.AddRange(FindHiddenSinglesInRow(puzzle, row));
        }

        for (int col = 0; col < 9; col++)
        {
            solvable.AddRange(FindHiddenSinglesInCol(puzzle, col));
        }

        for (int boxRow = 0; boxRow < 3; boxRow++)
        {
            for (int boxCol = 0; boxCol < 3; boxCol++)
            {
                solvable.AddRange(FindHiddenSinglesInBox(puzzle, boxRow * 3, boxCol * 3));
            }
        }

        return solvable;
    }

    List<SolvableCell> FindHiddenSinglesInRow(int[,] puzzle, int row)
    {
        List<SolvableCell> solvable = new List<SolvableCell>();
        Dictionary<int, List<int>> numToPositions = new Dictionary<int, List<int>>();

        for (int num = 1; num <= 9; num++)
        {
            numToPositions[num] = new List<int>();
        }

        for (int col = 0; col < 9; col++)
        {
            if (puzzle[row, col] == 0)
            {
                HashSet<int> candidates = GetCandidates(puzzle, row, col);
                foreach (int num in candidates)
                {
                    numToPositions[num].Add(col);
                }
            }
        }

        foreach (var pair in numToPositions)
        {
            if (pair.Value.Count == 1)
            {
                int col = pair.Value[0];
                solvable.Add(new SolvableCell(row, col, pair.Key, "Hidden Single (Row)"));
            }
        }

        return solvable;
    }

    List<SolvableCell> FindHiddenSinglesInCol(int[,] puzzle, int col)
    {
        List<SolvableCell> solvable = new List<SolvableCell>();
        Dictionary<int, List<int>> numToPositions = new Dictionary<int, List<int>>();

        for (int num = 1; num <= 9; num++)
        {
            numToPositions[num] = new List<int>();
        }

        for (int row = 0; row < 9; row++)
        {
            if (puzzle[row, col] == 0)
            {
                HashSet<int> candidates = GetCandidates(puzzle, row, col);
                foreach (int num in candidates)
                {
                    numToPositions[num].Add(row);
                }
            }
        }

        foreach (var pair in numToPositions)
        {
            if (pair.Value.Count == 1)
            {
                int row = pair.Value[0];
                solvable.Add(new SolvableCell(row, col, pair.Key, "Hidden Single (Col)"));
            }
        }

        return solvable;
    }

    List<SolvableCell> FindHiddenSinglesInBox(int[,] puzzle, int startRow, int startCol)
    {
        List<SolvableCell> solvable = new List<SolvableCell>();
        Dictionary<int, List<Vector2Int>> numToPositions = new Dictionary<int, List<Vector2Int>>();

        for (int num = 1; num <= 9; num++)
        {
            numToPositions[num] = new List<Vector2Int>();
        }

        for (int r = startRow; r < startRow + 3; r++)
        {
            for (int c = startCol; c < startCol + 3; c++)
            {
                if (puzzle[r, c] == 0)
                {
                    HashSet<int> candidates = GetCandidates(puzzle, r, c);
                    foreach (int num in candidates)
                    {
                        numToPositions[num].Add(new Vector2Int(r, c));
                    }
                }
            }
        }

        foreach (var pair in numToPositions)
        {
            if (pair.Value.Count == 1)
            {
                Vector2Int pos = pair.Value[0];
                solvable.Add(new SolvableCell(pos.x, pos.y, pair.Key, "Hidden Single (Box)"));
            }
        }

        return solvable;
    }

    // 모든 풀 수 있는 칸 찾기
    public List<SolvableCell> FindAllSolvableCells(int[,] puzzle)
    {
        List<SolvableCell> allSolvable = new List<SolvableCell>();

        allSolvable.AddRange(FindNakedSingles(puzzle));
        allSolvable.AddRange(FindHiddenSingles(puzzle));

        HashSet<string> seen = new HashSet<string>();
        List<SolvableCell> unique = new List<SolvableCell>();

        foreach (var cell in allSolvable)
        {
            string key = $"{cell.row}_{cell.col}";
            if (!seen.Contains(key))
            {
                seen.Add(key);
                unique.Add(cell);
            }
        }

        return unique;
    }
}

[System.Serializable]
public class SolvableCell
{
    public int row;
    public int col;
    public int answer;
    public string method;

    public SolvableCell(int r, int c, int ans, string m)
    {
        row = r;
        col = c;
        answer = ans;
        method = m;
    }
}
