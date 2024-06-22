using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Obsolete]
public class AttackButtonHider : MonoBehaviour
{
    private bool _ButtonAlreadyOpen;
    private bool _IsAlreadyInit;

    public void HideOrShow()
    {
        if (!_IsAlreadyInit)
        {
            _ButtonAlreadyOpen = AttackButtonController.Instance.IsButtonHide;

            if (_ButtonAlreadyOpen)
            {
                AttackButtonController.Instance.HideButton();
            }
            _IsAlreadyInit = true;
        }
        else
        {
            if (_ButtonAlreadyOpen)
            {
                AttackButtonController.Instance.ShowButton();
            }
            _IsAlreadyInit = false;
        }
    }
}
