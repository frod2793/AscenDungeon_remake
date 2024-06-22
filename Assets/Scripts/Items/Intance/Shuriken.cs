using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackType
{
    Continuous, ThreeDirection
}

public class Shuriken : Item
{
    [SerializeField] private Animator Animator;
    [SerializeField] private Projection Projection;

    [Header("Attack Info")]
    [Range(0f, 180f)]
    [SerializeField] private float BetweenAngle;
    [SerializeField] private float ShootSpeed;
    [SerializeField] private AttackType AttackType;

    private Pool<Projection> _ShurikenPool;
    private bool _IsAlreadyInit = false;

    private GameObject _Player;

    public override void AttackAction(GameObject attacker, ICombatable combatable)
    {
        Animator.SetBool   (Animator.GetParameter(0).nameHash, true);
        Animator.SetInteger(Animator.GetParameter(1).nameHash, (int)AttackType);
    }

    public override void OffEquipThis(SlotType offSlot)
    {
        switch (offSlot)
        {
            case SlotType.Accessory:
                {
                    Inventory.Instance.DashBeginEvent -= DashEvent;
                }
                break;

            case SlotType.Weapon:
                {
                    Inventory.Instance.ChargeEndAction += ChargeAction;
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
                {
                    Inventory.Instance.DashBeginEvent += DashEvent;
                }
                break;

            case SlotType.Weapon:
                {
                    Inventory.Instance.ChargeEndAction += ChargeAction;
                }
                break;
        }
    }
    private void DashEvent(Direction direction)
    {
        Vector2 dir = Vector2.zero;
        switch (direction)
        {
            case Direction.Right:
                dir = Vector2.right;
                break;
            case Direction.Left:
                dir = Vector2.left;
                break;
        }
        _ShurikenPool.Get().Shoot(_Player.transform.position, dir, ShootSpeed);
        SoundManager.Instance.PlaySound(SoundName.ThrowShuriken);
    }
    private void ShootShuriken()
    {
        var direction = Vector2.right;

        if (_Player.transform.localRotation.eulerAngles.y > 0f)
        {
            direction = Vector2.left;
        }
        switch (AttackType)
        {
            case AttackType.Continuous:
                _ShurikenPool.Get().Shoot(transform.GetChild(0).position, direction, ShootSpeed);
                MainCamera.Instance.Shake(0.1f, 0.8f);
                break;

            case AttackType.ThreeDirection:

                float betweenAngle = BetweenAngle;

                if (direction.x > 0)
                {
                    betweenAngle = 135f - BetweenAngle;
                }
                _ShurikenPool.Get().Shoot(transform.GetChild(0).position, direction, ShootSpeed);

                Vector2 offset = (new Vector2(Mathf.Cos(-betweenAngle), Mathf.Sin(-betweenAngle)) * Mathf.Rad2Deg).normalized;
                _ShurikenPool.Get().Shoot(transform.GetChild(0).position, (direction + offset).normalized, ShootSpeed);

                        offset = (new Vector2(Mathf.Cos(+betweenAngle), Mathf.Sin(+betweenAngle)) * Mathf.Rad2Deg).normalized;
                _ShurikenPool.Get().Shoot(transform.GetChild(0).position, (direction + offset).normalized, ShootSpeed);

                MainCamera.Instance.Shake(0.2f, 0.8f);
                break;
        }
        SoundManager.Instance.PlaySound(SoundName.ThrowShuriken);
    }

    // 공격방식 스왑
    private void ChargeAction(float charge)
    {
        if (charge >= 0.3f)
        {
            AttackType = AttackType == AttackType.Continuous ?
                    AttackType.ThreeDirection : AttackType.Continuous;
        }
    }

    private IEnumerator EAccessory_MoveBeginAction(Vector2 dir)
    {
        yield return null;
    }

    private void Init()
    {
        if (!_IsAlreadyInit)
        {
            _Player = GameObject.FindGameObjectWithTag("Player");

            _IsAlreadyInit = true;

            _ShurikenPool = new Pool<Projection>();
            _ShurikenPool.Init(3, Projection, o => 
            {
                o.transform.parent = ItemStateSaver.Instance.transform;

                o.SetAction(
                    hit => 
                    {
                        if (hit.TryGetComponent(out ICombatable combatable))
                        {
                            combatable.Damaged(StatTable[ItemStat.AttackPower], _Player);
                            Inventory.Instance.OnAttackEvent(_Player, combatable);
                            Inventory.Instance.ProjectionHit(hit, StatTable[ItemStat.AttackPower]);

                            MainCamera.Instance.Shake(0.1f, 0.8f);
                        }
                    }, 
                    pro => 
                    {
                        _ShurikenPool.Add(pro);
                    });
            });
        }
    }

    protected override void AttackAnimationPlayOver()
    {
        base.AttackAnimationPlayOver();

        Animator.SetBool(Animator.GetParameter(0).nameHash, false);
    }

    public override void AttackCancel()
    {
        Animator.SetBool(Animator.GetParameter(0).nameHash, false);
    }
}
