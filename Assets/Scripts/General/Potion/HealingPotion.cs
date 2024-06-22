using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HealingPotion : Potion
{
    [SerializeField, Range(0f, 1f)] private float _HealRate;

    public override void UsePotion()
    {
        EffectLibrary.Instance.UsingEffect(EffectKind.Twinkle, transform.position);

        float maxHealth = PotionPool.Instance.PlayerAbilityTable[Ability.MaxHealth];
        float curHealth = PotionPool.Instance.PlayerAbilityTable[Ability.CurHealth];

        float health = Mathf.Min(maxHealth, curHealth + maxHealth * _HealRate);

        PotionPool.Instance.PlayerAbilityTable.Table[Ability.CurHealth] = health;
        PotionPool.Instance.Add(this);
    }
    public override void Interaction()
    {
        if (_TriggerEntryPlayer) {

            UsePotion();
        }
    }
}
