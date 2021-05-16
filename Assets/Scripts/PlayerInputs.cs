using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class PlayerInputs : MonoBehaviour
{
    Vector3 startMousePosition = Vector3.zero;
    Vector3 dragMousePosition = Vector3.zero;

    float deltaHorizontal;
    float deltaVertical;

    bool isLocked;
    bool isStartPosition;
    bool isMovementTriggered;

    float mouseSensitive = 5.5f;

    // Start is called before the first frame update
    void Start()
    {
        isLocked = false;
        isStartPosition = false;
        isMovementTriggered = true;
        Field.instance.finishMove += unlockInput;
    }

    private Vector2Int GetKeyboardMoveDirection()
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

    private Vector2Int GetMouseMoveDirection()
    {
        Vector2Int direction = Vector2Int.zero;

        if (Input.GetMouseButton(0))
        {
            if (isStartPosition == false)
            {
                startMousePosition = Input.mousePosition;
                isStartPosition = true;
                isMovementTriggered = false;
            }

            if (isMovementTriggered == false)
            {
                dragMousePosition = Input.mousePosition;

                deltaHorizontal = dragMousePosition.x - startMousePosition.x;
                deltaVertical = dragMousePosition.y - startMousePosition.y;

                if (Screen.height / mouseSensitive < Mathf.Abs(deltaVertical))
                {
                    if (deltaVertical > 0)
                    {
                        direction = Vector2Int.up;
                    }
                    else
                    {
                        direction = Vector2Int.down;
                    }

                    isMovementTriggered = true;
                }
                else if (Screen.width / mouseSensitive < Mathf.Abs(deltaHorizontal))
                {
                    if (deltaHorizontal > 0)
                    {
                        direction = Vector2Int.right;
                    }
                    else
                    {
                        direction = Vector2Int.left;
                    }

                    isMovementTriggered = true;
                }
            }
        }

        return direction;
    }

    private void Move()
    {
        if (isLocked == false)
        {
            Vector2Int keyboardMoveDirection = GetKeyboardMoveDirection();
            Vector2Int mouseMoveDirection = GetMouseMoveDirection();

            if (keyboardMoveDirection != Vector2Int.zero)
            {
                isLocked = true;
                Field.instance.Move(keyboardMoveDirection);
            }
            else if (mouseMoveDirection != Vector2Int.zero)
            {
                isLocked = true;
                Field.instance.Move(mouseMoveDirection);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isStartPosition = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    private void unlockInput()
    {
        isLocked = false;
    }
}
