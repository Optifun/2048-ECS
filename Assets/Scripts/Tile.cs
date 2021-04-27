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

    public Color[] lightColorPallete = new Color[13];
    public Color[] darkColorPallete = new Color[13];
    private Color textColor;
    private Color spriteColor;

    public Animator animator;

    public void Merge()
    {
        StartCoroutine(Merging());
    }

    public void Initialize(Vector2Int _position, Theme _theme)
    {
        int randNum = Random.Range(0, 10);
        if (randNum == 9)
            value = 4;
        else
            value = 2;

        position = _position;

        SetAppearance(_theme);
        animator.SetInteger("state", 1);
    }

    public void Initialize(int _value, Vector2Int _position, Theme _theme)
    {
        value = _value;

        position = _position;

        SetAppearance(_theme);
        animator.SetInteger("state", 2);
    }

    public void Set(Vector2Int _position)
    {
        position = _position;
        //gameObject.transform.localPosition.magnitude / 10
        StartCoroutine(Moving());
    }

    public void SetAppearance(Theme _theme)
    {
        if (_theme == Theme.dark)
        {
            spriteColor = darkColorPallete[(int)Mathf.Log(value, 2) - 1];
        }
        else
        {
            spriteColor = lightColorPallete[(int)Mathf.Log(value, 2) - 1];
        }

        if (spriteColor.grayscale > 0.735f)
        {
            textColor = new Color(0.2f, 0.2f, 0.2f, 0.3f);
        }
        else
        {
            textColor = new Color(1, 1, 1, 0.3f);
        }

        sprite.color = spriteColor;
        numberText.color = textColor;
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

        Field.instance.SetReady();
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
