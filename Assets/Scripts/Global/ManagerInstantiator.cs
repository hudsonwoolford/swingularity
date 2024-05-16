using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerInstantiator : MonoBehaviour
{
    [SerializeField]GameObject manager;
    private void Awake()
    {
        if(GameManager.Instance == null)
        {
            Instantiate(manager);
        }
        Destroy(gameObject);
    }
}
