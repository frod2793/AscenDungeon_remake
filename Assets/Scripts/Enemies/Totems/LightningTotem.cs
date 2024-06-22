using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningTotem : MonoBehaviour, IObject, ICombatable, IAnimEventReceiver
{
    [SerializeField] private ItemDropper _ItemDropper;
    [SerializeField] private AbilityTable AbilityTable;

    [SerializeField] private EnemyAnimator EnemyAnimator;

    [SerializeField] private Lightning mLighting;

    [SerializeField] private Vector2 mLightingOffset;

    private Player mPlayer;

    private AttackPeriod mAttackPeriod;

    public AbilityTable GetAbility => AbilityTable;

    public void IInit()
    {
        EnemyAnimator.Init();
        HealthBarPool.Instance.UsingHealthBar(-1f, transform, AbilityTable);

        mAttackPeriod = new AttackPeriod(AbilityTable);

        mAttackPeriod.SetAction(Period.Attack, () => {
            EnemyAnimator.ChangeState(AnimState.Attack);
        });
        mLighting = Instantiate(mLighting);
    }

    public bool IsActive()
    {
        return gameObject.activeSelf;
    }

    public void IUpdate()
    {
        if (!mAttackPeriod.IsProgressing())
        {
            if (mPlayer != null)
            {
                mAttackPeriod.StartPeriod();
            }
        }
    }

    public void PlayerEnter(MESSAGE message, Player enterPlayer)
    {
        if (AbilityTable.CanRecognize(message))
            mPlayer = enterPlayer;
    }

    public void PlayerExit(MESSAGE message)
    {
        if (AbilityTable.CantRecognize(message))
            mPlayer = null;
    }

    public GameObject ThisObject() => gameObject;

    private void AttackAction()
    {
        SoundManager.Instance.PlaySound(SoundName.LightningTotem);
        MainCamera.Instance.Shake(0.1f, 0.3f);

        Vector2 playerPos = new Vector2
            (mPlayer.transform.position.x, Castle.Instance.GetMovePointY(mPlayer.GetUnitizedPosV()));

        mLighting.transform.position = mLightingOffset + playerPos;
        mLighting.gameObject.SetActive(true);

        mLighting.SetAttackPower(AbilityTable.AttackPower);
    }

    private void Pool_popMethod(Lightning lighting)
    {
        mPlayer.TryGetPosition(out Vector2 playerPos);

        lighting.transform.position = mLightingOffset + playerPos;
        lighting.gameObject.SetActive(true);
    }

    public void Damaged(float damage, GameObject attacker)
    {
        EffectLibrary.Instance.UsingEffect(EffectKind.Damage, transform.position);
        EnemyAnimator.ChangeState(AnimState.Damaged);

        if ((AbilityTable.Table[Ability.CurHealth] -= damage) <= 0)
        {
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
                {
                    EnemyAnimator.ChangeState(AnimState.Idle);
                }
                break;
            case AnimState.Death:
                gameObject.SetActive(false);
                break;
        }
    }
}
