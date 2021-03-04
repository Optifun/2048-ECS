using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputs : MonoBehaviour
{
    Vector3 startMousePosition = Vector3.zero;
    Vector3 dragMousePosition = Vector3.zero;
    Vector3 endMousePosition = Vector3.zero;
    Vector2Int moveDirection = Vector2Int.zero;
    float size;
    float deltaHorizontal;
    float deltaVertical;

    bool isLocked;
    bool isStartPosition;
    bool isMovementTriggered;

    //event UIDirectionHint;
    //event GameMoveDirection;

    // Start is called before the first frame update
    void Start()
    {
        isLocked = false;
        isStartPosition = false;
        isMovementTriggered = true;

        size = Mathf.Min(Screen.height, Screen.width);
    }

    private void OnMouseDown()
    {
        isMovementTriggered = false;
        startMousePosition = Input.mousePosition;
        Debug.Log("OK");
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

                    if (size / 12 < Mathf.Abs(deltaVertical))
                    {
                        if (deltaVertical > 0)
                        {
                            Field.instance.Move(Vector2Int.up);
                        }
                        else
                        {
                            Field.instance.Move(Vector2Int.down);
                        }

                        isMovementTriggered = true;
                    }
                    else if (size / 12 < Mathf.Abs(deltaHorizontal))
                    {
                        if (deltaHorizontal > 0)
                        {
                            Field.instance.Move(Vector2Int.right);
                        }
                        else
                        {
                            Field.instance.Move(Vector2Int.left);
                        }

                        isMovementTriggered = true;
                    }
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isStartPosition = false;
            }
        }
    }
}
