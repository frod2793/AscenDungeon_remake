using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using BackEnd;
public static class InputExtension
{
    // 현재 입력이 'UI를 대상으로한 입력인가'의 여부를 반환
    public static bool IsPointerInUIObject(this EventSystem eventSystem)
    {
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WindowsEditor:
                return eventSystem.IsPointerOverGameObject();

            case RuntimePlatform.Android:
                if (Input.touchCount > 0)
                {
                    Touch touch = Input.GetTouch(0);

                    return eventSystem.IsPointerOverGameObject(touch.fingerId);
                }
                return false;
        }
        return false;
    }
}

public class Player : MonoBehaviour, ICombatable
{
    private readonly Vector3 LookAtLeft = new Vector3(0f, 180f, 0f);

    #region COMMENT
    /// <summary>
    /// parameter1 : revert death event?
    /// </summary>
    #endregion
    public event Action<bool> DeathEvent;
    public event Action<UnitizedPosV, float> MovingEvent;
    public event Action<Player> OnceDashEndEvent;

    [SerializeField] private bool CanMoveDown;
    [SerializeField] private bool IsUsingHealthBar;
    
    [SerializeField]
    private Resurrectable _Resurrectable;

    [SerializeField]
    private GameObject EquipWeaponSlot;

    [SerializeField] 
    private Area RangeArea;

    [SerializeField]
    private AbilityTable AbilityTable;

    [SerializeField]
    private PlayerAnimator PlayerAnimator;

    [SerializeField]
    private float mDefense;

    [Header("Dash Property")]
    [SerializeField, Range(0f, 3.5f)] private float _DashLength;
    [SerializeField, Range(0f, 2.0f)] private float _DashTime;
    [SerializeField, Range(0f, 2.0f)] private float _DashCool;
    [SerializeField, Range(1f, 3.0f)] private float _DashSpeedScale;
    [SerializeField] private AnimationCurve _DashCurve;

    private Coroutine _DashRoutine;
    private Coroutine _DashCoolRoutine;

    [Space(), SerializeField]
    private UnitizedPos mLocation9;
    
    private IEnumerator mEMove;

    private Inventory mInventory;

    private SpriteRenderer mRenderer;

    private List<Collider2D> mCollidersOnMove;

    private AttackPeriod mAttackPeriod;

    private bool mCanElevation;

    private bool mIsMovingElevation;

    private bool mIsInputLock;

    private Coroutine _MoveRoutine;

    public AbilityTable GetAbility => AbilityTable;

    private CircleCollider2D mRangeCollider;

    private float DeltaTime
    { get => Time.deltaTime * Time.timeScale; }

    public UnitizedPosV GetUnitizedPosV()
    {
        switch (mLocation9)
        {
            case UnitizedPos.TOP_LEFT:
            case UnitizedPos.TOP:
            case UnitizedPos.TOP_RIGHT:
                return UnitizedPosV.TOP;

            case UnitizedPos.MID_LEFT:
            case UnitizedPos.MID:
            case UnitizedPos.MID_RIGHT:
                return UnitizedPosV.MID;

            case UnitizedPos.BOT_LEFT:
            case UnitizedPos.BOT:
            case UnitizedPos.BOT_RIGHT:
                return UnitizedPosV.BOT;

            default:
                break;
        }
        Debug.Log("Value Error");
        return UnitizedPosV.NONE;
    }

    private void Start()
    {
        PlayerAnimator.Init();
        if (IsUsingHealthBar)
        {
            HealthBarPool.Instance?.UsingPlayerHealthBar(-1f, transform, AbilityTable);
        }
        mIsInputLock  = false;
        mCanElevation = false;

        mIsMovingElevation = false;

        mAttackPeriod = new AttackPeriod(AbilityTable);
        mAttackPeriod.SetAction(Period.Attack, AttackAction);

        mCollidersOnMove = new List<Collider2D>();
        Debug.Assert(gameObject.TryGetComponent(out mRenderer));

        mInventory = Inventory.Instance;

        _MoveRoutine = _MoveRoutine ?? new Coroutine(this);
        _DashRoutine = _DashRoutine ?? new Coroutine(this);
        _DashCoolRoutine = new Coroutine(this);

        if (RangeArea.gameObject.TryGetComponent(out mRangeCollider))
        {
            mInventory.WeaponEquipEvent += o =>
            {               
                if (o == null)
                {
                    mRangeCollider.radius = AbilityTable.GetAblity[Ability.Range];

                    AbilityTable.Table[Ability.After_AttackDelay] = AbilityTable.GetAblity[Ability.After_AttackDelay];
                    AbilityTable.Table[Ability.Begin_AttackDelay] = AbilityTable.GetAblity[Ability.Begin_AttackDelay];

                    mAttackPeriod.StopPeriod();
                }
                else
                {
                    o.transform.parent = EquipWeaponSlot.transform;

                    o.transform.localRotation = Quaternion.Euler(Vector3.zero);
                    o.transform.localScale    = Vector3.one;
                    o.transform.localPosition = Vector3.zero;

                    mRangeCollider.radius = o.WeaponRange;

                    AbilityTable.Table[Ability.After_AttackDelay] = o.After_AttackDelay;
                    AbilityTable.Table[Ability.Begin_AttackDelay] = o.Begin_AttackDelay;

                    o.AttackOverAction = () => mAttackPeriod.AttackActionOver();

                    mAttackPeriod.StopPeriod();
                }
            };
            mInventory.WeaponChangeEvent += o =>
            {
                o.transform.parent   = ItemStateSaver.Instance.transform;
                o.transform.position = new Vector3(-10, 0, 0);

                mAttackPeriod.StopPeriod();
            };
        }
        var instance = ItemStateSaver.Instance.LoadSlotItem(SlotType.Weapon, 0);
        if (instance != null) {
            instance = ItemLibrary.Instance.GetItemObject(instance.ID);
        } mInventory.SetWeaponSlot(instance);

        _Resurrectable.ResurrectAction += ResurrectAction;
    }
    private void OnDestroy()
    {
        // 마을에서 다른 씬으로 이동하는 것이 아니라면, 인벤토리를 비운다.
        if (SceneManager.GetActiveScene().buildIndex != (int)SceneIndex.Town)
        {
            var list = new List<int>()
                {
                    // ___Weapon___
                    (int)ItemID.None,
                    
                    // ___Accessory___
                    (int)ItemID.None, (int)ItemID.None, (int)ItemID.None,

                    // ___Container___
                    (int)ItemID.None, (int)ItemID.None, (int)ItemID.None,
                    (int)ItemID.None, (int)ItemID.None, (int)ItemID.None
                };
            ItemStateSaver.Instance.SetInventoryItem(list);
        }
    }
    private void Update()
    {
        if (mEMove == null)
        {
            Vector2 interactionPoint = Vector2.zero;

            if (Input.touchCount > 0 || Input.GetMouseButtonDown(0))
            {
                if (!EventSystem.current.IsPointerInUIObject())
                {
                    switch (Application.platform)
                    {
                        case RuntimePlatform.WindowsPlayer:
                        case RuntimePlatform.WindowsEditor:
                            if (Input.GetMouseButtonDown(0))
                            {
                                interactionPoint =
                                    Camera.main.ScreenToWorldPoint(Input.mousePosition);
                            }
                            break;

                        case RuntimePlatform.Android:
                            if (Input.touchCount > 0)
                            {
                                interactionPoint =
                                    Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
                            }
                            break;
                    }
                }

            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (mEMove != null) mCollidersOnMove.Add(collision);
    }

    public void BackToOriginalAnim()
    {
        if (AbilityTable[Ability.CurHealth] > 0f)
        {
            if (!_MoveRoutine.IsFinished())
            {
                PlayerAnimator.ChangeState(PlayerAnim.Move);
            }
            else
            {
                PlayerAnimator.ChangeState(PlayerAnim.Idle);
            }
        }
    }

    public void SetLookAtLeft(bool lookLeft)
    {
        if (lookLeft)
        {
            transform.localRotation = Quaternion.Euler(LookAtLeft);
        }
        else
            transform.localRotation = Quaternion.Euler(Vector3.zero);
    }

    // ========== Dash Order ========== //
    public void DashOrder(UnitizedPosH direction)
    {
        if (_DashRoutine.IsFinished() && _DashCoolRoutine.IsFinished())
        {
            Direction dir = Direction.None;

            Vector2 dashPoint = transform.position;
            switch (direction)
            {
                case UnitizedPosH.LEFT:
                    {
                        dashPoint += Vector2.left * _DashLength;
                        SetLookAtLeft(true);

                        Inventory.Instance.PlayerBeginDash(dir = Direction.Left);
                    }
                    break;

                case UnitizedPosH.RIGHT:
                    {
                        dashPoint += Vector2.right * _DashLength;
                        SetLookAtLeft(false);

                        Inventory.Instance.PlayerBeginDash(dir = Direction.Right);
                    }
                    break;
            }
            _MoveRoutine.StopRoutine();
            _DashRoutine.StartRoutine(DashRoutine(dashPoint, dir));
        }
    }
    private IEnumerator DashRoutine(Vector2 dashPoint, Direction direction)
    {
        PlayerAnimator.ChangeState(PlayerAnim.Dash);

        Vector2 movePointMin = Vector2.zero;
        Vector2 movePointMax = Vector2.zero;

        switch (GetUnitizedPosV())
        {
            case UnitizedPosV.TOP:
                movePointMin = Castle.Instance.GetMovePoint(UnitizedPos.TOP_LEFT);
                movePointMax = Castle.Instance.GetMovePoint(UnitizedPos.TOP_RIGHT);
                break;
            case UnitizedPosV.MID:
                movePointMin = Castle.Instance.GetMovePoint(UnitizedPos.MID_LEFT);
                movePointMax = Castle.Instance.GetMovePoint(UnitizedPos.MID_RIGHT);
                break;
            case UnitizedPosV.BOT:
                movePointMin = Castle.Instance.GetMovePoint(UnitizedPos.BOT_LEFT);
                movePointMax = Castle.Instance.GetMovePoint(UnitizedPos.BOT_RIGHT);
                break;
        }
        Vector2 start = transform.position;

        for (float i = 0; i < _DashTime; i += Time.deltaTime * Time.timeScale * AbilityTable.MoveSpeed * _DashSpeedScale)
        {
            float ratio = _DashCurve.Evaluate(i / _DashTime);
            transform.position = Vector2.Lerp(start, dashPoint, ratio);

            if (transform.position.x < movePointMin.x)
            {
                transform.position = new Vector2(movePointMin.x, transform.position.y);
                break;
            }
            else if (transform.position.x > movePointMax.x)
            {
                transform.position = new Vector2(movePointMax.x, transform.position.y);
                break;
            }
            yield return null;
        }
        _DashRoutine.Finish();
        // BackToOriginalAnim();

        OnceDashEndEvent?.Invoke(this);
        OnceDashEndEvent = null;

        Inventory.Instance.PlayerEndDash(direction);

        _DashCoolRoutine.StartRoutine(DashCoolTime());
    }
    private IEnumerator DashCoolTime()
    {
        for (float i = 0f; i < _DashCool; i += Time.deltaTime * Time.timeScale) yield return null;
        _DashCoolRoutine.Finish();
    }
    // ========== Dash Order ========== //

    public void AttackCancel()
    {
        Inventory.Instance.AttackCancel();

        mAttackPeriod.StopPeriod();
    }

    public void AttackOrder()
    {
        if (mInventory.IsEquipWeapon() && AbilityTable[Ability.CurHealth] > 0f)
        {
            if (!mAttackPeriod.IsProgressing())
            {

                mAttackPeriod.StartPeriod();
            }
        }
    }

    private void AttackAction()
    {
        mInventory.AttackAction(gameObject, null);
    }
    private void ResurrectAction()
    {
        SoundManager.Instance.PlaySound(SoundName.Resurrection);

        RangeArea.enabled = true;
        PlayerAnimator.ChangeState(PlayerAnim.Damaged);

        if (TryGetComponent(out Collider2D collider))
        {
            collider.enabled = true;
        }
        DeathEvent?.Invoke(true);

        AbilityTable.Table[Ability.CurHealth] 
            = AbilityTable[Ability.MaxHealth] * 0.3f;
    }
    private void DeathAction()
    {
        RangeArea.enabled = false;
        if (TryGetComponent(out Collider2D collider)) {

            collider.enabled = false;
        }
        DeathEvent?.Invoke(false);
    }

    public bool IsMoving()
    {
        return !_MoveRoutine.IsFinished();
    }
    public void MoveStop()
    {
        _MoveRoutine.StopRoutine();

        if (AbilityTable[Ability.CurHealth] > 0f)
        {
            if (_DashRoutine.IsFinished())
            {
                PlayerAnimator.ChangeState(PlayerAnim.Idle);
            }
        }
    }

    public void MoveOrder(Direction direction)
    {
        if (!mIsInputLock && AbilityTable[Ability.CurHealth] > 0f && _MoveRoutine.IsFinished() && _DashRoutine.IsFinished())
        {
            Vector2 movePoint = Vector2.zero;

            UnitizedPos moveDir9 = UnitizedPos.END;
            switch (direction)
            {
                case Direction.Up:
                    AttackCancel();
                    switch (GetUnitizedPosV())
                    {
                        case UnitizedPosV.TOP:
                            {
                                if (Castle.Instance.CanNextPoint(out movePoint))
                                {
                                    mLocation9 += 6;
                                    Inventory.Instance.PlayerMoveUpDownBegin(GetUnitizedPosV(), direction);

                                    _MoveRoutine.StartRoutine(MoveWithPoint(movePoint));
                                }
                            }
                            break;

                        case UnitizedPosV.MID:
                        case UnitizedPosV.BOT:
                            {
                                mIsMovingElevation = true;

                                moveDir9 = mLocation9 - 3;

                                movePoint = Castle.Instance.GetMovePoint(moveDir9);
                                movePoint.x = transform.position.x;
                                _MoveRoutine.StartRoutine(MoveWithPoint(movePoint, direction));
                            }
                            break;
                    }
                    break;
                case Direction.Down:
                    AttackCancel();
                    switch (GetUnitizedPosV())
                    {
                        case UnitizedPosV.TOP:
                        case UnitizedPosV.MID:
                            {
                                mIsMovingElevation = true;

                                moveDir9 = mLocation9 + 3;

                                movePoint = Castle.Instance.GetMovePoint(moveDir9);
                                movePoint.x = transform.position.x;
                                _MoveRoutine.StartRoutine(MoveWithPoint(movePoint, direction));
                            }
                            break;
                        case UnitizedPosV.BOT:
                            {
                                if (CanMoveDown && Castle.Instance.CanPrevPoint(out movePoint))
                                {
                                    mLocation9 -= 6;
                                    Inventory.Instance.PlayerMoveUpDownBegin(GetUnitizedPosV(), direction);

                                    _MoveRoutine.StartRoutine(MoveWithPoint(movePoint));
                                }
                            }
                            break;
                    }
                    break;
                case Direction.Right:
                    _MoveRoutine.StartRoutine(MoveWithDir(Vector2.right));
                    SetLookAtLeft(false);
                    break;
                case Direction.Left:
                    _MoveRoutine.StartRoutine(MoveWithDir(Vector2.left));
                    SetLookAtLeft(true);
                    break;
            }
        }
    }
    private IEnumerator MoveWithDir(Vector3 direction)
    {
        Vector2 movePointMin = Vector2.zero;
        Vector2 movePointMax = Vector2.zero;

        switch (GetUnitizedPosV())
        {
            case UnitizedPosV.TOP:
                movePointMin = Castle.Instance.GetMovePoint(UnitizedPos.TOP_LEFT);
                movePointMax = Castle.Instance.GetMovePoint(UnitizedPos.TOP_RIGHT);
                break;
            case UnitizedPosV.MID:
                movePointMin = Castle.Instance.GetMovePoint(UnitizedPos.MID_LEFT);
                movePointMax = Castle.Instance.GetMovePoint(UnitizedPos.MID_RIGHT);
                break;
            case UnitizedPosV.BOT:
                movePointMin = Castle.Instance.GetMovePoint(UnitizedPos.BOT_LEFT);
                movePointMax = Castle.Instance.GetMovePoint(UnitizedPos.BOT_RIGHT);
                break;
        }
        bool IsOutOfRange()
        {
            return transform.position.x < movePointMin.x 
                || transform.position.x > movePointMax.x;
        }
        PlayerAnimator.ChangeState(PlayerAnim.Move);

        float rateTime = 0f;

        while (!IsOutOfRange())
        {
            float deltaTime = Time.deltaTime * Time.timeScale;
            rateTime += deltaTime;

            if (rateTime >= 0.3f)
            {
                rateTime = 0f;
                var dust = EffectLibrary.Instance.UsingEffect(EffectKind.Dust, transform.position - new Vector3(0.2f, 0.2f));

                dust.transform.rotation = Quaternion.Euler(IsLookAtLeft() ? Vector3.zero : LookAtLeft);
            }
            transform.position += direction * deltaTime * AbilityTable.MoveSpeed;
            yield return null;
        }
        if (IsOutOfRange())
        {
            Vector2 position = transform.position;
                    position.x = Mathf.Clamp(position.x, movePointMin.x, movePointMax.x);

            transform.position = position;
        }
        _MoveRoutine.Finish();
    }
    private IEnumerator MoveWithPoint(Vector3 movePoint)
    {
        SoundManager.Instance.PlaySound(SoundName.PlayerDash);
        float DeltaTime() => Time.deltaTime * Time.timeScale;

        for (float ratio = 0f; ratio < 1f; ratio += DeltaTime() * AbilityTable.MoveSpeed)
        {
            ratio = Mathf.Min(ratio, 1f);

            transform.position = Vector3.Lerp(transform.position, movePoint, ratio);
            yield return null;
        }

        _MoveRoutine.Finish();
    }
    private IEnumerator MoveWithPoint(Vector3 movePoint, Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                mLocation9 -= 3;
                Inventory.Instance.PlayerMoveUpDownBegin(GetUnitizedPosV(), direction);
                break;
            case Direction.Down:
                mLocation9 += 3;
                Inventory.Instance.PlayerMoveUpDownBegin(GetUnitizedPosV(), direction);
                break;
            case Direction.Right:
                mLocation9++;
                break;
            case Direction.Left:
                mLocation9--;
                break;
        }
        yield return StartCoroutine(MoveWithPoint(movePoint));
    }

    public void InputLock(bool isLock) {
        mIsInputLock = isLock;
    }

    public bool IsLookAtLeft()
    {
        return transform.rotation.eulerAngles.Equals(LookAtLeft);
    }
    
    public void Damaged(float damage, GameObject attacker)
    {
        MainCamera.Instance.Shake(0.3f, 1.0f);

        mInventory.OnDamaged(ref damage, attacker, gameObject);

        AbilityTable.Table[Ability.CurHealth] -= damage / mDefense;
        if (damage > 0f)
        {
            PlayerAnimator.ChangeState(PlayerAnim.Damaged);

            EffectLibrary.Instance.UsingEffect(EffectKind.Damage, transform.position);
            MainCamera.Instance.UseDamagedFilter();
        }
        if (AbilityTable.Table[Ability.CurHealth] <= 0f)
        {
            Debug.Log("저장");


            BackEndServerManager.Instance.SendDataToServerSchema();

            PlayerAnimator.ChangeState(PlayerAnim.Death);
        }
    }

    public void CastBuff(Buff buffType, IEnumerator castedBuff)
    {
        StartCoroutine(castedBuff);
    }

    #region Obsolete Function
    [Obsolete]
    private void InputAction()
    {
        if (!mIsInputLock)
        {
            UnitizedPos moveDir9 = UnitizedPos.END;

            if (Input.GetKey(KeyCode.UpArrow) || Finger.Instance.Swipe(Direction.Up))
            {
                switch (GetUnitizedPosV())
                {
                    case UnitizedPosV.TOP:
                        mCanElevation = Castle.Instance.CanNextPoint();

                        moveDir9 = mLocation9;
                        break;

                    case UnitizedPosV.MID:
                    case UnitizedPosV.BOT:
                        {
                            mIsMovingElevation = true;

                            moveDir9 = mLocation9 - 3;
                        }
                        break;
                }
            }
            else if (Input.GetKey(KeyCode.DownArrow) || Finger.Instance.Swipe(Direction.Down))
            {
                switch (GetUnitizedPosV())
                {
                    case UnitizedPosV.TOP:
                    case UnitizedPosV.MID:
                        {
                            mIsMovingElevation = true;

                            moveDir9 = mLocation9 + 3;
                        }
                        break;
                    case UnitizedPosV.BOT:
                        {
                            if (CanMoveDown)
                            {
                                mCanElevation = Castle.Instance.CanPrevPoint();

                                moveDir9 = mLocation9;
                            }
                        }
                        break;
                }
            }
            else if (Input.GetKey(KeyCode.LeftArrow) || Finger.Instance.Swipe(Direction.Left))
            {
                if (GetTPOSITION3() != UnitizedPosH.LEFT)
                    moveDir9 = mLocation9 - 1;
            }
            else if (Input.GetKey(KeyCode.RightArrow) || Finger.Instance.Swipe(Direction.Right))
            {
                if (GetTPOSITION3() != UnitizedPosH.RIGHT)
                    moveDir9 = mLocation9 + 1;
            }
            if (moveDir9 != UnitizedPos.END) MoveAction(moveDir9);
        }
    }
    [Obsolete]
    private void MoveAction(UnitizedPos moveDIR9)
    {
        if (mEMove == null)
        {
            mAttackPeriod.StopPeriod();
            AttackCancel();

            if (mCanElevation)
            {
                switch (moveDIR9)
                {
                    case UnitizedPos.TOP_LEFT:
                    case UnitizedPos.TOP:
                    case UnitizedPos.TOP_RIGHT:
                        if (Castle.Instance.CanNextPoint(out Vector2 nextPoint))
                        {
                            StartCoroutine(mEMove = EMove(nextPoint, moveDIR9 + 6));
                        }
                        break;

                    case UnitizedPos.BOT_LEFT:
                    case UnitizedPos.BOT:
                    case UnitizedPos.BOT_RIGHT:
                        if (Castle.Instance.CanPrevPoint(out Vector2 prevPoint))
                        {
                            StartCoroutine(mEMove = EMove(prevPoint, moveDIR9 - 6));
                        }
                        break;
                }

            }
            else
            {
                Vector2 movePoint = Castle.Instance.GetMovePoint(moveDIR9);

                StartCoroutine(mEMove = EMove(movePoint, moveDIR9));
            }
        }
    }
    [Obsolete]
    private IEnumerator EMove(Vector2 movePoint, UnitizedPos moveDIR9)
    {
        bool a = false;

        if (mIsMovingElevation)
        {
            PlayerAnimator.ChangeState(PlayerAnim.Jump);

            a = true;
        }
        else
            PlayerAnimator.ChangeState(PlayerAnim.Move);

        for (float lerpAmount = 0f; lerpAmount < 1f;)
        {
            lerpAmount = Mathf.Min(1f, lerpAmount + DeltaTime * AbilityTable.MoveSpeed);

            transform.position = Vector2.Lerp(transform.position, movePoint, lerpAmount);

            UnitizedPosV moveDIR3 = UnitizedPosV.NONE;

            switch (moveDIR9)
            {
                case UnitizedPos.TOP_LEFT:
                case UnitizedPos.TOP:
                case UnitizedPos.TOP_RIGHT:
                    moveDIR3 = UnitizedPosV.TOP;
                    break;
                case UnitizedPos.MID_LEFT:
                case UnitizedPos.MID:
                case UnitizedPos.MID_RIGHT:
                    moveDIR3 = UnitizedPosV.MID;
                    break;
                case UnitizedPos.BOT_LEFT:
                case UnitizedPos.BOT:
                case UnitizedPos.BOT_RIGHT:
                    moveDIR3 = UnitizedPosV.BOT;
                    break;
            }
            MovingEvent?.Invoke(moveDIR3, lerpAmount);

            if (a)
            {
                if (mIsMovingElevation && lerpAmount >= 0.1f)
                {
                    a = false;
                    PlayerAnimator.ChangeState(PlayerAnim.Dash);
                }
            }
            yield return null;
        }
        PlayerAnimator.ChangeState(PlayerAnim.Idle);

        mCollidersOnMove.Clear();

        if (mCanElevation)
        {
            mCanElevation = false;
        }
        mIsMovingElevation = false;

        mLocation9 = moveDIR9; mEMove = null;
        yield break;
    }
    [Obsolete]
    public UnitizedPosH GetTPOSITION3()
    {
        switch (mLocation9)
        {
            case UnitizedPos.TOP_LEFT:
            case UnitizedPos.MID_LEFT:
            case UnitizedPos.BOT_LEFT:
                return UnitizedPosH.LEFT;

            case UnitizedPos.TOP:
            case UnitizedPos.MID:
            case UnitizedPos.BOT:
                return UnitizedPosH.MID;

            case UnitizedPos.TOP_RIGHT:
            case UnitizedPos.MID_RIGHT:
            case UnitizedPos.BOT_RIGHT:
                return UnitizedPosH.RIGHT;

            default:
                break;
        }
        Debug.Log("Value Error");
        return UnitizedPosH.NONE;
    }
    [Obsolete]
    public UnitizedPos GetUnitizedPos()
    {
        return mLocation9;
    }
    [Obsolete]
    #region READ
    /// <summary>
    /// 플레이어 개체가 위/아래로 이동중이라면 false를, 그렇지 않다면 true를 반환합니다.
    /// </summary>
    /// <param name="playerPos">
    /// 함수의 반환값과는 관계없이 일관되게 플레이어의 위치를 전달합니다.
    /// </param>
    /// <returns></returns>
    #endregion
    public bool TryGetPosition(out Vector2 playerPos)
    {
        playerPos = transform.position;
        return !mIsMovingElevation;
    }
    #endregion

}
