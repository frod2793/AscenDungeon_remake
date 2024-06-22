using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrdinaryBow : Item
{
    [SerializeField] private Animator Animator;

    [Header("Arrow Info")]
    [SerializeField] private Projection Arrow;
    [SerializeField] private float ShootSpeed;
    [SerializeField] private Vector3 ShootOffset;

    private int mAnimPlayKey;

    private GameObject mPlayer;

    private Pool<Projection> mArrowPool;
    private bool mIsAlreadyInit = false;

    public override void AttackAction(GameObject attacker, ICombatable combatable)
    {
        mPlayer = attacker;

        Animator.SetBool(mAnimPlayKey, true);
        SoundManager.Instance.PlaySound(SoundName.BowPull);
    }

    public override void OffEquipThis(SlotType offSlot)
    {
        switch (offSlot)
        {
            case SlotType.Accessory:
                {
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
                    Inventory.Instance.ProjectionHitEvent += ProjectionHitEvent;
                }
                break;
        }
    }

    private void ProjectionHitEvent(GameObject victim, float damage)
    {
        if (victim.TryGetComponent(out ICombatable combatable))
        {
            combatable.Damaged(5f, mPlayer);
        }
    }

    private void Init()
    {
        if (!mIsAlreadyInit)
        {
            mArrowPool = new Pool<Projection>();
            mArrowPool.Init(3, Arrow, o =>
            {
                o.transform.parent = ItemStateSaver.Instance.transform;

                o.SetAction(hit =>
                {
                    if (hit.TryGetComponent(out ICombatable combatable))
                    {
                        combatable.Damaged(StatTable[ItemStat.AttackPower], mPlayer);

                        Inventory.Instance.OnAttackEvent(mPlayer, combatable);
                        Inventory.Instance.ProjectionHit(hit, StatTable[ItemStat.AttackPower]);

                        MainCamera.Instance.Shake();

                        SoundManager.Instance.PlaySound(SoundName.ArrowHit);
                    }
                },
                p => mArrowPool.Add(p));
            });

            mAnimPlayKey = Animator.GetParameter(0).nameHash;
            mIsAlreadyInit = true;
        }
    }

    private void ShootArrow()
    {
        var direction = Vector2.right;

        if (mPlayer.transform.localRotation.eulerAngles.y > 0f)
        {
            direction = Vector2.left;
        }
        Projection arrow = mArrowPool.Get();

        MainCamera.Instance.Shake();

        arrow.transform.rotation = mPlayer.transform.localRotation;
        arrow.Shoot(transform.position + ShootOffset, direction, ShootSpeed);

        SoundManager.Instance.PlaySound(SoundName.BowShoot);
    }

    protected override void AttackAnimationPlayOver()
    {
        base.AttackAnimationPlayOver();

        Animator.SetBool(mAnimPlayKey, false);
    }

    protected override void CameraShake()
    {
        MainCamera.Instance.Shake();
    }

    public override void AttackCancel()
    {
        Animator.SetBool(mAnimPlayKey, false);
    }
}
