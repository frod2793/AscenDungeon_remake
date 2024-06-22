using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DungeonSelectUI : MonoBehaviour
{
    [SerializeField] private Scrollbar _Scrollbar;

    [SerializeField] private GameObject _SelectedStamp;
    [SerializeField] private DungeonSelection[] _Selections;

    private DungeonSelection _SelectedDungeon;
    private bool _IsAlreadyInit = false;

    private void Start()
    {
        _Scrollbar.value = 0f;

        if (!_IsAlreadyInit)
        {
            _SelectedStamp.SetActive(false);

            for (int i = 0; i < _Selections.Length; i++)
            {
                bool isLocked = i > GameLoger.Instance.UnlockDungeonIndex;

                _Selections[i].Init(isLocked);

                if (!isLocked)
                {
                    _Selections[i].SelectedEvent += selected =>
                    {
                        if (_SelectedDungeon != selected)
                        {
                            _SelectedDungeon = selected;

                            _SelectedStamp.SetActive(true);
                            _SelectedStamp.transform.parent = selected.transform;
                            _SelectedStamp.transform.localPosition = new Vector3(0, 45, 0);
                        }
                        else if(_SelectedDungeon.Equals(selected))
                        {
                            _SelectedDungeon = null;

                            _SelectedStamp.SetActive(false);
                        }
                    };
                }
            }
        }
    }
    public void DungeonEnter()
    {
        if (_SelectedDungeon != null)
        {
            _SelectedDungeon.DungeonEnter();
        }
        else
        {
            SystemMessage.Instance.ShowMessage("입장할 던전을<line-height=120%>\n선택해야 합니다!");
        }
    }
}
