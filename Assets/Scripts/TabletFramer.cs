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

    bool framed;
    Vector3 startingPos;
    Quaternion startingRot;
    // Start is called before the first frame update
    void Start()
    {
        videoPlayer.clip = loopClip;
        videoPlayer.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if(framed)
        {
            transform.position = frameCamera.position;
            transform.rotation = frameCamera.rotation;
        }
        if(Input.GetKeyDown(KeyCode.Space) && !framed)
        {
            StartCoroutine(FrameCamera());
        }
    }
    public void FrameCameraVoid()
    {
        StartCoroutine(FrameCamera());
    }
    IEnumerator FrameCamera()
    {
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

        framed = true;

        void WhileLoopLogic()
        {
            transform.position = Vector3.Lerp(startPos, frameCamera.position, transitionCurve.Evaluate(timer / transitionDuration));
            transform.rotation = Quaternion.Lerp(startingRot, frameCamera.rotation, transitionCurve.Evaluate(timer / transitionDuration));
            if(transitionCurve.Evaluate(timer / transitionDuration) > 0.66f && videoPlayer.clip != mainClip)
            {
                videoPlayer.clip = mainClip;
                videoPlayer.Play();
            }
        }

        yield return new WaitForSeconds((float)mainClip.length);

        //StartCoroutine(UnframeCamera());
    }
    IEnumerator UnframeCamera()
    {
        framed = false;
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
    }
}
