using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionTutorial : TutorialBase
{
    private bool _IsAlreadyClear = false;

    public override void StartTutorial()
    {
        if (_IsAlreadyClear)
            return;
        
        base.StartTutorial();
        gameObject.SetActive(true);
    }

    public override void EndTutorial()
    {
        if (_IsAlreadyClear)
            return;

        StageEventLibrary.Instance.SetActiveInventoryButton(true);
        _IsAlreadyClear = true;
        base.EndTutorial();
    }

    public void OnInteraction()
    {
        EndTutorial();
    }
}
