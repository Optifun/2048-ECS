using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public enum ResetFieldType
{
    retry = 0,
    returnToLastTurn = 1
}

public class Field : MonoBehaviour
{
    public static Field instance;
    public GameObject cellPrefab;
    public GameObject tilePrefab;
    public Cell[,] field { get; private set; }

    //back to last turn
    List<(Vector2Int, int)> lastTurnTiles = new List<(Vector2Int, int)>();

    private Task Movement;

    //Количество клеток на игровом поле
    static public int cellCount { get; private set; }

    //Настройка размеров
    const float cameraToField = 1.777f; // 8 изначальный размер orthographicsize - 4 кол-во клеток

    public static float transformCellSize;
    public static float indention; // 0.25f
    public static float transformFieldsize;
    public static float centering;

    //tiles count on field
    private int tilesCount;
    //tiles that finished their animations (moving, merging, spawning)
    private int tilesReadyCount;
    //total value of the merged tiles
    private int mergingSummary;

    private List<(Tile, Tile)> mergingTiles = new List<(Tile, Tile)>();

    private List<UniTask> _accumulatedMoves = new List<UniTask>();

    public event Action finishMove;

    public void SetFieldSize(int _sign)
    {
        if ((_sign == -1 && cellCount > 4) || (_sign == 1 && cellCount < 7))
            cellCount += _sign;
        SetMapping();
        DestroyField();
        SetField();
    }

    //Функция создает поле (cellCount) х (cellCount)
    private void CreateField()
    {
        Vector3 cellPosition;

        for (int i = 0; i < cellCount; i++)
        {
            for (int j = 0; j < cellCount; j++)
            {
                cellPosition = new Vector3((transformCellSize + indention) * i - centering, (transformCellSize + indention) * j - centering, -1);
                field[i, j] = Instantiate(cellPrefab, cellPosition, new Quaternion(0, 0, 0, 0)).GetComponent<Cell>();
                field[i, j].Initialize(new Vector2Int(i, j));
                field[i, j].gameObject.transform.SetParent(this.gameObject.transform);
                //field[i, j].transform.localPosition = new Vector3(0, 0, -1);
            }
        }
    }

    public void SetTheme(Theme _theme)
    {
        for (int i = 0; i < cellCount; i++)
        {
            for (int j = 0; j < cellCount; j++)
            {
                if (field[i, j].isFree() == false)
                {
                    field[i, j].tile.SetAppearance(_theme);
                }
            }
        }
    }

    //Функция создает поле и спавнит 2 плитки (начало игры)
    public void SetField()
    {
        field = new Cell[cellCount, cellCount];

        tilesCount = 0;
        //Создается поле (cellCount) х (cellCount)
        CreateField();

        if (!LoadGame())
        {
            //Если прошлая игра не смогла загрузиться то, тогда появляется 2 плитки одна из них может принимать значения: 2 либо 4; другая только 2.
            CreateTile();
            CreateTile();
        }
    }

    private bool LoadGame()
    {
        int gameMode = PlayerPrefs.GetInt($"GameMode {cellCount} exists");

        if (gameMode == 1)
        {
            int tileValueInCell;

            for (int i = 0; i < cellCount; i++)
            {
                for (int j = 0; j < cellCount; j++)
                {
                    tileValueInCell = PlayerPrefs.GetInt($"GameMode {cellCount} Cell[{i}, {j}]");
                    if (tileValueInCell > 0)
                    {
                        CreateTile(new Vector2Int(i, j), tileValueInCell);
                    }
                }
            }

            return true;
        }
        else
        {
            return false;
        }
    }

    private void SaveField()
    {
        PlayerPrefs.SetInt($"GameMode {cellCount} exists", 1);
        for (int i = 0; i < cellCount; i++)
        {
            for (int j = 0; j < cellCount; j++)
            {
                if (field[i, j].isFree() == false)
                {
                    PlayerPrefs.SetInt($"GameMode {cellCount} Cell[{i}, {j}]", field[i, j].tile.value);
                }
                else
                {
                    PlayerPrefs.SetInt($"GameMode {cellCount} Cell[{i}, {j}]", 0);
                }
            }
        }
        PlayerPrefs.Save();
    }

    public void ResetGame()
    {
        tilesCount = 0;
        ResetField(ResetFieldType.retry);



        /*
        int _val = 1;
        for (int i = 0; i < cellCount; i++)
        {
            for (int j = 0; j < cellCount; j++)
            {
                if (_val <= 13)
                    CreateTile(new Vector2Int(i, j), (int)Mathf.Pow(2, _val));

                _val++;
            }
        }
        */

        CreateTile();
        CreateTile();
    }

    void DestroyField()
    {
        for (int i = 0; i < field.GetLength(0); i++)
        {
            for (int j = 0; j < field.GetLength(0); j++)
            {
                Destroy(field[i, j].gameObject);
            }
        }
    }

    private void ResetField(ResetFieldType _type)
    {
        if (_type == 0)
            PlayerPrefs.SetInt($"GameMode {cellCount} exists", 0);

        for (int i = 0; i < cellCount; i++)
        {
            for (int j = 0; j < cellCount; j++)
            {
                if (field[i, j].isFree() == false)
                {
                    Destroy(field[i, j].tile.gameObject);
                    field[i, j].Clear();
                }

                PlayerPrefs.SetInt($"GameMode {cellCount} Cell[{i}, {j}]", 0);
            }
        }
        
        PlayerPrefs.Save();
    }

    public void ReturnToLastTurn()
    {
        ResetField(ResetFieldType.returnToLastTurn);

        foreach (var tile in lastTurnTiles)
        {
            CreateTile(tile.Item1, tile.Item2);
            PlayerPrefs.SetInt($"GameMode {cellCount} Cell[{tile.Item1.x}, {tile.Item1.y}]", tile.Item2);
        }

        tilesCount = lastTurnTiles.Count;

        lastTurnTiles.Clear();
    }

    private void CreateTile()
    {
        Vector2Int _tilePosition = new Vector2Int(UnityEngine.Random.Range(0, cellCount), UnityEngine.Random.Range(0, cellCount));
        if (field[_tilePosition.x, _tilePosition.y].isFree())
        {
            Tile _newTile = Instantiate(tilePrefab, field[_tilePosition.x, _tilePosition.y].transform).GetComponent<Tile>();
            _newTile.Initialize(_tilePosition, InGameUI.instance.theme);

            field[_tilePosition.x, _tilePosition.y].tile = _newTile;
            _newTile.transform.SetParent(field[_tilePosition.x, _tilePosition.y].transform);

            tilesCount++;
        }
        else
        {
            CreateTile();
        }
    }

    private void CreateTile(Vector2Int _position, int _value)
    {
        if (field[_position.x, _position.y].isFree())
        {
            Tile _newTile = Instantiate(tilePrefab, field[_position.x, _position.y].transform).GetComponent<Tile>();
            _newTile.Initialize(_value, _position, InGameUI.instance.theme);

            field[_position.x, _position.y].tile = _newTile;
            _newTile.transform.SetParent(field[_position.x, _position.y].transform);

            tilesCount++;
        }
        else
        {
            CreateTile();
        }
    }

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
        foreach(var (tile1, tile2) in mergingTiles)
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

    private void Awake()
    {
        cellCount = 4;
        
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    //Настройка размеров поля и настройка камеры
    private void SetMapping()
    {
        transformCellSize = 2;
        indention = transformCellSize / 8;
        transformFieldsize = (transformCellSize * cellCount + indention * (cellCount + 1)) / 2;
        centering = transformFieldsize - transformCellSize / 2 - indention;

        Field.instance.gameObject.transform.localScale = new Vector3(transformFieldsize, transformFieldsize);

        Camera.main.orthographicSize = cameraToField * cellCount;
    }

    // Start is called before the first frame update
    void Start()
    {
        cellCount = 4;
        SetMapping();
        SetField();
    }

    // Update is called once per frame
    void Update()
    {
    }
}
