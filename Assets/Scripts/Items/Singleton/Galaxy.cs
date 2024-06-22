using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Galaxy : Singleton<Galaxy>
{
    [SerializeField] private Player _Player;
    [SerializeField] private Transform _OrbitObject;

    private Pool<Area> _MeteoPool 
        = new Pool<Area>();

    private List<Area> _EnabledMeteoList
         = new List<Area>();

    [HideInInspector] public float Resolving_Radius = 0.98f;
    [HideInInspector] public float Resolving_Speed = 130f;

    public  bool  IsAlreadyInit => _IsAlreadyInit;
    private bool _IsAlreadyInit = false;

    private IEnumerator _Resolving;
}
public partial class Galaxy
{
    private void Reset()
    {
        _Player = FindObjectOfType<Player>();
    }
    public void LazyInit(Area meteorite, Action<Area> meteoriteInit)
    {
        if (!_IsAlreadyInit)
        {
            _MeteoPool.Init(3, meteorite, meteoriteInit);
            _IsAlreadyInit = true;

            _OrbitObject.localScale = _Player.transform.localScale;
            _OrbitObject.localPosition = Vector3.zero;
        }
    }
    public void AddMeteorite(int count = 1)
    {
        var meteor = _MeteoPool.Get();

        _EnabledMeteoList.Add(meteor);

        meteor.transform.parent = _OrbitObject;
        meteor.transform.localScale = Vector3.one;

        if (--count > 0) {
            AddMeteorite(count);
        }
        if (_Resolving == null) {
            StartCoroutine(_Resolving = ResolvingRoutine());
        }
    }
    public void RemoveMeteorite(int count = 1)
    {
        _MeteoPool.Add(_EnabledMeteoList[0]);
        _EnabledMeteoList.RemoveAt(0);

        if (--count > 0) {
            RemoveMeteorite(count);
        }
        return;
    }
    private IEnumerator ResolvingRoutine()
    {
        float angle = 0f;

        _OrbitObject.gameObject.SetActive(true);
        do
        {
            float angleDistance = 360f / _EnabledMeteoList.Count;

            angle += Time.deltaTime * Time.timeScale * Resolving_Speed * Inventory.Instance.AttackSpeed;
            angle = Mathf.Abs(angle) > 360f ? angle % 360f : angle;

            for (int i = 0; i < _EnabledMeteoList.Count; ++i)
            {
                float rot = (angle + angleDistance * i + 90f * Mathf.Sign(Resolving_Speed)) * Mathf.Deg2Rad;

                _EnabledMeteoList[i].transform.localPosition =
                    new Vector2(Mathf.Cos(rot), Mathf.Sin(rot)) * Resolving_Radius;

                _EnabledMeteoList[i].transform.localRotation =
                    Quaternion.AngleAxis(angle + angleDistance * i, Vector3.forward);
            }
            yield return null;

            _OrbitObject.localPosition = _Player.transform.localPosition;

        } while (_EnabledMeteoList.Count != 0);

        _Resolving = null;
        _OrbitObject.gameObject.SetActive(false);
    }
}