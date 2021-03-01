using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputs : MonoBehaviour
{
    Vector2Int startMousePosition = Vector2Int.zero;
    Vector2Int dragMousePosition = Vector2Int.zero;
    Vector2Int endMousePosition = Vector2Int.zero;
    Vector2Int moveDirection = Vector2Int.zero;

    bool isLocked;

    //event UIDirectionHint;
    //event GameMoveDirection;

    // Start is called before the first frame update
    void Start()
    {
        isLocked = false;
    }

    private void OnMouseDown()
    {
        //startMousePosition = Input.mousePosition;
        //event для UI (о том, что нужно показать подсказку)
    }

    private void OnMouseDrag()
    {
        //dragMousePosition = Input.mousePosition;
        //event для UI (о том, что мышь была сдвинута и нужно пересчитать позицию)
    }

    private void OnMouseUp()
    {
        //endMousePosition = Input.mousePosition;
        //event для UI (о том, чтобы отключить подсказку) и игры (о направлении передвижения)
    }

    private Vector2Int GetMoveDirectionFromKeyboard()
    {
        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow))
        {
            return Vector2Int.up;
        }
        else if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.LeftArrow))
        {
            return Vector2Int.left;
        }
        else if (Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.DownArrow))
        {
            return Vector2Int.down;
        }
        else if (Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.RightArrow))
        {
            return Vector2Int.right;
        }
        else
        {
            return Vector2Int.zero;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isLocked == false)
        {
            moveDirection = GetMoveDirectionFromKeyboard();

            if (moveDirection != Vector2Int.zero)
            {
                // event для игры (о направлении передвижения)
                Field.instance.Move(moveDirection);
            }
        }

    }
}
