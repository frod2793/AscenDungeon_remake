using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToastMessage : MonoBehaviour
{
    [SerializeField] private Animation _Animation;
    [SerializeField] private TMPro.TextMeshProUGUI _TextComponent;

    public event System.Action<ToastMessage> OnAnimPlayOver;

    public void ShowMessage(string message)
    {
        _TextComponent.text = message;
        _Animation.Play();
    }
    private void AE_AnimPlayOver()
    {
        gameObject.SetActive(false);
        OnAnimPlayOver?.Invoke(this);
    }
}
