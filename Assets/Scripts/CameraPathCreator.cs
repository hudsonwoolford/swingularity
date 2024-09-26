using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode()]
public class CameraPathCreator : MonoBehaviour
{
    [Range(0, 1)]
    [SerializeField] float time;
    [SerializeField] Vector3[] checkPoints = new Vector3[3];

    [SerializeField] Transform dolly;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(dolly == null || checkPoints.Length == 0 || checkPoints.Length-1%2!=0) { return; }

        Vector3[] mPoints = checkPoints;

        float[] totalDist = new float[mPoints.Length-1/2];
        totalDist[0] = 0;
        for (int i = 1; i < mPoints.Length; i+=2)
        {
            totalDist[i] += totalDist[i-1/2] + (mPoints[i+1] - mPoints[i - 1]).magnitude;
        }

        float currentDist = time * totalDist[mPoints.Length - 1];
        int[] bP = new int[3];
        for (int i = 1; i < totalDist.Length; i++)
        {
            if (totalDist[i-1] <= currentDist && currentDist <= totalDist[i+1])
            {
                bP[0] = i - 1;
                bP[1] = i;
                bP[2] = i + 1;
                break;
            }
        }
        

        Vector3 dir1 = (mPoints[bP[1]] - mPoints[bP[0]]).normalized;
        Vector3 dir2 = (mPoints[bP[2]] - mPoints[bP[1]]).normalized;

        float dist1 = (mPoints[bP[1]] - mPoints[bP[0]]).magnitude;
        float dist2 = (mPoints[bP[2]] - mPoints[bP[1]]).magnitude;

        Vector3 point1 = mPoints[bP[0]] + dir1 * (time *  dist1);
        Vector3 point2 = mPoints[bP[1]] + dir2 * (time *  dist2);
        
        Vector3 dir3 = (point2 - point1).normalized;
        float dist3 = (point2 - point1).magnitude;

        Vector3 point3 = point1 + dir3 * (time * dist3);

        dolly.position = point3;
    }

    private void OnDrawGizmos()
    {
        Color[] pointColors = new Color[] {Color.red, Color.green};
        int pci = 0;
        for (int i = 0; i < checkPoints.Length; i++)
        {
            Gizmos.color = pointColors[pci];
            Gizmos.DrawCube(checkPoints[i], Vector3.one);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(checkPoints[i], checkPoints[Mathf.Min(i+1, checkPoints.Length-1)]);

            pci++;
            pci %= pointColors.Length;
        }
    }
}
