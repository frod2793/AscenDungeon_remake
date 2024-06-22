using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Summoner : EnemyBase, IAnimEventReceiver
{
    [SerializeField]
    private GameObject mSummonTagret;

    [SerializeField]
    private Vector2 mSummonOffset;

    [SerializeField]
    private EnemyAnimator EnemyAnimator;

    private Timer mWaitForMove;

    private Room mBelongRoom;

    private AttackPeriod mAttackPeriod;

    public override void Damaged(float damage, GameObject attacker)
    {
        EnemyAnimator.ChangeState(AnimState.Damaged);
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

        Debug.Assert(transform.parent.TryGetComponent(out mBelongRoom));

        mAttackPeriod = new AttackPeriod(AbilityTable);

        mAttackPeriod.SetAction(Period.Attack, () =>
        {
            MoveStop();
            EnemyAnimator.ChangeState(AnimState.Attack);
        });
        mAttackPeriod.SetAction(Period.After, AttackAction);

        mWaitForMove = new Timer();
        mWaitForMove.Start(WaitMoveTime);
    }
    public override void IUpdate()
    {
        if (mWaitForMove.IsOver() && !HasPlayerOnRange())
        {
            if (IsMoveFinish)
            {
                Vector2 movePoint;

                movePoint.x = Random.Range(-HalfMoveRangeX, HalfMoveRangeX) + OriginPosition.x;
                movePoint.y = Random.Range(-HalfMoveRangeY, HalfMoveRangeY) + OriginPosition.y;

                EnemyAnimator.ChangeState(AnimState.Move);

                MoveToPoint(movePoint);
            }
        }
        else
        {
            mWaitForMove.Update();
        }

        if (HasPlayerOnRange() && IsLookAtPlayer())
        {
            mAttackPeriod.StartPeriod();
        }    
    }

    protected override void MoveFinish()
    {
        EnemyAnimator.ChangeState(AnimState.Idle);

        mWaitForMove.Start(WaitMoveTime);
    }

    private void AttackAction()
    {
        Vector2 summonPoint = mSummonOffset;

        summonPoint.x += Random.Range(-HalfMoveRangeX, HalfMoveRangeX);
        summonPoint.y += Random.Range(-HalfMoveRangeY, HalfMoveRangeY);

        GameObject newObject = Instantiate(mSummonTagret, transform.parent, false);

        if (newObject.TryGetComponent(out IObject @object))
        {
            mBelongRoom.AddIObject(@object);
        }
        newObject.transform.localPosition = summonPoint;
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
