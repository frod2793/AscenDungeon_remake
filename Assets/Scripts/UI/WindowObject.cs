using UnityEngine;

public class WindowObject : MonoBehaviour
{
    public enum UsingCheck
    { Using, UnUsing }


    [SerializeField] private bool _IsEnablePause; [Space()]


    [SerializeField] private UsingCheck _IntractableLayer;
    [SerializeField] private BlockingLayer _BlockingLayer;

    private void OnEnable()
    {
        switch (_IntractableLayer)
        {
            case UsingCheck.Using:
                _BlockingLayer.SetActive(true, gameObject);
                break;

            case UsingCheck.UnUsing:
                _BlockingLayer.gameObject.SetActive(true);
                break;
        }

        if (_IsEnablePause)
        {
            Time.timeScale = 0f;
        }


    }
    private void OnDisable()
    {
        switch (_IntractableLayer)
        {
            case UsingCheck.Using:
                _BlockingLayer.SetActive(false, gameObject);
                break;

            case UsingCheck.UnUsing:
                _BlockingLayer.gameObject.SetActive(false);
                break;
        }

        if (_IsEnablePause)
        {
            Time.timeScale = 1f;
        }

    }
}
