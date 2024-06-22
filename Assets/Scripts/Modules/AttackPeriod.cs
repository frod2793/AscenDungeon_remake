using System;
using System.Collections;
using UnityEngine;

public enum Period { Begin, Attack, After };

public class AttackPeriod
{
    public Period CurrentPeriod
    { 
        get; 
        private set; 
    }

    private Action mEnterBeginAction;
    private Action mEnterAfterAction;
    private Action mEnterAttackAction;

    private AbilityTable mAbilityTable;

    private IEnumerator mEUpdate;
    private bool mIsAttackOver;

    public AttackPeriod(AbilityTable abilityTable)
    {
        mAbilityTable = abilityTable;

        CurrentPeriod = Period.Begin;
    }
    public void AttackActionOver()
    {
        mIsAttackOver = true;
    }

    public bool IsProgressing()
    {
        return mEUpdate != null;
    }
    public void SetAction(Period period, Action action)
    {
        switch (period)
        {
            case Period.Begin: mEnterBeginAction = action;
                break;

            case Period.After: mEnterAfterAction = action;
                break;

            case Period.Attack: mEnterAttackAction = action;
                break;
        }
    }

    public void StartPeriod()
    {
        if (mEUpdate == null) {
            mAbilityTable.StartCoroutine(mEUpdate = EUpdate());
        }
    }
    public void StopPeriod()
    {
        if (mEUpdate != null) {
            mAbilityTable.StopCoroutine(mEUpdate);

            mEUpdate = null;
        }
    }

    private IEnumerator EUpdate()
    {
        mIsAttackOver = false;

        float DeltaTime() {
            return Time.deltaTime * Time.timeScale;
        }
        CurrentPeriod = Period.Begin;
        mEnterBeginAction?.Invoke();
        for (float w = 0f; w < mAbilityTable.BeginAttackDelay; w += DeltaTime())
            yield return null;

        CurrentPeriod = Period.Attack;
        mEnterAttackAction?.Invoke();
        yield return new WaitUntil(() => mIsAttackOver);

        CurrentPeriod = Period.After;
        mEnterAfterAction?.Invoke();
        for (float w = 0f; w < mAbilityTable.AfterAttackDelay; w += DeltaTime())
            yield return null;

        mEUpdate = null;
        CurrentPeriod = Period.Begin;
        mIsAttackOver = false;
    }
}
