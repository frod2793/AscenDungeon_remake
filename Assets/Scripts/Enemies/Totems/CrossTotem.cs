using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossTotem : MonoBehaviour, IObject, ICombatable
{
    public enum SHOOTING_TYPE
    {
        CROSS, XSHAPE
    }

    [SerializeField] private SHOOTING_TYPE mShootingType;

    [SerializeField] private AbilityTable AbilityTable;

    [SerializeField] private Arrow mDartOrigin;

    [SerializeField] private float mDartSpeed;

    private Pool<Arrow> mDartPool;

    private Player mPlayer;

    private AttackPeriod mAttackPeriod;

    public AbilityTable GetAbility => AbilityTable;

    public void CastBuff(Buff buffType, IEnumerator castedBuff)
    {
        StartCoroutine(castedBuff);
    }

    public void Damaged(float damage, GameObject attacker)
    {
        EffectLibrary.Instance.UsingEffect(EffectKind.Damage, transform.position);

        if ((AbilityTable.Table[Ability.CurHealth] -= damage) <= 0)
        {
            gameObject.SetActive(false);

            HealthBarPool.Instance.UnUsingHealthBar(transform);
        }
    }

    public void IInit()
    {
        HealthBarPool.Instance.UsingHealthBar(-1f, transform, AbilityTable);

        mDartPool = new Pool<Arrow>();
        mDartPool.Init(4, mDartOrigin, o => 
        {
            o.Setting(
                a => { a.Damaged(AbilityTable.AttackPower, gameObject); },
                i => { return i > 0; },
                a => { mDartPool.Add(a); });
        });

        mAttackPeriod = new AttackPeriod(AbilityTable);

        mAttackPeriod.SetAction(Period.Attack, Attack);
    }

    public bool IsActive()
    {
        return gameObject.activeSelf;
    }

    public void IUpdate()
    {
        if (mPlayer != null)
        {
            mAttackPeriod.StartPeriod();
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

    private void Attack()
    {
        for (int i = 0; i < 4; i++)
        {
            Arrow arrow = mDartPool.Get();

            switch (mShootingType)
            {
                case SHOOTING_TYPE.CROSS:
                    switch (i)
                    {
                        case 0:
                            arrow.Setting(mDartSpeed, Vector2.left);
                            break;

                        case 1:
                            arrow.Setting(mDartSpeed, Vector2.right);
                            break;

                        case 2:
                            arrow.Setting(mDartSpeed, Vector2.down);
                            break;

                        case 3:
                            arrow.Setting(mDartSpeed, Vector2.up);
                            break;
                        default:
                            break;
                    }
                    break;

                case SHOOTING_TYPE.XSHAPE:
                    float rotation = (45f + 90f * i) * Mathf.Deg2Rad;

                    arrow.Setting(mDartSpeed, new Vector2(Mathf.Cos(rotation), Mathf.Sin(rotation)));
                    arrow.transform.position = transform.position;
                    break;
            }
        }
    }
}
