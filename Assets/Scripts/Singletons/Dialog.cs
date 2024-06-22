using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dialog : Singleton<Dialog>
{
    private static readonly string BeginAnimation = "Dialog_Begin";

    [SerializeField] private float _WriteDelay = 0.1f;
    
    [SerializeField] private TMPro.TextMeshProUGUI _LogText;
    [SerializeField] private TMPro.TextMeshProUGUI _NameText;
    [SerializeField] private Image _ImageOfCharacter;

    [SerializeField] private Animator _Animator;


    [Header("Init Property")]
    [SerializeField] private GameObject _RootOfCharacter;
    [SerializeField] private GameObject _RootOfDialogBox;
    
    [Space()]

    [SerializeField] private Vector3    _InitPosOfCharacter;
    [SerializeField] private Vector3    _InitPosOfDialogBox;
    [SerializeField] private Button     _DialogButton;


    [Header("Test Property")]
    [SerializeField] [TextArea(3, 6)] private string _TestOfTextField;
    
    /// <summary>
    /// 출력이 모두 끝난 상태에서 터치 (이름 겁나 긺)
    /// </summary>
    public event Action OnTouchOutputFinish;

    private int _AnimControlKey;
    private Action _WriteLogCallback;
    private Coroutine _WriteLogCoroutine;

    private Queue<string> _TextQueue = new Queue<string>();

    private StringBuilder _QueueBuilder = new StringBuilder();
    private StringBuilder _WriteBuilder = new StringBuilder();

    private void Awake()
    {
        _WriteLogCoroutine = new Coroutine(this);

        _AnimControlKey = _Animator.GetParameter(0).nameHash;
        _Animator.enabled = false;

        _RootOfCharacter.transform.localPosition = _InitPosOfCharacter;
        _RootOfDialogBox.transform.localPosition = _InitPosOfDialogBox;

        _RootOfCharacter.SetActive(false);
        _RootOfDialogBox.SetActive(false);
    }

    public void UpdateLog(string name, Sprite image)
    {
        _NameText.text = name;
        _ImageOfCharacter.sprite = image;
    }

    public void WriteLog(string name, string text, Action callBack)
    {
        PlayerActionManager.Instance.SetEnableController(false);

        SetTextQueue(text);
        _WriteLogCallback = callBack;
        _NameText.text = name;

        if (_Animator.enabled)
        {
            _WriteLogCoroutine.StartRoutine(WriteLogRoutine());
            return;
        }
        _RootOfCharacter.SetActive(true);
        _RootOfDialogBox.SetActive(true);

        _Animator.enabled = true;
        _Animator.Play(BeginAnimation);
    }

    public void SkipLog()
    {
        if (_WriteLogCoroutine.IsFinished())
        {
            CloseLog();
        }
        else if (_TextQueue.Count != 0)
        {
            while (_TextQueue.Count != 0) {
                _WriteBuilder.Append(_TextQueue.Dequeue());
            }
            _LogText.text = _WriteBuilder.ToString();
        }
    }

    public void CloseLog()
    {
        _DialogButton.enabled = false;
        PlayerActionManager.Instance.SetEnableController(true);

        _WriteLogCoroutine.StopRoutine();
        _Animator.SetBool(_AnimControlKey, true);
        
        _TextQueue.Clear();
    }

#region Touch Event

    public void OnTouchDialog()
    {
        if (_TextQueue.Count != 0)
        {
            while (_TextQueue.Count != 0) {
                _WriteBuilder.Append(_TextQueue.Dequeue());
            }
            _LogText.text = _WriteBuilder.ToString();
            return;
        }
        OnTouchOutputFinish?.Invoke();
    }

#endregion Touch Event
    private void SetTextQueue(string text)
    {
        _QueueBuilder.Clear();

        for (int i = 0; i < text.Length; ++i)
        {
            char character = text[i];
            _QueueBuilder.Append(character);

            if (character.Equals(' '))
            {
                if (_QueueBuilder.ToString() == "  ")
                {
                    _QueueBuilder.Clear();
                    _QueueBuilder.AppendLine();
                }
                continue;
            }
            _TextQueue.Enqueue(_QueueBuilder.ToString());
            _QueueBuilder.Clear();
        }
    }

    private IEnumerator WriteLogRoutine()
    {
        _WriteBuilder.Clear();

        while (_TextQueue.Count != 0)
        {
            _WriteBuilder.Append(_TextQueue.Dequeue());
            _LogText.text = _WriteBuilder.ToString();

            yield return new WaitForSeconds(_WriteDelay);
        }
        _WriteLogCoroutine.Finish();
        _WriteLogCallback?.Invoke();
    }

#region Animation Event

    private void AE_PlayOverBegin()
    {
        _DialogButton.enabled = true;
        _WriteLogCoroutine.StartRoutine(WriteLogRoutine());
    }
    private void AE_PlayOverEnd()
    {
        _Animator.enabled = false;
        _Animator.SetBool(_AnimControlKey, false);

        _RootOfCharacter.SetActive(false);
        _RootOfDialogBox.SetActive(false);

         _LogText.text = string.Empty;
        _NameText.text = string.Empty;
    }

#endregion Animation Event
}
