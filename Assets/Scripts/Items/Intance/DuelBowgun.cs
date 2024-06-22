using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class DuelBowgun : Item
{
    private const int Idle = 0;
    private const int Attack = 1;

    [SerializeField] private Animator _Animator;

    [Header("Projection Property")]
    [SerializeField] private float _ShootForce;
    [SerializeField] private Projection _Arrow;

    [Space()]
    [SerializeField] private Transform _ShootPoint_Front;
    [SerializeField] private Transform _ShootPoint_Back;

    private Pool<Projection> _Pool;
    private Player _Player;

    private int _AnimHash;
    private bool _IsAlreadyInit = false;

    public override void AttackCancel()
    {
        _Animator.SetInteger(_AnimHash, Idle);
        _Animator.Play("Idle");
    }
    public override void OffEquipThis(SlotType offSlot)
    { }
    public override void OnEquipThis(SlotType onSlot)
    {
        if (!_IsAlreadyInit)
        {
            _AnimHash = _Animator.GetParameter(0).nameHash;

            _Pool = new Pool<Projection>();
            _Pool.Init(2, _Arrow, o =>
            {
                o.SetAction(
                    hit => 
                    {
                        if (hit.TryGetComponent(out ICombatable combat)) 
                        {
                            float attackPower = StatTable[ItemStat.AttackPower];
                            combat.Damaged(attackPower, _Player.gameObject);

                            Inventory.Instance.ProjectionHit(hit, attackPower);
                            Inventory.Instance.OnAttackEvent(_Player.gameObject, combat);
                        }
                    }, 
                    pro => { _Pool.Add(pro); });
            });
            _IsAlreadyInit = true;
        }
    }

    public override void AttackAction(GameObject attacker, ICombatable combatable)
    {
        if (_Player == null)
            attacker.TryGetComponent(out _Player);

        if (_Animator.GetInteger(_AnimHash) == Idle) {
            _Animator.SetInteger(_AnimHash, Attack);
        }
    }
}

public partial class DuelBowgun
{
    private readonly Quaternion ArrowRotate2Left  = Quaternion.identity;
    private readonly Quaternion ArrowRotate2Right = Quaternion.Euler(0, 0, 180);

    protected override void AttackAnimationPlayOver()
    {
        _Animator.SetInteger(_AnimHash, Idle);
        base.AttackAnimationPlayOver();
    }
    private void AE_ShootBackBow() {
        ShootArrow(_ShootPoint_Back);
    }
    private void AE_ShootFrontBow() {
        ShootArrow(_ShootPoint_Front);
    }
    private void ShootArrow(Transform shootPoint)
    {
        bool isLeft = _Player.IsLookAtLeft();
        var arrow = _Pool.Get();

        arrow.transform.rotation = isLeft ? ArrowRotate2Left : ArrowRotate2Right;
        arrow.Shoot(shootPoint.position, isLeft ? Vector2.left : Vector2.right, _ShootForce);

        MainCamera.Instance.Shake(0.3f, 0.55f);
    }
}