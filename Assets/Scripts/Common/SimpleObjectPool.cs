using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class SimpleObjectPool<T> where T : Component
{
    [SerializeField] private T prefab;

    [SerializeField] private Transform parent;

    private Stack<T> _instanceStack = new();
    private HashSet<T> _markedReturnSet = new();

    public SimpleObjectPool(T thePrefab)
    {
        prefab = thePrefab;
        parent = thePrefab.transform.parent;
    }

    public SimpleObjectPool(T prefab, Transform parent)
    {
        this.parent = parent;
        this.prefab = prefab;
    }

    public T GetObject()
    {
        T instance;
        if (_instanceStack.Count > 0)
            instance = _instanceStack.Pop();
        else
            instance = Object.Instantiate(prefab, parent);

        instance.transform.SetParent(parent);
        instance.gameObject.SetActive(true);

        return instance;
    }

    public void ReturnObject(T toReturn)
    {
        toReturn.gameObject.SetActive(false);
        _instanceStack.Push(toReturn);
    }

    public void MarkReturnObject(T toReturn)
    {
        _markedReturnSet.Add(toReturn);
    }

    public void ReturnMarkedObjects()
    {
        foreach (var item in _markedReturnSet) ReturnObject(item);
        _markedReturnSet.Clear();
    }
}