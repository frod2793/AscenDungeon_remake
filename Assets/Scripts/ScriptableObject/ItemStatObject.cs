using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemStatObject", menuName = "ScriptableObject/ItemStatObject")]
public class ItemStatObject : ScriptableObject
{
    private const string JsonTableName = "ItemData";

    private Dictionary<ItemStat, float> _Table;

    [SerializeField] private string JsonLabelName;

    [Header("Data Section")]
    [SerializeField] private float _Range;
    [SerializeField] private float _AttackPower;
    [SerializeField] private float _BeginAttackDelay;
    [SerializeField] private float _AfterAttackDelay;

    [SerializeField] private ItemID _ID;
    [SerializeField] private ItemRating _Rating;
    [SerializeField] private string _NameKR;
    [SerializeField] private int _Cost;

    public float Range => _Range;
    public float AttackPower => _AttackPower;
    public float BeginAttackDelay => _BeginAttackDelay;
    public float AfterAttackDelay => _AfterAttackDelay;

    public ItemID ID => _ID;
    public ItemRating Rating => _Rating;
    public string NameKR => _NameKR;
    public int Cost => _Cost;

    public void Init()
    {
        _Table = new Dictionary<ItemStat, float>();

        string JsonString(string s)
        {
            return DataUtil.GetDataValue(JsonTableName, "ID", JsonLabelName, s);
        }
        _Range = float.Parse(JsonString("Range"));
        _AttackPower = float.Parse(JsonString("AttackPower"));
        _BeginAttackDelay = float.Parse(JsonString("Begin_AttackDelay"));
        _AfterAttackDelay = float.Parse(JsonString("After_AttackDelay"));

        _Rating = (ItemRating)System.Enum.Parse(typeof(ItemRating), JsonString("Rating"));
        _NameKR = JsonString("NameKR");
        _ID = (ItemID)System.Enum.Parse(typeof(ItemID), JsonLabelName);

        _Cost = int.Parse(JsonString("Cost"));
    }
}