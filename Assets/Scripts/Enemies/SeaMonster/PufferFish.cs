using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PufferFish : MonoBehaviour, IObject, ICombatable, IAnimEventReceiver
{
    [SerializeField] private ItemDropper _ItemDropper;

    [Header("Ability")]
    [SerializeField] private bool IsLookAtLeft;
    [SerializeField] private AbilityTable AbilityTable;
    [SerializeField] private EnemyAnimator EnemyAnimator;
    [SerializeField] private Area Range;

    [Header("Effect Info")]
    [SerializeField] private  Area StunArea;
    [SerializeField] private float StunAreaScale;
    [SerializeField] private float StunDuration;

    [Header("Movement Info")]
    [SerializeField] private float WaitMoveTimeMin;
    [SerializeField] private float WaitMoveTimeMax;
    [SerializeField] private Vector2 InitPosition;

    private AttackPeriod _AttackPeriod;

    private IEnumerator _Move;

    private Player _Player;

    public void AnimationPlayOver(AnimState anim)
    {
        switch (anim)
        {
            case AnimState.Attack:
                {
                    _AttackPeriod.StopPeriod();

                    HealthBarPool.Instance.UnUsingHealthBar(transform);
                    gameObject.SetActive(false);
                }
                break;

            case AnimState.Damaged:
                if (IsMoving())
                {
                    EnemyAnimator.ChangeState(AnimState.Move);
                }
                else
                    EnemyAnimator.ChangeState(AnimState.Idle);
                break;

            case AnimState.Death:
                gameObject.SetActive(false);
                break;
        }
    }

    public void Damaged(float damage, GameObject attacker)
    {
        EffectLibrary.Instance.UsingEffect(EffectKind.Damage, transform.position);
        
        if(EnemyAnimator.CurrentState() <= AnimState.Move)
        {
            EnemyAnimator.ChangeState(AnimState.Damaged);
        }
        if ((AbilityTable.Table[Ability.CurHealth] -= damage) <= 0)
        {
            _AttackPeriod.StopPeriod();

            MoveStop();
            EnemyAnimator.ChangeState(AnimState.Death);
            HealthBarPool.Instance.UnUsingHealthBar(transform);

            _ItemDropper.CoinDrop(5);
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
        HealthBarPool.Instance.UsingHealthBar(-2f, transform, AbilityTable);

        _AttackPeriod = new AttackPeriod(AbilityTable);

        _AttackPeriod.SetAction(Period.Attack, () => 
        {
            MoveStop();

            EnemyAnimator.ChangeState(AnimState.Attack);
        });

        float range = AbilityTable.Range;

        StunArea.SetEnterAction(o => 
        {
            if (o.TryGetComponent(out ICombatable c))
            {

                c.CastBuff(Buff.Stun, BuffLibrary.Instance.Stun(StunDuration, c.GetAbility));
            }
        });
        StunArea.SetScale(range * StunAreaScale);
             Range.SetScale(range);

        StartCoroutine(MoveRoutine());
    }

    public void IUpdate()
    {
        if (AbilityTable[Ability.CurHealth] > 0)
        {

            if (!_AttackPeriod.IsProgressing())
            {
                if (Range.HasAny() && IsLookAtPlayer())
                {

                    MoveStop();
                    _AttackPeriod.StartPeriod();
                }
            }
        }
    }

    // 애니메이션 이벤트로 실행됨!
    private void AttackAction()
    {
        MainCamera.Instance.Shake(0.8f, 1.1f);

        StunArea.gameObject.SetActive(true);
        SoundManager.Instance.PlaySound(SoundName.PufferFish_Explosion);
    }

    private bool IsLookAtPlayer()
    {
        if (_Player != null)
        {
            return (_Player.transform.position.x < transform.position.x &&  IsLookAtLeft ||
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
    }

    private IEnumerator MoveRoutine()
    {
        bool CanMovement()
        {
            return EnemyAnimator.CurrentState() == AnimState.Idle
                && !_AttackPeriod.IsProgressing();
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

            EnemyAnimator.ChangeState(AnimState.Move);
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
            transform.localPosition += direction * AbilityTable.MoveSpeed * DeltaTime();

            yield return null;

        } while (CanMoving());

        transform.localPosition = movePoint;

        EnemyAnimator.ChangeState(AnimState.Idle);
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
