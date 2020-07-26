using System;
using System.Collections.Generic;
using System.Linq;

public class CircleList
{
    public CircleListNode Head;
    public List<int> InnerList { get; }

    public int Count => InnerList.Count;

    public CircleList()
    {
        InnerList = new List<int>();
    }

    public void AddFirst(int value)
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

    public bool TryGetRowOfDigits(int searchValue, int countForRow, out int startIndex, out int count)
    {
        if (InnerList.Count(x => x == searchValue) == Count)
        {
            startIndex = 0;
            count = Count;
            return true;
        }

        var circle = Head;
        var offset = 0;
        while (circle.Value == searchValue)
        {
            offset++;
            circle = circle.Next;
        }

        startIndex = 0;
        var isRange = false;
        count = 0;
        for (var i = 0; i < InnerList.Count; i++)
        {
            if (circle.Value == searchValue && !isRange)
            {
                startIndex = (i + offset) % InnerList.Count;
                isRange = true;
                count++;
            }
            else if (circle.Value == searchValue && isRange)
            {
                count++;
            }

            if (InnerList[(i + offset) % InnerList.Count] != searchValue && isRange
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

    public void AddAt(int index, int value)
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
    }

    public CircleListNode FindLast()
    {
        var el = Head;
        while (el.Next != Head)
        {
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

    public void RemoveAt(int index, int count = 1)
    {
        if (index < 0 || index > InnerList.Count || count < 0 || count > InnerList.Count)
        {
            throw new ArgumentException("Bad arguments!");
        }


        var afterRemoved = FindAt(index + count);
        CircleListNode beforeRemoved;
        if (index == 0)
        {
            beforeRemoved = FindLast();
        }
        else
        {
            beforeRemoved = FindAt(index - 1);
        }

        if (index + count >= Count)
        {
            Head = afterRemoved;
        }

        beforeRemoved.Next = afterRemoved;

        if (Count - index < count)
        {
            var range1 = Count - index;
            InnerList.RemoveRange(index, range1);
            InnerList.RemoveRange(0, count - range1);
        }
        else
        {
            InnerList.RemoveRange(index, count);
        }
    }
}

public class CircleListNode
{
    public CircleListNode Next { get; set; }
    public int Value { get; }

    public CircleListNode(int value)
    {
        Value = value;
    }
}