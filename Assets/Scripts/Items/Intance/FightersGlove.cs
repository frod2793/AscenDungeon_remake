using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FightersGlove : Item
{
    private enum Anim {
        Default, Attack_Begin, Attack_End, Attack
    }

    [Header("FightersGlove Property")]
    [SerializeField] private Animator _Animator;
    private int _AnimatorHash;

    [SerializeField] private Area _LeftFist;
    [SerializeField] private Area _RightFist;

    private bool _IsAlreadyInit = false;
    private GameObject _Player;

    public override void AttackCancel()
    {
        _Animator.SetInteger(_AnimatorHash, (int)Anim.Default);
    }
    public override void OffEquipThis(SlotType offSlot)
    {
        switch (offSlot)
        {
            case SlotType.Accessory:
                Inventory.Instance.AttackEvent -= AttackEvent;
                break;

            case SlotType.Weapon:

                break;
        }
    }
    public override void OnEquipThis(SlotType onSlot)
    {
        Init();

        switch (onSlot)
        {
            case SlotType.Accessory:
                Inventory.Instance.AttackEvent += AttackEvent;
                break;

            case SlotType.Weapon:
                if (_Player == null) {
                    _Player = transform.parent.parent.gameObject;
                }
                break;
        }
    }
    public override void AttackAction(GameObject attacker, ICombatable combatable)
    {
        _Animator.SetInteger(_AnimatorHash, (int)Anim.Attack_Begin);
    }
    protected override void AttackAnimationPlayOver()
    {
        base.AttackAnimationPlayOver();
        _Animator.SetInteger(_AnimatorHash, (int)Anim.Attack_End);
    }
    protected override void CameraShake()
    {
        MainCamera.Instance.Shake(0.4f, 0.7f);
    }
    private void AttackEvent(GameObject attacker, ICombatable target)
    {
        target.Damaged(5f, attacker);
    }
    private void Init()
    {
        if (!_IsAlreadyInit)
        {
            _IsAlreadyInit = true;

             _LeftFist.SetEnterAction(Hit);
            _RightFist.SetEnterAction(Hit);

            _AnimatorHash = _Animator.GetParameter(0).nameHash;
        }
    }
    private void Hit(GameObject target)
    {
        if (target.TryGetComponent(out ICombatable combatable))
        {
            combatable.Damaged(StatTable[ItemStat.AttackPower], _Player);
            Inventory.Instance.OnAttackEvent(_Player, combatable);
        }
    }
}
