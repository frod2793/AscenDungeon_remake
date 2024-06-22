using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum ButtonState { Down, Up }

public class SubscribableButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public event Action<ButtonState> ButtonAction;
    public ButtonState CurrentState
    {
        get;
        private set;
    }

    public void ButtonActionReset()
    {
        ButtonAction = null;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        ButtonAction?.Invoke(CurrentState = ButtonState.Down);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        ButtonAction?.Invoke(CurrentState = ButtonState.Up);
    }
}
