using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    public bool IsPlayOnEnable;
    public SoundName Name;

    public void PlaySound()
    {
        SoundManager.Instance.PlaySound(Name);
    }
    public void PlaySound(SoundName name)
    {
        SoundManager.Instance.PlaySound(name);
    }
    private void OnEnable()
    {
        if (IsPlayOnEnable) { PlaySound(); }
    }
}
