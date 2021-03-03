using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEnvironment : MonoBehaviour
{
    private int score;
    private int best;
    private int fieldSize;

    public void SaveGame()
    {

    }

    public void LoadGame()
    {

    }

    public void ResetGame()
    {
        
    }

    public void IncreaseScore(int _amount)
    {
        score += _amount;

        //invoke UI
    }

    public void SetFieldSize(int _sign)
    {
        if ((_sign == -1 && fieldSize > 4) || (_sign == 1 && fieldSize < 7))
            fieldSize += _sign;
        
    }

    public void Move(Vector2Int _direction)
    {
        Field.instance.Move(_direction);
    }

    // Start is called before the first frame update
    void Start()
    {
        score = 0;
        best = 0;
        fieldSize = 4;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
