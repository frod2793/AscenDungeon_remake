using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Buff
{
    Heal, SpeedUp, PowerBoost, Stun, Poision
}
public class BuffLibrary : Singleton<BuffLibrary>
{
    public const float HEAL = 15f;
    public const float SPEEDUP = 0.2f;
    public const float POWER_BOOST = 0.3f;

    public const float PoisionDamage = 0.5f;
    public const float PoisionDelay = 0.3f;

    public float DeltaTime => Time.deltaTime * Time.timeScale;

    private IEnumerable HealBuff(uint level, AbilityTable ability)
    {
        float healingMin = ability[Ability.CurHealth] + level * HEAL;
        float healingMax = ability[Ability.MaxHealth];

        ability.Table[Ability.CurHealth] = Mathf.Min(healingMin, healingMax);
        yield break;
    }

    private IEnumerable SpeedUpBuff(float durate, uint level, AbilityTable ability)
    {
        float increment = ability.Table[Ability.MoveSpeed] * level * SPEEDUP;

        ability.Table[Ability.IMoveSpeed] += increment;
        for (float i = 0; i < durate; i += DeltaTime) { yield return null; }
        ability.Table[Ability.IMoveSpeed] -= increment;
    }
  
    private IEnumerable PowerBoostBuff(float durate, uint level, AbilityTable ability)
    {
        float increment = ability.Table[Ability.AttackPower] * level * SPEEDUP;

        ability.Table[Ability.IAttackPower] += increment;
        for (float i = 0; i < durate; i += DeltaTime) { yield return null; }
        ability.Table[Ability.IAttackPower] -= increment;
    }
    private IEnumerable PoisionBuff(float durate, uint level, AbilityTable ability)
    {
        for (float i = 0; i < durate; i += PoisionDelay + DeltaTime) 
        {
            Vector2 effectPoint = ability.transform.position + Random.onUnitSphere * 0.5f;
            EffectLibrary.Instance.UsingEffect(EffectKind.Poision, effectPoint).transform.localScale = Vector2.one * 1.3f;

            ability.Table[Ability.CurHealth] -= PoisionDamage * level;

            if (ability[Ability.CurHealth] <= 0f)
            {
                if (ability.TryGetComponent(out ICombatable combatable)) {

                    combatable.Damaged(0f, gameObject);
                    yield break;
                }
            }
            for (float delay = 0f; delay < PoisionDelay; delay += DeltaTime)
            { yield return null; }
        }
    }
    public IEnumerator Stun(float duration, AbilityTable ability)
    {
        return StunBuff(duration, ability).GetEnumerator();
    }
    private IEnumerable StunBuff(float duration, AbilityTable ability)
    {
        for (float i = 0; i < duration; i += DeltaTime) 
        {
            ability.Table[Ability.IMoveSpeed] = -ability.Table[Ability.MoveSpeed];
            ability.Table[Ability.IBegin_AttackDelay] = 99999f;
            ability.Table[Ability.IAfter_AttackDelay] = 99999f;
            yield return null;

            ability.Table[Ability.IMoveSpeed] = -ability.Table[Ability.MoveSpeed];
            ability.Table[Ability.IBegin_AttackDelay] = 99999f;
            ability.Table[Ability.IAfter_AttackDelay] = 99999f;
        }
        ability.Table[Ability.IMoveSpeed] = 0f;
        ability.Table[Ability.IBegin_AttackDelay] = 0f;
        ability.Table[Ability.IAfter_AttackDelay] = 0f;
    }

    public IEnumerator GetBuff(Buff buff, uint level, float duration, AbilityTable ability)
    {
        switch (buff)
        {
            case Buff.Heal:
                return HealBuff(level, ability).GetEnumerator();

            case Buff.SpeedUp:
                return SpeedUpBuff(duration, level, ability).GetEnumerator();

            case Buff.PowerBoost:
                return PowerBoostBuff(duration, level, ability).GetEnumerator();

            case Buff.Poision:
                return PoisionBuff(duration, level, ability).GetEnumerator();
            default:
                Debug.LogError("Undefined");
                return null;
        }
    }
}
