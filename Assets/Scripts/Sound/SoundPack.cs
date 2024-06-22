using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPack : MonoBehaviour
{
    public SoundName Name
    {
        get => _SoundName;
    }
    public AudioSource Source
    {
        get => _Source;
    }
    [SerializeField] private SoundName _SoundName;
    [SerializeField] private AudioSource _Source;

    private void Reset()
    {
        TryGetComponent(out _Source);
    }

    public void PlaySound()
    {
        _Source.Play();
    }
}
