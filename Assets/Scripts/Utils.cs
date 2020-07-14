using System;
using System.Collections.Generic;

public static class Utils
{
    public static void InsertAt<T>(this LinkedList<T> list, int index, T data)
    {
        if (index < 0 || index > list.Count)
        {
            throw new ArgumentException("Index cannot be less than 0, or greater than list count.");
        }

        if (index == 0)
        {
            list.AddFirst(data);
        }
        else if (index == list.Count)
        {
            list.AddLast(data);
        }
        else
        {
            var el = list.First;
            if (el is null)
            {
                throw new ArgumentException("Some element in list is null");
            }

            for (var i = 0; i < index; i++)
            {
                el = el.Next;
            }

            list.AddBefore(el, data);
        }
    }

    public static void RemoveRange<T>(this LinkedList<T> list, int index, int count)
    {
        if (index < 0 || index > list.Count)
        {
            throw new ArgumentException("Index cannot be less than 0, or greater than list count.");
        }

        if (index == 0)
        {
            for (var i = 0; i < count; i++)
            {
                list.RemoveFirst();
            }
        }

        
    }

    public static T GetElementAt<T>(this LinkedList<T> list, int index)
    {
        if (index < 0 || index > list.Count)
        {
            throw new ArgumentException("Index cannot be less than 0, or greater than list count.");
        }

        var el = list.First;
        if (index > 0 && index < list.Count)
        {
            for (var i = 0; i < index; i++)
            {
                el = el.Next;
            }
        }

        return el.Value;
    }

    public static LinkedListNode<T> GetNodeAt<T>(this LinkedList<T> list, int index)
    {
        if (index < 0 || index > list.Count)
        {
            throw new ArgumentException("Index cannot be less than 0, or greater than list count.");
        }

        var el = list.First;
        if (index > 0 && index < list.Count)
        {
            for (var i = 0; i < index; i++)
            {
                el = el.Next;
            }
        }

        return el;
    }
}