using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HammerBot : MonoBehaviour, IObject, ICombatable, IAnimEventReceiver
{
    [Header("Ability")]
    [SerializeField] private AbilityTable AbilityTable;
    [SerializeField] private EnemyAnimator EnemyAnimator;

    [Header("Modules")]
    [SerializeField] private MovementModule _Movement;
    [SerializeField] private RecognitionModule _Recognition;
    [SerializeField] private ShortRangeModule _AttackModule;
    [SerializeField] private ItemDropper _ItemDropper;

    public AbilityTable GetAbility => AbilityTable;


    public void AnimationPlayOver(AnimState anim)
    {
        switch (anim)
        {
            case AnimState.Attack:
                {
                    EnemyAnimator.ChangeState(AnimState.Idle);

                    _AttackModule.PeriodAttackPartOver();
                }
                break;

            case AnimState.Damaged:
                {
                    if (_Movement.IsMoving())
                    {
                        EnemyAnimator.ChangeState(AnimState.Move);
                    }
                    else
                        EnemyAnimator.ChangeState(AnimState.Idle);
                }
                break;

            case AnimState.Death:
                gameObject.SetActive(false);
                break;
        }
    }

    public void Damaged(float damage, GameObject attacker)
    {
        EffectLibrary.Instance.UsingEffect(EffectKind.Damage, transform.position);

        EnemyAnimator.ChangeState(AnimState.Damaged);
        if ((AbilityTable.Table[Ability.CurHealth] -= damage) <= 0)
        {
            _AttackModule.SetActivePeriod(false);

            _Movement.MoveStop();
            EnemyAnimator.ChangeState(AnimState.Death);
            HealthBarPool.Instance.UnUsingHealthBar(transform);

            _ItemDropper.CoinDrop(11);
            _ItemDropper.TryPotionDrop(PotionName.SHealingPotion, PotionName.MHealingPotion);

            if (TryGetComponent(out Collider2D collider))
            {
                collider.enabled = false;
            }
        }
    }

    public void IInit()
    {
        EnemyAnimator.Init();
        HealthBarPool.Instance.UsingHealthBar(-1.5f, transform, AbilityTable);

        _AttackModule.Init(gameObject, AbilityTable);
        _AttackModule.RunningDrive();
        _AttackModule.SetPeriodAction(Period.Attack, AttackAction);
        _AttackModule.HitEvent += o => SoundManager.Instance.PlaySound(SoundName.HammerBot_Attack);

        MovementModuleInit();
    }

    public void IUpdate()
    {
        if (AbilityTable[Ability.CurHealth] > 0)
        {

            if (!_AttackModule.IsPeriodProgressing())
            {
                if (_AttackModule.RangeHasAny() && _Recognition.IsLookAtPlayer())
                {

                    _Movement.MoveStop();
                    _AttackModule.SetActivePeriod(true);
                }
            }
        }
    }
    private void MovementModuleInit()
    {
        _Movement.SetMovementEvent(_Recognition,
        () =>
        {
            EnemyAnimator.ChangeState(AnimState.Move);
        },
        () =>
        {
            EnemyAnimator.ChangeState(AnimState.Idle);
        });
        _Movement.SetMovementLogic(_Recognition, 
        () =>
        {
            return EnemyAnimator.CurrentState() == AnimState.Idle &&
                 !_AttackModule.IsPeriodProgressing();
        });
        _Movement.RunningDrive(AbilityTable);
    }

    private void AttackAction()
    {
        _Movement.MoveStop();

        EnemyAnimator.ChangeState(AnimState.Attack);
    }
    private void CameraShake()
    {
        MainCamera.Instance.Shake(0.25f, 1f);
    }

    public void PlayerEnter(MESSAGE message, Player enterPlayer)
    {
        if (AbilityTable.CanRecognize(message))
        {
            _Recognition.PlayerEnter(enterPlayer);
        }
    }
    public void PlayerExit(MESSAGE message)
    {
        if (AbilityTable.CantRecognize(message))
        {
            _Recognition.PlayerExit();
        }
    }

    public void CastBuff(Buff buffType, IEnumerator castedBuff)
    {
        StartCoroutine(castedBuff);
    }
    public bool IsActive()
    {
        return gameObject.activeSelf;
    }
    public GameObject ThisObject()
    {
        return gameObject;
    }
}
