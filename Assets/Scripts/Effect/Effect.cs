using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect : MonoBehaviour
{
    public event Action OnDisableEvent;

    [SerializeField] private EffectKind EffectKind;
    [SerializeField] private float LifeTime;

    public void LifeTimeOver() 
    {
        EffectLibrary.Instance.UnUsingEffect(EffectKind, this);
    }
    private void OnEnable()
    {
        if (LifeTime > 0f)
        {
            StartCoroutine(ELifeTime());
        }
    }
    private void OnDisable()
    {
        OnDisableEvent?.Invoke();
        OnDisableEvent = null;
    }
    private IEnumerator ELifeTime()
    {
        yield return new WaitForSeconds(LifeTime);

        LifeTimeOver();
    }
}
