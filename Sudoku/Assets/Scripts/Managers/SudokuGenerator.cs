using UnityEngine;
using System.Collections.Generic;

public class SudokuGenerator : MonoBehaviour
{
    private int[,] board = new int[9, 9];

    public int[,] GenerateFullBoard()
    {
        board = new int[9, 9];
        FillDiagonal();
        FillRemaining(0, 3);
        return board;
    }

    void FillDiagonal()
    {
        for (int i = 0; i < 9; i += 3)
        {
            FillBox(i, i);
        }
    }

    void FillBox(int row, int col)
    {
        List<int> numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int index = Random.Range(0, numbers.Count);
                board[row + i, col + j] = numbers[index];
                numbers.RemoveAt(index);
            }
        }
    }

    bool FillRemaining(int i, int j)
    {
        if (j >= 9 && i < 8)
        {
            i++;
            j = 0;
        }
        if (i >= 9 && j >= 9)
        {
            return true;
        }

        if (i < 3)
        {
            if (j < 3)
                j = 3;
        }
        else if (i < 6)
        {
            if (j == (int)(i / 3) * 3)
                j += 3;
        }
        else
        {
            if (j == 6)
            {
                i++;
                j = 0;
                if (i >= 9)
                    return true;
            }
        }

        for (int num = 1; num <= 9; num++)
        {
            if (IsSafe(i, j, num))
            {
                board[i, j] = num;
                if (FillRemaining(i, j + 1))
                    return true;
                board[i, j] = 0;
            }
        }
        return false;
    }

    bool IsSafe(int row, int col, int num)
    {
        return !UsedInRow(row, num) && !UsedInCol(col, num) && !UsedInBox(row - row % 3, col - col % 3, num);
    }

    bool UsedInRow(int row, int num)
    {
        for (int col = 0; col < 9; col++)
        {
            if (board[row, col] == num)
                return true;
        }
        return false;
    }

    bool UsedInCol(int col, int num)
    {
        for (int row = 0; row < 9; row++)
        {
            if (board[row, col] == num)
                return true;
        }
        return false;
    }

    bool UsedInBox(int boxStartRow, int boxStartCol, int num)
    {
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                if (board[row + boxStartRow, col + boxStartCol] == num)
                    return true;
            }
        }
        return false;
    }

    public int[,] RemoveDigitsWithCheck(int[,] fullBoard, int difficulty, SudokuSolver solver)
    {
        int[,] puzzle = (int[,])fullBoard.Clone();
        int targetRemove = difficulty;

        List<Vector2Int> positions = GetAllPositions();
        ShuffleList(positions);

        int removed = 0;
        int attempts = 0;
        int maxAttempts = 200; // 충분한 시도

        foreach (var pos in positions)
        {
            if (removed >= targetRemove) break;
            if (attempts >= maxAttempts) break;

            attempts++;

            // 백업
            int backup = puzzle[pos.x, pos.y];

            if (backup == 0) continue; // 이미 빈 칸

            // 제거 시도
            puzzle[pos.x, pos.y] = 0;

            // 유일 해 유지 확인
            if (solver.HasUniqueSolution(puzzle))
            {
                removed++;
            }
            else
            {
                // 복원 (유일 해가 깨지면)
                puzzle[pos.x, pos.y] = backup;
            }
        }

        Debug.Log($"목표: {targetRemove}칸, 실제: {removed}칸 제거 (시도: {attempts}회)");
        return puzzle;
    }

    List<Vector2Int> GetAllPositions()
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                positions.Add(new Vector2Int(row, col));
            }
        }
        return positions;
    }

    void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}
