using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionCalculations : MonoBehaviour
{


    //calculation of the next move and its visualization
    public void Move(Vector2Int _direction)
    {
        bool _isMoved = false;
        _accumulatedMoves.Clear();
        Cell[] _cells = new Cell[cellCount];
        int _previousCellIndex = -1;
        int _currentCellIndex = -1;
        int _index;

        int _signDirection;
        int j;

        lastTurnTiles.Clear();
        mergingSummary = 0;

        if (_direction.x > 0 || _direction.y > 0)
        {
            _signDirection = 1;
        }
        else
        {
            _signDirection = -1;
        }

        for (int i = 0; i < cellCount; i++)
        {
            _cells = GetCellsInRow(i, _direction);
            _previousCellIndex = -1;

            if (_direction.x > 0 || _direction.y > 0)
            {
                j = cellCount - 1;
            }
            else
            {
                j = 0;
            }

            while (j >= 0 && j <= (cellCount - 1))
            {
                if (_previousCellIndex == -1 && _cells[j].isFree() == false)
                {
                    lastTurnTiles.Add((_cells[j].position, _cells[j].tile.value));

                    _previousCellIndex = j;
                    _index = MoveTileToAvailableCell(_signDirection, _previousCellIndex, _cells);

                    if (_index != _previousCellIndex)
                    {
                        _isMoved = true;
                    }

                    _previousCellIndex = _index;
                }
                else
                {
                    _currentCellIndex = j;
                    if (_cells[_currentCellIndex].isFree() == false)
                    {
                        lastTurnTiles.Add((_cells[j].position, _cells[j].tile.value));

                        if (isMergiable(_currentCellIndex, _previousCellIndex, _cells) == true)
                        {
                            MergeTiles(_signDirection, _previousCellIndex, _currentCellIndex, ref _cells);
                            _previousCellIndex = -1;
                            _isMoved = true;
                        }
                        else
                        {
                            _index = MoveTileToAvailableCell(_signDirection, _currentCellIndex, _cells);

                            if (_currentCellIndex != _index)
                            {
                                _isMoved = true;
                            }

                            _currentCellIndex = _index;

                            _previousCellIndex = _currentCellIndex;
                        }
                    }
                }
                j -= _signDirection;
            }
            //TODO: работа с линией с помощью измененных функций merge, move и т.д.
        }
        MoveResult(_isMoved);
    }

    private async void MoveResult(bool _isMoved)
    {
        await UniTask.WhenAll(_accumulatedMoves);
        Merge();
        //Если произошло движение
        if (_isMoved)
        {
            //Если сумма номиналов объединенных плиток > 0 (т.е. были объединенные плитки в этой ходу)
            if (mergingSummary > 0)
            {
                await UniTask.Delay(150);
                InGameUI.instance.IncreaseScore(mergingSummary);
            }

            //После смещения всех плиток появляется новая плитка
            CreateTile();
            await UniTask.Delay(150);
            SaveField();

            //TODO: узнать есть ли какой-то ход для того, чтобы "разрулить" ситуацию
            if (tilesCount >= cellCount * cellCount && isExistNextMove() == false)
            {
                PlayerPrefs.SetInt($"GameMode {cellCount} exists", 0);
                InGameUI.instance.EndGame();
            }

            InGameUI.instance.InteractBackArrow();
        }
        finishMove?.Invoke();
    }

    private Cell[] GetCellsInRow(int _k, Vector2Int direction)
    {
        Cell[] _cells = new Cell[cellCount];
        for (int i = 0; i < cellCount; i++)
        {
            if (direction.x != 0)
            {
                _cells[i] = field[i, _k];
            }
            else
            {
                _cells[i] = field[_k, i];
            }
        }

        return _cells;
    }

    private int GetAvailablePosition(int _direction, int i, ref Cell[] _cells)
    {
        int _currentPosition = i;
        int _nextposition = i + _direction;

        while (_nextposition >= 0 && _nextposition <= (cellCount - 1) && _cells[_nextposition].isFree())
        {
            _currentPosition = _nextposition;
            _nextposition = _nextposition + _direction;
        }

        return _currentPosition;
    }

    private bool isMergiable(int _from, int _to, Cell[] _cells)
    {
        return _cells[_from].tile.value == _cells[_to].tile.value;
    }

    //                     направление   сливаемое  сливаемый             клетки
    private void MergeTiles(int _direction, int _merged, int _mergeable, ref Cell[] _cells)
    {
        //2 плитки перемещаются в 1
        int _to = GetAvailablePosition(_direction, _merged, ref _cells);

        Tile _mergedTile = _cells[_merged].tile;
        Tile _mergeableTile = _cells[_mergeable].tile;
        mergingTiles.Add((_mergedTile, _mergeableTile));

        MoveTile(_merged, _to, ref _cells);
        MoveTile(_mergeable, _to, ref _cells);
    }

    private void Merge()
    {
        foreach (var (tile1, tile2) in mergingTiles)
        {
            int _doubleValue = tile1.value * 2;
            field[tile1.position.x, tile1.position.y].tile = null;
            CreateTile(tile1.position, _doubleValue);
            Destroy(tile1.gameObject);
            Destroy(tile2.gameObject);
            tilesCount -= 2;
            mergingSummary += _doubleValue;
        }

        mergingTiles.Clear();
    }

    private void MoveTile(int _from, int _to, ref Cell[] _cells)
    {
        Tile movingCellTile = _cells[_from].tile;
        _cells[_to].tile = movingCellTile;
        movingCellTile.gameObject.transform.SetParent(_cells[_to].transform);
        var task = movingCellTile.Set(_cells[_to].position);
        _accumulatedMoves.Add(task);
        _cells[_from].Clear();
        //TODO: Заменить данную строку на другую функцию, выполняющую планое перемещение всех клеток одновременно
    }

    private int MoveTileToAvailableCell(int _direction, int _currentPosition, Cell[] _cells)
    {
        int _availablePosition = GetAvailablePosition(_direction, _currentPosition, ref _cells);
        if (_currentPosition != _availablePosition)
            MoveTile(_currentPosition, _availablePosition, ref _cells);

        return _availablePosition;
    }

    public bool hasMergiableTilesInLane(Cell[] _lane)
    {
        int _previousCellIndex = -1;

        for (int j = 0; j < cellCount; j++)
        {
            if (_previousCellIndex == -1 && _lane[j].isFree() == false)
            {
                _previousCellIndex = j;
            }
            else
            {
                if (_lane[j].isFree() == false)
                {
                    if (isMergiable(j, _previousCellIndex, _lane))
                    {
                        return true;
                    }
                    else
                    {
                        _previousCellIndex = j;
                    }
                }
            }
        }

        return false;
    }

    public bool isExistNextMove()
    {
        Cell[] verticalLane;
        Cell[] horizontalLane;

        for (int i = 0; i < cellCount; i++)
        {
            verticalLane = GetCellsInRow(i, new Vector2Int(0, 1));
            horizontalLane = GetCellsInRow(i, new Vector2Int(1, 0));

            if (hasMergiableTilesInLane(verticalLane) || hasMergiableTilesInLane(horizontalLane))
                return true;
        }

        return false;
    }
}
