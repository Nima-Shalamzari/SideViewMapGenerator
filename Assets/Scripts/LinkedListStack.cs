using System.Collections.Generic;
using UnityEngine;

public class LinkedListStack<T> {
    private LinkedList<T> list = new LinkedList<T>();

    // Push operation
    public void Push(T item) {
        list.AddFirst(item);
    }

    // Pop operation
    public T Pop() {
        if (list.Count == 0) {
            throw new System.InvalidOperationException("The stack is empty.");
        }

        T value = list.First.Value;
        list.RemoveFirst();
        return value;
    }

    // Peek operation
    public T Peek() {
        if (list.Count == 0) {
            throw new System.InvalidOperationException("The stack is empty.");
        }

        return list.First.Value;
    }

    // Check if the stack is empty
    public bool IsEmpty() {
        return list.Count == 0;
    }

    // Get the count of items in the stack
    public int Count() {
        return list.Count;
    }
}
