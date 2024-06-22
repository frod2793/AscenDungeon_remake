using UnityEngine;

public class LongSword : Item
{
    [Header("Item Action Property")]
    [SerializeField] private Animator Animator;
    [SerializeField] private Area CollisionArea;

    [Header("SwordDance Property")]
    [SerializeField] private Projection _SwordDance;
    [SerializeField, Range(1f, 3f)] private float _DamageScale;
    [SerializeField, Range(1f, 9f)] private float _Speed;

    private Pool<Projection> _SwordDancePool;

    private int _AnimPlayKey;
    private int _AnimControlKey;

    private GameObject mPlayer;
    private bool _IsAttackOver = true;
    private bool _CanUsingSwordDance = false;

    private void Reset()
    {
        Debug.Assert(TryGetComponent(out Animator));
    }
    public override void OffEquipThis(SlotType offSlot)
    {
        if (offSlot.Equals(SlotType.Weapon))
        {
            AttackOverAction = null;
            Inventory.Instance.ChargeEndAction -= ChargeAction;
        }
    }
    protected override void AttackAnimationPlayOver()
    {
        base.AttackAnimationPlayOver();

        MainCamera.Instance.Shake(0.3f, 0.3f);

        Effect effect = EffectLibrary.Instance.UsingEffect(EffectKind.Swing, _EffectSummonPoint.position);

        effect.transform.localRotation = transform.rotation;
        effect.transform.localScale = Vector3.one * 1.3f;

        _IsAttackOver = true;
    }
    public override void OnEquipThis(SlotType onSlot)
    {
        if (onSlot.Equals(SlotType.Weapon))
        {
            CollisionArea.SetEnterAction(HitAction);

            _AnimPlayKey    = Animator.GetParameter(0).nameHash;
            _AnimControlKey = Animator.GetParameter(1).nameHash;

            mPlayer = mPlayer ?? transform.parent.parent.gameObject;

            if (_SwordDancePool == null)
            {
                _SwordDancePool = new Pool<Projection>();
                _SwordDancePool.Init(2, _SwordDance, o => 
                {
                    o.transform.localScale = Vector2.one * 0.7f;
                    o.SetAction(hit =>
                    {
                        if (hit.TryGetComponent(out ICombatable combatable))
                        {
                            float damage = StatTable[ItemStat.AttackPower] * _DamageScale;
                            combatable.Damaged(damage, mPlayer.gameObject);

                            Inventory.Instance.ProjectionHit(hit, damage);
                        }
                    }, p => _SwordDancePool.Add(p));
                });
            }
            Inventory.Instance.ChargeEndAction += ChargeAction;
        }
    }
    public void ShootSwordDance()
    {
        if (_CanUsingSwordDance)
        {
            var projction = _SwordDancePool.Get();

            var direction = Vector2.right;

            if (mPlayer.transform.localRotation.eulerAngles.y > 0f)
            {
                direction = Vector2.left;
            }
            projction.Shoot(transform.position + new Vector3(0.1f, 0.2f), direction, _Speed);
            projction.transform.localRotation = mPlayer.transform.localRotation;

            _CanUsingSwordDance = false;
        }
    }
    public override void AttackAction(GameObject attacker, ICombatable combatable)
    {
        Animator.SetBool(_AnimPlayKey, true);
        Animator.SetBool(_AnimControlKey, !Animator.GetBool(_AnimControlKey));

        _IsAttackOver = false;
    }
    private void HitAction(GameObject hitObject)
    {
        if (hitObject.TryGetComponent(out ICombatable combatable))
        {
            combatable.Damaged(StatTable[ItemStat.AttackPower], mPlayer);

            Inventory.Instance.OnAttackEvent(mPlayer, combatable);
        }
    }
    private void ChargeAction(float charge)
    {
        AttackAction(null, null);
        _CanUsingSwordDance = true;
    }
    public override void AttackCancel()
    {
        if (!_IsAttackOver)
        {
            Animator.SetBool(_AnimPlayKey, false);
            Animator.SetBool(_AnimControlKey, true);
        }
    }
}