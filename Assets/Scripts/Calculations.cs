using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    static class Calculations
    {
        //Перемещение все плиток в указанном направлении
        //calculation of the next move and its visualization
        public static Cell[,] Move(Vector2Int _direction, out bool _isMoved)
        {
            bool _isMoved = false;
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
                _cells = GetLane(i, _direction);
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
                    /*
                    if (_cells[j].isFree() == false)
                    {
                        if (_previousCellIndex == -1)
                        {
                            _previousCellIndex = j;
                            _previousCellIndex = MoveTileToAvailableCell(_signDirection, _previousCellIndex, _cells);
                        }
                        else
                        {
                            _currentCellIndex = j;

                            if (isMergiable(_currentCellIndex, _previousCellIndex, _cells) == true)
                            {
                                Merge(_signDirection, _currentCellIndex, _previousCellIndex, ref _cells);
                                _previousCellIndex = -1;
                            }

                            MoveTileToAvailableCell(_signDirection, _currentCellIndex, _cells);
                        }


                        _previousCellIndex = _currentCellIndex;
                    }
                    */
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
                                Merge(_signDirection, _previousCellIndex, _currentCellIndex, ref _cells);
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

            //Если произошло движение
            if (_isMoved)
            {
                //После смещения всех плиток появляется новая плитка
                CreateTile();

                if (mergingSummary > 0)
                {
                    InGameUI.instance.IncreaseScore(mergingSummary);
                }

                //TODO: узнать есть ли какой-то ход для того, чтобы "разрулить" ситуацию
                if (tilesCount >= cellCount * cellCount && isExistNextMove() == false)
                {
                    InGameUI.instance.GameOver();
                }

                InGameUI.instance.InteractBackArrow();

                SaveField();
            }
        }

        //Взять линию из поля
        private static Cell[] GetLane(int _k, Vector2Int direction)
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

        //Найти свободное место для плитки

        //Переместить плитку
        private static void MoveTile(int _from, int _to, ref Cell[] _cells)
        {
            Tile movingCellTile = _cells[_from].tile;
            _cells[_to].tile = movingCellTile;
            movingCellTile.gameObject.transform.SetParent(_cells[_to].transform);
            movingCellTile.Set(_cells[_to].position);
            _cells[_from].Clear();
            //TODO: Заменить данную строку на другую функцию, выполняющую планое перемещение всех клеток одновременно
        }

        //Найти подходящее место для плитки при сдвиге
        private static int GetAvailablePosition(int _direction, int i, ref Cell[] _cells)
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

        private static Vector2Int GetAvailablePosition(Vector2Int _tilePosition, Vector2Int _direction)
        {
            Vector2Int _currentPosition = _tilePosition;
            Vector2Int _nextposition = _tilePosition + _direction;
            Cell _nextCell = field[_nextposition.x, _nextposition.y];

            while (_nextCell.isFree())
            {
                _currentPosition = _nextposition;
                _nextposition = _nextposition + _direction;
                _nextCell = field[_nextposition.x, _nextposition.y];
            }

            return _currentPosition;
        }

        //Определить конечную позицию плитки при сдвиге
        private static int MoveTileToAvailableCell(int _direction, int i, Cell[] _cells)
        {
            int _availablePosition = GetAvailablePosition(_direction, i, ref _cells);
            if (i != _availablePosition)
                MoveTile(i, _availablePosition, ref _cells);

            return _availablePosition;
        }

        //Узнать можно ли соединить указанные плитки
        private static bool isMergiable(Cell _fromCell, Cell _toCell)
        {
            if (_fromCell.tile.value == _toCell.tile.value)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool isMergiable(int _from, int _to, Cell[] _cells)
        {
            if (_cells[_from].tile.value == _cells[_to].tile.value)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //Соединить плитки
        private static void Merge(int _direction, int _merged, int _mergeable, ref Cell[] _cells)
        {
            //2 плитки перемещаются в 1
            int _availablePosition1 = GetAvailablePosition(_direction, _merged, ref _cells);
            int _to = _availablePosition1;
            Tile _mergedTile = _cells[_merged].tile;
            Tile _mergeableTile = _cells[_mergeable].tile;

            _mergedTile.gameObject.transform.SetParent(_cells[_to].transform);
            _mergeableTile.gameObject.transform.SetParent(_cells[_to].transform);

            _cells[_merged].Clear();
            _cells[_mergeable].Clear();

            int _doubleValue = _mergedTile.value * 2;
            mergingSummary += _doubleValue;
            CreateTile(_cells[_to].position, _doubleValue);

            _mergedTile.Merge();
            _mergeableTile.Merge();
            tilesCount -= 2;
        }
    }
}
