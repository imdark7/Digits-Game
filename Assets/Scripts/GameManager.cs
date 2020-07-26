using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
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
    private Circle nextCircle;
    private Vector3 targetNextCircleDirection;
    private readonly UnityEvent playerTurnEvent = new UnityEvent();
    private int insertedIndex;


    private void Start()
    {
        list.NeedCheckSumEvent.AddListener(() => StartCoroutine(CollapseSum()));
        playerTurnEvent.AddListener(() => nextCircle = CreateCircle());

        CircleList.Center = transform.position;
        StartCoroutine(PlaceStartCircles());
    }

    private IEnumerator PlaceStartCircles()
    {
        var startDigits = new[] {6, 6, 6, 5, 6, 7, 8, 9};
        for (var i = 0; i < StartCount;)
        {
            var circle = CreateCircle(startDigits[i]);
            yield return list.AddAt(circle, i, false);
            var pos = CircleList.GetCirclePosition(i, StartCount);
            circle.Move(pos);

            while (list.IsMoving)
            {
                yield return null;
            }

            i++;
        }

        yield return list.RenderList();
        playerTurnEvent.Invoke();
    }

    private Circle CreateCircle(int digit = default, Vector3 position = default, Quaternion quaternion = default)
    {
        return Instantiate(prefab, position, Quaternion.identity).Init(digit);
    }

    private void Update()
    {
        if (list.IsMoving)
        {
            return;
        }

        if (nextCircle != null)
        {
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
                StartCoroutine(InsertIntoList(nextCircle, targetNextCircleDirection));
                nextCircle = null;
                return;
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
                    StartCoroutine(InsertIntoList(nextCircle, targetNextCircleDirection));
                    nextCircle = null;
                }
            }
        }
    }

    private IEnumerator CollapseRow()
    {
        if (list.TryGetRowOfDigits(DigitCap, CountForRow, out var startIndex, out var count))
        {
            var indexesForDestroy =
                GetIndexesForDestroy(startIndex, count)
                    .OrderByDescending(i => i);
            foreach (var i in indexesForDestroy)
            {
                Destroy(list.InnerList[i].gameObject);
            }

            list.RemoveAt(startIndex, count);
            yield return list.RenderList();
        }

        playerTurnEvent.Invoke();
    }

    private IEnumerable<int> GetIndexesForDestroy(int startIndex, int count)
    {
        var indexes = new int[count];
        for (var i = 0; i < count; i++)
        {
            indexes[i] = startIndex;
            startIndex = startIndex >= list.Count - 1 ? 0 : startIndex + 1;
        }

        return indexes;
    }

    private IEnumerator InsertIntoList(Circle circle, Vector3 direction)
    {
        var sectorAngle = 360f / list.Count;
        var vectorAngle = Vector3.Angle(Vector3.up, direction);
        if (direction.x < 0)
        {
            vectorAngle = 360f - vectorAngle;
        }

        var index = (int) (vectorAngle / sectorAngle) + 1;
        insertedIndex = index;
        yield return list.AddAt(circle, index);
    }


    private IEnumerator CollapseSum()
    {
        var sum = 0;
        var indexList = new List<int>();

        for (var i = 0; i < list.Count; i++)
        {
            var j = 0;
            var index = i;

            if (list.InnerList[index].Digit == DigitCap)
            {
                sum = 0;
                indexList.Clear();
                continue;
            }

            while (j < list.Count)
            {
                sum += list.InnerList[index].Digit;
                indexList.Add(index);
                if (sum > DigitCap)
                {
                    sum = 0;
                    indexList.Clear();
                    break;
                }

                if (sum == DigitCap)
                {
                    yield return StartCoroutine(Collapse(indexList));
                    yield break;
                }

                j++;
                index = GetNextIndex(index);
            }
        }
        
        playerTurnEvent.Invoke();
    }

    private IEnumerator Collapse(IEnumerable<int> indexes)
    {
        var indexArray = indexes.ToArray();
        var positionToCreate = list.InnerList[insertedIndex].transform.position;
        foreach (var i in indexArray.OrderByDescending(i => i))
        {
            Destroy(list.InnerList[i].gameObject);
        }


        yield return list.Replace(indexArray, CreateCircle(DigitCap, positionToCreate));
        yield return CollapseRow();
    }

    private int GetNextIndex(int i) => i == list.Count - 1 ? 0 : i + 1;
}