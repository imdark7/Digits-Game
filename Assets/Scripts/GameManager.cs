using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class GameManager : MonoBehaviour
{
    public Circle prefab;
    [FormerlySerializedAs("intList")] public CircleList list = new CircleList();
    public static int DigitCap = 10;
    public static int StartCount = 8;
    public static int CountForRow = 5;
    public float radius = 5.0f;
    private Vector3 center;
    private Circle nextCircle;
    private Vector3 targetNextCircleDirection;
    [SerializeField] private GameState gameState = GameState.Init;
    private int insertedIndex;

    private void Start()
    {
        center = transform.position;
        StartCoroutine(PlaceStartCircles());
        gameState = GameState.WaitPlayersTurn;
    }

    private IEnumerator PlaceStartCircles()
    {
        for (var i = 0; i < StartCount; i++)
        {
            var circle = CreateCircle();
            list.AddAt(i, circle);
            var pos = GetCirclePosition(center, radius, GetAngleOfElement(i, StartCount));
            yield return StartCoroutine(circle.MoveCoroutine(pos));
        }

        nextCircle = CreateCircle();
    }

    private Circle CreateCircle(int digit = default, Vector3 position = default) =>
        Instantiate(prefab, position, Quaternion.identity).Init(digit);

    private void Update()
    {
        #region WaitPlayersTurn

        if (gameState == GameState.WaitPlayersTurn)
        {
            if (nextCircle is null) return;

            if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0))
            {
                var mousePosition = Input.mousePosition;
                targetNextCircleDirection = 2f * Camera.main
                    .ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 1))
                    .normalized;

                nextCircle.Move(targetNextCircleDirection);
            }

            if (Input.GetMouseButtonUp(0))
            {
                InsertIntoList(nextCircle, targetNextCircleDirection);
                nextCircle = CreateCircle();
            }

            if (Input.touches.Any())
            {
                var touch = Input.GetTouch(0);
                if (touch.phase != TouchPhase.Canceled || touch.phase != TouchPhase.Ended)
                {
                    targetNextCircleDirection = 2f * Camera.main
                        .ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y,
                            1))
                        .normalized;
                    nextCircle.Move(targetNextCircleDirection);
                }
                else if (touch.phase == TouchPhase.Ended)
                {
                    InsertIntoList(nextCircle, targetNextCircleDirection);
                    nextCircle = CreateCircle();
                }
            }
        }

        #endregion

        #region PlaceCircle

        if (gameState == GameState.PlaceCircle)
        {
            RenderList();

            if (list.InnerList.All(x => x.transform.position == x.targetPosition))
            {
                gameState = GameState.Collapse;
            }
        }

        #endregion

        #region Collapse

        if (gameState == GameState.Collapse)
        {
            CollapseSum();
        }

        #endregion

        #region CollapseRow

        if (gameState == GameState.CollapseRow)
        {
            CollapseRow();
        }

        #endregion

        #region CollapseRow

        if (gameState == GameState.CheckLose)
        {
            CheckDeath();
        }

        #endregion
    }

    private void CheckDeath()
    {
        if (list.Count >= 20)
        {
            gameState = GameState.GameOver;
        }

        gameState = GameState.WaitPlayersTurn;
    }

    private void CollapseRow()
    {
        if (list.TryGetRowOfDigits(DigitCap, CountForRow, out var startIndex, out var count))
        {
            for (var i = 0; i < count; i++)
            {
                Destroy(list.InnerList[i].gameObject);
            }

            list.RemoveAt(startIndex, count);
            RenderList();
        }

        gameState = GameState.CheckLose;
    }

    private void InsertIntoList(Circle circle, Vector3 direction)
    {
        var sectorAngle = 360f / list.Count;
        var vectorAngle = Vector3.Angle(Vector3.up, direction);
        if (direction.x < 0)
        {
            vectorAngle = 360f - vectorAngle;
        }

        var index = (int) (vectorAngle / sectorAngle) + 1;
        list.AddAt(index, circle);
        insertedIndex = index;
        gameState = GameState.PlaceCircle;
    }

    private void RenderList()
    {
        for (var i = 0; i < list.Count; i++)
        {
            list.InnerList[i].Move(GetCirclePosition(center, radius, GetAngleOfElement(i, list.Count)));
        }
    }

    private static float GetAngleOfElement(int i, int elementsCount) => i * 360f / elementsCount;

    private static Vector3 GetCirclePosition(Vector3 center, float radius, float ang)
    {
        Vector3 pos;
        pos.x = center.x + radius * Mathf.Sin(ang * Mathf.Deg2Rad);
        pos.y = center.y + radius * Mathf.Cos(ang * Mathf.Deg2Rad);
        pos.z = center.z;
        return pos;
    }

    private void CollapseSum()
    {
        var sum = 0;
        var indexList = new List<int>();

        for (var i = 0; i < list.Count; i++)
        {
            var j = 0;
            var index = i;

            if (list.InnerList[index].digit == DigitCap)
            {
                indexList.Clear();
                continue;
            }

            while (j < list.Count)
            {
                sum += list.InnerList[index].digit;
                indexList.Add(index);
                if (sum > DigitCap)
                {
                    sum = 0;
                    indexList.Clear();
                    break;
                }

                if (sum == DigitCap)
                {
                    gameState = GameState.CollapseRow;
                    Collapse(indexList);
                    return;
                }

                j++;
                index = GetNextIndex(index);
            }
        }

        gameState = GameState.CollapseRow;
    }

    private void Collapse(IEnumerable<int> indexes)
    {
        var indexArray = indexes.ToArray();
        var min = indexArray.Min();
        var positionToCreate = list.InnerList[insertedIndex].transform.position;
        foreach (var i in indexArray)
        {
            Destroy(list.InnerList[i].gameObject);
        }

        list.RemoveAt(min, indexArray.Length);
        list.AddAt(min, CreateCircle(DigitCap, positionToCreate));
        RenderList();
    }

    private int GetNextIndex(int i) => i == list.Count - 1 ? 0 : i + 1;

    private enum GameState
    {
        Init,
        WaitPlayersTurn,
        PlaceCircle,
        Collapse,
        CollapseRow,
        CheckLose,
        GameOver
    }
}