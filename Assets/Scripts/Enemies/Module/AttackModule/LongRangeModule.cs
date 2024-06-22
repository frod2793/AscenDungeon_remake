using System;
using UnityEngine;

public class LongRangeModule : AttackModule
{
    public event Action<ICombatable> HitEvent;

    public event Action<GameObject> TargetHitEvent;
    public event Action<Projection>  LifeOverEvent;

    [SerializeField] private Area _Range;

    [Header("Prohection Section")]
    [SerializeField] private Projection _Projection;
    [SerializeField] private int _HoldingCount;

    [Header("Shoot Section")]
    [SerializeField] private Vector3   _ShootPosition;
    [SerializeField] private Transform _ShootPivot;
    [SerializeField] private float     _ShootSpeed;
    
    private Pool<Projection> _Pool;

    public bool RangeHasAny()
    {
        return _Range.HasAny();
    }
    public void RunningDrive()
    {
        // Pool Setting
        _Pool = new Pool<Projection>();
        _Pool.Init(_HoldingCount, _Projection, pro => 
        {
            pro.SetAction(
                hit => TargetHitEvent?.Invoke(hit), 
                lif =>  LifeOverEvent?.Invoke(lif));
        });

        // Default Event Setting
        TargetHitEvent += hit =>
        {
            if (hit.TryGetComponent(out ICombatable combatable))
            {
                HitEvent?.Invoke(combatable);
                combatable.Damaged(_AbilityTable.AttackPower, _User);
            }
        };
        LifeOverEvent += lif => _Pool.Add(lif);

        _Range.SetScale(_AbilityTable.Range);
    }
    public void ShootProjection(Vector2 direction)
    {
        var projection = _Pool.Get();

        projection.Shoot(_ShootPivot.position + _ShootPosition, direction, _ShootSpeed);
    }
    public void AddTargetHitAction(Action<GameObject> action)
    {
        TargetHitEvent += hit => action.Invoke(hit);
    }
    public void AddLifeOverAction(Action<Projection> action)
    {
        LifeOverEvent += lif => action.Invoke(lif);
    }
}
