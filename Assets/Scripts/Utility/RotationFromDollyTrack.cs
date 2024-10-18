using UnityEngine;
using Cinemachine;

/// <summary>
/// An custom extension for Cinemachine Virtual Camera that pulls rotation from the path
/// </summary>
[SaveDuringPlay]
[AddComponentMenu("")] // Hide in menu
public class RotationFromDollyTrack : CinemachineExtension
{
    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (stage == CinemachineCore.Stage.Body)
        {
            var dollyVcam = vcam as CinemachineVirtualCamera;
            if (dollyVcam != null)
            {
                var dolly = dollyVcam.GetCinemachineComponent<CinemachineTrackedDolly>();
                var path = dolly == null ? null : dolly.m_Path;
                if (path != null)
                {
                    state.RawOrientation = path.EvaluateOrientationAtUnit(dolly.m_PathPosition, dolly.m_PositionUnits);
                }
            }
        }
    }
}