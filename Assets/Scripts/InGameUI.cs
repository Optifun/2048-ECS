using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGameUI : MonoBehaviour
{
    public static InGameUI instance;

    public Text scoreText;
    //TODO: Перенести в днугой класс
    public int scoreAmount { get; private set; }
    public Text bestText;
    public int bestAmount { get; private set; }
    public GameObject popupTextPrefab;
    public GameObject panel;
    public Button BackArrow;
    public Text fieldNameText;
    public Button DecreaseCellCountButton;
    public Button IncreaseCellCountButton;
    public Button ThemeButton;
    public Sprite light;
    public Sprite dark;

    private int cellCount = 4;

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
        scoreAmount += _amount;
        scoreText.text = scoreAmount.ToString();

        SpawnPopUpText(_amount);

        PlayerPrefs.SetInt($"GameMode {GameEnvironment.cellCount} Score", scoreAmount);
        PlayerPrefs.Save();

        BackArrow.interactable = true;
    }

    public void GameOver()
    {
        panel.SetActive(true);
        if (bestAmount < scoreAmount)
        {
            bestAmount = scoreAmount;
            bestText.text = bestAmount.ToString();
            PlayerPrefs.SetInt($"GameMode {GameEnvironment.cellCount} Best", bestAmount);
            PlayerPrefs.Save();
        }
        //TODO: transition
    }

    public void Retry()
    {
        ResetUI();
        StartNewGame();
        Field.instance.ResetGame();
        panel.SetActive(false);

        //TODO: retry
    }

    public void ResetUI()
    {
        scoreAmount = 0;
        PlayerPrefs.SetInt($"GameMode {GameEnvironment.cellCount} Score", 0);
        scoreText.text = scoreAmount.ToString();
    }

    public void StartNewGame()
    {
        int amount = PlayerPrefs.GetInt($"GameMode {GameEnvironment.cellCount} Best");
        if (amount > 0)
        {
            bestAmount = amount;
        }
        else
        {
            bestAmount = 0;

        }
        bestText.text = bestAmount.ToString();

        amount = PlayerPrefs.GetInt($"GameMode {GameEnvironment.cellCount} Score");
        if (amount > 0)
        {
            scoreAmount = amount;
        }
        else
        {
            scoreAmount = 0;
        }
        scoreText.text = scoreAmount.ToString();

        BackArrow.interactable = false;
    }

    public void ChangeTheme()
    {
        if (GameEnvironment.instance.theme == Theme.dark)
        {
            ThemeButton.image.sprite = dark;
        }
        else
        {
            ThemeButton.image.sprite = light;
        }

        GameEnvironment.instance.ChangeTheme();
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
        cellCount += _sign;
        fieldNameText.text = $"{cellCount}x{cellCount}";
        GameEnvironment.instance.SetFieldSize(_sign);
        StartNewGame();

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
        StartNewGame();
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
