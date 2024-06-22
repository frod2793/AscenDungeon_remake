using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResolutionSetter : MonoBehaviour
{
    private void Awake()
    {
        if (Application.platform == RuntimePlatform.WindowsPlayer)
        {

            Screen.SetResolution(540, 960, false);
        }
    }
}
