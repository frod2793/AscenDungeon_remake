using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBoxObject : MonoBehaviour, ICombatable
{
    /// <Summary>
    /// 피격 이벤트 (param1 : 공격자)
    /// </Summary>
    public event Action<GameObject>         HitEvent;

    [SerializeField] private AbilityTable   _AbilityTable;
    [SerializeField] private Animation      _HitAnimation;

    [SerializeField] private string _HitAnimationName;
    [SerializeField] private string _BreakAnimationName;

    public AbilityTable GetAbility
    {
        get => _AbilityTable;
    }

    public void CastBuff(Buff buffType, IEnumerator castedBuff)
    {
        StartCoroutine(castedBuff);
    }

    public void Damaged(float damage, GameObject attacker)
    {
        if (_HitAnimation.IsPlaying(_BreakAnimationName))
            return;
        
        _HitAnimation.Rewind();
        _HitAnimation.Play(_HitAnimationName);

        HitEvent?.Invoke(attacker);

        var offset = new Vector3(0f, 0.6f, 0f);
        EffectLibrary.Instance.UsingEffect(EffectKind.Damage, offset + transform.position);
    }

    public void Break()
    {
        _HitAnimation.Play(_BreakAnimationName);
    }

    private void AE_Disable()
    {
        gameObject.SetActive(false);
    }
}
