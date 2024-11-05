using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode()]
public class TestGolfTrail : MonoBehaviour
{
    [SerializeField] private ShopItem trailShopItem;
    [SerializeField] private uint trailRes = 10;
    [SerializeField] private float circleRad = 1;
    [SerializeField] private float circleRadOffset = 1;
    [Range(0, 1f)]
    [SerializeField] private float circDist = 1;
    [SerializeField] private bool clockwise;
    [SerializeField] private bool update;
    TrailRenderer tr;
    Transform trailRendObject;
    void Start()
    {
    }

    void Update()
    {
        if(update)
        {
            StartCoroutine(DrawTrail());
            update = false;
        }
    }
    IEnumerator DrawTrail()
    {
        if (trailShopItem == null) { yield break; }

        NullChecks();
        DestroyImmediate(trailRendObject.gameObject);
        NullChecks();

        tr.time = 1000;
        tr.emitting = false;

        int dir = clockwise ? -1 : 1;

        float ang = circleRadOffset;
        float jump = (Mathf.PI * 2 * circDist) / trailRes;
        transform.position = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * circleRad;
        tr.Clear();
        tr.emitting = true;

        for (int i = 0; i < trailRes; i++)
        {
            transform.position = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * circleRad;

            ang += jump * dir;
            yield return null;
        }
    }
    void NullChecks()
    {
        if (trailRendObject == null && GetComponentInChildren<TrailRenderer>() != null) { trailRendObject = GetComponentInChildren<TrailRenderer>().transform; }
        if (trailRendObject == null) { trailRendObject = Instantiate(trailShopItem.golfBallTrailElement.trailRendObj, transform).transform; }
        if (tr == null) { tr = trailRendObject.GetComponent<TrailRenderer>(); }
    }
}
