using System;
using System.Collections.Generic;
using UnityEngine;

public enum EffectKind
{
    Damage, Twinkle, Brocking, Coin, Dust, Poision
}

[System.Serializable]
public struct EffectPage
{
    public EffectKind EffectKind;
    public Effect     EffectOrigin;

    public uint HoldingCount;
}

public class EffectLibrary : Singleton<EffectLibrary>
{
    [SerializeField]
    private EffectPage[] Effects;

    private Dictionary<EffectKind, Stack<Effect>> EffectLib;

    private void Awake()
    {
        EffectLib = new Dictionary<EffectKind, Stack<Effect>>();

        for (int i = 0; i < Effects.Length; i++) 
        {

            var origin = Effects[i].EffectOrigin;
            var   kind = Effects[i].EffectKind;

            EffectLib.Add(kind, new Stack<Effect>());

            for (int j = 0; j < Effects[i].HoldingCount; ++j) 
            {
                var instance = Instantiate(origin);

                EffectLib[kind].Push(instance);
                                     instance.gameObject.SetActive(false);
            }
        }
    }

    public void UnUsingEffect(EffectKind kind, Effect effect)
    {        
        EffectLib[kind].Push(effect);
                             effect.gameObject.SetActive(false);
    }


    public Effect UsingEffect(EffectKind kind, Vector2 worldPoint)
    {
        if (EffectLib[kind].Count == 1) {
            EffectLib[kind].Push(Instantiate(EffectLib[kind].Peek()));
        }
        var effect
            = EffectLib[kind].Pop();

        float effectZ = effect.transform.position.z;

        effect.transform.position = worldPoint;
        effect.transform.SetZ(effectZ);

        effect.gameObject.SetActive(true);

        return effect;
    }
    public Effect UsingEffect(EffectKind kind, Vector2 worldPoint, Action disableAction)
    {
        if (EffectLib[kind].Count == 1)
        {
            EffectLib[kind].Push(Instantiate(EffectLib[kind].Peek()));
        }
        var effect
            = EffectLib[kind].Pop();

        effect.OnDisableEvent += disableAction;
        effect.transform.position = worldPoint;
        effect.gameObject.SetActive(true);

        return effect;
    }
}
