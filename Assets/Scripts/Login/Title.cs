using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Battlehub.Dispatcher;
using UnityEngine.SceneManagement;
public class Title : MonoBehaviour
{
    private static Title instance;
    public GameObject touchStart;
    void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
   
    // Update is called once per frame
    void Update()
    {
        
    }
}
