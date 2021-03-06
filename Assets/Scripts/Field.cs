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

    //fieldSize
    static public int cellCount { get; private set; }
    //tiles count on field
    private int tilesCount;
    //tiles that finished their animations (moving, merging, spawning)
    private int tilesReadyCount;
    //total value of the merged tiles
    private int mergingSummary;

    //for scaling
    const float camToField = 1.6f; // 8 изначальный размер orthographicsize - 4 кол-во клеток

    //for scaling
    static float size = 2;
    static float indention = size / 8; // 0.25f
    static float fieldsize;
    static float centering;

    public void SetCellCount(int _cellCount)
    {
        DestroyField();
        cellCount = _cellCount;
        StartGame();
    }

    //Функция создает поле (cellCount) х (cellCount)
    private void CreateField()
    {
        Vector3 cellPosition;

        for (int i = 0; i < cellCount; i++)
        {
            for (int j = 0; j < cellCount; j++)
            {
                cellPosition = new Vector3((size + indention) * i - centering, (size + indention) * j - centering, -1);
                field[i, j] = Instantiate(cellPrefab, cellPosition, new Quaternion(0, 0, 0, 0)).GetComponent<Cell>();
                field[i, j].Initialize(new Vector2Int(i, j));
                field[i, j].gameObject.transform.SetParent(this.gameObject.transform);
                //field[i, j].transform.localPosition = new Vector3(0, 0, -1);
            }
        }
    }

    //Функция создает поле и спавнит 2 плитки (начало игры)
    public void StartGame()
    {
        fieldsize = (size * cellCount + indention * (cellCount + 1)) / 2;
        centering = fieldsize - size / 2 - indention;

        this.gameObject.transform.localScale = new Vector3(fieldsize, fieldsize);

        Camera.main.orthographicSize = camToField * cellCount;

        field = new Cell[cellCount, cellCount];

        tilesCount = 0;
        //Создается поле (cellCount) х (cellCount)
        CreateField();

        if (!LoadGame())
        {
            //Если прошлая игра не смогла загрузиться то, тогда появляется 2 плитки одна из них может принимать значения: 2 либо 4; другая только 2.
            CreateTile(new Vector2Int(0, 0), 256);
            CreateTile();

            /*
            int n = 1;
            int value = 2;
            for (int i = 0; i < cellCount; i++)
            {
                for (int j = 0; j < cellCount; j++)
                {
                    if (n <= 13)
                    {
                        CreateTile(new Vector2Int(i, j), value);
                        value *= 2;
                        n++;
                    }
                }
            }
            */
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
        CreateTile();
        CreateTile();

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
    }

    void DestroyField()
    {
        for (int i = 0; i < cellCount; i++)
        {
            for (int j = 0; j < cellCount; j++)
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
            //Vector3 _tileGlobalPosition;
            //_tileGlobalPosition = new Vector3(field[_tilePosition.x, _tilePosition.y].transform.position.x, field[_tilePosition.x, _tilePosition.y].transform.position.y, -1); 
            Tile _newTile = Instantiate(tilePrefab, field[_tilePosition.x, _tilePosition.y].transform).GetComponent<Tile>();
            _newTile.Initialize(_tilePosition);

            field[_tilePosition.x, _tilePosition.y].tile = _newTile;
            _newTile.transform.SetParent(field[_tilePosition.x, _tilePosition.y].transform);
            //_newTile.transform.localPosition = new Vector3(0, 0 -1);

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
            //Vector3 _tileGlobalPosition;
            //_tileGlobalPosition = new Vector3(field[_position.x, _position.y].transform.position.x, field[_position.x, _position.y].transform.position.y, -10);
            Tile _newTile = Instantiate(tilePrefab, field[_position.x, _position.y].transform).GetComponent<Tile>();
            _newTile.Initialize(_value, _position);

            field[_position.x, _position.y].tile = _newTile;
            _newTile.transform.SetParent(field[_position.x, _position.y].transform);
            //_newTile.transform.localPosition = new Vector3(0, 0, -10);

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
            if (tilesCount >= cellCount*cellCount && isExistNextMove() == false)
            {
                InGameUI.instance.GameOver();
            }

            InGameUI.instance.InteractBackArrow();

            SaveField();
        }
    }

    private Cell[] GetLane(int _k, Vector2Int direction)
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

    private Vector2Int GetAvailablePosition(Vector2Int _tilePosition, Vector2Int _direction)
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

    private bool isMergiable(Cell _fromCell, Cell _toCell)
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

    private bool isMergiable(int _from, int _to, Cell[] _cells)
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

    //                     направление   сливаемое  сливаемый             клетки
    private void Merge(int _direction, int _merged, int _mergeable, ref Cell[] _cells)
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

    private void MoveTile(int _from, int _to, ref Cell[] _cells)
    {
        Tile movingCellTile = _cells[_from].tile;
        _cells[_to].tile = movingCellTile;
        movingCellTile.gameObject.transform.SetParent(_cells[_to].transform);
        movingCellTile.Set(_cells[_to].position);
        _cells[_from].Clear();
        //TODO: Заменить данную строку на другую функцию, выполняющую планое перемещение всех клеток одновременно
    }

    private int MoveTileToAvailableCell(int _direction, int i, Cell[] _cells)
    {
        int _availablePosition = GetAvailablePosition(_direction, i, ref _cells);
        if (i != _availablePosition)
            MoveTile(i, _availablePosition, ref _cells);
        
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
            verticalLane = GetLane(i, new Vector2Int(0, 1));
            horizontalLane = GetLane(i, new Vector2Int(1, 0));

            if (hasMergiableTilesInLane(verticalLane) || hasMergiableTilesInLane(horizontalLane))
                return true;
        }

        return false;
    }

    private void Awake()
    {
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

    public void increaseReady()
    {
        tilesReadyCount++;
        if (tilesReadyCount == tilesCount)
        {
            //Запустить следующий этап, либо закончить этот в Task'е
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        cellCount = 4;

        StartGame();
    }

    // Update is called once per frame
    void Update()
    {
    }
}
