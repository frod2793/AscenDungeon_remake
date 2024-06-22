using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class WindDagger : Item
{
    private const int Idle = 0;
    private const int Attack_1 = 1;
    private const int Attack_2 = 2;

    private const float IncreaseAttackSpeed = 0.25f;

    [Header("Item Action Property")]
    [SerializeField] private Animator _Animator;
    [SerializeField] private Area _CollisionArea;

    private int _AnimHash;
    private bool _IsAlreadyInit = false;

    private GameObject _Player;

    public override void AttackCancel()
    {
        _Animator.SetInteger(_AnimHash, Idle);
        _Animator.Play("Idle");
    }
    public override void AttackAction(GameObject attacker, ICombatable combatable)
    {
        _Player = attacker;

        switch (_Animator.GetInteger(_AnimHash))
        {
            case Idle:
                _Animator.SetInteger(_AnimHash, Attack_1);
                break;
            case Attack_1:
                _Animator.SetInteger(_AnimHash, Attack_2);
                break;
        }
    }
    public override void OffEquipThis(SlotType offSlot)
    {
        if (offSlot == SlotType.Accessory)
            Inventory.Instance.AttackSpeed -= IncreaseAttackSpeed;
    }
    public override void OnEquipThis(SlotType onSlot)
    {
        if (!_IsAlreadyInit)
        {
            _AnimHash = _Animator.GetParameter(0).nameHash;

            _CollisionArea.SetEnterAction(enter => 
            {
               if (enter.TryGetComponent(out ICombatable combatable))
               {
                    combatable.Damaged(StatTable[ItemStat.AttackPower], _Player);
                    Inventory.Instance.OnAttackEvent(_Player, combatable);
               } 
            });
            _IsAlreadyInit = true;
        }
        switch (onSlot)
        {
            case SlotType.Accessory:
                Inventory.Instance.AttackSpeed += IncreaseAttackSpeed;
                break;
            case SlotType.Weapon:
                _Player = transform.root.gameObject;
                break;
        }
    }
}
public partial class WindDagger
{
    private void AE_PlayEffect()
    {
        var effect = EffectLibrary.Instance.UsingEffect
            (EffectKind.Swing, _EffectSummonPoint.position);

        effect.transform.rotation = transform.rotation;
        effect.transform.localScale = Vector3.one;
    }
    private void AE_EndAttack2()
    {
        _Animator.SetInteger(_AnimHash, Idle);
    }
}
