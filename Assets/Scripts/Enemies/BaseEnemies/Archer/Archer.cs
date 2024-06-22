using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Archer : EnemyBase, IAnimEventReceiver
{
    [SerializeField]
    private Arrow mArrow;

    [SerializeField]
    private Vector2 mArrowPos;
    [SerializeField]
    private float mArrowSpeed;

    [SerializeField]
    private EnemyAnimator EnemyAnimator;

    private Timer mWaitForMoving;
    private Timer mWaitForATK;

    private Pool<Arrow> mArrowPool;

    private AttackPeriod mAttackPeriod;
    
    public override void Damaged(float damage, GameObject attacker)
    {
        EffectLibrary.Instance.UsingEffect(EffectKind.Damage, transform.position);

        if ((AbilityTable.Table[Ability.CurHealth] -= damage) <= 0)
        {
            mAttackPeriod.StopPeriod();
            EnemyAnimator.ChangeState(AnimState.Death);

            HealthBarPool.Instance.UnUsingHealthBar(transform);
        }
    }

    public override void IInit()
    {
        EnemyAnimator.Init();
        HealthBarPool.Instance.UsingHealthBar(-1f, transform, AbilityTable);

        mArrowPool = new Pool<Arrow>();
        mArrowPool.Init(3, mArrow, o =>
        {
            o.Setting(
                a => { a.Damaged(AbilityTable.AttackPower, gameObject); },
                i => { return i > 0; },
                a => { mArrowPool.Add(a); });
        });

        mWaitForMoving = new Timer();
        mWaitForATK    = new Timer();

        mAttackPeriod = new AttackPeriod(AbilityTable);

        mAttackPeriod.SetAction(Period.Attack, () => 
        {
            MoveStop();
            EnemyAnimator.ChangeState(AnimState.Attack);
        });
        mAttackPeriod.SetAction(Period.After, AttackAction);

        mWaitForATK.Start(AbilityTable.BeginAttackDelay);
    }

    public override void IUpdate()
    {
        if (!mAttackPeriod.IsProgressing())
        {
            if (HasPlayerOnRange() && IsLookAtPlayer())
            {
                MoveStop();

                mAttackPeriod.StartPeriod();
            }
            else if (mWaitForMoving.IsOver())
            {
                if (IsMoveFinish && !HasPlayerOnRange())
                {
                    Vector2 movePoint;

                    movePoint.x = Random.Range(-HalfMoveRangeX, HalfMoveRangeX) + OriginPosition.x;
                    movePoint.y = Random.Range(-HalfMoveRangeY, HalfMoveRangeY) + OriginPosition.y;

                    EnemyAnimator.ChangeState(AnimState.Move);

                    if (mPlayer != null)
                    {
                        MoveToPlayer(movePoint);
                    }
                    else MoveToPoint(movePoint);
                }
            }
            else
            {
                mWaitForMoving.Update();
            }
        }
        
    }

    protected override void MoveFinish()
    {
        mWaitForMoving.Start(WaitMoveTime);

        EnemyAnimator.ChangeState(AnimState.Idle);
    }

    private void AttackAction()
    {
        Arrow arrow = mArrowPool.Get();

        Vector2 direction;

        if (SpriteFlipX)
        {
            direction = Vector2.right;
        }
        else
            direction = Vector2.left;

        arrow.transform.position = mArrowPos + (Vector2)transform.position;

        arrow.Setting(mArrowSpeed, direction);
        arrow.transform.position = transform.position;
    }
    public void AnimationPlayOver(AnimState anim)
    {
        switch (anim)
        {
            case AnimState.Attack:
                {
                    mAttackPeriod.AttackActionOver();

                    EnemyAnimator.ChangeState(AnimState.Idle);
                }
                break;
            case AnimState.Damaged:
                {
                    if (IsMoving)
                        EnemyAnimator.ChangeState(AnimState.Move);

                    else
                        EnemyAnimator.ChangeState(AnimState.Idle);
                }
                break;

            case AnimState.Death:
                gameObject.SetActive(false);
                break;
        }
    }
}