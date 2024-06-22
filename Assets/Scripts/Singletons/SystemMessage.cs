using System;
using UnityEngine;

public class SystemMessage : Singleton<SystemMessage>
{
    [SerializeField] private GameObject Background;
    [SerializeField] private GameObject MessageBox;
    [SerializeField] private TMPro.TextMeshProUGUI MessageText;

    [Header("Check Btn Property")]
    [SerializeField] private GameObject _CheckMessageBox;
    [SerializeField] private SubscribableButton _YesButton;
    [SerializeField] private SubscribableButton  _NoButton;
    
    public void ShowMessage(string message)
    {
        Time.timeScale = 0f;

        Background.SetActive(true);
        MessageBox.SetActive(true);

        MessageText.text = message;

        SoundManager.Instance.PlaySound(SoundName.ErrorWindow);
    }
    public void ShowCheckMessage(string message, Action<bool> result)
    {
        _CheckMessageBox.SetActive(true);
        ShowMessage(message);
        MessageBox.SetActive(false);

        _YesButton.ButtonAction += state =>
        {
            if (state == ButtonState.Up) 
                ButtonAction(true);
        };
        _NoButton.ButtonAction += state =>
        {
            if (state == ButtonState.Up) 
                ButtonAction(false);
        };
        void ButtonAction(bool parameter)
        {
            result.Invoke(parameter);

            CloseMessage();
             _NoButton.ButtonActionReset();
            _YesButton.ButtonActionReset();
        }
    }
    public void CloseMessage()
    {
        Time.timeScale = 1f;

        Background.SetActive(false);
        MessageBox.SetActive(false);
        _CheckMessageBox.SetActive(false);
    }
}
