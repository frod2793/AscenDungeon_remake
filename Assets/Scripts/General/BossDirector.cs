using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class BossDirector : MonoBehaviour
{
    [SerializeField] private PlayableDirector _Director;
    [SerializeField] private Floor _BossFloor;

    [Header("Audio Property")]
    [SerializeField] private AudioClip _BossThema;
    [SerializeField] private AudioSource _AudioSource;

    [Header("Controller Property")]
    [SerializeField] private GameObject _VirtualJoystick;
    [SerializeField] private GameObject _TouchController;

    private void Awake()
    {
        Inventory.Instance.MoveUpDownEvent += (pos, dir) =>
        {
            // 다음 층으로 이동한다면
            if (dir == Direction.Up && pos == UnitizedPosV.BOT)
            {
                // 보스 이전 층에 입장
                if (Castle.Instance.PlayerFloor.FloorIndex == _BossFloor.FloorIndex - 1)
                {
                    Castle.Instance.CanPlayerFloorNotify = false;
                }
                // 보스 층에 입장
                else if (Castle.Instance.PlayerFloor.FloorIndex == _BossFloor.FloorIndex)
                {
                    _Director.Play();
                    Castle.Instance.ForceStopUpdate();


                }
            }
        };
    }
    public void SE_Start()
    {
        _AudioSource.clip = _BossThema;

        if (GameLoger.Instance.UsingVJoystick)
        {
            _VirtualJoystick.SetActive(false);
        }
        else
        {
            _TouchController.SetActive(false);
        }
    }
    public void SE_CameraSetPosition()
    {
        Camera.main.transform.position = 
            _BossFloor.transform.localPosition + Vector3.back * 10;
    }
    public void SE_Finish()
    {
        Castle.Instance.CanPlayerFloorNotify = true;
        Castle.Instance.PlayerFloorNotify();

        Castle.Instance.ReStartUpdate();

        if (GameLoger.Instance.UsingVJoystick)
        {
            _VirtualJoystick.SetActive(true);
        }
        else
        {
            _TouchController.SetActive(true);
        }
    }
}
