using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;
using Cysharp.Threading.Tasks;

//Менеджер действий плиток в игре. Действия: переместить, объединить.
public static class ActionsManager
{
    //Список кортежей, состоящих из позиции и значения плитки на прошлом ходе
    public static List<(Vector2Int, int)> lastTurnTiles = new List<(Vector2Int, int)>();

    //Список кортежей, состоящих из двух плиток, которые будут объединены в течении данного хода
    private static List<(Tile, Tile)> mergingTiles = new List<(Tile, Tile)>();

    //Событие, которое наступает вконце хода. Привязано к методу unlockInput в классе GameControls
    public static event Action finishMove;

    //Список задач UniTask, который позволяет отслеживать процесс перемещения плиток
    private static List<UniTask> accumulatedMoves = new List<UniTask>();

    //Суммарное значение объединенных клеток
    private static int mergingSummary;

    //Функция выполняет движение клеток в определенном направлении. Для этого она находит подходящие плитки на поле
    public static void Move(Vector2Int _direction)
    {
        //Количество клеток на поле
        int _cellCount = Field.field.GetLength(0);
        //Переменная, показывающая то, произошло ли движение хотя бы одной плитки на поле в результате хода
        bool _isMoved = false;

        accumulatedMoves.Clear();

        //Ряд клеток
        Cell[] _row;
        //Положение уже рассмотренной (преыдущей) клетки в ряду
        int _previousCellIndex = -1;
        //Положение текущей клетке в ряду
        int _currentCellIndex = -1;

        int _index;
        
        int _signDirection;
        int j;

        //Очистка списка клеток предыдущего хода и сброс значения суммарного значения объединяемых плиток
        lastTurnTiles.Clear();
        mergingSummary = 0;

        //В зависимости от направления переменной _signDirection присваивается "знак" направления
        if (_direction.x > 0 || _direction.y > 0)
        {
            _signDirection = 1;
        }
        else
        {
            _signDirection = -1;
        }

        //Происходит проход по каждому ряду поля
        for (int i = 0; i < _cellCount; i++)
        {
            //Присваивается i-тый ряд клеток
            _row = ActionsManager.GetCellsInRow(i, _direction);
            _previousCellIndex = -1;

            if (_direction.x > 0 || _direction.y > 0)
            {
                j = _cellCount - 1;
            }
            else
            {
                j = 0;
            }

            //Происходит проход по ряду клеток
            while (j >= 0 && j <= (_cellCount - 1))
            {
                //Если на рассмотрении нет клетки и текущая клетка занята плиткой
                if (_previousCellIndex == -1 && _row[j].isFree() == false)
                {
                    lastTurnTiles.Add((_row[j].position, _row[j].tile.value));

                    _previousCellIndex = j;
                    //Перемещение рассматриваемой плитки в подходящую клетку
                    _index = MoveTileToAvailableCell(_signDirection, _previousCellIndex, _row);

                    if (_index != _previousCellIndex)
                    {
                        _isMoved = true;
                    }

                    _previousCellIndex = _index;
                }
                else
                {
                    _currentCellIndex = j;
                    //Если текущая клетка занята плиткой
                    if (_row[_currentCellIndex].isFree() == false)
                    {
                        lastTurnTiles.Add((_row[j].position, _row[j].tile.value));

                        //Если текущая клетка может быть объединена с рассматриваемой клеткой
                        if (ActionsManager.isMergiable(_currentCellIndex, _previousCellIndex, _row) == true)
                        {
                            MergeTiles(_signDirection, _previousCellIndex, _currentCellIndex, ref _row);
                            _previousCellIndex = -1;
                            _isMoved = true;
                        }
                        else
                        {
                            //Перемещение текущей плитки в доступное место
                            _index = MoveTileToAvailableCell(_signDirection, _currentCellIndex, _row);

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
        }
        //Запуск функции для подведения итогов хода
        MoveResult(_isMoved);
    }

    //Функция позволяет подвести итоги хода
    private static async void MoveResult(bool _isMoved)
    {
        //Ожидание окончания передвижения всех плиток
        //await UniTask.WhenAll(accumulatedMoves);

        //Переход на этап объединения плиток
        Merge();

        //Количество плиток на поле
        int _cellCount = Field.field.GetLength(0);

        //Если за время хода произошло движение
        if (_isMoved)
        {
            //Если сумма номиналов объединенных плиток > 0 (т.е. были объединенные плитки в этой ходу)
            if (mergingSummary > 0)
            {
                //Подождать пока анимация объединения выполнится и увеличить счёт на значение mergingSummary
                //await UniTask.Delay(150);
                UserInterface.instance.IncreaseScore(mergingSummary);
            }

            //После объединения всех плиток появляется новая плитка
            Field.instance.CreateTile();
            //Подождать выполнение анимации появления плитки
            //await UniTask.Delay(150);
            //Происходит сохранение поля
            Field.instance.SaveField();

            //Если на поле не осталось свободного места для плиток и следующего хода нет
            if (Field.instance.tilesCount >= _cellCount * _cellCount && ActionsManager.isExistNextMove() == false)
            {
                //Происходит сброс сохранения и появляется панель конца игры
                PlayerPrefs.SetInt($"GameMode {_cellCount} exists", 0);
                UserInterface.instance.EndGame();
            }

            //После всех этих действий кнопка Шага назад становится интерактивной
            UserInterface.instance.InteractBackArrow();
        }
        finishMove?.Invoke();
    }

    //Объединие двух плиток
    private static void MergeTiles(int _direction, int _merged, int _mergeable, ref Cell[] _cells)
    {
        int _to = ActionsManager.GetAvailablePosition(_direction, _merged, _cells);
        Tile _mergedTile = _cells[_merged].tile;
        Tile _mergeableTile = _cells[_mergeable].tile;

        //Добавление объединяемых плиток в специальный список
        mergingTiles.Add((_mergedTile, _mergeableTile));

        //2 плитки перемещаются в 1
        MergeTile(_merged, _to, ref _cells);
        MergeTile(_mergeable, _to, ref _cells);
    }

    private static void MergeTile(int _from, int _to, ref Cell[] _cells)
    {
        Tile movingCellTile = _cells[_from].tile;
        _cells[_to].tile = movingCellTile;
        movingCellTile.gameObject.transform.SetParent(_cells[_to].transform);
        //Добавление задачи по передвижению плитки в accumulatedMoves
        var task = movingCellTile.Merge(_cells[_to]);
        accumulatedMoves.Add(task);

        _cells[_from].Clear();
    }

    //Функция осуществляет этап объединения всех плиток
    private static void Merge()
    {
        foreach (var (tile1, tile2) in mergingTiles)
        {
            int _doubleValue = tile1.value * 2;
            Field.field[tile1.position.x, tile1.position.y].tile = null;
            //Создание новой плитки с удвоенным значением в месте нахождения плиток tile1 и tile2
            Field.instance.CreateTile(tile1.position, _doubleValue);
            //Уничтожение плиток и убирание 2 плиток из tilesCount

            //GameObject.Destroy(tile1.gameObject);
            //GameObject.Destroy(tile2.gameObject);

            Field.instance.tilesCount -= 2;
            mergingSummary += _doubleValue;
        }

        mergingTiles.Clear();
    }

    //Перемещение плитки из позиции _from в позицию _to в массиве клеток _cells
    private static void MoveTile(int _from, int _to, ref Cell[] _cells)
    {
        Tile movingCellTile = _cells[_from].tile;
        _cells[_to].tile = movingCellTile;
        movingCellTile.gameObject.transform.SetParent(_cells[_to].transform);
        //Добавление задачи по передвижению плитки в accumulatedMoves
        var task = movingCellTile.Move(_cells[_to]);
        accumulatedMoves.Add(task);

        _cells[_from].Clear();
    }

    //Перемещение плитки из массива _cells с индексом _currentPosition в направлении _direction в подходящую клетку. Также возвращается номер подходящей клетки
    private static int MoveTileToAvailableCell(int _direction, int _currentPosition, Cell[] _cells)
    {
        int _availablePosition = ActionsManager.GetAvailablePosition(_direction, _currentPosition, _cells);
        //Если текущая позиция не совпадает с подходящей, то производитя перемещение
        if (_currentPosition != _availablePosition)
            MoveTile(_currentPosition, _availablePosition, ref _cells);

        return _availablePosition;
    }

    //Функция возвращает _k-тый ряд клеток поля. Ряд берется горизонтально либо вертикально в зависимости от направления direction
    public static Cell[] GetCellsInRow(int _k, Vector2Int direction)
    {
        int _cellCount = Field.field.GetLength(0);
        Cell[] _cells = new Cell[_cellCount];
        for (int i = 0; i < _cellCount; i++)
        {
            if (direction.x != 0)
            {
                _cells[i] = Field.field[i, _k];
            }
            else
            {
                _cells[i] = Field.field[_k, i];
            }
        }

        return _cells;
    }

    //Возвращается номер доступной клетки из массива _cells для плитки, которая находится в позиции i
    private static int GetAvailablePosition(int _direction, int i, Cell[] _cells)
    {
        int _currentPosition = i;
        int _nextposition = i + _direction;
        int _cellCount = _cells.Length;

        //Пока следующая клетка не выходит за границы _cells и свободна
        while (_nextposition >= 0 && _nextposition <= (_cellCount - 1) && _cells[_nextposition].isFree())
        {
            _currentPosition = _nextposition;
            _nextposition = _nextposition + _direction;
        }

        return _currentPosition;
    }

    //Функция определяет то, могут ли плитки в позициях _from и _to быть объединены
    public static bool isMergiable(int _from, int _to, Cell[] _cells)
    {
        return _cells[_from].tile.value == _cells[_to].tile.value;
    }

    //Функция определяет наличие в массиве клеток _row плитки, которых могут быть объединены
    public static bool hasMergiableTilesInRow(Cell[] _row)
    {
        int _previousCellIndex = -1;

        for (int j = 0; j < Field.field.GetLength(0); j++)
        {
            if (_previousCellIndex == -1 && _row[j].isFree() == false)
            {
                _previousCellIndex = j;
            }
            else
            {
                if (_row[j].isFree() == false)
                {
                    if (isMergiable(j, _previousCellIndex, _row))
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

    //Функция позволяет определить существование следующего хода
    public static bool isExistNextMove()
    {
        Cell[] verticalLane;
        Cell[] horizontalLane;

        for (int i = 0; i < Field.field.GetLength(0); i++)
        {
            verticalLane = GetCellsInRow(i, new Vector2Int(0, 1));
            horizontalLane = GetCellsInRow(i, new Vector2Int(1, 0));

            if (hasMergiableTilesInRow(verticalLane) || hasMergiableTilesInRow(horizontalLane))
                return true;
        }

        return false;
    }
}
