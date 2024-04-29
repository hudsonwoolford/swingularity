using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyData : ScriptableObject
{
    [ColorUsage(true, true)]
    [SerializeField] Color portalColor = new Color(0.3f, 1, 1, 1);
    [ColorUsage(true, true)]
    [SerializeField] public Color ballColor = new Color(0.3f, 1, 1, 1);
    [SerializeField] public Color[] nebulaColors = new Color[4]{
        new Color (1f, 1f, 1f, 1f),
        new Color (1f, 1f, 1f, 1f),
        new Color (1f, 1f, 1f, 1f),
        new Color (1f, 1f, 1f, 1f),
    };
}
