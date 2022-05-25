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

    //Двумерный массив из клеток (игровое поле)
    public static Cell[,] field { get; private set; }

    //Количество клеток на игровом поле
    static public int cellCount { get; private set; }

    //Настройка размеров
    const float cameraToField = 1.25f; // 8 изначальный размер orthographicsize - 4 кол-во клеток

    //Размер одной клетки
    public static float transformCellSize;
    //Отступ между клетками
    public static float indention; // 0.25f
    //Размеры поля
    public static float transformFieldsize;
    //Значение для отступа, чтобы переместить поле в центр экрана
    public static float centering;

    //Количество плиток на поле
    public int tilesCount;

    //Функция позволяет изменить количество клеток на поле.
    public void SetFieldSize(int _sign)
    {
        if ((_sign == -1 && cellCount > 4) || (_sign == 1 && cellCount < 7))
            cellCount += _sign;
        SetMapping();
        //Прошлое поле уничтожается
        DestroyField();
        //На его месте появляется новое
        CreateField();
    }

    //Функция позволяет изменить тему у всех плиток на поле
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

    //Функция создает поле, на котором появляется 2 плитки (начало игры)
    public void CreateField()
    {
        //Создается поле (cellCount) на (cellCount)
        field = new Cell[cellCount, cellCount];

        tilesCount = 0;

        Vector3 cellPosition;

        //В массиве происходит создание всех клеток поля
        for (int i = 0; i < cellCount; i++)
        {
            for (int j = 0; j < cellCount; j++)
            {
                cellPosition = new Vector3((transformCellSize + indention) * i - centering, (transformCellSize + indention) * j - centering, -1);
                field[i, j] = Instantiate(cellPrefab, cellPosition, new Quaternion(0, 0, 0, 0)).GetComponent<Cell>();
                field[i, j].Initialize(new Vector2Int(i, j));
                field[i, j].gameObject.transform.SetParent(this.gameObject.transform);
            }
        }

        if (!LoadField())
        {
            //Если прошлая игра не смогла загрузиться то, тогда появляется 2 плитки одна из них может принимать значения: 2 либо 4; другая только 2.
            CreateTile();
            CreateTile();
        }
    }

    //Функция позволяет загрузить поле из загрузочного файла. Если загрить поле не удается, то функция возвращает значение false
    private bool LoadField()
    {
        int gameMode = PlayerPrefs.GetInt($"GameMode {cellCount} exists");

        //Если игра с количеством клеток cellCount существует
        if (gameMode == 1)
        {
            int tileValueInCell;

            for (int i = 0; i < cellCount; i++)
            {
                for (int j = 0; j < cellCount; j++)
                {
                    tileValueInCell = PlayerPrefs.GetInt($"GameMode {cellCount} Cell[{i}, {j}]");
                    //Если полученное значние tileValueInCell больше нуля, то происходит создание плитки на поле в клетке с координаами i, j
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

    //Функция позволяет сохранить игровое поле
    public void SaveField()
    {
        PlayerPrefs.SetInt($"GameMode {cellCount} exists", 1);
        for (int i = 0; i < cellCount; i++)
        {
            for (int j = 0; j < cellCount; j++)
            {
                //Если текущая клетка занята, то в PlayerPrefs с названием GameMode {cellCount} Cell[{i}, {j}] записывается значение плитки
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

    //Функция позволяет сбросить (очистить) поле от плиток
    public void ResetField()
    {
        tilesCount = 0;
        ResetField(ResetFieldType.retry);

        CreateTile();
        CreateTile();
    }

    //Функция позволяет уничтожить поле
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

    //Функция позволяет сбросить (очистить) поле от плиток
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

    //Функция позволяет вернуть положение плиток, соответствующее прошлому ходу
    public void ReturnToLastTurn()
    {
        var lastTurnTiles = ActionsManager.lastTurnTiles;

        //Сброс поля
        ResetField(ResetFieldType.returnToLastTurn);

        //Создание плиток в определенных местах поля
        foreach (var tile in lastTurnTiles)
        {
            CreateTile(tile.Item1, tile.Item2);
            PlayerPrefs.SetInt($"GameMode {cellCount} Cell[{tile.Item1.x}, {tile.Item1.y}]", tile.Item2);
        }

        //Количество плиток равно количеству плиток на прошлом ходу
        tilesCount = lastTurnTiles.Count;

        lastTurnTiles.Clear();
    }

    //Функция позволяет создать плитку в случайном месте на поле со значением 2 либо 4
    public void CreateTile()
    {
        Vector2Int _tilePosition = new Vector2Int(UnityEngine.Random.Range(0, cellCount), UnityEngine.Random.Range(0, cellCount));
        //Если в позиции _tilePosition клетка пуста
        if (field[_tilePosition.x, _tilePosition.y].isFree())
        {
            //Создание новой плитки _newTile в позиции _tilePosition
            Tile _newTile = Instantiate(tilePrefab, field[_tilePosition.x, _tilePosition.y].transform).GetComponent<Tile>();
            _newTile.Initialize(_tilePosition, UserInterface.instance.theme);

            field[_tilePosition.x, _tilePosition.y].tile = _newTile;
            _newTile.transform.SetParent(field[_tilePosition.x, _tilePosition.y].transform);

            tilesCount++;
        }
        else
        {
            CreateTile();
        }
    }

    //Функция позволяет создать плитку со значением _value в позиции _position
    public void CreateTile(Vector2Int _position, int _value)
    {
        if (field[_position.x, _position.y].isFree())
        {
            //Создание новой плитки _newTile в позиции _position
            Tile _newTile = Instantiate(tilePrefab, field[_position.x, _position.y].transform).GetComponent<Tile>();
            _newTile.Initialize(_value, _position, UserInterface.instance.theme);

            field[_position.x, _position.y].tile = _newTile;
            _newTile.transform.SetParent(field[_position.x, _position.y].transform);

            tilesCount++;
        }
        else
        {
            CreateTile();
        }
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
        CreateField();
    }
}
