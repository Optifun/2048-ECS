using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    int value;
    public Vector2Int position { get; private set; }
    Color color;

    public void Initialize(int _value, Vector2Int _position)
    {
        value = _value;
        position = _position;
        //color ...
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
