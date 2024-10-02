using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class DollyTravel : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] CinemachineSmoothPath track;
    [SerializeField] CinemachineVirtualCamera vCam;
    [SerializeField] AnimationCurve movementCurve;
    CinemachineTrackedDolly trackedDolly;
    float realTracker = 0;
    // Start is called before the first frame update
    void Start()
    {
        trackedDolly = vCam.GetCinemachineComponent<CinemachineTrackedDolly>();
    }

    // Update is called once per frame
    void Update()
    {
        //trackedDolly.m_PathPosition = Mathf.Clamp(trackedDolly.m_PathPosition + Time.deltaTime * speed, 0, track.PathLength);

        realTracker += Time.deltaTime * speed;
        trackedDolly.m_PathPosition = Mathf.Clamp(movementCurve.Evaluate(realTracker), 0, track.PathLength);
    }
}
