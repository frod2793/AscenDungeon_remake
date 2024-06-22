using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum Direction
{
    Up, Down, Right, Left, None
}

public class Finger : Singleton<Finger>
{
    public const float PRESS_TIME = 0.5f;

    private float DeltaTime => Time.deltaTime;

    [SerializeField]
    private ChargeGauge mChargeGauge;
    public  ChargeGauge Gauge
    { get => mChargeGauge; }

    public  Item  CarryItem
    {
        get => mCarryItem;
        set
        {
            if (value == null)
            {
                CarryItemImage.sprite = _EmptySprite;
            }
            else
            {
                CarryItemImage.sprite = value.Sprite;
            }
            mCarryItem = value; 
        }
    }
    private Item mCarryItem;

    private float mCurPressTime;

    private bool _IsMustBeReleased;
    private bool _HasBeginTouch;

    private IEnumerator mEOnBulletTime;

    private Sprite _EmptySprite;
    [SerializeField] private Image CarryItemImage;
    [SerializeField] private float SwipeLength;

    private Vector2 mTouchBeganPos;
    private Vector2 mTouchEndedPos;
    private Vector2 mSwipeDirection;

    private bool _ChargeEnable;

    private void Awake() 
    {
        mCurPressTime = 0f;
        mTouchBeganPos = mTouchEndedPos = mSwipeDirection = Vector2.zero;

        _EmptySprite = CarryItemImage.sprite;
        _ChargeEnable = false;
    }

    public void StartCharging()
    {
        _ChargeEnable = true;

        mChargeGauge.gameObject.SetActive(true);
        StartCoroutine(mEOnBulletTime = EOnBulletTime(1.5f, 0.45f));

        Inventory.Instance.BeginOfCharge();
    }
    public void EndCharging()
    {
        if (_ChargeEnable)
        {
            _ChargeEnable = false;

            Inventory.Instance.EndOfCharge(mChargeGauge.Charge);

            mChargeGauge.gameObject.SetActive(false);
            mCurPressTime = 0;

            StartCoroutine(EDisBulletTime(1.75f));
        }
    }

    private void Update()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WindowsEditor:
                {
                    CarryItemImage.transform.position = Input.mousePosition;
                }
                break;

            case RuntimePlatform.Android:
                if (Input.touchCount > 0)
                {
                    Touch touch = Input.GetTouch(0);

                    CarryItemImage.transform.position = touch.position;
                }
                break;
        }
        transform.SetZ(0);

        bool windowEnable = Inventory.Instance.InventoryWindow.activeSelf;

        if (CarryItemImage.IsActive() != windowEnable)
        {
            CarryItemImage.gameObject.SetActive(windowEnable);
        }
        if (_ChargeEnable)
        {
            mCurPressTime += Time.deltaTime;
            mChargeGauge.GaugeUp(0.8f);
        }
        #region Old Version Charging Input
        /*
        else if (mCurPressTime >= PRESS_TIME)
        {
            Inventory.Instance.OnCharge(mChargeGauge.Charge);

            mChargeGauge.gameObject.SetActive(false);
            mCurPressTime = 0;

            StartCoroutine(EDisBulletTime(1.75f));
        }
        if (!EventSystem.current.IsPointerInUIObject())
        {
            // Begin Touch
            if (BeginInputCheck())
            {
                mChargeGauge.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                mChargeGauge.transform.Translate(0, 0, 10);
            }
            // Touuuuuuuuuuuuuch
            else if (StationaryInputCheck())
            {
                if (mCurPressTime >= PRESS_TIME)
                {
                    mChargeGauge.gameObject.SetActive(true);

                    mChargeGauge.GaugeUp(0.8f);

                    if (mEOnBulletTime == null)
                    {
                        StartCoroutine(mEOnBulletTime = EOnBulletTime(1.5f, 0.45f));
                    }
                }
                else
                {
                    mCurPressTime += Time.deltaTime;
                }
            }
            // End Touch
            else InputReleaseCheck();
        }
        else InputReleaseCheck();

        #region Local Functions
        bool BeginInputCheck()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    return Input.GetMouseButtonDown(0);

                case RuntimePlatform.Android:
                    if (Input.touchCount > 0)
                    {
                        return Input.GetTouch(0).phase == TouchPhase.Began;
                    }
                    return false;
                default:
                    return false;
            }
        }
        bool StationaryInputCheck()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    return Input.GetMouseButton(0);

                case RuntimePlatform.Android:
                    if (Input.touchCount > 0)
                    {
                        var touch = Input.GetTouch(0);

                        return touch.phase == TouchPhase.Stationary ||
                               touch.phase == TouchPhase.Moved;
                    }
                    return false;
                default:
                    return false;
            }
        }
        bool EndInputCheck()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    return Input.GetMouseButtonUp(0);

                case RuntimePlatform.Android:
                    if (Input.touchCount > 0)
                    {
                        return Input.GetTouch(0).phase == TouchPhase.Ended;
                    }
                    return false;
                default:
                    return false;
            }
        }
        void InputReleaseCheck()
        {
            if (EndInputCheck() && mCurPressTime >= PRESS_TIME)
            {
                Inventory.Instance.OnCharge(mChargeGauge.Charge);

                mChargeGauge.gameObject.SetActive(false);
                mCurPressTime = 0;

                StartCoroutine(EDisBulletTime(1.75f));
            }
        }
        #endregion
        */
        #endregion
    }
    private IEnumerator EOnBulletTime(float accel, float slowMax)
    {
        float lerpAmount = 0f;

        while (!Input.GetMouseButtonUp(0))
        {
            lerpAmount = Mathf.Min(slowMax, lerpAmount + accel * DeltaTime);

            Time.timeScale = Mathf.Lerp(Time.timeScale, slowMax, lerpAmount);

            yield return null;
        }
        mEOnBulletTime = null;

        yield break;
    }

    private IEnumerator EDisBulletTime(float accel, float origin = 1f)
    {
        float lerpAmount = 0f;

        while (Time.timeScale != origin)
        {
            lerpAmount = Mathf.Min(origin, lerpAmount + accel * DeltaTime);

            Time.timeScale = Mathf.Lerp(Time.timeScale, origin, lerpAmount);

            yield return null;
        }
        yield break;
    }

    public bool Swipe(Direction inputDriection)
    {
        bool canMove = false;

        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);

            if (!EventSystem.current.IsPointerInUIObject())
            {
                if (t.phase == TouchPhase.Began)
                {
                    _HasBeginTouch = true;

                    mTouchBeganPos = t.position;
                }
                if (t.phase == TouchPhase.Ended)
                {
                    _HasBeginTouch = false;

                    _IsMustBeReleased = false;
                }
            }
            if (_HasBeginTouch && t.phase == TouchPhase.Moved && !_IsMustBeReleased)
            {
                mTouchEndedPos = t.position;

                if (Vector2.Distance(mTouchBeganPos, mTouchEndedPos) >= SwipeLength)
                {
                    mSwipeDirection = mTouchEndedPos - mTouchBeganPos;

                    mSwipeDirection.Normalize();

                    if (Mathf.Abs(mSwipeDirection.x) > Mathf.Abs(mSwipeDirection.y))
                    {
                        canMove =
                            (mSwipeDirection.x > 0 && Direction.Right == inputDriection) ||
                            (mSwipeDirection.x < 0 && Direction.Left == inputDriection);
                    }
                    else
                    {
                        canMove =
                            (mSwipeDirection.y > 0 && Direction.Up == inputDriection) ||
                            (mSwipeDirection.y < 0 && Direction.Down == inputDriection);
                    }

                }
            }
        }
        if (canMove)
        {
            _IsMustBeReleased = true;
        }
        return canMove;
    }
}
