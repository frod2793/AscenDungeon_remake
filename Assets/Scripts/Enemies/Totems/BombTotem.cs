using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombTotem : MonoBehaviour, IObject, ICombatable, IAnimEventReceiver
{
    [SerializeField] private ItemDropper _ItemDropper;

    [Header("Range Info")]
    [SerializeField] private Area Range;
    [SerializeField] private CircleCollider2D RangeCollider;

    [Header("Another Info")]
    [SerializeField] private AbilityTable AbilityTable;
    [SerializeField] private EnemyAnimator EnemyAnimator;

    private IEnumerator mEOnFuse;

    private AttackPeriod mAttackPeriod;

    private Player mPlayer;

    public AbilityTable GetAbility => AbilityTable;

    public void IInit()
    {
        EnemyAnimator.Init();
        HealthBarPool.Instance.UsingHealthBar(-1f, transform, AbilityTable);

        mAttackPeriod = new AttackPeriod(AbilityTable);

        mAttackPeriod.SetAction(Period.Attack, () => {
            EnemyAnimator.ChangeState(AnimState.Attack);
        });
    }

    [ContextMenu("Range Setting")]
    private void SetRange()
    {
        RangeCollider.radius = AbilityTable.Range;
    }

    public bool IsActive()
    {
        return gameObject.activeSelf;
    }

    public void IUpdate() 
    {
        if (Range.HasAny() && !mAttackPeriod.IsProgressing())
        {
            mAttackPeriod.StartPeriod();
        }
    }

    public void PlayerEnter(MESSAGE message, Player enterPlayer)
    { }

    public void PlayerExit(MESSAGE message)
    { }

    private void OnFuse()
    {
        MainCamera.Instance.Shake(0.1f, 0.3f);
    }

    private void AttackAction()
    {
        MainCamera.Instance.Shake(0.4f, 1.2f);
        SoundManager.Instance.PlaySound(SoundName.ExplosionTotem);

        if (Range.TryEnterTypeT(out Player player))
        {
            player.Damaged(AbilityTable.AttackPower, gameObject);
        }
    }

    public GameObject ThisObject() => gameObject;

    public void Damaged(float damage, GameObject attacker)
    {
        EffectLibrary.Instance.UsingEffect(EffectKind.Damage, transform.position);
        EnemyAnimator.ChangeState(AnimState.Damaged);

        if ((AbilityTable.Table[Ability.CurHealth] -= damage) <= 0)
        {
            EnemyAnimator.ChangeState(AnimState.Death);

            HealthBarPool.Instance.UnUsingHealthBar(transform);
            _ItemDropper.CoinDrop(4);
            _ItemDropper.TryPotionDrop(PotionName.SHealingPotion, PotionName.MHealingPotion);

            if (TryGetComponent(out Collider2D collider))
            {
                collider.enabled = false;
            }
        }
    }

    public void CastBuff(Buff buffType, IEnumerator castedBuff)
    {
        StartCoroutine(castedBuff);
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
                EnemyAnimator.ChangeState(AnimState.Idle);
                break;

            case AnimState.Death:
                gameObject.SetActive(false);
                break;
        }
    }
}
