using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagneticFieldDraw : MonoBehaviour
{
    const double boltzmannConstant = 1.38064852e-23;
    [SerializeField] Vector2 testPoint = Vector2.zero;
    [SerializeField] Vector2 pole1 = Vector2.zero;
    [SerializeField] Vector2 pole2 = Vector2.zero;
    [SerializeField] float pole1Charge = 10;
    [SerializeField] float pole2Charge = -10;
    [SerializeField] uint xSeg = 10;
    [SerializeField] uint ySeg = 10;
    [SerializeField] float cellSize = 1;
    [SerializeField] float lineLength = 1;

    // Start is called before the first frame update
    void Start()
    {
        Vector2 offset = new Vector2(-xSeg / 2 * cellSize, -ySeg / 2 * cellSize);
        for (int y = 0; y < ySeg; y++)
        {
            for (int x = 0; x < xSeg; x++)
            {
                Vector2 trackPoint = new Vector2(x * cellSize, y * cellSize) + offset + testPoint;
                float t = -lineLength;
                Vector2 wSum = WeightedSum(trackPoint, pole1, pole2);
                float distance = wSum.magnitude;

                Vector2 point1 = new Vector2(wSum.x * t / distance + trackPoint.x, wSum.y * t / distance + trackPoint.y);

                t = lineLength;
                Vector2 point2 = new Vector2(wSum.x * t / distance + trackPoint.x, wSum.y * t / distance + trackPoint.y);

                Debug.DrawLine(point1, point2, Color.red, 50, false);
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    float FieldMagnitude(Vector2 point, Vector2 polePoint, float chargeProduct)
    {
        return (chargeProduct * (float)boltzmannConstant) / Mathf.Pow((Vector2.Distance(point, polePoint)), 3);
    }
    Vector2 WeightedSum(Vector2 point, Vector2 pole1Point, Vector2 pole2Point)
    {
        float dY =  (point.x - pole1Point.x) * FieldMagnitude(point, pole1Point, pole1Charge) + (point.x - pole2Point.x) * FieldMagnitude(point, pole2Point, pole2Charge);
        float dX = -(point.y - pole1Point.y) * FieldMagnitude(point, pole1Point, pole1Charge) - (point.y - pole2Point.y) * FieldMagnitude(point, pole2Point, pole2Charge);
        print(dX);
        return new Vector2(dX, dY);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(testPoint, 0.5f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(pole1, 0.5f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(pole2, 0.5f);

    }
}
