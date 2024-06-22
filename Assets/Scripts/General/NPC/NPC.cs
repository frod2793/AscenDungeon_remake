using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Manager = NPCInteractionManager;

public abstract class NPC : MonoBehaviour
{
    [SerializeField] protected string _Key;
    [SerializeField] protected SubscribableButton _InteractionBtn;
    
    private void Start() => Init();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) 
        {
            PlayerEvent(true);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) 
        {
            PlayerEvent(false);
        }
    }

    public abstract void Interaction();

    protected virtual void Init()
    {
        _InteractionBtn.ButtonAction += state => 
        {
            if (state == ButtonState.Down) 
                Interaction();
        };
    }    
    public virtual void PlayerEvent(bool enter)
    {
        SetEnable(enter);
        Manager.Instance.SetNowEnableNPC(this, enter);

        if (enter)
        {
            Manager.Instance.VJoystick_SetCoreBtnMode(CoreBtnMode.InteractionOrder);
        }
        else
        {
            Manager.Instance.VJoystick_SetCoreBtnMode(CoreBtnMode.AttackOrder);
        }
    }
    public virtual void SetEnable(bool enable)
    {
        Manager.Instance.SetActive(_Key, enable);
    }
}