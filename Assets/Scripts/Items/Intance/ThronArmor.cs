using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThronArmor : Item
{
    [SerializeField] private Collider2D _Collider;
    [SerializeField] private Animator _Animator;
    [SerializeField, Range(0f, 1f)] private float _MirrorRate;

    private int _AnimatorHash;
    private bool _IsAlreadyInit = false;

    private GameObject _Player;

    public override void AttackCancel()
    {
        _Animator.SetBool(_AnimatorHash, false);
    }

    public override void OffEquipThis(SlotType offSlot)
    {
        switch (offSlot)
        {
            case SlotType.Accessory:
                Inventory.Instance.BeDamagedAction -= PlayerDamagedAction;
                break;

            case SlotType.Weapon:
                {
                    Inventory.Instance.DashBeginEvent -= DashBeginEvent;
                    Inventory.Instance.DashEndEvent -= DashEndEvent;
                }
                break;
        }
    }
    public override void OnEquipThis(SlotType onSlot)
    {
        Init();

        switch (onSlot)
        {
            case SlotType.Accessory:
                Inventory.Instance.BeDamagedAction += PlayerDamagedAction;
                break;

            case SlotType.Weapon:
                {
                    if (_Player == null) {
                        _Player = transform.parent.parent.gameObject;
                    }
                    Inventory.Instance.DashBeginEvent += DashBeginEvent;
                    Inventory.Instance.DashEndEvent += DashEndEvent;
                }
                break;
        }
    }
    private void Init()
    {
        if (!_IsAlreadyInit)
        {
            _AnimatorHash = _Animator.GetParameter(0).nameHash;
        }
    }
    private void DashBeginEvent(Direction direction)
    {
        _Collider.enabled = true;
        _Animator.SetBool(_AnimatorHash, true);
    }
    private void DashEndEvent(Direction direction)
    {
        _Collider.enabled = false;
        _Animator.SetBool(_AnimatorHash, false);
    }
    private void PlayerDamagedAction(ref float damage, GameObject attacker, GameObject victim)
    {
        if (attacker.TryGetComponent(out ICombatable combatable))
        {
            combatable.Damaged(damage * _MirrorRate, victim);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            if (collision.TryGetComponent(out ICombatable combatable))
            {
                MainCamera.Instance.Shake(0.3f, 0.5f);
                Inventory.Instance.OnAttackEvent(_Player, combatable);

                combatable.Damaged(StatTable[ItemStat.AttackPower], _Player);
            }
        }
    }
    protected override void AttackAnimationPlayOver()
    {
        _Animator.SetBool(_AnimatorHash, false);

        base.AttackAnimationPlayOver();
    }
}
