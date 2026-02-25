using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class VirtualJoystickReposer : MonoBehaviour
{
    // HalfScreen removed; using RectTransform-based coordinates for UI

    [SerializeField] private GameObject _SettingWindow;
    [SerializeField] private VirtualJoystick _Controller;

    private bool _IsButtonStateDown = false;
    private bool _IsAlreadyInit = false;

    private void OnEnable()
    {
        if (!_IsAlreadyInit)
        {
            _Controller.AddInteractionAction(Reposition);
        }
        _SettingWindow.SetActive(false);
        _Controller.SetActiveInteraction(true);

        for (Direction d = 0; (int)d < 4; d++)
        {
            if (_Controller[d].TryGetComponent(out Button btn))
            {
                btn.enabled = false;
            }
            _Controller[d].enabled = false;
        }
        {
            if (_Controller.AttackButton.TryGetComponent(out Button btn))
            {
                btn.enabled = false;
            }
            _Controller.AttackButton.enabled = false;
        }
    }
    private void OnDisable()
    {
        _SettingWindow.SetActive(true);
        _Controller.SetActiveInteraction(false);
        
        for (Direction d = 0; (int)d < 4; d++)
        {
            _Controller[d].enabled = true;

            if (_Controller[d].TryGetComponent(out Button btn))
            {
                btn.enabled = true;
            }
        }
        {
            if (_Controller.AttackButton.TryGetComponent(out Button btn))
            {
                btn.enabled = true;
            }
            _Controller.AttackButton.enabled = true;
        }
    }

    private void Reposition(ButtonState state)
    {
        switch (state)
        {
            case ButtonState.Down:
                _IsButtonStateDown = true;

                StartCoroutine(EUpdate());
                break;

            case ButtonState.Up:
                _IsButtonStateDown = false;

                GameLoger.Instance.ControllerPos = _Controller.transform.localPosition;
                break;
        }
    }
    private IEnumerator EUpdate()
    {
        while (_IsButtonStateDown)
        {
            yield return null;
            // Convert screen point to local point in the joystick's parent canvas rect
            RectTransform parentRect = _Controller.GetComponent<RectTransform>().parent as RectTransform;
            if (parentRect != null)
            {
                Vector2 localPoint;
                bool ok = RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, Input.mousePosition, null, out localPoint);
                if (ok)
                {
                    _Controller.SetPositionWithLocalPoint(localPoint);
                }
            }
        }
    }
}
