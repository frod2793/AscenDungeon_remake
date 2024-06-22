using System;
using UnityEngine;

public class AttackModule : MonoBehaviour
{
    protected AbilityTable _AbilityTable;
    protected AttackPeriod _AttackPeriod;

    protected GameObject _User;

    public bool IsPeriodProgressing()
    {
        return _AttackPeriod.IsProgressing();
    }
    public void PeriodAttackPartOver()
    {
        _AttackPeriod.AttackActionOver();
    }
    public void SetPeriodAction(Period period, Action action)
    {
        _AttackPeriod.SetAction(period, action);
    }
    public void SetActivePeriod(bool isActive)
    {
        if (isActive)
        {
            _AttackPeriod.StartPeriod();
        }
        else
        {
            _AttackPeriod.StopPeriod();
        }
    }
    public void Init(GameObject user, AbilityTable abilityTable)
    {
        _User = user;

        _AbilityTable = abilityTable;
        _AttackPeriod = new AttackPeriod(abilityTable);
    }
}
