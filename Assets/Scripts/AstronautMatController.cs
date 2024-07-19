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

    void Start(){
        emmission.SetColor("_EmissionColor", GameManager.LevelManager.currentTheme.ballColor * 0.25f/6f);    
        emmissionFade.SetColor("_Color", GameManager.LevelManager.currentTheme.ballColor * 0.5f/6f);    
        EndFade();
    }
    void Update(){
        if(ShootBall.liningUpShot){
            GolfSwingPullBack();
        }
        else{
            astronautAnim.CrossFade("Idle", transitionDuration, 0);
        }
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
        float lerpVal = ShootBall.shootPower/100f;
        if(ShootBall.shootAngle > 1)
        if(left){
            astronautAnim.CrossFade("Pull Left", transitionDuration, 0, lerpVal);
        }
        else{
            astronautAnim.CrossFade("Pull Right", transitionDuration, 0, lerpVal);
        }
    }

    void BallSwing(){
        if(left){
            astronautAnim.CrossFade("Swing Left", transitionDuration);
        }
        else{
            astronautAnim.CrossFade("Swing Right", transitionDuration);
        }
    }

    void OnEnable(){
        ShootBall.onSwing += BallSwing;
    }
    void OnDisable(){
        ShootBall.onSwing -= BallSwing;
    }
}
