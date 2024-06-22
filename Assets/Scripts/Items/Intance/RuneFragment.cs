using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuneFragment : Item
{
    private readonly Vector3 EquipPosition = new Vector3(0.03f, 0.15f, 0f);
    private readonly Quaternion RightRotation = Quaternion.Euler(0, 180, 0);

    [Header("RuneFragment Property")]
    [SerializeField] private Animator _Animator;
    private int _AnimatorHash;

    [SerializeField] private Area _AttackArea;
    [SerializeField, Range(0f, 1f)] private float _NeedCharge;

    [Header("RedStorm Property")]
    [SerializeField] private Projection _RedStorm;
    [SerializeField] private float _RedStormSpeed;
    [SerializeField] private float _RedStormOffsetY;
    private Pool<Projection> _RedStormPool;

    private float _ChargingPower = 0f;
    private bool _IsAlreadyInit = false;
    private GameObject _Player;

    private enum Anim
    {
        Default, Attack, Charging_Begin, Charging_End
    }
    public override void AttackAction(GameObject attacker, ICombatable combatable)
    {
        _Animator.SetInteger(_AnimatorHash, (int)Anim.Attack);
    }
    protected override void AttackAnimationPlayOver()
    {
        base.AttackAnimationPlayOver();

        _Animator.SetInteger(_AnimatorHash, (int)Anim.Default);
    }
    public override void AttackCancel()
    {
        _Animator.SetInteger(_AnimatorHash, (int)Anim.Default);
    }
    public override void OffEquipThis(SlotType offSlot)
    {
        switch (offSlot)
        {
            case SlotType.Accessory:
                Inventory.Instance.PlayerEnterFloorEvent -= PlayerEnterFloorEvent;
                break;

            case SlotType.Weapon:
                Inventory.Instance.ChargeBeginAction -= ChargeBeginAction;
                Inventory.Instance.ChargeEndAction -= ChargeEndAction;
                break;
        }
    }
    public override void OnEquipThis(SlotType onSlot)
    {
        Init();

        switch (onSlot)
        {
            case SlotType.Accessory:
                Inventory.Instance.PlayerEnterFloorEvent += PlayerEnterFloorEvent;
                break;

            case SlotType.Weapon:
                transform.localPosition = EquipPosition;

                if (_Player == null) {
                    _Player = transform.parent.parent.gameObject;
                }
                Inventory.Instance.ChargeBeginAction += ChargeBeginAction;
                Inventory.Instance.ChargeEndAction   += ChargeEndAction;
                break;
        }
    }

    private void PlayerEnterFloorEvent()
    {
        MainCamera.Instance.Shake(0.6f, 0.5f);

        ShootRedStorm(UnitizedPosV.BOT, (Vector2.right * (Random.value - Random.value)).normalized);
    }
    private void ChargeBeginAction()
    {
        _Animator.SetInteger(_AnimatorHash, (int)Anim.Charging_Begin);
    }
    private void ChargeEndAction(float charge)
    {
        if (charge >= _NeedCharge)
        {
            _ChargingPower = charge;
            _Animator.SetInteger(_AnimatorHash, (int)Anim.Charging_End);
        }
        else 
            _Animator.SetInteger(_AnimatorHash, (int)Anim.Default);
    }

    private void Init()
    {
        if (!_IsAlreadyInit)
        {
            _IsAlreadyInit = true;
            _AnimatorHash  = _Animator.GetParameter(0).nameHash;

            _AttackArea.SetEnterAction(o => 
            {
                if (o.TryGetComponent(out ICombatable combatable))
                {
                    combatable.Damaged(StatTable[ItemStat.AttackPower], _Player);
                    Inventory.Instance.OnAttackEvent(_Player, combatable);
                }
            });
            _RedStormPool = new Pool<Projection>();
            _RedStormPool.Init(4, _RedStorm, o => 
            {
                o.SetAction(hit => {
                    
                    if (hit.TryGetComponent(out ICombatable combatable))
                    {
                        float damage = StatTable[ItemStat.AttackPower];

                        combatable.Damaged(damage, _Player);
                        Inventory.Instance.ProjectionHit(hit, damage);
                    }

                }, lif => 
                {
                    _RedStormPool.Add(lif);
                });
            });
        }
    }
    private void ChargingSkill()
    {
        MainCamera.Instance.Shake(1.3f, 1f);

        for (UnitizedPosV i = UnitizedPosV.TOP; i <= UnitizedPosV.BOT; i++)
        {
            ShootRedStorm(i, (Vector2.right * (Random.value - Random.value)).normalized);
        }
        _ChargingPower = 0f;
    }
    private void ShootRedStorm(UnitizedPosV point, Vector2 direction)
    {
        Projection projection = _RedStormPool.Get();
        Vector2 shootPoint;

        if (direction.x < 0)
        {
            float maxX = Castle.Instance.GetMovePoint(UnitizedPos.MID_RIGHT).x + 3f;

            projection.transform.rotation = RightRotation;
            shootPoint = new Vector2(maxX, Castle.Instance.GetMovePointY(point) + _RedStormOffsetY);
        }
        else
        {
            float minX = Castle.Instance.GetMovePoint(UnitizedPos.MID_LEFT).x - 3f;

            projection.transform.rotation = Quaternion.identity;
            shootPoint = new Vector2(minX, Castle.Instance.GetMovePointY(point) + _RedStormOffsetY);
        }
        projection.Shoot(shootPoint, direction, _RedStormSpeed);
    }
}
