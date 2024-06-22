using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeGauge : MonoBehaviour
{
    public readonly Vector3 MAX_SCALE = new Vector3(1.8f, 1.8f, 1.8f);

    public event System.Action  OnChargeEvent;
    public event System.Action DisChargeEvent;

    private Player mPlayer;

    public float Charge
    {
        get;
        private set;
    }

    public void GaugeUp(float accel = 1.0f)
    {
        Charge = Mathf.Min(Charge + (Time.deltaTime * accel), 1);

        transform.localScale = Vector3.Lerp(Vector3.zero, MAX_SCALE, Charge);
    }

    private void OnEnable()
    {
        OnChargeEvent?.Invoke();

        if (mPlayer == null) {
            GameObject.FindGameObjectWithTag("Player").TryGetComponent(out mPlayer);
        }
        mPlayer.InputLock(true);

        void PlayerZoom(float zoomScale, float time)
        {
            float playerX;
            if (mPlayer.transform.position.x > 0)
            {
                playerX = Mathf.Min(+4.5f * (1 - zoomScale), mPlayer.transform.position.x);
            }
            else
            {
                playerX = Mathf.Max(-4.5f * (1 - zoomScale), mPlayer.transform.position.x);
            }
            float playerY = ((int)mPlayer.GetUnitizedPosV() - 1) * -(8 - 8 * zoomScale);

            MainCamera.Instance.ZoomIn(new Vector2(playerX, playerY), time, zoomScale);
        }
        PlayerZoom(0.6f, 4.5f);
    }

    private void OnDisable()
    {
        DisChargeEvent?.Invoke();

        mPlayer.InputLock(false);
        MainCamera.Instance.ZoomIn(Vector2.zero, 2.5f, 1f);

        transform.localScale = Vector3.zero;

        Charge = 0.0f;
    }
}
