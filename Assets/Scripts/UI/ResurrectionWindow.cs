using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Assets.Scripts.Ad;

public class ResurrectionWindow : MonoBehaviour
{
    [FormerlySerializedAs("_NextWindow")]
    [SerializeField] private GameObject _ResultWindow;
    [SerializeField] private GameObject _ResurrectionWindow;
    [SerializeField] private Resurrectable _Resurrectable;

    private bool _IsAlreadyEarn = false;

    [ContextMenu("FindPlayer")]
    private void FindPlayer()
    {
        if (_Resurrectable == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");

            Debug.Assert(player.TryGetComponent(out _Resurrectable));
        }
    }
    private void Awake()
    {
        FindPlayer();

        if (_Resurrectable.TryGetComponent(out Player player))
        {
            player.DeathEvent += revertAction =>
            {
                if (!revertAction)
                {
                    if (!_IsAlreadyEarn)
                    {
                        SetActiveResurrectionWindow(true);
                    }
                    else
                    {
                        _ResultWindow.SetActive(true);
                    }
                }
            };
        }
    }
    public void ShowAD()
    {
        if (!_IsAlreadyEarn)
        {
            // [안전 장치]: 광고 서비스 미초기화 시 광고 생략
            if (AdsManager.Service != null)
            {
                AdsManager.Service.ShowRewardedAd(() =>
                {
                    _IsAlreadyEarn = true;
                    SetActiveResurrectionWindow(false);

                    _Resurrectable.Resurrect();
                });
            }
            else
            {
                Debug.LogWarning("[ResurrectionWindow] 광고 서비스가 준비되지 않았습니다.");
            }
        }
    }
    public void Close()
    {
        SetActiveResurrectionWindow(false);

        _ResultWindow.SetActive(true);
    }
    public void SetActiveResurrectionWindow(bool active)
    {
        _ResurrectionWindow.SetActive(active);

        Time.timeScale = active ? 0f : 1f;
        MainCamera.Instance.Shake(0f, 0f);
    }
}
