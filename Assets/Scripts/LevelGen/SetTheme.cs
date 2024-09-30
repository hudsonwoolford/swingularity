using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetTheme : MonoBehaviour
{
    [SerializeField] GalaxyData theme;
    void Start()
    {
        LevelGenerator.levelInit(theme); 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
