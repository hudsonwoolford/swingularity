using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ManualQuasar : QuasarGenerator
{
    [Header("Manual Variables")]
    [SerializeField] Transform t1;
    [SerializeField] Transform t2;
    [SerializeField] float additionalLength;
    protected override void Start()
    {
        MatchToPlanets();
        base.Start();
    }
    protected override void Update()
    {
        base.Update();
        if(!Application.isPlaying)
        {
            MatchToPlanets();
        }
    }
    void MatchToPlanets()
    {
        //Vector3 dir = planets[index2].planet.transform.position - planets[index1].planet.transform.position;
        //float distance = dir.magnitude - (planets[index2].forceField.transform.localScale.x + planets[index1].forceField.transform.localScale.x);
        //dir = dir.normalized;
        //float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90;
        //Vector3 midPoint = ((planets[index1].planet.transform.position + planets[index1].forceField.transform.localScale.x * dir) + (planets[index2].planet.transform.position - planets[index2].forceField.transform.localScale.x * dir)) / 2;
        if (t1 == null || t2 == null) { return; }
        Vector3 dir = t2.position - t1.position;
        float distance = dir.magnitude - (t2.localScale.x / 2 + t1.localScale.x / 2);
        dir = dir.normalized;
        float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90;
        Vector3 midPoint = ((t1.position + t1.localScale.x / 2 * dir) + (t2.position - t2.localScale.x / 2 * dir)) / 2;
        //ExtensionMethods.DrawSquare(t1.position + t1.localScale.x * dir, Vector2.one, Color.red, 0.1f);
        Debug.DrawLine((t1.position + t1.localScale.x/2 * dir), (t2.position - t2.localScale.x/2 * dir));
        radius = distance / 4 + additionalLength * distance;
        transform.position = midPoint;
        angle = ang;
    }
}
