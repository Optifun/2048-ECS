using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEnvironment : MonoBehaviour
{/*
    public static GameEnvironment instance;
    public Color lightBackground; //FAE6D2
    public Color darkBackground; //242830
    private int score;
    private int best;

    public static int cellCount { get; private set; }

    public Theme theme;

    //for scaling
    const float camToField = 1.777f; // 8 изначальный размер orthographicsize - 4 кол-во клеток

    //for scaling
    public static float transformCellSize;
    public static float indention; // 0.25f
    public static float transformFieldsize;
    public static float centering;

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
        if ((_sign == -1 && cellCount > 4) || (_sign == 1 && cellCount < 7))
            cellCount += _sign;
        SetMapping();
        Field.instance.SetCellCount();
    }

    public void Move(Vector2Int _direction)
    {
        Field.instance.Move(_direction);
    }

    public void ChangeTheme()
    {
        if (theme == Theme.light)
        {
            Camera.main.backgroundColor = darkBackground;
            theme = Theme.dark;
        }
        else
        {
            Camera.main.backgroundColor = lightBackground;
            theme = Theme.light;
        }

        Field.instance.EstablishTheme(theme);
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

    private void SetMapping()
    {
        transformCellSize = 2;
        indention = transformCellSize / 8;
        transformFieldsize = (transformCellSize * cellCount + indention * (cellCount + 1)) / 2;
        centering = transformFieldsize - transformCellSize / 2 - indention;

        Field.instance.gameObject.transform.localScale = new Vector3(transformFieldsize, transformFieldsize);

        Camera.main.orthographicSize = camToField * cellCount;
        Camera.main.backgroundColor = lightBackground;
    }

    // Start is called before the first frame update
    void Start()
    {
        score = 0;
        best = 0;
        cellCount = 4;
        theme = Theme.light;
        SetMapping();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    */
}
