using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public Tile tile { get; set; }
    //Позиция клетки. Совпадает с индексами клетки в двумерном массиве поля
    public Vector2Int position { get; private set; }

    //Функция позволяет задать позицию клетки
    public void Initialize(Vector2Int _position)
    {
        tile = null;
        position = _position;
        this.gameObject.name = $"Cell ({position.x},{position.y})";
    }

    //Функция позволяет узнать то, есть ли у клетки плитка
    public bool isFree()
    {
        if (tile == null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //Функция позволяет очистить клетку от плитки
    public void Clear()
    {
        tile = null;
    }
}
