using System;
using System.Collections.Generic;
using UnityEngine;

public enum ItemStat
{
    AttackPower, Range,
    Begin_AttackDelay, 
    After_AttackDelay,
    End
}
public class ItemStatTable : MonoBehaviour
{
    public float this[ItemStat stat]
    {
        get {
            if (!_IsAlreadyInit)
            {
                Init();
            }
            return _Table[stat];
        }
    }
    public  ItemRating  Rating
    {
        get
        {
            if (!_IsAlreadyInit)
            {
                Init();
            }
            return _Rating;
        }
    }
    private ItemRating _Rating;

    public  string  NameKR
    {
        get
        {
            if (!_IsAlreadyInit)
            {
                Init();
            }
            return _NameKR;
        }
    }
    private string _NameKR;

    public ItemID ID
    {
        get
        {
            return StatObject.ID;
        }
    }

    public  int  Cost
    {
        get
        {
            if (!_IsAlreadyInit)
            {
                Init();
            }
            return _Cost;
        }
    }
    private int _Cost;

    private const string JsonTableName = "ItemData";

    private Dictionary<ItemStat, float> _Table;

    private bool _IsAlreadyInit = false;

    [SerializeField] private ItemStatObject StatObject;

    private void Init()
    {
        _Table = new Dictionary<ItemStat, float>();

        _Table.Add(ItemStat.After_AttackDelay, StatObject.AfterAttackDelay);
        _Table.Add(ItemStat.Begin_AttackDelay, StatObject.BeginAttackDelay);

        _Table.Add(ItemStat.AttackPower, StatObject.AttackPower);
        _Table.Add(ItemStat.Range, StatObject.Range);

        _Rating = StatObject.Rating;
        _NameKR = StatObject.NameKR;

        _Cost = StatObject.Cost;

        _IsAlreadyInit = true;
    }
}
