using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class GameControls : MonoBehaviour
{
    //Позиция мыши при нажатии
    Vector3 startMousePosition = Vector3.zero;
    //Позиция мыши при перемещении
    Vector3 dragMousePosition = Vector3.zero;

    //Приращение dragMousePosition к startMousePosition по вертикали и горизонтали
    float deltaHorizontal;
    float deltaVertical;

    //Управление заблокировано?
    bool isLocked;

    bool isStartPosition;
    bool isMovementTriggered;

    private Vector2Int _nextMoveDirection;

    //Чувствительность мыши
    public float mouseSensitive = 12.5f;

    // Start is called before the first frame update
    void Start()
    {
        isLocked = false;
        isStartPosition = false;
        isMovementTriggered = true;
        _nextMoveDirection = Vector2Int.zero;
        ActionsManager.finishMove += unlockInput;
    }

    //Функция возвращает направление движения, исходя из нажатой пользователем клавиши
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

    //Функция возвращает направление движения, исходя из направления движения мыши
    private Vector2Int GetMouseMoveDirection()
    {
        Vector2Int direction = Vector2Int.zero;

        //Если левая кнопка мыши была нажата
        if (Input.GetMouseButton(0))
        {
            if (isStartPosition == false)
            {
                //Присваивание стартовой позиции
                startMousePosition = Input.mousePosition;
                isStartPosition = true;
                isMovementTriggered = false;
            }

            if (isMovementTriggered == false)
            {
                dragMousePosition = Input.mousePosition;

                deltaHorizontal = dragMousePosition.x - startMousePosition.x;
                deltaVertical = dragMousePosition.y - startMousePosition.y;

                float size = Mathf.Min(Screen.height, Screen.width);

                //Если разница между dragMousePosition и startMousePosition > предельной по вертикали
                if (size / mouseSensitive < Mathf.Abs(deltaVertical))
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
                //Иначе если разница между dragMousePosition и startMousePosition > предельной по горизонтали
                else if (size / mouseSensitive < Mathf.Abs(deltaHorizontal))
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

    //Функция позволяет пользователю задать направление передвижения с помощью клавиатуры и мыши и вызывает метод передвижения в классе Игровое Поле
    private void Move()
    {
        Vector2Int currentDirection = Vector2Int.zero;
        Vector2Int keyboardMoveDirection = GetKeyboardMoveDirection();
        Vector2Int mouseMoveDirection = GetMouseMoveDirection();

        if (keyboardMoveDirection != Vector2Int.zero)
        {
            currentDirection = keyboardMoveDirection;
        }
        else if (mouseMoveDirection != Vector2Int.zero)
        {
            currentDirection = mouseMoveDirection;
        }

        if (isLocked == false)
        {
            if (currentDirection != Vector2Int.zero)
            {
                Debug.Log("make move");
                isLocked = true;
                ActionsManager.Move(currentDirection);
                _nextMoveDirection = Vector2Int.zero;
            }
            else if (_nextMoveDirection != Vector2Int.zero)
            {
                Debug.Log("make move");
                isLocked = true;
                ActionsManager.Move(_nextMoveDirection);
                _nextMoveDirection = Vector2Int.zero;
            }
        }
        else
        {
            if (currentDirection != Vector2Int.zero)
            {
                _nextMoveDirection = currentDirection;
                Time.timeScale = 0f;
                Debug.Log("go slow");
            }
            Debug.Log("i'm here");
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

    //Функция позволяет разблокировать управление пользователя
    private void unlockInput()
    {
        Time.timeScale = 1;
        isLocked = false;
        Debug.Log("move is done");
    }
}
