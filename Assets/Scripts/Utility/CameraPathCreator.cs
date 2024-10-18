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
    [Header("Scene Visuals")]
    [Range(0, 50)]
    [SerializeField] int curveResolution; 

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(dolly == null || checkPoints.Length == 0 || (checkPoints.Length+1)%2!=0) { return; }

        Vector3[] mPoints = checkPoints;

        float[] totalDist = new float[(mPoints.Length+1)/2];
        totalDist[0] = 0;
        int pointIt = 1;
        for (int i = 1; i < totalDist.Length; i++)
        {
            totalDist[i] += totalDist[i-1] + ((mPoints[i+1] - mPoints[i]).magnitude + (mPoints[i]-mPoints[i-1]).magnitude);
            pointIt+=2;
        }

        float currentDist = time * totalDist[totalDist.Length - 1];
        float adjustedTime = 0;
        int[] bP = new int[3];
        for (int i = 1; i < totalDist.Length; i++)
        {
            if (totalDist[i-1] <= currentDist && currentDist <= totalDist[i])
            {
                bP[0] = i * 2 - 2;
                bP[1] = i * 2 - 1;
                bP[2] = i * 2;
                adjustedTime = (currentDist - totalDist[i-1]) / (totalDist[i] - totalDist[i-1]);
                break;
            }
        }
        
        Vector3 dir1 = (mPoints[bP[1]] - mPoints[bP[0]]).normalized;
        Vector3 dir2 = (mPoints[bP[2]] - mPoints[bP[1]]).normalized;

        float dist1 = (mPoints[bP[1]] - mPoints[bP[0]]).magnitude;
        float dist2 = (mPoints[bP[2]] - mPoints[bP[1]]).magnitude;

        Vector3 point1 = mPoints[bP[0]] + dir1 * (adjustedTime *  dist1);
        Vector3 point2 = mPoints[bP[1]] + dir2 * (adjustedTime *  dist2);
        
        Vector3 dir3 = (point2 - point1).normalized;
        float dist3 = (point2 - point1).magnitude;

        Vector3 point3 = point1 + dir3 * (adjustedTime * dist3);

        dolly.position = GetPosAtTime(time);
    }
    Vector3 GetPosAtTime(float inputTime)
    {
        if (dolly == null || checkPoints.Length == 0 || (checkPoints.Length + 1) % 2 != 0) { return Vector3.zero; }

        Vector3[] mPoints = checkPoints;

        float[] totalDist = new float[(mPoints.Length + 1) / 2];
        totalDist[0] = 0;
        int pointIt = 1;
        for (int i = 1; i < totalDist.Length; i++)
        {
            totalDist[i] += totalDist[i - 1] + ((mPoints[i + 1] - mPoints[i]).magnitude + (mPoints[i] - mPoints[i - 1]).magnitude);
            pointIt += 2;
        }

        float currentDist = inputTime * totalDist[totalDist.Length - 1];
        float adjustedTime = 0;
        int[] bP = new int[3];
        for (int i = 1; i < totalDist.Length; i++)
        {
            if (totalDist[i - 1] <= currentDist && currentDist <= totalDist[i])
            {
                bP[0] = i * 2 - 2;
                bP[1] = i * 2 - 1;
                bP[2] = i * 2;
                adjustedTime = (currentDist - totalDist[i - 1]) / (totalDist[i] - totalDist[i - 1]);
                break;
            }
        }

        Vector3 dir1 = (mPoints[bP[1]] - mPoints[bP[0]]).normalized;
        Vector3 dir2 = (mPoints[bP[2]] - mPoints[bP[1]]).normalized;

        float dist1 = (mPoints[bP[1]] - mPoints[bP[0]]).magnitude;
        float dist2 = (mPoints[bP[2]] - mPoints[bP[1]]).magnitude;

        Vector3 point1 = mPoints[bP[0]] + dir1 * (adjustedTime * dist1);
        Vector3 point2 = mPoints[bP[1]] + dir2 * (adjustedTime * dist2);

        Vector3 dir3 = (point2 - point1).normalized;
        float dist3 = (point2 - point1).magnitude;

        Vector3 point3 = point1 + dir3 * (adjustedTime * dist3);

        return point3;
    }

    private void OnDrawGizmos()
    {
        Color[] pointColors = new Color[] {Color.green, Color.red};
        int pci = 0;

        //Lines
        for (int i = 0; i < checkPoints.Length; i++)
        {
            Gizmos.color = pointColors[pci];
            Gizmos.DrawCube(checkPoints[i], Vector3.one);

            Gizmos.color = new Color(0, 1, 0, 0.2f);
            Gizmos.DrawLine(checkPoints[i], checkPoints[Mathf.Min(i+1, checkPoints.Length-1)]);

            pci++;
            pci %= pointColors.Length;
        }

        //Curves
        Vector3[] calcPoints = new Vector3[curveResolution];
        for (int i = 0; i < curveResolution; i++)
        {
            calcPoints[i] = GetPosAtTime((float)(i)/curveResolution);
        }

        Gizmos.color = Color.yellow;
        for (int i = 1; i < calcPoints.Length; i++)
        {
            Gizmos.DrawLine(calcPoints[i-1], calcPoints[i]);
        }
    }
}
