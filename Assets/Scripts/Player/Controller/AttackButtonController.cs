using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Obsolete]
public class AttackButtonController : Singleton<AttackButtonController>
{
    public bool IsButtonHide
    {
        get;
        private set;
    }
    private readonly Vector2 TopPosition = new Vector2(0, 440);
    private readonly Vector2 BotPosition = new Vector2(0, 0);

    private readonly Vector2 LHidePosition = new Vector2(-510f, -360f);
    private readonly Vector2 LShowPosition = new Vector2(-250f, -360f);

    private readonly Vector2 RHidePosition = new Vector2(+510f, -360f);
    private readonly Vector2 RShowPosition = new Vector2(+250f, -360f);

    [SerializeField] private SubscribableButton _LAttackButton;
    [SerializeField] private SubscribableButton _RAttackButton;

    private Coroutine _ButtonShowRoutine;

    private Player _Player;

    private void Awake()
    {
        _Player = FindObjectOfType(typeof(Player)) as Player;

        _LAttackButton.ButtonAction += (state) => 
        {
            if (state == ButtonState.Down)
            {
                _Player.SetLookAtLeft(true);

                _Player.AttackOrder();
            }
        };
        _RAttackButton.ButtonAction += (state) => 
        {
            if (state == ButtonState.Down)
            {
                _Player.SetLookAtLeft(false);

                _Player.AttackOrder();
            }
        };
        _ButtonShowRoutine = new Coroutine(this);

        //_Player.MovingEvent += OnMoving;
    }
    public void ShowButton()
    {
        _ButtonShowRoutine.StartRoutine(EShowButton(true));
    }
    public void HideButton()
    {
        _ButtonShowRoutine.StartRoutine(EShowButton(false));
    }
    private IEnumerator EShowButton(bool show)
    {
        float time = 2f;

        Vector2 lTargetPosition = LHidePosition;
        Vector2 rTargetPosition = RHidePosition;

        IsButtonHide = show;

        if (show)
        {
            lTargetPosition = LShowPosition;
            rTargetPosition = RShowPosition;
        }
        for (float i = 0f; i < time; i += Time.deltaTime)
        {
            float ratio = Mathf.Min(i, time) / time;

            _LAttackButton.transform.localPosition
                = Vector2.Lerp(_LAttackButton.transform.localPosition, lTargetPosition, ratio);

            _RAttackButton.transform.localPosition
                = Vector2.Lerp(_RAttackButton.transform.localPosition, rTargetPosition, ratio);

            yield return null;
        }
        _ButtonShowRoutine.Finish();
    }
    private void OnMoving(UnitizedPosV movePosition, float ratio)
    {
        switch (movePosition)
        {
            case UnitizedPosV.TOP:
                {
                    transform.localPosition = Vector2.Lerp(transform.localPosition, TopPosition, ratio);
                }
                break;

            case UnitizedPosV.MID:
            case UnitizedPosV.BOT:
                {
                    transform.localPosition = Vector2.Lerp(transform.localPosition, BotPosition, ratio);
                }
                break;
        }
    }
}
