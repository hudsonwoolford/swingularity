using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class AstronautMatController : MonoBehaviour
{
    [SerializeField] SkinnedMeshRenderer astronaut;
    [Header("Normal Materials")]
    [SerializeField] Material alu, blackGloss, white, emmission;

    [Header("Fade Materials")]
    [SerializeField] Material aluFade, blackFade, whiteFade, emmissionFade;
    [SerializeField] Animator astronautAnim;    
    bool left;
    [SerializeField] float transitionDuration;
    [SerializeField] float animSmoothing;
    float lerpVal;

    void Start(){
        emmission.SetColor("_EmissionColor", GameManager.LevelManager.currentTheme.ballColor * 0.25f/6f);    
        emmissionFade.SetColor("_Color", GameManager.LevelManager.currentTheme.ballColor * 0.65f/6f);    
        EndFade();
    }
    void Update(){
            astronautAnim.SetBool("liningUp", ShootBall.liningUpShot);
            lerpVal = ShootBall.shootPower/100f;

            GolfSwingPullBack();
    }
    public void StartFade(){
        Material[] fadeMats = new Material[5]{
            whiteFade,
            whiteFade,
            emmissionFade,
            whiteFade,
            aluFade
        };
        astronaut.materials = fadeMats;
    }
    public void EndFade(){
        Material[] mats = new Material[5]{
            white,
            white,
            emmission,
            white,
            alu,
        };
        astronaut.materials = mats;
    }
    
    void GolfSwingPullBack(){
        // if(ShootBall.shootAngle > 1)
        astronautAnim.SetBool("left", left);
        astronautAnim.SetFloat("pullbackPos", lerpVal);
    }

    void BallSwing(){
        float offset = Mathf.Lerp(0, 5f/23f, lerpVal);
        if(left){
            astronautAnim.CrossFade("Swing Left", transitionDuration, 0, offset);
        }
        else{
            astronautAnim.CrossFade("Swing Right", transitionDuration, 0, offset);
        }
    }

    void OnEnable(){
        ShootBall.onSwing += BallSwing;
        ShootBall.triggerReset += EndFade;
    }
    void OnDisable(){
        ShootBall.onSwing -= BallSwing;
        ShootBall.triggerReset -= EndFade;

    }
}
