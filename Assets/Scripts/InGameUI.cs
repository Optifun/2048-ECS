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

        PlayerPrefs.SetInt("Score", scoreAmount);
        PlayerPrefs.Save();
    }

    public void GameOver()
    {
        panel.SetActive(true);
        if (bestAmount < scoreAmount)
        {
            bestAmount = scoreAmount;
            PlayerPrefs.SetInt("Best", bestAmount);
            PlayerPrefs.Save();
        }
        //TODO: transition
    }

    public void Retry()
    {
        ResetUI();
        Field.instance.ResetGame();
        panel.SetActive(false);

        //TODO: retry
    }

    public void ResetUI()
    {
        scoreAmount = 0;
        PlayerPrefs.SetInt("Score", -1);
        bestAmount = 0;
        bestText.text = "0";
        scoreText.text = scoreAmount.ToString();
    }

    public void StartNewGame()
    {
        //TODO: start new game
    }

    // Start is called before the first frame update
    void Start()
    {
        int amount = PlayerPrefs.GetInt("Best");
        if (amount > 0)
        {
            bestAmount = amount;
        }
        else
        {
            bestAmount = 0;
            
        }
        bestText.text = bestAmount.ToString();

        amount = PlayerPrefs.GetInt("Score");
        if (amount > 0)
        {
            scoreAmount = amount;
        }
        else
        {
            scoreAmount = 0;
        }
        scoreText.text = scoreAmount.ToString();
    }

    public void SpawnPopUpText(int _amount)
    {
        PopUp _popUpText = Instantiate(popupTextPrefab, new Vector3(25.71f, 11.822f, 0f), new Quaternion(0, 0, 0, 0)).GetComponent<PopUp>();
        _popUpText.Initialize(_amount);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
