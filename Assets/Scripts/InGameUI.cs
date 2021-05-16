using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Theme
{
    light = 0,
    dark = 1,
}

public class InGameUI : MonoBehaviour
{
    public static InGameUI instance;

    public Text scoreText;
    public int scoreAmount { get; private set; }
    private int previousTurnScoreAmount;
    public Text bestText;
    public int bestAmount { get; private set; }
    public GameObject popupTextPrefab;
    public GameObject panel;
    public Button BackArrow;
    public Text fieldNameText;
    public Button DecreaseCellCountButton;
    public Button IncreaseCellCountButton;
    public Button ThemeButton;

    //Текущая тема
    public Theme theme;

    //Спрайты иконок для кнопки смены темы
    public Sprite light;
    public Sprite dark;

    //Цвета заднего фона
    public Color lightBackground; //FAE6D2
    public Color darkBackground; //242830

    Animator panelAnimator;
    Animator scoreAnimator;

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

    public void IncreaseScore(int _amount)
    {
        previousTurnScoreAmount = scoreAmount;
        scoreAmount += _amount;
        scoreText.text = scoreAmount.ToString();

        SpawnPopUpText(_amount);

        PlayerPrefs.SetInt($"GameMode {Field.cellCount} Score", scoreAmount);
        PlayerPrefs.Save();

        BackArrow.interactable = true;
    }

    public void EndGame()
    {
        panel.SetActive(true);
        if (bestAmount < scoreAmount)
        {
            bestAmount = scoreAmount;
            bestText.text = bestAmount.ToString();
            PlayerPrefs.SetInt($"GameMode {Field.cellCount} Best", bestAmount);
            PlayerPrefs.Save();
        }
        //TODO: transition
    }

    public void Retry()
    {
        ResetUI();
        StartGame();
        Field.instance.ResetGame();
        panel.SetActive(false);

        //TODO: retry
    }

    public void ResetUI()
    {
        scoreAmount = 0;
        PlayerPrefs.SetInt($"GameMode {Field.cellCount} Score", 0);
        scoreText.text = scoreAmount.ToString();
    }

    public void StartGame()
    {
        int amount = PlayerPrefs.GetInt($"GameMode {Field.cellCount} Best");
        if (amount > 0)
        {
            bestAmount = amount;
        }
        else
        {
            bestAmount = 0;
        }
        bestText.text = bestAmount.ToString();

        amount = PlayerPrefs.GetInt($"GameMode {Field.cellCount} Score");
        if (amount > 0)
        {
            scoreAmount = amount;
        }
        else
        {
            scoreAmount = 0;
        }
        scoreText.text = scoreAmount.ToString();

        theme = (Theme)PlayerPrefs.GetInt("Game theme");

        BackArrow.interactable = false;
    }

    public void ChangeTheme()
    {
        if (theme == Theme.dark)
        {
            ThemeButton.image.sprite = dark;
        }
        else
        {
            ThemeButton.image.sprite = light;
        }

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

        PlayerPrefs.SetInt("Game theme", (int)theme);
        Field.instance.SetTheme(theme);
    }

    public void IncreaseCellCount()
    {
        ChangeField(1);
        //Invoke GameEnvironment (там увеличивается кол-во клеток в поле и загружается новое поле)
    }

    public void DecreaseCellCount()
    {
        ChangeField(-1);
        //Invoke GameEnvironment (там увеличивается кол-во клеток в поле и загружается новое поле)
    }

    public void ChangeField(int _sign)
    {
        int cellCount = Field.cellCount + _sign;
        fieldNameText.text = $"{cellCount}x{cellCount}";
        Field.instance.SetFieldSize(_sign);
        StartGame();

        if (cellCount >= 6)
        {
            IncreaseCellCountButton.interactable = false;
        }
        else if (cellCount <= 4)
        {
            DecreaseCellCountButton.interactable = false;
        }
        else
        {
            IncreaseCellCountButton.interactable = true;
            DecreaseCellCountButton.interactable = true;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        DecreaseCellCountButton.interactable = false;
        StartGame();
    }

    public void SpawnPopUpText(int _amount)
    {
        PopUp _popUpText = Instantiate(popupTextPrefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0)).GetComponent<PopUp>();
        _popUpText.gameObject.transform.SetParent(scoreText.gameObject.transform);
        _popUpText.gameObject.transform.localPosition = new Vector3(25f, 9.5f);
        _popUpText.Initialize(_amount);
    }

    public void ReturnToLastTurn()
    {
        IncreaseScore(previousTurnScoreAmount - scoreAmount);
        Field.instance.ReturnToLastTurn();
        BackArrow.interactable = false;
    }

    public void InteractBackArrow()
    {
        BackArrow.interactable = true;
    }

    public void CloseGame()
    {
        Application.Quit();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
