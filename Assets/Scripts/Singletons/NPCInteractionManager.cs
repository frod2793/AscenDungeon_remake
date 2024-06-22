using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInteractionManager : Singleton<NPCInteractionManager>
{
    [System.Serializable]
    public struct InteractionUISet
    {
        public string Key;

        [Space()]
        public GameObject InteractionBtn;
        public GameObject InteractionObj;
    }

    public NPC LastEnableNPC 
    {
        get
        {
            try
            {
                return _EnableNPClist.First.Value;
            }
            catch
            {
                return null;
            }
        }
    }

    [SerializeField, Header("Controller Property")] 
    private PlayerControllerSetting _PlayerController;
    [SerializeField]
    private VirtualJoystick _VirtualJoystick;

    [SerializeField, Space()]
    private InteractionUISet[] _InteractionUISets;

    private Dictionary<string, GameObject> _InteractionObjDic;
    private LinkedList<NPC> _EnableNPClist;

    private void Awake()
    {
        _InteractionObjDic = new Dictionary<string, GameObject>();
        _EnableNPClist = new LinkedList<NPC>();

        for (int i = 0; i < _InteractionUISets.Length; i++)
        {
            var uiSet = _InteractionUISets[i];
            _InteractionObjDic.Add(uiSet.Key, uiSet.InteractionObj);
        }
        _PlayerController.ControllerSwapEvnt += con =>
        {
            bool enable = con == PlayerControllerSetting.Controller.Touch;

            for (int i = 0; i < _InteractionUISets.Length; i++)
            {
                _InteractionUISets[i].InteractionBtn.SetActive(enable);
            }
        };
    }
    
    public void SetNowEnableNPC(NPC npc, bool enable)
    {
        if (enable)
        {
            if (_EnableNPClist.Count != 0)
                _EnableNPClist.First.Value.SetEnable(false);

            _EnableNPClist.AddFirst(npc);
            npc.SetEnable(true);
        }
        else
        {
            var first = LastEnableNPC;
            _EnableNPClist.Remove(npc);

            if (_EnableNPClist.Count != 0 && first.Equals(npc))
                _EnableNPClist.First.Value.SetEnable(true);
        }
    }
    
    public void SetActive(string key, bool active)
    {
        if (_InteractionObjDic[key] == null)
            return;
        
        _InteractionObjDic[key]?.SetActive(active);
    }
    public void VJoystick_SetCoreBtnMode(CoreBtnMode btnMode)
    {
        switch (btnMode)
        {
            case CoreBtnMode.AttackOrder:
                if (_EnableNPClist.Count == 0)
                {
                    _VirtualJoystick.SetCoreBtnMode(CoreBtnMode.AttackOrder);
                }
                break;
            case CoreBtnMode.InteractionOrder:
                _VirtualJoystick.SetCoreBtnMode(CoreBtnMode.InteractionOrder);
                break;
        }
    }
}
