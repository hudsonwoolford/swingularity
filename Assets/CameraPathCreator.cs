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
        if(dolly == null || checkPoints.Length == 0) { return; }

        //ensure at least 3 in checkpoint list
        Vector3[] mPoints = checkPoints;
        if(checkPoints.Length < 3)
        {
            mPoints = new Vector3[3];
            for (int i = 0; i < checkPoints.Length; i++)
            {
                mPoints[i] = checkPoints[Mathf.Min(i, checkPoints.Length - 1)];
            }
        }

        //perform bezier curve
        
        Vector3 dir1 = (checkPoints[1] - checkPoints[0]).normalized;
        Vector3 dir2 = (checkPoints[2] - checkPoints[1]).normalized;

        float dist1 = (checkPoints[1] - checkPoints[0]).magnitude;
        float dist2 = (checkPoints[2] - checkPoints[1]).magnitude;

        Vector3 point1 = checkPoints[0] + dir1 * (time *  dist1);
        Vector3 point2 = checkPoints[1] + dir2 * (time *  dist2);

        Vector3 dir3 = (point2 - point1).normalized;
        float dist3 = (point2 - point1).magnitude;

        Vector3 point3 = point1 + dir3 * (time * dist3);

        dolly.position = point3;
    }

    private void OnDrawGizmos()
    {
        Color[] pointColors = new Color[] {Color.red, Color.green, Color.red};
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
