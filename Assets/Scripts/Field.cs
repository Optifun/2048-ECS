using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Field : MonoBehaviour
{
    GameObject cellPrefab;
    GameObject tilePrefab;
    Cell[,] field = new Cell[4, 4];
    //List<Tile> tiles = new List<Tile>();

    //Функция создает поле 4х4
    private void CreateField()
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                Transform cellPosition = this.transform;
                cellPosition.position = new Vector2(i * 5, j * 5);

                field[i, j] = Instantiate(cellPrefab, cellPosition);
            }
        }
    }

    //Функция создает поле и спавнит 2 плитки (начало игры)
    public void StartGame()
    {
        //Создается поле 4х4
        CreateField();
        //Появляется 2 плитки одна из них может принимать значения: 2 либо 4; другая только 2.
        CreateTile(1);
        CreateTile(Random.Range(1, 3));
    }

    private void CreateTile(int _value)
    {
        Vector2Int _tilePosition = new Vector2Int(Random.Range(0, 4), Random.Range(0, 4));
        if (field[_tilePosition.x, _tilePosition.y].isFree())
        {
            Tile _newTile = Instantiate(tilePrefab, field[_tilePosition.x, _tilePosition.y].transform).GetComponent<Tile>();
            _newTile.Initialize(_value, _tilePosition);
            
        }
        else
        {
            CreateTile(_value);
        }
    }

    public void Slide(Vector2 _direction)
    {
        
    }

    private void SetTilePosition(Vector2Int _fromPosition, Vector2Int _toPosition)
    {
        field[_position.x, _position.y].tile
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
