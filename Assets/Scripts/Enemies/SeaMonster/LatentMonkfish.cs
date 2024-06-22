using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LatentMonkfish : MonoBehaviour, IObject, ICombatable, IAnimEventReceiver
{
    [SerializeField] private ItemDropper _ItemDropper;

    [Header("Ability")]
    [SerializeField] private bool IsLookAtLeft;
    [SerializeField] private AbilityTable AbilityTable;
    [SerializeField] private EnemyAnimator EnemyAnimator;
    [SerializeField] private Area Range;

    [Header("Burrow Info")]
    [SerializeField] private float BurrowAttackScale;
    [SerializeField] private float BurrowSpeedScale;
    [SerializeField] private Area BurrowAttackArea;

    [Header("Movement Info")]
    [SerializeField] private float WaitMoveTimeMin;
    [SerializeField] private float WaitMoveTimeMax;
    [SerializeField] private Vector2 InitPosition;

    private AttackPeriod _AttackPeriod;

    private IEnumerator _Move;
    private bool _IsBurrowOver;

    private Player _Player;

    public void AnimationPlayOver(AnimState anim)
    {
        switch (anim)
        {
            case AnimState.Attack:
                {
                    EnemyAnimator.ChangeState(AnimState.Idle);

                    _AttackPeriod.AttackActionOver();
                }
                break;

            case AnimState.Damaged:
                {
                    if (IsMoving())
                    {
                        EnemyAnimator.ChangeState(AnimState.Move);
                    }
                    else
                        EnemyAnimator.ChangeState(AnimState.Idle);
                }
                break;

            case AnimState.AttackAfter:
                {
                    _IsBurrowOver = true;

                    if (IsMoving())
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
            _AttackPeriod.StopPeriod();

            MoveStop();
            EnemyAnimator.ChangeState(AnimState.Death);
            HealthBarPool.Instance.UnUsingHealthBar(transform);

            _ItemDropper.CoinDrop(6);
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
        HealthBarPool.Instance.UsingHealthBar(-1f, transform, AbilityTable);

        _AttackPeriod = new AttackPeriod(AbilityTable);

        _AttackPeriod.SetAction(Period.Attack, () => 
        {
            EnemyAnimator.ChangeState(AnimState.Attack);
        });

        Range.SetScale(AbilityTable[Ability.Range]);
        Range.SetEnterAction(o => 
        {
            if (o.CompareTag("Player") && !_IsBurrowOver)
            {
                if (_Move != null)
                {
                    StopCoroutine(_Move);
                    _Move = null;
                }
                EnemyAnimator.ChangeState(AnimState.AttackAfter);
            }
        });
        BurrowAttackArea.SetEnterAction(o => 
        {
            if (o.TryGetComponent(out ICombatable combatable))
            {
                float atk = BurrowAttackScale * AbilityTable.AttackPower;

                combatable.Damaged(atk, gameObject);
            }
        });
        _IsBurrowOver = false;

        StartCoroutine(MoveRoutine());
    }

    public void IUpdate()
    {
        if (AbilityTable[Ability.CurHealth] > 0)
        {

            if (!_AttackPeriod.IsProgressing() && _IsBurrowOver)
            {
                if (Range.HasAny() && IsLookAtPlayer())
                {

                    MoveStop();
                    _AttackPeriod.StartPeriod();
                }
            }
        }
    }

    // == 애니메이션 이벤트로 실행 ==
    private void AttackAction()
    {
        SoundManager.Instance.PlaySound(SoundName.LatentMonkfish_Attack);

        if (IsLookAtPlayer()) 
        {
            if (Range.HasThis(_Player.gameObject)) 
            {
                _Player.Damaged(AbilityTable.AttackPower, gameObject);
            }
        }
    }
    private void BurrowAttackEnable()
    {
        SoundManager.Instance.PlaySound(SoundName.LatentMonkfish_Swoop);

        MainCamera.Instance.Shake();
        BurrowAttackArea.gameObject.SetActive(true);
    }
    private void BurrowAttackDisable() 
    {
        BurrowAttackArea.gameObject.SetActive(false);
    }
    // == 애니메이션 이벤트로 실행 ==

    private bool IsLookAtPlayer()
    {
        if (_Player != null)
        {
            return (_Player.transform.position.x < transform.position.x && IsLookAtLeft ||
                    _Player.transform.position.x > transform.position.x && !IsLookAtLeft);
        }
        return false;
    }
    private void SetLookingLeft(bool lookingLeft)
    {
        IsLookAtLeft = lookingLeft;

        if (IsLookAtLeft)
        {
            transform.rotation = Quaternion.Euler(Vector3.zero);
        }
        else
            transform.rotation = Quaternion.Euler(Vector3.up * 180);
    }
    private bool IsMoving()
    {
        return _Move != null;
    }
    private void MoveStop()
    {
        if (_Move != null)
        {
            StopCoroutine(_Move);
            _Move = null;
        }
        EnemyAnimator.ChangeState(AnimState.Idle);
    }

    private IEnumerator MoveRoutine()
    {
        bool CanMovement()
        {
            return (EnemyAnimator.CurrentState() == AnimState.Idle 
                || (EnemyAnimator.CurrentState() != AnimState.AttackAfter && !_IsBurrowOver))
                && !_AttackPeriod.IsProgressing() && _Move == null;
        }
        while (AbilityTable[Ability.CurHealth] > 0)
        {
            yield return new WaitUntil(CanMovement);
            float waitTime = Random.Range(WaitMoveTimeMin, WaitMoveTimeMax);

            yield return new WaitForSeconds(waitTime);
            yield return new WaitUntil(CanMovement);

            Vector2 movePoint = InitPosition;

            if (IsLookAtPlayer())
            {
                if (IsLookAtLeft)
                {
                    movePoint.x = -3.5f;
                }
                else
                    movePoint.x = +3.5f;
            }
            else
            {
                movePoint.x += Random.Range(-3.5f, 3.5f);
            }
            SetLookingLeft(movePoint.x < transform.localPosition.x);
            StartCoroutine(_Move = Move(movePoint));

            if (_IsBurrowOver)
            {
                EnemyAnimator.ChangeState(AnimState.Move);
            }
        }
    }

    private IEnumerator Move(Vector2 movePoint)
    {
        Vector3 direction = (movePoint.x > transform.localPosition.x)
            ? Vector3.right : Vector3.left;

        float DeltaTime()
        {
            return Time.deltaTime * Time.timeScale;
        }
        bool CanMoving()
        {
            return direction.x > 0 && transform.localPosition.x < movePoint.x ||
                   direction.x < 0 && transform.localPosition.x > movePoint.x;
        }
        do
        {
            float speed = AbilityTable.MoveSpeed;

            if (!_IsBurrowOver)
            {
                speed *= BurrowSpeedScale;
            }
            transform.localPosition += direction * speed * DeltaTime();

            yield return null;

        } while (CanMoving());

        transform.localPosition = movePoint;

        if (_IsBurrowOver)
        {
            EnemyAnimator.ChangeState(AnimState.Idle);
        }
        _Move = null;
    }

    public void PlayerEnter(MESSAGE message, Player enterPlayer)
    {
        if (AbilityTable.CanRecognize(message))
        {

            _Player = enterPlayer;
        }
    }

    public void PlayerExit(MESSAGE message)
    {
        if (AbilityTable.CantRecognize(message))
        {

            _Player = null;
        }
    }

    public GameObject ThisObject() => gameObject;
    public AbilityTable GetAbility => AbilityTable;
    public bool IsActive()
    {
        return gameObject.activeSelf;
    }
    public void CastBuff(Buff buffType, IEnumerator castedBuff)
    {
        StartCoroutine(castedBuff);
    }
}