using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public int value { get; private set; }
    public Vector2Int position { get; private set; }

    public Text numberText;
    public SpriteRenderer sprite;

    public Color[] colorPallete = new Color[13];
    private Color textColor;
    private Color spriteColor;

    public Animator animator;
    /*
    public void Merged()
    {
        value *= 2;
        StartCoroutine(Moving());
        SetAppearance();
    }
    */

    public void Merge()
    {
        StartCoroutine(Merging());
    }

    public void Initialize(Vector2Int _position)
    {
        int randNum = Random.Range(0, 10);
        if (randNum == 9)
            value = (new PowerOfTwo(2)).value;
        else
            value = (new PowerOfTwo(1)).value;

        numberText.text = value.ToString();

        position = _position;

        SetAppearance();
        animator.SetInteger("state", 1);
    }

    public void Initialize(int _value, Vector2Int _position)
    {
        value = _value;

        position = _position;

        SetAppearance();
        animator.SetInteger("state", 2);
    }

    public void Set(Vector2Int _position)
    {
        position = _position;
        //gameObject.transform.localPosition.magnitude / 10
        StartCoroutine(Moving());
    }

    public void SetAppearance()
    {
        spriteColor = colorPallete[(int)Mathf.Log(value, 2)];

        if (spriteColor.grayscale >= 160)
        {
            textColor = new Color(45, 45, 45);
        }
        else
        {
            textColor = new Color(236, 236, 236);
        }

        sprite.color = colorPallete[(int)Mathf.Log(value, 2) - 1];
        Debug.Log((int)Mathf.Log(value, 2));
        numberText.text = value.ToString();
    }

    public IEnumerator Moving()
    {
        float _startMagnitude = Mathf.Abs(gameObject.transform.localPosition.magnitude);
        float _currentMagnitude = _startMagnitude;

        while (_startMagnitude / 10 < _currentMagnitude)
        {
            gameObject.transform.localPosition = Vector2.Lerp(gameObject.transform.localPosition, Vector2.zero, Time.fixedDeltaTime * 17);
            _currentMagnitude = Mathf.Abs(gameObject.transform.localPosition.magnitude);
            yield return new WaitForSeconds(0.01f);
        }

        gameObject.transform.localPosition = new Vector3(0, 0, -1);

        StopCoroutine(Moving());
    }

    public IEnumerator Merging()
    {
        float _startMagnitude = Mathf.Abs(gameObject.transform.localPosition.magnitude);
        float _currentMagnitude = _startMagnitude;

        while (_startMagnitude / 10 < _currentMagnitude)
        {
            gameObject.transform.localPosition = Vector2.Lerp(gameObject.transform.localPosition, Vector2.zero, Time.fixedDeltaTime * 17);
            _currentMagnitude = Mathf.Abs(gameObject.transform.localPosition.magnitude);
            yield return new WaitForSeconds(0.01f);
        }

        gameObject.transform.localPosition = new Vector3(0, 0, -1);

        Destroy(this.gameObject);
        StopCoroutine(Moving());
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
