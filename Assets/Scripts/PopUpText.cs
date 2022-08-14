using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopUpText : MonoBehaviour
{
    private Text popUpText;

    //Задает значение всплывающему тексту
    public void Initialize(int _amount)
    {
        popUpText = this.GetComponent<Text>();
        if (_amount > 0)
            popUpText.text = "+";
        popUpText.text += $"{_amount.ToString()}";
        //Уничтожение текста происходит через 2 секунды
        Destroy(this.gameObject, 2);
    }
}
