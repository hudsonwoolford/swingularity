using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
public class TrailCollisionInfo : MonoBehaviour
{
    [SerializeField] public Sprite colImage1;
    [SerializeField] public Sprite colImage2;
    [SerializeField] public Color colImageColour1;
    [SerializeField] public Color colImageColour2;
}
