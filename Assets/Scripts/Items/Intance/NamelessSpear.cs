using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NamelessSpear : Item
{
    private const int NeedAttackStack_Weapon    = 3;
    private const int NeedAttackStack_Accessory = 5;

    private const int Idle   = 0;
    private const int Attack = 1;
    private const int Special = 2;

    private const string FrontLayerName = "Weapon";
    private const string  BackLayerName = "Weapon_CharBack";

    private readonly Quaternion EffectRotate2Left  = Quaternion.Euler(0, 0, -90);
    private readonly Quaternion EffectRotate2Right = Quaternion.Euler(0, 0,  90);

    [Header("Item Action Property")]
    [SerializeField] private Animator _Animator;
    [SerializeField] private Area _CollisionArea;
    [SerializeField] private SpriteRenderer _Renderer;

    private Player _Player;

    private int _AnimHash;
    private int _AttackStack;

    private bool _IsAlreadyInit = false;

    public override void AttackCancel()
    {
        _Animator.SetInteger(_AnimHash, Idle);
        _Animator.Play("Idle");

        AE_SetLayerOrderBack();
    }
    public override void AttackAction(GameObject attacker, ICombatable combatable)
    {
        if (_Animator.GetInteger(_AnimHash) == Idle) 
        {
            if (_AttackStack == NeedAttackStack_Weapon - 1)
            {
                _AttackStack = 0;
                _Animator.SetInteger(_AnimHash, Special);
            }
            else
                _Animator.SetInteger(_AnimHash, Attack);
        }
    }
    public override void OffEquipThis(SlotType offSlot)
    {
        switch (offSlot)
        {
            case SlotType.Accessory:
                {
                    Inventory.Instance.OrderAttackActionEvent -= Accessory_OrderAttackActionEvent;
                    Inventory.Instance.AttackEvent -= Accessory_AttackEvent;
                }
                break;
            case SlotType.Weapon:
                break;
        }
    }
    public override void OnEquipThis(SlotType onSlot)
    {
        _AttackStack = 0;

        switch (onSlot)
        {
            case SlotType.Accessory:
                {
                    Inventory.Instance.OrderAttackActionEvent += Accessory_OrderAttackActionEvent;
                    Inventory.Instance.AttackEvent += Accessory_AttackEvent;
                }
                break;
            case SlotType.Weapon:
                {
                    if (!_IsAlreadyInit)
                    {
                        _AnimHash = _Animator.GetParameter(0).nameHash;
                        _IsAlreadyInit = true;

                        transform.root.TryGetComponent(out _Player);
                        _CollisionArea.SetEnterAction(HitAction);
                    }
                }
                break;
        }
    }

    #region 오직 액세서리 슬롯에 장비했을 때에만 발동
    bool _IsAttackTakePlace = false;

    private void Accessory_OrderAttackActionEvent()
    {
        _IsAttackTakePlace = false;
    }
    private void Accessory_AttackEvent(GameObject a, ICombatable v)
    {
        if (!_IsAttackTakePlace)
        {
            _AttackStack += _AttackStack == NeedAttackStack_Accessory ? 0 : 1;
            _IsAttackTakePlace = true;
        }
        if (_AttackStack == NeedAttackStack_Accessory) 
        {
            OnSpecialSkill();
            _AttackStack = 0;
        }
    }
    #endregion

    private void HitAction(GameObject enter)
    {
        if (enter.TryGetComponent(out ICombatable combatable))
        {
            combatable.Damaged(StatTable[ItemStat.AttackPower], _Player.gameObject);
            Inventory.Instance.AttackAction(_Player.gameObject, combatable);
        }
    }
    private void OnSpecialSkill()
    {
        SoundManager.Instance.PlaySound(SoundName.LightningTotem);

        for (int i = 0; i < 3; i++)
        {
            var list = Castle.Instance.GetFloorRooms()[i].GetIObjects();
            float y = Castle.Instance.GetMovePointY((UnitizedPosV)i) + 0.75f;

            foreach (var enemy in list)
            {
                ICombatable enemyCombat = enemy as ICombatable;

                if (enemyCombat == null) continue;

                enemyCombat.CastBuff(Buff.Stun, BuffLibrary.Instance.Stun(1f, enemyCombat.GetAbility));
                enemyCombat.Damaged(5f, gameObject);

                EffectLibrary.Instance.UsingEffect
                    (EffectKind.NamelessLightning, new Vector2(enemy.ThisObject().transform.position.x, y));
            }
        }
    }
    protected override void AttackAnimationPlayOver()
    {
        ++_AttackStack;

        _Animator.SetInteger(_AnimHash, Idle);
        base.AttackAnimationPlayOver();
    }
    private void AE_UseAttackEffect()
    {
        MainCamera.Instance.Shake(0.4f, 1.2f);

        Effect effect = EffectLibrary.Instance.UsingEffect
            (EffectKind.SwordAfterImage, _EffectSummonPoint.position);

        effect.transform.rotation = _Player.IsLookAtLeft() ? 
            EffectRotate2Left : EffectRotate2Right;
    }
    private void AE_Special_Shake()
    {
        MainCamera.Instance.Shake(0.18f, 1f);
    }
    private void AE_Special_Method()
    {
        MainCamera.Instance.Shake(0.6f, 1f);
        _AttackStack = 0;

        OnSpecialSkill();
    }
    private void AE_Special_End()
    {
        _Animator.SetInteger(_AnimHash, Idle);
    }
    private void AE_SetLayerOrderFront()
    {
        _Renderer.sortingLayerName = FrontLayerName;
    }
    private void AE_SetLayerOrderBack()
    {
        _Renderer.sortingLayerName = BackLayerName;
    }
}
