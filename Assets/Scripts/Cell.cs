using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    GameObject tile = null;

    public bool isFree()
    {
        if (tile == null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        tile = null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
