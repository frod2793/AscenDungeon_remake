using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

// 플레이어에게 이동을 지시할 수 있어야 한다.
// 플레이어의 이동은 플레이어 내부적으로 처리한다.
// 플레이어의 이동 방식을 변경해야한다. MovePoint는 움직일 수 있는 지점을 제한하는 용도로 사용하자.
// 또한 위/아래로 이동할 때에는 y값만 MovePoint로 부터 불러오도록..
// 일단 공격 버튼부터 만들자. 이동은 좀 걸릴것 같으니까.

public enum CoreBtnMode
{
    AttackOrder, InteractionOrder
}

public class VirtualJoystick : MonoBehaviour
{
    private readonly Vector3 HalfScreen
        = new Vector3(Screen.width / 2f, Screen.height / 2f);

    [SerializeField] private RectTransform _RectTransform;
    [SerializeField, FormerlySerializedAs("_AttackButton")] 
    private SubscribableButton _CoreButton;

    [Header("___Scaling Prop___")]
    [SerializeField] private float _ScalingTime;
    [SerializeField] private float _DefaultScale;
    [SerializeField] private float _MaxScale;

    [Header("___MoveButton Prop___")]
    [SerializeField] private SubscribableButton _UMoveButton;
    [SerializeField] private SubscribableButton _DMoveButton;
    [SerializeField] private SubscribableButton _LMoveButton;
    [SerializeField] private SubscribableButton _RMoveButton;

    [Header("___Interaction Prop___")]
    [SerializeField] private SubscribableButton _Button;
    [SerializeField] private Image  _ButtonImage;

    [Header("___CoreBtn Sprite Prop___")]
    [SerializeField] private Image _CoreBtnImage;
    [SerializeField] private Sprite _CoreBtnSprite_Attack;
    [SerializeField] private Sprite _CoreBtnSprite_Interaction;

    private const float IntervalClickTime = 0.2f;
    private const float DefaultSizeDelta = 393.84615f;
    private float _LastClickTime = 0f;
    private Direction _PrevInputButton = Direction.None;

    public Rect GetRect
    {
        get => _RectTransform.rect;
    }
    public CoreBtnMode CrntCoreBtnMode
    { get; private set; }

    public SubscribableButton this[Direction dir]
    {
        get
        {
            switch (dir)
            {
                case Direction.Up:
                    return _UMoveButton;
                case Direction.Down:
                    return _DMoveButton;
                case Direction.Right:
                    return _RMoveButton;
                case Direction.Left:
                    return _LMoveButton;

                default:
                    return _CoreButton;
            }
        }
    }
    public SubscribableButton AttackButton => _CoreButton;

    private Player _Player;

    private bool _IsAlreadyInit = false;
    private Coroutine _WaitCharge;
    private Coroutine _WaitScaling;

    private Image[] _AllButtonImages;

    private void Start()
    {
        if (!_IsAlreadyInit)
        {
            _WaitCharge  = new Coroutine(this);
            _WaitScaling = new Coroutine(this);

            var find = GameObject.FindGameObjectWithTag("Player");
            if (find != null)
            {
                if (find.TryGetComponent(out _Player))
                {
                    _CoreButton.ButtonAction += state =>
                    {
                        // 컨트롤러가 비활성화 되어있다면 중단.
                        if (!PlayerActionManager.Instance.EnableController)
                            return;
                        
                        switch (CrntCoreBtnMode)
                        {
                            case CoreBtnMode.AttackOrder:
                                switch (state)
                                {
                                    case ButtonState.Down:

                                        _WaitCharge.StartRoutine(WaitForStartCharging());
                                        break;

                                    case ButtonState.Up:

                                        if (!_WaitCharge.IsFinished())
                                        {
                                            _Player.AttackOrder();
                                        }
                                        else
                                        {
                                            Finger.Instance.EndCharging();

                                            _WaitScaling.StopRoutine();
                                            _WaitScaling.StartRoutine(AttackBtnScaling(true));
                                        }
                                        _WaitCharge.StopRoutine();
                                        break;
                                }
                                break;
                            case CoreBtnMode.InteractionOrder:
                                if (state == ButtonState.Down)
                                {
                                    var npc = NPCInteractionManager.Instance.LastEnableNPC;

                                    if (npc != null)
                                        npc.Interaction();
                                }
                                break;
                        }
                    };

                    _UMoveButton.ButtonAction += state => MoveOrderToPlayer(_UMoveButton, state, Direction.Up);
                    _DMoveButton.ButtonAction += state => MoveOrderToPlayer(_DMoveButton, state, Direction.Down);
                    _LMoveButton.ButtonAction += state => MoveOrderToPlayer(_LMoveButton, state, Direction.Left);
                    _RMoveButton.ButtonAction += state => MoveOrderToPlayer(_RMoveButton, state, Direction.Right);
                }
            }
            _IsAlreadyInit = true;

            SetButtonScale(GameLoger.Instance.ControllerDefScale, GameLoger.Instance.ControllerMaxScale);
            SetButtonOffset(GameLoger.Instance.ControllerOffset);

            // ====== _AllButtonImages Init ===== //
            if (_AllButtonImages == null)
            {
                var buttons = GetAllButtons();
                _AllButtonImages = new Image[buttons.Length];

                for (int i = 0; i < _AllButtonImages.Length; i++)
                {
                    buttons[i].TryGetComponent(out _AllButtonImages[i]);
                }
            }
            // ====== _AllButtonImages Init ===== //
            SetButtonAlpha(GameLoger.Instance.ControllerAlpha);

            transform.localPosition = GameLoger.Instance.ControllerPos;
        }

        void MoveOrderToPlayer(SubscribableButton button, ButtonState state, Direction direction)
        {
            // 컨트롤러가 비활성화 되어있다면 중단
            if (!PlayerActionManager.Instance.EnableController)
                return;
            
            switch (state)
            {
                case ButtonState.Down:
                    if (direction == _PrevInputButton && Time.time - _LastClickTime < IntervalClickTime)
                    {
                        switch (direction)
                        {
                            case Direction.Right:
                                _Player.DashOrder(UnitizedPosH.RIGHT);
                                break;
                            case Direction.Left:
                                _Player.DashOrder(UnitizedPosH.LEFT);
                                break;
                        }
                        _PrevInputButton = Direction.None;

                        _Player.OnceDashEndEvent += p =>
                        {
                            if (button.CurrentState == ButtonState.Down)
                            {
                                p.MoveOrder(direction);
                            }
                        };
                    }
                    else
                    {
                        _Player.MoveOrder(direction);

                        _PrevInputButton = direction;
                    }
                    break;
                case ButtonState.Up:
                    {
                        switch (direction)
                        {
                            case Direction.Right:
                            case Direction.Left:
                                _Player.MoveStop();
                                break;
                        }
                    }
                    break;
            }
            _LastClickTime = Time.time;
        }
    }
    
    public SubscribableButton[] GetAllButtons()
    {
        return new SubscribableButton[5]
        {
            _UMoveButton, _DMoveButton,
            _LMoveButton, _RMoveButton, _CoreButton
        };
    }

    public void SetCoreBtnMode(CoreBtnMode btnMode)
    {
        switch (CrntCoreBtnMode = btnMode)
        {
            case CoreBtnMode.AttackOrder:
                _CoreBtnImage.sprite = _CoreBtnSprite_Attack;
                break;

            case CoreBtnMode.InteractionOrder:
                _CoreBtnImage.sprite = _CoreBtnSprite_Interaction;
                break;
        }
    }
    public void SetPositionWithScreenRange(Vector2 position)
    {
        Rect rect = _RectTransform.rect;

        rect.width  /= 2f;
        rect.height /= 2f;

        position.x = Mathf.Clamp(position.x, -HalfScreen.x + rect.width,  HalfScreen.x - rect.width);
        position.y = Mathf.Clamp(position.y, -HalfScreen.y + rect.height, HalfScreen.y - rect.height - HalfScreen.y * 0.5f);

        transform.localPosition = position;
    }
    public void SetActiveInteraction(bool enable)
    {
        _ButtonImage.enabled = _Button.enabled = enable;
    }
    public void AddInteractionAction(Action<ButtonState> action)
    {
        _Button.ButtonAction += action;
    }

    public void SetButtonScale(float defaultScale, float maxScale)
    {
        _DefaultScale = defaultScale;
        _MaxScale = maxScale;

        Vector3 scale = Vector3.one * defaultScale;
        _UMoveButton.transform.localScale = scale;
        _DMoveButton.transform.localScale = scale;
        _LMoveButton.transform.localScale = scale;
        _RMoveButton.transform.localScale = scale;

        _CoreButton.transform.localScale = scale;

        _RectTransform.sizeDelta = Vector2.one * defaultScale * DefaultSizeDelta;
        _WaitScaling.StopRoutine();
    }
    public void SetButtonOffset(float offset)
    {
        _UMoveButton.transform.localPosition = Vector3.up    * offset;
        _DMoveButton.transform.localPosition = Vector3.down  * offset;
        _LMoveButton.transform.localPosition = Vector3.left  * offset;
        _RMoveButton.transform.localPosition = Vector3.right * offset;
    }
    public void SetButtonAlpha(float alpha)
    {
        var newColor = _AllButtonImages[0].color;
            newColor.a = alpha;

        for (int i = 0; i < _AllButtonImages.Length; ++i)
        {
            _AllButtonImages[i].color = newColor;
        }
    }

    private IEnumerator WaitForStartCharging()
    {
        for (float i = 0f; i < Finger.PRESS_TIME; i += Time.deltaTime)
        {
            yield return null;
        }
        _WaitCharge.Finish();

        Finger.Instance.StartCharging();

        _WaitScaling.StopRoutine();
        _WaitScaling.StartRoutine(AttackBtnScaling(false));
    }
    private IEnumerator AttackBtnScaling(bool toDeafult)
    {
        Vector3 targetScale = Vector3.one * (toDeafult ? _DefaultScale : _MaxScale);

        for (float i = 0; i < _ScalingTime; i += Time.deltaTime)
        {
            float ratio = Mathf.Min(i / _ScalingTime, 1f);

            _CoreButton.transform.localScale = 
                Vector3.Lerp(_CoreButton.transform.localScale, targetScale, ratio);

            yield return null;
        }
        _WaitScaling.Finish();
    }
}
