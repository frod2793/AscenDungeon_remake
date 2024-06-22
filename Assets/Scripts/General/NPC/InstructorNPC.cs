using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InstructorNPC : NPC
{
    public override void Interaction()
    {
        SystemMessage.Instance.ShowCheckMessage("튜토리얼을\n진행하시겠습니까?", delegate(bool result) 
        {
            if (result)
            {
                MainCamera.Instance.Fade(1f, FadeType.In, 
                    () => SceneManager.LoadScene((int)SceneIndex.Tutorial));
            }
        });
    }
}
