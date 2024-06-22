using System;
using System.Collections.Generic;
using UnityEngine;

public class Resurrectable : MonoBehaviour
{
    public event Action ResurrectAction;

    public void Resurrect()
    {
        ResurrectAction?.Invoke();
    }
}
