using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TouchController : MonoBehaviour
{
    private const float NeedMovingLength = 20f;
    private const float NeedDashLength = 30f;
    private const float AttackableInputRange = 2.5f;

    private Coroutine _MoveRoutine;
    private TouchPhase _CurrentPhase;
    private TouchPhase _PreviousPhase;
    private Vector2 _BeganInputPoint;

    private Vector2 _FirstInputPoint;
    private Vector2  _LastInputPoint;

    private Player _Player;
    private float _StationaryTime;

    private bool _IsAlreadyCharging;
    private bool _IsAlreadyInit = false;
    private bool _IsPrevInputOnUI = false;

    private bool _CanDashOrder = true;

    public bool IsMobilePlatform()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.Android:
                return true;
            default:
                return false;
        }
    }
    public bool HasInput(TouchPhase phase)
    {
        if (IsMobilePlatform())
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == phase)
                {
                    return EventSystem.current.IsPointerOverGameObject(touch.fingerId);
                }
            }
            return false;
        }
        else
        {
            switch (phase)
            {
                case TouchPhase.Began:
                    {
                        return Input.GetMouseButtonDown(0);
                    }
                case TouchPhase.Moved:
                    {
                        return Input.GetMouseButton(0) && 
                            Mathf.Abs(Input.GetAxis("Mouse X")) + 
                            Mathf.Abs(Input.GetAxis("Mouse Y")) > 0f;
                    }
                case TouchPhase.Stationary:
                    {
                        return Input.GetMouseButton(0);
                    }
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    {
                        return Input.GetMouseButtonUp(0);
                    }
            }
        }
        return false;
    }
    public Vector2 InputPosition()
    {
        if (IsMobilePlatform())
        {
            return Input.GetTouch(0).position;
        }
        else
        {
            return Input.mousePosition;
        }
    }
    private void Start()
    {
        _StationaryTime = 0f;
        _IsAlreadyCharging = false;
        
        _CurrentPhase = TouchPhase.Ended;

        if (!_IsAlreadyInit)
        {
            _MoveRoutine = new Coroutine(this);

            _Player = FindObjectOfType<Player>();
        }
    }
    private void Update()
    {
#if UNITY_EDITOR
        CurrentPhaseCheck();
#else
        if (Input.touchCount > 0)
        {
            _CurrentPhase = Input.GetTouch(0).phase;
        }
        else
        {
            _CurrentPhase = TouchPhase.Ended;
        }
#endif
        if (!EventSystem.current.IsPointerInUIObject())
        {
            switch (_CurrentPhase)
            {
                case TouchPhase.Began:
                    {
                        _FirstInputPoint = _BeganInputPoint = InputPosition();
                    }
                    break;
                case TouchPhase.Moved:
                    {
                        Vector2 inputPosition = InputPosition();
                        Vector2 direction = (inputPosition - _BeganInputPoint).normalized;

                        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                        {
                            float distance = Vector2.Distance(inputPosition, _BeganInputPoint);

                            if (distance > NeedDashLength && _CanDashOrder)
                            {
                                void DashEndEvent(Player player, Direction dir)
                                {
                                    if (_CurrentPhase == TouchPhase.Stationary) 
                                        player.MoveOrder(dir);
                                }
                                if (direction.x > 0)
                                {
                                    _Player.DashOrder(UnitizedPosH.RIGHT);
                                    _Player.OnceDashEndEvent += p => DashEndEvent(p, Direction.Right);
                                }
                                else
                                {
                                    _Player.DashOrder(UnitizedPosH.LEFT);
                                    _Player.OnceDashEndEvent += p => DashEndEvent(p, Direction.Left);
                                }
                                _CanDashOrder = false;
                                _BeganInputPoint = inputPosition;
                            }
                            else if (Vector2.Distance(inputPosition, _BeganInputPoint) >= NeedMovingLength)
                            {
                                _BeganInputPoint = inputPosition;

                                if (direction.x > 0)
                                {
                                    _MoveRoutine.StartRoutine(Move(Direction.Right));
                                }
                                else
                                {
                                    _MoveRoutine.StartRoutine(Move(Direction.Left));
                                }
                            }
                        }
                        else
                        {
                            if (Vector2.Distance(inputPosition, _BeganInputPoint) >= NeedMovingLength * 1.5f)
                            {
                                _BeganInputPoint = inputPosition;

                                if (direction.y > 0)
                                {
                                    _MoveRoutine.StartRoutine(Move(Direction.Up));
                                }
                                else
                                {
                                    _MoveRoutine.StartRoutine(Move(Direction.Down));
                                }
                                _StationaryTime = 0f;
                            }
                        }
                    }
                    break;
                case TouchPhase.Stationary:
                    {
                        if (_MoveRoutine.IsFinished())
                        {
                            _StationaryTime += Time.deltaTime * Time.timeScale;

                            if (!_IsAlreadyCharging)
                            {
                                if (_StationaryTime > Finger.PRESS_TIME)
                                {
                                    _IsAlreadyCharging = true;
                                    Finger.Instance.StartCharging();
                                }
                            }
                        }
                    }
                    break;
                case TouchPhase.Ended:
                    {
                        _LastInputPoint = InputPosition();
                        _CanDashOrder = true;

                        if (_MoveRoutine.IsFinished())
                        {
                            if (_IsAlreadyCharging)
                            {
                                Finger.Instance.EndCharging();
                                _IsAlreadyCharging = false;
                            }
                            else if (_IsPrevInputOnUI)
                            {
                                if (Vector2.Distance(_LastInputPoint, _FirstInputPoint) <= AttackableInputRange)
                                {
                                    if (_PreviousPhase == TouchPhase.Began ||
                                        _PreviousPhase == TouchPhase.Stationary)
                                    {
                                        _Player.AttackOrder();
                                    }
                                }
                            }
                        }
                        _StationaryTime = 0f;
                    }
                    break;
                case TouchPhase.Canceled:
                    break;
                default:
                    break;
            }
        }
        _PreviousPhase = _CurrentPhase;
        _IsPrevInputOnUI = !EventSystem.current.IsPointerInUIObject();
    }
    private void CurrentPhaseCheck()
    {
        for (TouchPhase phase = TouchPhase.Began; (int)phase < 5; phase++)
        {
            if (HasInput(phase))
            {
                _CurrentPhase = phase;
                break;
            }
        }
    }
    private IEnumerator Move(Direction direction)
    {
        _Player.MoveOrder(direction);

        switch (direction)
        {
            case Direction.Up:
            case Direction.Down:
                {
                    while (_Player.IsMoving())
                    {
                        yield return null;
                    }
                }
                break;
            case Direction.Right:
            case Direction.Left:
                {
#if UNITY_EDITOR
                    while (!Input.GetMouseButtonUp(0))
                    {
                        yield return null;
                    }
#else
                    while (Input.touchCount > 0)
                    {
                        yield return null;
                    }
#endif
                }
                break;
            default:
                yield return null;
                break;
        }
        _Player.MoveStop();
        _MoveRoutine.Finish();
    }
    private void OnDisable()
    {
        _MoveRoutine?.StopRoutine();
    }
}
