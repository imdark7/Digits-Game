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
    [FormerlySerializedAs("prefab")] public Circle circlePrefab;
    [FormerlySerializedAs("sliderPrefab")] public SliderBar sliderBarPrefab;

    [FormerlySerializedAs("purposePrefab")]
    public Purpose[] purposePrefabs;

    [FormerlySerializedAs("intList")] public CircleList list = new CircleList();
    [FormerlySerializedAs("Purposes")] public Queue<Purpose> purposes = new Queue<Purpose>();
    public ParticleSystem lvlUpEffect;
    public static int DigitCap = 8;
    public static int StartCount = 8;
    public static int CountForRow = 5;
    private static int expForLvlUp = 5;
    private static int countForDeath = 10;
    private Circle nextCircle;
    private Vector3 targetNextCircleDirection;
    private readonly UnityEvent playerTurnEvent = new UnityEvent();
    private SliderBar experience;
    private SliderBar death;

    private bool firstTurn = true;

    private void Start()
    {
        var canvas = transform.Find("Canvas");
        var canvasPosition = canvas.position;
        experience =
            Instantiate(sliderBarPrefab, canvas)
                .Init(new Vector3(-450 + canvasPosition.x, 635 + canvasPosition.y),
                    new Color(0.4157809f, 0.735849f, 0.2742079f),
                    expForLvlUp);
        death =
            Instantiate(sliderBarPrefab, canvas)
                .Init(new Vector3(450 + canvasPosition.x, 635 + canvasPosition.y),
                    new Color(0.764151f, 0.025849f, 0.04650033f),
                    countForDeath);

        lvlUpEffect = transform.Find("LvlUpEffect").GetComponent<ParticleSystem>();

        for (var i = 0; i < 5; i++)
        {
            purposes.Enqueue(Instantiate(purposePrefabs[i], new Vector3(0, 11, i + 20), Quaternion.identity));
        }

        list.NeedCheckSumEvent.AddListener(() => StartCoroutine(NeedCheckSumEventHandler()));
        playerTurnEvent.AddListener(() => StartCoroutine(BeforePlayerTurn()));

        CircleList.Center = transform.position;
        StartCoroutine(PlaceStartCircles());
    }

    private IEnumerator NeedCheckSumEventHandler()
    {
        yield return CollapseSum();
        playerTurnEvent.Invoke();
    }

    private IEnumerator BeforePlayerTurn()
    {
        if (firstTurn)
        {
            firstTurn = false;
        }
        else
        {
            experience.Increase();
            yield return new WaitForSecondsRealtime(0.2f);
        }

        if (experience.Value == expForLvlUp)
        {
            yield return LvlUp();
        }

        yield return death.SetValue(list.Count);
        if (death.Value >= countForDeath)
        {
            yield return Death();
            yield break;
        }
        nextCircle = CreateCircle();
    }

    private IEnumerator Death()
    {
        yield return null;
    }

    private IEnumerator LvlUp()
    {
        lvlUpEffect.Play();
        yield return experience.SetValue(0);
        expForLvlUp = (int) (expForLvlUp * 1.2);
        experience.SetMaxValue(expForLvlUp);
        purposes.Dequeue().dissolveEvent.Invoke();
        for (var i = 0; i < list.Count; i++)
        {
            if (list.InnerList[i].Digit == DigitCap)
            {
                yield return list.ChangeValue(i, DigitCap / 2);
            }
        }

        DigitCap++;
        yield return new WaitForSecondsRealtime(1.3f);
        yield return CollapseSum();
    }

    private IEnumerator PlaceStartCircles()
    {
        var startDigits = new[] {2, 1, 2, 2, 2, 2, 4, 4};
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

            death.Increase();
            i++;
        }

        yield return list.RenderList();
        playerTurnEvent.Invoke();
    }

    private Circle CreateCircle(int digit = default, Vector3 position = default)
    {
        return Instantiate(circlePrefab, position, Quaternion.identity).Init(digit);
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
                    .ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 0))
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
                            0))
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

    private IEnumerator InsertIntoList(Circle circle, Vector2 direction)
    {
        var sectorAngle = 360f / list.Count;
        var vectorAngle = Vector2.Angle(Vector2.up, direction);
        if (direction.x < 0)
        {
            vectorAngle = 360f - vectorAngle;
        }

        var index = (int) (vectorAngle / sectorAngle) + 1;
        yield return list.AddAt(circle, index);
    }


    private IEnumerator CollapseSum()
    {
        var finished = false;
        while (!finished)
        {
            yield return CollapseSumOnce();
        }

        yield return CollapseRow();

        IEnumerator CollapseSumOnce()
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

            finished = true;
        }
    }

    private IEnumerator Collapse(IEnumerable<int> indexes)
    {
        var indexArray = indexes.ToArray();
        var positionToCreate = list.InnerList[indexArray[indexArray.Length / 2]].transform.position;
        foreach (var i in indexArray.OrderByDescending(i => i))
        {
            Destroy(list.InnerList[i].gameObject);
        }


        yield return list.Replace(indexArray, CreateCircle(DigitCap, positionToCreate));
    }

    private int GetNextIndex(int i) => i == list.Count - 1 ? 0 : i + 1;
}