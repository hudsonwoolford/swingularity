using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class EdgeDistortion : MonoBehaviour
{
    [Header("Wrapping")]
    [SerializeField] AudioObj[] wrapAudio;


    [Header("Shader variables")]
    [SerializeField] Material distortMat;
    [SerializeField] bool blit;
    [SerializeField] bool updateInPlayMode = false;
    [SerializeField] float distortionRes;
    [SerializeField] float distortionStrength;
    [SerializeField] float distortionDistance;

    CameraFollow cameraFollow;
    Vector2 minWrap, maxWrap;
    Rigidbody ball;
    TrailRenderer ballTR;
    void Start()
    {
        //camera
        cameraFollow = GetComponent<CameraFollow>();

        //retrieve ball variables
        ball = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>();

        //define min and max wrap positions dependent on shader variables
        minWrap = CameraFollow.innerShape.bounds.min;
        minWrap += Vector2.one * distortionDistance;
        maxWrap = CameraFollow.innerShape.bounds.max;
        maxWrap -= Vector2.one * distortionDistance;

        //init shader
        InitShader();
    }

    void Update()
    {
        CheckBallWrap();
        SetShaderVar();
    }
    //checks if ball should wrap
    void CheckBallWrap()
    {
        Vector2 pos = ball.position;
        // Check horizontal wrap
        if (pos.x > maxWrap.x)
            pos.x = minWrap.x;
        else if (pos.x < minWrap.x)
            pos.x = maxWrap.x;

        // Check vertical wrap
        if (pos.y > maxWrap.y)
            pos.y = minWrap.y;
        else if (pos.y < minWrap.y)
            pos.y = maxWrap.y;

        if(pos != (Vector2)ball.position)
        {
            if (ballTR == null)
            {
                ballTR = GameObject.FindGameObjectWithTag("Trail").GetComponent<TrailRenderer>();
            }
            ballTR.emitting = false;
            ballTR.enabled = false;
            ballTR.Clear();


            StartCoroutine(WrapBall());
        }
    }
    //coroutine to wrap ball
    IEnumerator WrapBall()
    {
        foreach (var a in wrapAudio)
        {
            GameManager.SFXManager.PlaySFX(a);
        }
        Vector2 pos = ball.position;

        // Check horizontal wrap
        if (pos.x > maxWrap.x)
            pos.x = minWrap.x;
        else if (pos.x < minWrap.x)
            pos.x = maxWrap.x;

        // Check vertical wrap
        if (pos.y > maxWrap.y)
            pos.y = minWrap.y;
        else if (pos.y < minWrap.y)
            pos.y = maxWrap.y;



        cameraFollow.SetTargetZoom(true);
        ballTR.Clear();

        yield return null;
        ball.position = pos;
        yield return null;
        yield return null;
        yield return null;
        ballTR.Clear();
        ballTR.emitting = true;
        ballTR.enabled = true;
    }
    #region Shader
    private void OnValidate()
    {
        if(distortMat!=null)
        {
            EditorUpdateShader();
        }
    }
    void EditorUpdateShader()
    {
        distortMat.SetFloat("_DistortionStrength", distortionStrength);
        distortMat.SetFloat("_DistortionRes", distortionRes);
        distortMat.SetFloat("_DistortionDistance", distortionDistance);
    }
    void InitShader()
    {
        if (CameraFollow.innerShape != null)
        {
            distortMat.SetVector("_MinBounds", CameraFollow.innerShape.bounds.min);
            distortMat.SetVector("_MaxBounds", CameraFollow.innerShape.bounds.max);
        }
    }
    //Sets updating shader variables
    void SetShaderVar()
    {
        distortMat.SetVector("_CamSize", CalculateCamsize(Camera.main.orthographicSize));
        distortMat.SetVector("_CameraSize", CalculateCamsize(Camera.main.orthographicSize));

        InitShader();
    }
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if(blit)
        {
            Graphics.Blit(source, destination, distortMat);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }
    Vector2 CalculateCamsize(float orthoSize)
    {
        float height = orthoSize * 2; //camera height in units
        float width = height * Camera.main.aspect; //camera width in units
        return new Vector2(width, height);
    }
    #endregion
}
