using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Ability
{
    MoveSpeed, IMoveSpeed, CurHealth, MaxHealth,
    AttackPower, IAttackPower,
    Begin_AttackDelay, IBegin_AttackDelay,
    After_AttackDelay, IAfter_AttackDelay,
    Range, IRange,
    End
}
public class AbilityTable : MonoBehaviour
{
    private const string JsonTableName = "CharacterAbility";

    [SerializeField] private CharacterAblityObject AblityObject;
                     public CharacterAblityObject GetAblity => AblityObject;

    public float this[Ability ability]
    {
        get
        {
            if (mTable == null)
            {
                Init();
            }
            return mTable[ability];
        }
    }
    public  Dictionary<Ability, float>  Table
    {
        get
        {
            if (mTable == null) { Init(); }

            return mTable;
        }
    }
    private Dictionary<Ability, float> mTable;

    public  RecognitionArea  Area
    {
        get
        {
            if (mArea.Equals(RecognitionArea.Default))
            {
                Init();
            }
            return mArea;
        }
    }
    private RecognitionArea mArea;

    public bool CanRecognize(MESSAGE message)
    {
        switch (message)
        {
            case MESSAGE.THIS_ROOM:
                return Area.Equals(RecognitionArea.Room) || 
                       Area.Equals(RecognitionArea.Floor);

            case MESSAGE.BELONG_FLOOR:
                return Area.Equals(RecognitionArea.Floor);
        }
        return false;
    }

    public bool CantRecognize(MESSAGE message)
    {
        switch (message)
        {
            case MESSAGE.THIS_ROOM:
                return Area.Equals(RecognitionArea.Room);

            case MESSAGE.BELONG_FLOOR:
                return Area.Equals(RecognitionArea.Floor);
        }
        return false;
    }

    private void Init()
    {
        mTable = new Dictionary<Ability, float>();

        mArea = AblityObject.Area;

        for (Ability i = 0; i < Ability.End; ++i)
        {
            string abilityName = i.ToString();
                       
            if (abilityName[0].Equals('I')) {
                mTable.Add(i, default); 
            }
            else
            {
                if (i.Equals(Ability.CurHealth))
                {
                     mTable.Add(i, AblityObject[Ability.MaxHealth]);
                }
                else mTable.Add(i, AblityObject[i]);
            }
        }
    }

    public float MoveSpeed
    { get => Table[Ability.MoveSpeed] + Table[Ability.IMoveSpeed]; }
    public float AttackPower
    { get => Table[Ability.AttackPower] + Table[Ability.IAttackPower]; }
    public float BeginAttackDelay
    { get => Table[Ability.Begin_AttackDelay] + Table[Ability.IBegin_AttackDelay]; }
    public float AfterAttackDelay
    { get => Table[Ability.After_AttackDelay] + Table[Ability.IAfter_AttackDelay]; }
    public float Range
    { get => Table[Ability.Range] + Table[Ability.IRange]; }
}
