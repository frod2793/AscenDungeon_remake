using System;
using System.Collections.Generic;
using UnityEngine;

public class Pool<T> where T : MonoBehaviour
{
    private  Stack<T> _Pool;
    private Action<T> _ObjectInitAction;

    private T _OriginalObject;

    public void Init(int hold, T origin, Action<T> objectInitAction = null)
    {
        _OriginalObject = origin;

        _ObjectInitAction = objectInitAction;

        if (_Pool == null)
        {
            _Pool = new Stack<T>();
        }
        CreatePoolObject(hold);
    }

    public void Add(T obj)
    {
        obj.gameObject.SetActive(false);

        _Pool.Push(obj);
    }

    public T Get()
    {
        if (_Pool.Count == 0)
        {
            CreatePoolObject();
        }
        _Pool.Peek().gameObject.SetActive(true);

        return _Pool.Pop();
    }

    private void CreatePoolObject(int count = 1)
    {
        for (int i = 0; i < count; ++i)
        {
            var _object = UnityEngine.Object.Instantiate(_OriginalObject);

            _ObjectInitAction?.Invoke(_object);
            _object.gameObject.SetActive(false);

            _Pool.Push(_object);
        }
    }
}
