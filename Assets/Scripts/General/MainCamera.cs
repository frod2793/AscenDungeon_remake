using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum FadeType { In, Out }

public static class MathExtension
{
    public static void Range(this ref float value, float min, float max)
    {
        if (value < min) value = min;
        if (value > max) value = max;
    }

    public static void SetZ(this Transform transform, float newZ)
    {
        float offsetZ = newZ - transform.position.z;

        transform.position += new Vector3(0, 0, offsetZ);
    }
}

public class MainCamera : Singleton<MainCamera>
{
    [Header("____Filter Props____")]
    [SerializeField] private Image FadeFilter;
    [SerializeField] private GameObject DamagedFilter;

    [Header("____Camera Props____")]
    [SerializeField] private Camera ThisCamera;
    [SerializeField] private float OriginCameraScale;

    private Action mFadeOverAction;
    private Player _Player;

    private IEnumerator mCameraFade;
    private IEnumerator mCameraShake;
    private IEnumerator mCameraMove;
    private IEnumerator mCameraZoom;

    private Vector3 mOriginPosition;

    private bool mIsZoomIn;

    private void Reset()
    {
        if (TryGetComponent(out ThisCamera))
        {
            OriginCameraScale = ThisCamera.orthographicSize;
        }
    }
    private void Start()
    {
        mIsZoomIn = false;
        mOriginPosition = transform.position;

        // -- 화면이 서서히 밝아지게 --
        FadeFilter.color = Color.black;

        Fade(1f, FadeType.Out);
        // -- 화면이 서서히 밝아지게 --

        _Player = FindObjectOfType<Player>();
    }
    public void UseDamagedFilter()
    {
        DamagedFilter.SetActive(true);
    }

    public void Shake()
    {
        Shake(0.25f, 0.8f);
    }

    public void Shake(float time, float power)
    {
        if (mCameraShake != null)
        {
            transform.parent.position = Vector3.zero;
            StopCoroutine(mCameraShake);
        }
        StartCoroutine(mCameraShake = CameraShake(time, power));
    }

    public void Move(Vector2 point, float speed = 1f)
    {
        if (mCameraMove != null)
        {
            StopCoroutine(mCameraMove);
        }
        StartCoroutine(mCameraMove = CameraMove(point, speed));
    }

    public void Fade(float time, FadeType fadeType)
    {
        Color fadeColor = Color.white;

        switch (fadeType)
        {
            case FadeType.In:
                fadeColor = Color.black;
                break;

            case FadeType.Out:
                fadeColor = Color.clear;
                break;
        }
        if (mCameraFade != null)
        {
            StopCoroutine(mCameraFade);
        }
        StartCoroutine(mCameraFade = CameraFade(time, fadeColor));
    }
    public void Fade(float time, FadeType fadeType, Action fadeOverAction)
    {
        mFadeOverAction = fadeOverAction;

        Fade(time, fadeType);
    }

    public void ZoomIn(float time, float percent, bool usingTimeScale)
    {
        ZoomIn(mOriginPosition, time, percent, usingTimeScale);
    }
    public void ZoomIn(Vector2 offset, float time, float percent)
    {
        if (mCameraZoom != null)
        {
            StopCoroutine(mCameraZoom);
        }
        float targetScale = 8 * percent;

        StartCoroutine(mCameraZoom = CameraZoomIn((Vector2)mOriginPosition + offset, time, targetScale, true));
    }
    public void ZoomIn(Vector2 point, float time, float percent, bool usingTimeScale)
    {
        if (mCameraZoom != null)
        {
            StopCoroutine(mCameraZoom);
        }
        float targetScale = OriginCameraScale * percent;

        //
        float range = 8 - targetScale;

        point.x.Range(
            mOriginPosition.x + range * -0.5625f, mOriginPosition.x + range * 0.5625f);

        point.y.Range(
            mOriginPosition.y - range, mOriginPosition.y + range);
        //
        mIsZoomIn = true;
        StartCoroutine(mCameraZoom = CameraZoomIn(point, time, targetScale, usingTimeScale));
    }

    public void ZoomOut(float time, bool usingTimeScale)
    {
        if (mCameraZoom != null)
        {
            StopCoroutine(mCameraZoom);
        }
        mIsZoomIn = false;
        StartCoroutine(mCameraZoom = CameraZoomOut(time, usingTimeScale));
    }

    private IEnumerator CameraFade(float time, Color fadeColor)
    {
        var initColor = FadeFilter.color;

        for (float i = 0f; i < time; i += Time.deltaTime * Time.timeScale)
        {
            FadeFilter.color = Color.Lerp(initColor, fadeColor, i / time);

            yield return null;
        }
        mCameraFade = null;

        mFadeOverAction?.Invoke();
        mFadeOverAction = null;
    }

    private IEnumerator CameraShake(float time, float power)
    {
        float deltaTime;

        power *= 0.1f;

        for (float i = 0; i < time; i += deltaTime)
        {
            transform.parent.position = (Vector3)(UnityEngine.Random.insideUnitCircle * power);

            deltaTime = Time.deltaTime;

            yield return null;
        }
        transform.parent.position = Vector3.zero;
    }

    private IEnumerator CameraMove(Vector2 point, float speed)
    {
        float lerpAmount = 0f;

        mOriginPosition = (Vector3)point + Vector3.back * 10f;

        while (lerpAmount < 1f)
        {
            lerpAmount = Mathf.Min(1f, lerpAmount + Time.deltaTime * Time.timeScale * speed);

            transform.position = Vector2.Lerp(transform.position, point, lerpAmount);
            transform.SetZ(-10f);

            yield return null;
        }
    }
    private IEnumerator CameraZoomIn(Vector2 point, float time, float targetScale, bool usingTimeScale)
    {
        float deltaTime = 0f;

        for (float i = 0; i < time; i += deltaTime)
        {
            deltaTime = Time.deltaTime;

            if (usingTimeScale)
            {
                deltaTime *= Time.timeScale;
            }
            float lerp = i / time;

            if (Mathf.Abs(targetScale - ThisCamera.orthographicSize) < 0.02f)
            {
                transform.position = point;
                transform.SetZ(-10f);

                ThisCamera.orthographicSize = targetScale;
                yield break;
            }
            transform.position = Vector2.Lerp(transform.position, point, lerp);
            transform.SetZ(-10f);

            ThisCamera.orthographicSize = Mathf.Lerp(ThisCamera.orthographicSize, targetScale, lerp);

            yield return null;
        }
        mCameraZoom = null;
    }

    private IEnumerator CameraZoomOut(float time, bool usingTimeScale)
    {
        float deltaTime = 0f;

        for (float i = 0; i < time; i += deltaTime)
        {
            deltaTime = Time.deltaTime;

            if (usingTimeScale)
            {
                deltaTime *= Time.timeScale;
            }
            float lerp = i / time;

            if (Mathf.Abs(OriginCameraScale - ThisCamera.orthographicSize) < 0.02f)
            {
                transform.position = mOriginPosition;
                transform.SetZ(-10f);

                ThisCamera.orthographicSize = OriginCameraScale;
                yield break;
            }
            transform.position = Vector2.Lerp(transform.position, mOriginPosition, lerp);
            transform.SetZ(-10f);

            ThisCamera.orthographicSize = Mathf.Lerp(ThisCamera.orthographicSize, OriginCameraScale, lerp);

            yield return null;
        }
        mCameraZoom = null;
    }
}
