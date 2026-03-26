using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Events : MonoBehaviour
{
    public event EventHandler OnSpacePressed;

    private void Start()
    {
        OnSpacePressed += CallOnSpace;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
                OnSpacePressed?.Invoke(this, EventArgs.Empty);
        }
    }

    private void CallOnSpace(object sender, EventArgs e)
    {
        Debug.Log("space");
    }
}
