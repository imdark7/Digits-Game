using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class CircleList
{
    public CircleListNode Head;
    public List<Circle> InnerList { get; }
    public static readonly float Radius = 6.0f;
    public static Vector3 Center;
    public int Count => InnerList.Count;
    public readonly UnityEvent NeedCheckSumEvent = new UnityEvent();
    internal bool IsMoving => InnerList.Any(x => x.IsMoving);

    public CircleList()
    {
        InnerList = new List<Circle>();
    }

    private void AddFirst(Circle value)
    {
        var newEl = new CircleListNode(value);
        if (Count == 0)
        {
            newEl.Next = newEl;
            Head = newEl;
        }
        else
        {
            newEl.Next = Head;
            FindLast().Next = newEl;
            Head = newEl;
        }

        InnerList.Insert(0, value);
    }

    public IEnumerator AddAt(Circle value, int index, bool needCheckSum = true)
    {
        if (index < 0 || index > InnerList.Count)
        {
            index = 0;
        }

        if (index == 0)
        {
            AddFirst(value);
        }
        else
        {
            var prev = Head;
            for (var i = 0; i < index - 1; i++)
            {
                prev = prev.Next;
            }

            var next = prev.Next;
            prev.Next = new CircleListNode(value)
            {
                Next = next
            };
            InnerList.Insert(index, value);
        }

        if (needCheckSum)
        {
            yield return RenderList();
            NeedCheckSumEvent.Invoke();
        }
    }

    public bool TryGetRowOfDigits(int searchValue, int countForRow, out int startIndex, out int count)
    {
        if (InnerList.Count(x => x.Digit == searchValue) == Count)
        {
            startIndex = 0;
            count = Count;
            return true;
        }

        var circle = Head;
        var offset = 0;
        while (circle.Value.Digit == searchValue)
        {
            offset++;
            circle = circle.Next;
        }

        startIndex = 0;
        var isRange = false;
        count = 0;
        for (var i = 0; i < InnerList.Count; i++)
        {
            if (circle.Value.Digit == searchValue && !isRange)
            {
                startIndex = (i + offset) % InnerList.Count;
                isRange = true;
                count++;
            }
            else if (circle.Value.Digit == searchValue && isRange)
            {
                count++;
            }

            if (InnerList[(i + offset) % InnerList.Count].Digit != searchValue && isRange
                || i == InnerList.Count - 1)
            {
                if (count >= countForRow)
                {
                    return true;
                }

                isRange = false;
                count = 0;
            }

            circle = circle.Next;
        }

        return false;
    }

    public CircleListNode FindLast()
    {
        var el = Head;
        var count = 0;
        while (el.Next != Head )
        {
            if (count > Count)
            {
                throw new Exception("Something wrong!");
            }

            count++;
            el = el.Next;
        }

        return el;
    }

    public CircleListNode FindAt(int index)
    {
        var el = Head;
        for (var i = 0; i < index; i++)
        {
            el = el.Next;
        }

        return el;
    }

    public IEnumerator Replace(int[] indexesForRemove, Circle newCircle)
    {
        var first = indexesForRemove.First();
        RemoveAt(first, indexesForRemove.Length);
        var insertPosition = first > indexesForRemove.Last() ? 0 : first;
        yield return AddAt(newCircle, insertPosition, false);
        yield return RenderList();
    }

    public void RemoveAt(int index, int count = 1)
    {
        if (index < 0 || index > InnerList.Count || count < 0 || count > InnerList.Count)
        {
            throw new ArgumentException("Bad arguments!");
        }


        var afterRemoved = FindAt(index + count);
        var beforeRemoved = index == 0 ? FindLast() : FindAt(index - 1);

        beforeRemoved.Next = afterRemoved;

        if (index + count > Count)
        {
            Head = afterRemoved;
            var range1 = Count - index;
            InnerList.RemoveRange(index, range1);
            InnerList.RemoveRange(0, count - range1);
        }
        else
        {
            if (index == 0)
            {
                Head = count == Count ? null : FindAt(count);
            }
            InnerList.RemoveRange(index, count);
        }
    }

    public IEnumerator RenderList()
    {
        for (var i = 0; i < Count; i++)
        {
            InnerList[i].Move(GetCirclePosition(i, Count));
        }
        
        yield return new WaitUntil(() => !IsMoving);
        yield return new WaitForSecondsRealtime(0.3f);
    }

    private static float GetAngleOfElement(int i, int elementsCount) => i * 360f / elementsCount;


    public static Vector3 GetCirclePosition(int i, int count)
    {
        var ang = GetAngleOfElement(i, count);
        Vector3 pos;
        pos.x = Center.x + Radius * Mathf.Sin(ang * Mathf.Deg2Rad);
        pos.y = Center.y + Radius * Mathf.Cos(ang * Mathf.Deg2Rad);
        pos.z = Center.z;
        return pos;
    }
}

public class CircleListNode
{
    public CircleListNode Next { get; set; }
    public Circle Value { get; }

    public CircleListNode(Circle value)
    {
        Value = value;
    }
}