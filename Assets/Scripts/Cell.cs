using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public Tile tile { get; set; }
    public Vector2Int position { get; private set; }

    public void Initialize(Vector2Int _position)
    {
        tile = null;
        position = _position;
        this.gameObject.name = $"Cell ({position.x},{position.y})";
    }

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

    public void Clear()
    {
        tile = null;
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
