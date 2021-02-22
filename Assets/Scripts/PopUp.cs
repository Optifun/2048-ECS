using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopUp : MonoBehaviour
{
    private Text popUpText;

    public void Initialize(int _amount)
    {
        popUpText = this.GetComponent<Text>();
        popUpText.text = _amount.ToString();
        Destroy(this.gameObject, 2);
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
