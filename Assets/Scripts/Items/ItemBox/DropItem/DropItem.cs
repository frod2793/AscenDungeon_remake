using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.EventSystems;

public class DropItem : NPC
{
    public SubscribableButton InteractBtn
    {
        get => _InteractionBtn;
        set => _InteractionBtn = value;
    }

    [SerializeField] private Animator Animator;
    [SerializeField] private SpriteRenderer Renderer;
   
    private bool _HasPlayer;
    private Item mContainItem;

    public void Init(Item containItem)
    {
                          mContainItem = containItem;
        Renderer.sprite = mContainItem?.Sprite;
    }

    private void AnimationPlayOver()
    {
        gameObject.SetActive(false);

        Inventory.Instance.AddItem(mContainItem);
        PlayerEvent(false);
    }
    private void Start()
    {
        if (InteractBtn != null)
            InteractBtn.ButtonAction += IteractionMethod;
    }
    private void Reset()
    {
        Debug.Assert(TryGetComponent(out Animator));
        Debug.Assert(TryGetComponent(out Renderer));
    }
    public override void PlayerEvent(bool enter)
    {
        base.PlayerEvent(enter);
        _HasPlayer = enter;
    }
    public override void Interaction()
    {
        if (_HasPlayer)
        {
            int animControlKey = Animator.GetParameter(0).nameHash;

            if (!Animator.GetBool(animControlKey))
            {
                Animator.SetBool(animControlKey, true);
            }
        }
        if (InteractBtn != null)
            InteractBtn.ButtonAction -= IteractionMethod;
    }

    private void IteractionMethod(ButtonState state)
    {
        if (state == ButtonState.Down)
            Interaction();
    }
}
