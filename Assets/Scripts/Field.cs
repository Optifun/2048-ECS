using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerOfTwo
{
    public int value { get; private set; }

    public PowerOfTwo()
    {
        value = 2;
    }

    public PowerOfTwo(int _pow)
    {
        value = (int)Mathf.Pow(2, _pow);
    }
}


public class Field : MonoBehaviour
{
    public static Field instance;
    public GameObject cellPrefab;
    public GameObject tilePrefab;
    Cell[,] field = new Cell[4, 4];
    //List<Tile> tiles = new List<Tile>();

    private int tileCount;
    private int mergingSummary;

    static float size = 2;
    static float indention = 0.25f;
    static float fieldsize = (size * 4 + indention * 5) / 2;
    static float centering = fieldsize - size / 2 - indention;

    //Функция создает поле 4х4
    private void CreateField()
    {
        this.gameObject.transform.localScale = new Vector3(fieldsize, fieldsize);
        Vector3 cellPosition;

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                cellPosition = new Vector3((size + indention) * i - centering, (size + indention) * j - centering, 0);
                field[i, j] = Instantiate(cellPrefab, cellPosition, new Quaternion(0, 0, 0, 0)).GetComponent<Cell>();
                field[i, j].Initialize(new Vector2Int(i, j));
                field[i, j].gameObject.transform.SetParent(this.gameObject.transform);
            }
        }
    }

    //Функция создает поле и спавнит 2 плитки (начало игры)
    public void StartGame()
    {
        tileCount = 0;
        //Создается поле 4х4
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
        int gameMode = PlayerPrefs.GetInt("GameMode");

        if (gameMode == 4)
        {
            int tileValueInCell;

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    tileValueInCell = PlayerPrefs.GetInt($"Cell[{i}, {j}]");
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
        PlayerPrefs.SetInt("GameMode", 4);
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (field[i, j].isFree() == false)
                {
                    PlayerPrefs.SetInt($"Cell[{i}, {j}]", field[i, j].tile.value);
                }
                else
                {
                    PlayerPrefs.SetInt($"Cell[{i}, {j}]", -1);
                }
            }
        }
        PlayerPrefs.Save();
    }

    public void ResetGame()
    {
        tileCount = 0;
        ResetField();
        CreateTile();
        CreateTile();
    }

    private void ResetField()
    {
        PlayerPrefs.SetInt("GameMode", -1);

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (field[i, j].isFree() == false)
                {
                    Destroy(field[i, j].tile.gameObject);
                    field[i, j].Clear();
                }

                PlayerPrefs.SetInt($"Cell[{i}, {j}]", -1);
            }
        }
        
        PlayerPrefs.Save();
    }

    private void CreateTile()
    {
        Vector2Int _tilePosition = new Vector2Int(Random.Range(0, 4), Random.Range(0, 4));
        if (field[_tilePosition.x, _tilePosition.y].isFree())
        {
            //Vector3 _tileGlobalPosition;
            //_tileGlobalPosition = new Vector3(field[_tilePosition.x, _tilePosition.y].transform.position.x, field[_tilePosition.x, _tilePosition.y].transform.position.y, -1); 
            Tile _newTile = Instantiate(tilePrefab, field[_tilePosition.x, _tilePosition.y].transform).GetComponent<Tile>();
            _newTile.Initialize(_tilePosition);

            field[_tilePosition.x, _tilePosition.y].tile = _newTile;
            _newTile.transform.SetParent(field[_tilePosition.x, _tilePosition.y].transform);
            //_newTile.transform.localPosition = new Vector3(0, 0 -10);

            tileCount++;
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

            tileCount++;
        }
        else
        {
            CreateTile();
        }
    }

    public void Move(Vector2Int _direction)
    {
        bool _isMoved = false;
        Cell[] _cells = new Cell[4];
        int _previousCellIndex = -1;
        int _currentCellIndex = -1;
        int _index;

        int _signDirection;
        int j;

        mergingSummary = 0;

        if (_direction.x > 0 || _direction.y > 0)
        {
            _signDirection = 1;
        }
        else
        {
            _signDirection = -1;
        }

        for (int i = 0; i < 4; i++)
        {
            _cells = GetLane(i, _direction);
            _previousCellIndex = -1;

            if (_direction.x > 0 || _direction.y > 0)
            {
                j = 3;
            }
            else
            {
                j = 0;
            }

            while (j >= 0 && j <= 3)
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
            if (tileCount == 16)
            {
                InGameUI.instance.GameOver();
            }

            SaveField();
        }
    }

    private Cell[] GetLane(int _k, Vector2Int direction)
    {
        Cell[] _cells = new Cell[4];
        for (int i = 0; i < 4; i++)
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

        while (_nextposition >= 0 && _nextposition <= 3 && _cells[_nextposition].isFree())
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
        tileCount -= 2;
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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
