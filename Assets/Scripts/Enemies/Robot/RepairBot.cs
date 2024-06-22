using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairBot : MonoBehaviour, IObject, ICombatable, IAnimEventReceiver
{
    [SerializeField] private ItemDropper _ItemDropper;

    [Header("Ability Section")]
    [SerializeField] private bool IsLookAtLeft;
    [SerializeField] private AbilityTable AbilityTable;
    [SerializeField] private EnemyAnimator EnemyAnimator;
    [SerializeField] private Area Range;

    [Header("Healing Section")]
    [SerializeField] private Area HealingArea;
    [Range(0f, 1f)]
    [SerializeField] private float HealingScale;

    [Header("Movement Section")]
    [SerializeField] private float WaitMoveTimeMin;
    [SerializeField] private float WaitMoveTimeMax;
    [SerializeField] private Vector2 InitPosition;

    private IEnumerator _Move;
    private GameObject _HealingFriendly;
    private AttackPeriod _AttackPeriod;

    public AbilityTable GetAbility => AbilityTable;

    [ContextMenu("Contect Check")]
    private void ContextCheck()
    {
        Debug.Log($"AttackPeriod : {_AttackPeriod.CurrentPeriod}");
        Debug.Log($"AttackPeriod Progressing: {_AttackPeriod.IsProgressing()}");
        Debug.Log($"Has Healing Fiendly : {_HealingFriendly != null}");
    }

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

            _ItemDropper.CoinDrop(7);
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

        _AttackPeriod = new AttackPeriod(AbilityTable);
        _AttackPeriod.SetAction(Period.Attack, () => 
        {
            EnemyAnimator.ChangeState(AnimState.Attack);
        });

        Range.SetScale(AbilityTable[Ability.Range]);
        StartCoroutine(MoveRoutine());
    }
    public void IUpdate()
    {
        if (AbilityTable[Ability.CurHealth] > 0)
        {
            if (_HealingFriendly != null)
            {
                if (!_HealingFriendly.activeSelf)
                {
                    _HealingFriendly = null;
                }
            }
            if (_HealingFriendly == null)
            {
                var list = HealingArea.EntryList;
                float leastRatio = 1f;

                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].TryGetComponent(out ICombatable combatable))
                    {
                        float ratio = 
                            combatable.GetAbility[Ability.CurHealth] /
                            combatable.GetAbility[Ability.MaxHealth];

                        if (leastRatio > ratio)
                        {
                            leastRatio = ratio;

                            _HealingFriendly = list[i];
                        }
                    }
                }
                if (leastRatio > 0.5f)
                {
                    _HealingFriendly = null;
                }
            }
            if (_HealingFriendly != null)
            {
                if (!IsLookAtFriendly())
                {
                    SetLookingLeft(!IsLookAtLeft);
                }
            }
            if (!_AttackPeriod.IsProgressing())
            {
                if (Range.HasThis(_HealingFriendly))
                {
                    MoveStop();

                    _AttackPeriod.StartPeriod();
                }
            }
        }
    }
    private void Healing()
    {
        if (Range.HasThis(_HealingFriendly))
        {
            if (_HealingFriendly.TryGetComponent(out ICombatable combatable))
            {
                SoundManager.Instance.PlaySound(SoundName.RepairBot_Attack);

                for (int i = 0; i < 5; i++)
                {
                    Vector2 point = (Vector2)_HealingFriendly.transform.position + Random.insideUnitCircle;

                    EffectLibrary.Instance.UsingEffect(EffectKind.Twinkle, point);
                }
                float healing = 
                    combatable.GetAbility[Ability.MaxHealth] * HealingScale;
                    combatable.GetAbility.Table[Ability.CurHealth] += healing;

                if (combatable.GetAbility[Ability.CurHealth] /
                    combatable.GetAbility[Ability.MaxHealth] > 0.75f)
                {

                    _HealingFriendly = null;
                }
            }
        }
    }

    private bool IsLookAtFriendly()
    {
        if (_HealingFriendly != null)
        {
            return (_HealingFriendly.transform.position.x < transform.position.x && IsLookAtLeft ||
                    _HealingFriendly.transform.position.x > transform.position.x && !IsLookAtLeft);
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

            if (IsLookAtFriendly())
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
    { }
    public void PlayerExit(MESSAGE message)
    { }

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