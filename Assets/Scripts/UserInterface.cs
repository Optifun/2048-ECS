using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Theme
{
    light = 0,
    dark = 1,
}

public class UserInterface : MonoBehaviour
{
    public static UserInterface instance;

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

    //Функция позволяет увеличить игровой счёт на значение _amount
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

    //Функция отображает панель конца игры
    public void EndGame()
    {
        panel.SetActive(true);

        //Если лучший счёт меньше игрового, то происходит присваивание значения нового лучшего счёта
        if (bestAmount < scoreAmount)
        {
            bestAmount = scoreAmount;
            bestText.text = bestAmount.ToString();
            PlayerPrefs.SetInt($"GameMode {Field.cellCount} Best", bestAmount);
            PlayerPrefs.Save();
        }

        //Сбрасывается значение игрового счета
        PlayerPrefs.SetInt($"GameMode {Field.cellCount} Score", 0);
    }

    //Функция позволяет начать новую игру
    public void Retry()
    {
        ResetScore();
        StartGame();
        Field.instance.ResetField();
        panel.SetActive(false);
    }

    //Функция сбрасывает значение игрового счёта
    public void ResetScore()
    {
        scoreAmount = 0;
        PlayerPrefs.SetInt($"GameMode {Field.cellCount} Score", 0);
        scoreText.text = scoreAmount.ToString();
    }

    //Функция отвечает за начало игры
    public void StartGame()
    {
        //Происходит подгрузка значения текущего счета, лучшего счёта и текущей темы оформления
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
        if (theme == Theme.light)
        {
            ThemeButton.image.sprite = dark;
            Camera.main.backgroundColor = lightBackground;
        }
        else
        {
            ThemeButton.image.sprite = light;
            Camera.main.backgroundColor = darkBackground;
        }

        BackArrow.interactable = false;
    }

    //Функция меняет текущую тему
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

    //Функция увеличивает количество клеток на 1
    public void IncreaseCellCount()
    {
        ChangeField(1);
    }

    //Функция уменьшает количество клеток на 1
    public void DecreaseCellCount()
    {
        ChangeField(-1);
    }

    //Функция изменяет количество клеток на поле
    public void ChangeField(int _sign)
    {
        int cellCount = Field.cellCount + _sign;
        fieldNameText.text = $"{cellCount}x{cellCount}";

        Field.instance.SetFieldSize(_sign);
        StartGame();

        //Если количество клеток достигло предельных значений, то соответствующие кнопки становятся неинтерактивными
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

    //Функция позволяет создать всплывающий текст со значением _amount
    public void SpawnPopUpText(int _amount)
    {
        PopUpText _popUpText = Instantiate(popupTextPrefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0)).GetComponent<PopUpText>();
        _popUpText.gameObject.transform.SetParent(scoreText.gameObject.transform);
        _popUpText.gameObject.transform.localPosition = new Vector3(25f, 9.5f);
        _popUpText.Initialize(_amount);
    }

    //Функция возвращает значение игрового счёта, соответствующее прошлому ходу. Также вызывается функция из класса Игровое Поле, которая возвращает прежние места плиток
    public void ReturnToLastTurn()
    {
        IncreaseScore(previousTurnScoreAmount - scoreAmount);
        Field.instance.ReturnToLastTurn();
        BackArrow.interactable = false;
    }

    //Функция позволяет сделать кнопку Шаг назад интерактивной
    public void InteractBackArrow()
    {
        BackArrow.interactable = true;
    }

    //Функция позволяет закрыть окно игры
    public void CloseGame()
    {
        Application.Quit();
    }
}
