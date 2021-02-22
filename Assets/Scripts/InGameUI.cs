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
        StartNewGame();
        Field.instance.ResetGame();
        panel.SetActive(false);

        //TODO: retry
    }

    public void ResetUI()
    {
        scoreAmount = 0;
        PlayerPrefs.SetInt("Score", -1);
        scoreText.text = scoreAmount.ToString();
    }

    public void StartNewGame()
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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SpawnPopUpText(int _amount)
    {
        PopUp _popUpText = Instantiate(popupTextPrefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0)).GetComponent<PopUp>();
        _popUpText.gameObject.transform.SetParent(scoreText.gameObject.transform);
        _popUpText.gameObject.transform.localPosition = new Vector3(25f, 9.5f);
        _popUpText.Initialize(_amount);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
