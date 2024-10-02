using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class TabletFramer : MonoBehaviour
{
    [SerializeField] Transform frameCamera;
    [SerializeField] AnimationCurve transitionCurve;
    [SerializeField] VideoPlayer videoPlayer;
    [SerializeField] VideoClip mainClip;
    [SerializeField] VideoClip loopClip;
    [SerializeField] float transitionDuration;
    [SerializeField] CameraState currentState;
    enum CameraState
    {
        Framed,
        Unframed
    }

    Vector3 startingPos;
    Quaternion startingRot;
    // Start is called before the first frame update
    void Start()
    {
        //GetComponent<Animator>().enabled = false;

        videoPlayer.clip = mainClip;
        videoPlayer.Play();
        videoPlayer.time += 2f;


        startingPos = transform.position;
        startingRot = transform.rotation;

        if (currentState == CameraState.Framed)
        {
            transform.position = frameCamera.position;
            transform.rotation = frameCamera.rotation;
        }
        //StartCoroutine(UnframeCamera());
        StartCoroutine(WaitForNextClip());
    }

    // Update is called once per frame
    void Update()
    {
        //if(currentState == CameraState.Framed)
        //{
        //    transform.position = frameCamera.position;
        //    transform.rotation = frameCamera.rotation;
        //}
        if(Input.GetKeyDown(KeyCode.Space) && currentState == CameraState.Unframed)
        {
            StartCoroutine(FrameCamera());
        }
        if (Input.GetKeyDown(KeyCode.Space) && currentState == CameraState.Framed)
        {
            StartCoroutine(UnframeCamera());
        }
    }
    IEnumerator WaitForNextClip()
    {
        yield return new WaitForSeconds((float)mainClip.length-2f);
        videoPlayer.clip = loopClip; videoPlayer.Play();
    }
    IEnumerator FrameCamera()
    {
        videoPlayer.clip = loopClip; videoPlayer.Stop(); videoPlayer.time = 0; videoPlayer.Play();

        Vector3 startPos = transform.position;
        startingPos = transform.position;
        startingRot = transform.rotation;

        float timer = 0;
        while(timer < transitionDuration)
        {
            WhileLoopLogic();
            timer += Time.deltaTime;
            yield return null;
        }

        timer = transitionDuration;
        WhileLoopLogic();

        currentState = CameraState.Framed;

        void WhileLoopLogic()
        {
            transform.position = Vector3.Lerp(startPos, frameCamera.position, transitionCurve.Evaluate(timer / transitionDuration));
            transform.rotation = Quaternion.Lerp(startingRot, frameCamera.rotation, transitionCurve.Evaluate(timer / transitionDuration));
            if(transitionCurve.Evaluate(timer / transitionDuration) > 0.56f && videoPlayer.clip != mainClip)
            {
                videoPlayer.clip = mainClip;
                videoPlayer.Play();
            }
        }

        yield return new WaitForSeconds((float)mainClip.length);

        StartCoroutine(UnframeCamera());
    }
    IEnumerator UnframeCamera()
    {
        yield return new WaitForSeconds(0.5f);

        currentState = CameraState.Unframed;

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        float timer = 0;
        while (timer < transitionDuration)
        {
            WhileLoopLogic();
            timer += Time.deltaTime;
            yield return null;
        }

        timer = transitionDuration;
        WhileLoopLogic();


        void WhileLoopLogic()
        {
            transform.position = Vector3.Lerp(startPos, startingPos, transitionCurve.Evaluate(timer / transitionDuration));
            transform.rotation = Quaternion.Lerp(startRot, startingRot, transitionCurve.Evaluate(timer / transitionDuration));
        }
        GetComponent<Animator>().enabled = true;
    }
}
