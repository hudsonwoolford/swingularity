using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using UnityEngine;
using static LevelGenerator;
using UnityEngine.UIElements;
using static UnityEngine.EventSystems.EventTrigger;

public class MockMoon : MockPlanet
{
    [Header("Moon Var")]
    public Transform orbitPlanet;
    [Range(0f, 1f)]
    public float eccentricity = 1;
    public float speed = 1;
    [Min(0.0001f)]
    public float apoapsis = 1;
    [Range(0,360f)]
    public float rotation;

    private void OnDrawGizmosSelected()
    {
        float semiMajorAxis = apoapsis / (1 - eccentricity);
        float semiMinorAxis = semiMajorAxis * Mathf.Sqrt(1 - Mathf.Pow(eccentricity, 2));
        float centerDistance = semiMajorAxis * eccentricity;
        Vector2 focus1 = orbitPlanet.transform.position;
        Vector2 focus2 = focus1 + new Vector2(centerDistance * 2, 0f);
        Vector2 centre = (focus1 + focus2) / 2f;

        Vector3 lastPoint = Vector3.zero;
        float mockProgress = 0;
        for (int i = 0; i < 90; i++)
        {
            mockProgress += Mathf.PI * 2 / 90;
            Vector3 dir = MoonManager.RotateAround(rotation, new Vector3(centre.x + semiMajorAxis * Mathf.Cos(mockProgress), centre.y + semiMinorAxis * Mathf.Sin(mockProgress)), orbitPlanet.transform.position);
            Gizmos.DrawLine(lastPoint, dir);
            if (i == 0) { transform.position = dir; }
            lastPoint = dir;
        }
    }
}

