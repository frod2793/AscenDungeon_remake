using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialNPC : NPC
{
    [SerializeField] private InteractionTutorial _Tutorial;

    public override void Interaction()
    {
        _Tutorial.OnInteraction();
    }
    
    public override void PlayerEvent(bool enter)
    {
        base.PlayerEvent(enter);
        if (enter)
        {
            EffectLibrary.Instance.UsingEffect(EffectKind.Twinkle, transform.position + Vector3.down);
        }
    }
}
