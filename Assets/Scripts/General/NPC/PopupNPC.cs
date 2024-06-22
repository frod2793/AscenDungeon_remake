using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupNPC : NPC
{
    [SerializeField, Header("PopupNPC Property")]
    private GameObject _Popup;

    [SerializeField]
    private bool _WaitForEffectEnd;

    public override void Interaction()
    {
        if (_WaitForEffectEnd)
        {
            EffectLibrary.Instance.UsingEffect(EffectKind.Twinkle, transform.position, () =>
            {
                _Popup.SetActive(true);
            });
        }
        else
        {
            EffectLibrary.Instance.UsingEffect(EffectKind.Twinkle, transform.position);
            _Popup.SetActive(true);
        }
    }
}
