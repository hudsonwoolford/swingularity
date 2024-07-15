using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="audioObject", menuName = "audioObject")]
public class AudioObj : ScriptableObject
{
    public AudioClip clip;
    [Range(0, 1f)]
    public float inherentVol =1;
}
