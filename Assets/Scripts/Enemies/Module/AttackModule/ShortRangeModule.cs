using System;
using UnityEngine;

public class ShortRangeModule : AttackModule
{
    public event Action<ICombatable> HitEvent;

    [SerializeField] private Area _Range;
    [SerializeField] private Area _AttackArea;

    public bool RangeHasAny()
    {
        return _Range.HasAny();
    }
    public void RunningDrive()
    {
        _AttackArea.SetEnterAction(o => 
        {
            if (o.TryGetComponent(out ICombatable combatable))
            {
                HitEvent?.Invoke(combatable);

                combatable.Damaged(_AbilityTable.AttackPower, _User);
            }
        });
        _Range.SetScale(_AbilityTable.Range);
    }
}
