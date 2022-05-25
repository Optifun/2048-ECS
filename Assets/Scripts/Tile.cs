using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Tile : MonoBehaviour
{
    //Значение плитки, её номинал
    public int value { get; private set; }
    //Позиция плитки. Совпадает с индесом клетки в двумерном массиве поля
    public Vector2Int position { get; private set; }
   
    public Text numberText;
    public SpriteRenderer sprite;

    //Палитра цветов для плитки для светлой и темной тем
    public Color[] lightColorPallete = new Color[13];
    public Color[] darkColorPallete = new Color[13];
    private Color textColor;
    private Color spriteColor;

    public Animator animator;

    public float moveDuration = 0.01f;
    public float spawnDuration = 0.3f;

    //Функция позволяет указать позицию плитки и текущую тему
    public void Initialize(Vector2Int _position, Theme _theme)
    {
        //Для плитки подбирается значение номинала. Значение 2 либо 4
        int randNum = Random.Range(0, 10);
        if (randNum == 9)
            value = 4;
        else
            value = 2;

        position = _position;

        SetAppearance(_theme);
        //Запускается анимация появления плитки
        //animator.SetInteger("state", 1);
        transform.localScale = new Vector3(0, 0, 1);
        transform.DOScale(new Vector2(1f, 1f), spawnDuration);
    }

    //Функция позволяет указать значение и позицию плитки, а также текущую тему
    public void Initialize(int _value, Vector2Int _position, Theme _theme)
    {
        value = _value;

        position = _position;

        SetAppearance(_theme);
        //Запускается анимация появления плитки после объединения
        //animator.SetInteger("state", 2);
        transform.DOPunchScale(new Vector2(0.2f, 0.2f), spawnDuration * 1.5f, 1, 1);
    }

    //Функция позволяет запустить корутину перемещения и вернуть значение типа UniTask, которая позволяет отслеживать состояние корутины
    public UniTask Move(Cell _cell)
    {
        position = _cell.position;

        return Moving(_cell.transform.position).ToUniTask(this);
    }

    //Функция позволяет запустить корутину перемещения и вернуть значение типа UniTask, которая позволяет отслеживать состояние корутины
    public UniTask Merge(Cell _cell)
    {
        return Merging(_cell.transform.position).ToUniTask(this);
    }

    //Изменяет внешний облик плитки, соответствующий текущей теме
    public void SetAppearance(Theme _theme)
    {
        int powerNum = (int)Mathf.Log(value, 2);

        if (_theme == Theme.dark)
        {
            if (powerNum <= darkColorPallete.Length)
                spriteColor = darkColorPallete[powerNum - 1];
            else
                spriteColor = darkColorPallete[darkColorPallete.Length - 1];
        }
        else
        {
            if (powerNum <= lightColorPallete.Length)
                spriteColor = lightColorPallete[powerNum - 1];
            else
                spriteColor = lightColorPallete[lightColorPallete.Length - 1];
        }

        //Определение цвета текста. Он зависит от оттенка серого принятого цвета плитки
        if (spriteColor.grayscale > 0.735f)
        {
            textColor = new Color(0.2f, 0.2f, 0.2f, 1);
        }
        else
        {
            textColor = new Color(1, 1, 1, 1);
        }

        sprite.color = spriteColor;
        numberText.color = textColor;
        numberText.text = value.ToString();
    }

    //Корутина передвижения плитки
    public IEnumerator Moving(Vector2 _position)
    {
        transform.DOMove(_position, moveDuration);
        yield return new WaitForSeconds(moveDuration);
    }

    //Корутина объединения плитки
    public IEnumerator Merging(Vector2 _position)
    {
        numberText.transform.DOScale(new Vector2(0, 0), moveDuration);
        transform.DOMove(_position, moveDuration);
        yield return new WaitForSeconds(moveDuration);
        Destroy(this.gameObject);
    }
}
