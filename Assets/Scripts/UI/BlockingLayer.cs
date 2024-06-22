using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BlockingLayer : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private RectTransform _RectTransform;

    private GameObject _User;

    private void Awake()
    {
        if (TryGetComponent(out _RectTransform))
        {
            _RectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
        }
    }
    public void SetActive(bool isActive, GameObject user)
    {
        if (isActive)
        {
            _User = user;
        }
        else
        {
            _User = null;
        }
        gameObject.SetActive(isActive);
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (_User)
        {
            _User.SetActive(false);
            _User = null;

            gameObject.SetActive(false);
        }
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (_User)
        {
            _User.SetActive(false);
            _User = null;

            gameObject.SetActive(false);
        }
    }
}
