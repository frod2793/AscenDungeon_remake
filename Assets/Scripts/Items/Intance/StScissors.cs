using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StScissors : Item
{
    private const string FrontLayerName = "Weapon";
    private const string  BackLayerName = "Weapon_CharBack";

    private const int Idle = 0;
    private const int AttackPrepare = 1;
    private const int Attack = 2;
    private const int AttackHolding = 3;
    private const int AttackDC = 4;
    private const int AttackCharge = 5;
    private const int AttackChargePrepare = 6;

    [Header("Item Action Property")]
    [SerializeField] private Animator _Animator;
    [SerializeField] private Area _CollisionArea;
    [SerializeField] private BoxCollider2D _BoxCollider;

    private int _AnimHash;
    private bool _IsAlreadyInit = false;
    private bool _CanAttack = true;

    private GameObject _Player;
    private Coroutine _AttackHolding;

    [SerializeField]
    private SpriteRenderer[] _Renderers;

    public override void AttackCancel()
    {
        AE_SetLayerOrderBack();

        _Animator.SetInteger(_AnimHash, Idle);
        _Animator.Play("Idle");
    }
    public override void AttackAction(GameObject attacker, ICombatable combatable)
    {
        _Player = attacker;

        if (_Animator.GetInteger(_AnimHash) == AttackHolding && _CanAttack)
        {
            _AttackHolding.StopRoutine();
            _Animator.SetInteger(_AnimHash, Attack);
        }
        else if (_Animator.GetInteger(_AnimHash) == Idle)
            _Animator.SetInteger(_AnimHash, AttackPrepare);
    }
    public override void OffEquipThis(SlotType offSlot)
    {
        switch (offSlot)
        {
            case SlotType.Accessory:
                Inventory.Instance.AttackEvent -= Accessory_AttackEvent;
                break;

            case SlotType.Weapon:
                Inventory.Instance.ChargeBeginAction -= Weapon_ChargeBeginAction;
                Inventory.Instance.ChargeEndAction   -= Weapon_ChargeEndAction;
                break;
        }
    }
    public override void OnEquipThis(SlotType onSlot)
    {
        if (!_IsAlreadyInit)
        {
            _AnimHash = _Animator.GetParameter(0).nameHash;
            _IsAlreadyInit = true;

            _AttackHolding = new Coroutine(this);
        }
        switch (onSlot)
        {
            case SlotType.Accessory:
                Inventory.Instance.AttackEvent += Accessory_AttackEvent;
                break;

            case SlotType.Weapon:
                if (!_IsAlreadyInit)
                {
                    _AnimHash = _Animator.GetParameter(0).nameHash;
                    _IsAlreadyInit = true;

                    _Player = transform.root.gameObject;
                }
                _CollisionArea.SetEnterAction(HitAction);

                Inventory.Instance.ChargeBeginAction += Weapon_ChargeBeginAction;
                Inventory.Instance.ChargeEndAction += Weapon_ChargeEndAction;
                break;
        }
    }

    private void Weapon_ChargeEndAction(float charge)
    {
        if (_Animator.GetInteger(_AnimHash) == AttackChargePrepare) {
            _Animator.SetInteger(_AnimHash, AttackCharge);
        }
    }
    private void Weapon_ChargeBeginAction()
    {
        if (_Animator.GetInteger(_AnimHash) == Idle)
            _Animator.SetInteger(_AnimHash, AttackChargePrepare);
    }
    
    private void HitAction(GameObject enter)
    {
        if (enter.TryGetComponent(out ICombatable combatable))
        {
            combatable.Damaged(StatTable[ItemStat.AttackPower], _Player);
            Inventory.Instance.OnAttackEvent(_Player, combatable);
        }
    }
    private void Accessory_AttackEvent(GameObject a, ICombatable v) 
    {
        v.Damaged(5f, a);
    }
    private IEnumerator AttackHoldingRoutine()
    {
        for (float i = 0f; i < 1f; i += Time.deltaTime * Time.timeScale)
            yield return null;

        if (_Animator.GetInteger(_AnimHash) == AttackHolding)
            _Animator.SetInteger(_AnimHash, AttackDC);

        _AttackHolding.Finish();
    }
    private void AE_SetAttackState()
    {
        _Animator.SetInteger(_AnimHash, Attack);
    }
    private void AE_AttackBegin()
    {
        _CanAttack = false;
        _Animator.SetInteger(_AnimHash, AttackHolding);
    }
    private void AE_AttackEnd()
    {
        _CanAttack = true;
        _AttackHolding.StartRoutine(AttackHoldingRoutine());
    }
    private void AE_AttackAction()
    {
        Collider2D[] result = new Collider2D[12];

        ContactFilter2D contact = new ContactFilter2D() { useTriggers = true };
        int count = _BoxCollider.OverlapCollider(contact, result);

        for (int i = 0; i < count; i++)
        {
            if (!result[i].CompareTag("Enemy")) continue;
            if (!result[i].TryGetComponent(out ICombatable combatable)) continue;

            combatable.Damaged(StatTable[ItemStat.AttackPower], _Player);
            Inventory.Instance.OnAttackEvent(_Player, combatable);
        }
    }
    private void AE_SetLayerOrderFront()
    {
        _Renderers[0].sortingLayerName = FrontLayerName;
        _Renderers[1].sortingLayerName = FrontLayerName;
    }
    private void AE_SetLayerOrderBack()
    {
        _Renderers[0].sortingLayerName = BackLayerName;
        _Renderers[1].sortingLayerName = BackLayerName;
    }
    protected override void AttackAnimationPlayOver()
    {
        _Animator.SetInteger(_AnimHash, Idle);
        base.AttackAnimationPlayOver();
    }
}
