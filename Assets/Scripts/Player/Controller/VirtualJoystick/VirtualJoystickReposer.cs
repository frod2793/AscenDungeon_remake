using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class VirtualJoystickReposer : MonoBehaviour
{
    private readonly Vector3 HalfScreen 
        = new Vector3(Screen.width / 2f, Screen.height / 2f);

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
            Vector2 position = Input.mousePosition - HalfScreen;

            _Controller.SetPositionWithScreenRange(position);
        }
    }
}
