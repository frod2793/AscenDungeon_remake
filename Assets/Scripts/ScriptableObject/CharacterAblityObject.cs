using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterAblityObject", menuName = "ScriptableObject/CharacterAblityObject")]
public class CharacterAblityObject : ScriptableObject
{
    private const string JsonTableName = "CharacterAbility";

    public float this[Ability ability]
    {
        get
        {
            switch (ability)
            {
                case Ability.MoveSpeed: return _MoveSpeed;
                case Ability.MaxHealth: return _MaxHealth;
                case Ability.AttackPower: return _AttackPower;
                case Ability.Begin_AttackDelay: return _BeginAttackDelay;
                case Ability.After_AttackDelay: return _AfterAttackDelay;
                case Ability.Range: return _Range;
            }
            return 0f;
        }
    }

    [SerializeField] private string JsonLabelName;

    [Header("Data Section")]
    [SerializeField] private float _MoveSpeed;
    [SerializeField] private float _MaxHealth;
    [SerializeField] private float _AttackPower;
    [SerializeField] private float _BeginAttackDelay;
    [SerializeField] private float _AfterAttackDelay;
    [SerializeField] private float _Range;

    [SerializeField] private RecognitionArea _Area;
                     public  RecognitionArea  Area => _Area;

    public void Init()
    {
        string JsonString(string s)
        {
            return DataUtil.GetDataValue(JsonTableName, "ID", JsonLabelName, s);
        }
        _MoveSpeed = float.Parse(JsonString("MoveSpeed"));
        _MaxHealth = float.Parse(JsonString("MaxHealth"));
        _AttackPower = float.Parse(JsonString("AttackPower"));
        _BeginAttackDelay = float.Parse(JsonString("Begin_AttackDelay"));
        _AfterAttackDelay = float.Parse(JsonString("After_AttackDelay"));
        _Range = float.Parse(JsonString("Range"));

        _Area = (RecognitionArea)
            System.Enum.Parse(typeof(RecognitionArea) ,JsonString("RecognitionArea"));
    }
}