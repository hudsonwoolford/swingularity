using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NPC : MonoBehaviour, IInteractable
{
    protected virtual void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Interact();
        }
    }
    public abstract void Interact();
}
