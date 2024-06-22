using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scarecrow : EnemyBase, IAnimEventReceiver
{
    [SerializeField]
    private EnemyAnimator EnemyAnimator;

    [SerializeField]
    private float AttackTime;

    private Timer mWaitForMove;

    private AttackPeriod mAttackPeriod;

    public override void Damaged(float damage, GameObject attacker)
    {
        EnemyAnimator.ChangeState(AnimState.Damaged);

        EffectLibrary.Instance.UsingEffect(EffectKind.Damage, transform.position);

        if ((AbilityTable.Table[Ability.CurHealth] -= damage) <= 0)
        {
            EnemyAnimator.ChangeState(AnimState.Death);

            HealthBarPool.Instance.UnUsingHealthBar(transform);
        }
    }

    public override void IInit()
    {
        EnemyAnimator.Init();
        HealthBarPool.Instance.UsingHealthBar(-1f, transform, AbilityTable);

        mWaitForMove = new Timer();

        mAttackPeriod = new AttackPeriod(AbilityTable);

        mAttackPeriod.SetAction(Period.Begin,  () => MoveStop()); 
        mAttackPeriod.SetAction(Period.Attack, () => {
            EnemyAnimator.ChangeState(AnimState.Attack);
        }); 
    }
    public override void IUpdate()
    {
        if (!mAttackPeriod.IsProgressing())
        {
            if (HasPlayerOnRange() && IsLookAtPlayer())
            {
                mAttackPeriod.StartPeriod();
            }
            else if (mWaitForMove.IsOver() && IsMoveFinish)
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
            else
            {
                mWaitForMove.Update();
            }
        }
    }

    protected override void MoveFinish()
    {
        mWaitForMove.Start(WaitMoveTime);

        EnemyAnimator.ChangeState(AnimState.Idle);
    }

    private void AttackAction()
    {
        if (HasPlayerOnRange() && IsLookAtPlayer()) {
            mPlayer.Damaged(AbilityTable.AttackPower, gameObject);
        }
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
