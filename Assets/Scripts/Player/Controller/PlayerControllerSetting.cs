using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControllerSetting : MonoBehaviour
{
    [System.Serializable] public struct Selection
    {
        public Image Image;
        public TMPro.TextMeshProUGUI TextUI;
        public SubscribableButton Button;
    }

    [Header("____Controller Props_____")]
    [SerializeField] private VirtualJoystick _Controller;

    [SerializeField] float _ControllerDefScale;
    [SerializeField] float _ControllerMaxScale;
    [SerializeField] float _ControllerOffset;

    [Header("____Slider Props____")]
    [SerializeField] private Slider _ScaleSlider;
    [SerializeField] private TMPro.TextMeshProUGUI _ScaleValueText;

    [Space()]
    [SerializeField] private Slider _AlphaSlider;
    [SerializeField] private TMPro.TextMeshProUGUI _AlphaValueText;

    [Header("____Reposer Props____")]
    [SerializeField] private VirtualJoystickReposer _Reposer;
    [SerializeField] private Button _ReposerButton;

    [Header("____Method Props____")]
    [SerializeField] private Sprite _SelectedImage;
    [SerializeField] private Sprite  _DefaultImage; 
    [Space()]
    [SerializeField] private VirtualJoystick _VirtualJoystick;
    [SerializeField] private TouchController _TouchController;
    [Space()]
    [SerializeField] private Selection _JoystickSelection;
    [SerializeField] private Selection _TouchConSelection;

    private bool _IsAlreadyInit = false;

    private void Start()
    {
        if (!_IsAlreadyInit)
        {
            _IsAlreadyInit = true;

            float scale = GameLoger.Instance.ControllerDefScale / _ControllerDefScale * 50f;

            _ScaleValueText.text = ((int)scale).ToString();
            _ScaleSlider.value = scale;

            float alpha = GameLoger.Instance.ControllerAlpha * 100f;
            _AlphaSlider.value = alpha;
            _AlphaValueText.text = ((int)alpha).ToString();

            _ScaleSlider.onValueChanged.AddListener(ScaleValueChanged);
            _AlphaSlider.onValueChanged.AddListener(AlphaValueChanged);

            _ReposerButton.onClick.AddListener(ReposerButtonClick);

            // ====== Selection Init ====== //
            if (GameLoger.Instance.UsingVJoystick) 
            {
                JoystickEnable();
            }
            else
            {
                TouchEnable();
            }
            _JoystickSelection.Button.ButtonAction += state => { JoystickEnable(); };
            _TouchConSelection.Button.ButtonAction += state => {    TouchEnable(); };
            // ====== Selection Init ====== //
        }
    }
    public void JoystickEnable()
    {
        GameLoger.Instance.UsingVJoystick = true;

        _JoystickSelection.Image.sprite = _SelectedImage;
        _TouchConSelection.Image.sprite = _DefaultImage;

        _VirtualJoystick.gameObject.SetActive(true);
        _TouchController.enabled = false;
    }
    public void TouchEnable()
    {
        GameLoger.Instance.UsingVJoystick = false;

        _JoystickSelection.Image.sprite = _DefaultImage;
        _TouchConSelection.Image.sprite = _SelectedImage;

        _VirtualJoystick.gameObject.SetActive(false);
        _TouchController.enabled = true;
    }
    private void ScaleValueChanged(float value)
    {
        _ScaleValueText.text = ((int)value).ToString();
        value *= 0.02f;

        float defScale = value * _ControllerDefScale;
        float maxScale = value * _ControllerMaxScale;

        float offset = _ControllerOffset * value;

        GameLoger.Instance.ControllerDefScale = defScale;
        GameLoger.Instance.ControllerMaxScale = maxScale;
        GameLoger.Instance.ControllerOffset   = offset;

        _Controller.SetButtonScale (defScale, maxScale);
        _Controller.SetButtonOffset(offset);
    }
    private void AlphaValueChanged(float value)
    {
        _AlphaValueText.text = ((int)value).ToString();

        float alpha = value * 0.01f;
        _Controller.SetButtonAlpha(alpha);
        
        GameLoger.Instance.ControllerAlpha = alpha;
    }
    private void ReposerButtonClick()
    {
        _Reposer.gameObject.SetActive(true);
    }
#if UNITY_EDITOR
#else
    private void OnDisable()
    {
        BackEndServerManager.Instance.Optionsaver();
    }
#endif
}
