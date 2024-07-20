using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
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
    [SerializeField] AnimatorController astronautAnimController;    
    public static bool left;
    [SerializeField] float transitionDuration;
    [SerializeField] float animConversionFloat;
    [SerializeField] AnimatorStateTransition leftSwingTransition, rightSwingTransition;
    float lerpVal;

    void Start(){
        emmission.SetColor("_EmissionColor", GameManager.LevelManager.currentTheme.ballColor * 0.25f/6f);    
        emmissionFade.SetColor("_Color", GameManager.LevelManager.currentTheme.ballColor * 0.65f/6f);    
        EndFade();
        SetTransitions();
    }
    void Update(){
        Debug.Log(ShootBall.shootAngle);
        lerpVal = ShootBall.shootPower/100f;
        GolfSwingPullBack();
        astronautAnim.SetBool("liningUp", ShootBall.liningUpShot && ShootBall.shootPower >= 1);
                        // astronautAnim.SetBool("swing", (!ShootBall.liningUpShot));
        if(ShootBall.astronautFading){
            astronautAnim.Play("Idle", 0);
        }
    }
    void SetTransitions(){
        AnimatorStateMachine machine = astronautAnimController.layers[0].stateMachine;
        foreach(var state in machine.states){
            if(state.state.name == "Pull Left"){
                foreach(var transition in state.state.transitions){
                    if(transition.destinationState.name == "Swing Left"){
                        leftSwingTransition = transition;
                    }
                }
            }
            else if(state.state.name == "Pull Right"){
                foreach(var transition in state.state.transitions){
                    if(transition.destinationState.name == "Swing Right"){
                        rightSwingTransition = transition;
                    }
                }
            }
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
        // if(ShootBall.shootAngle > 1)
        left = ShootBall.shootAngle >= 0;
        astronautAnim.SetBool("left", left);
        astronautAnim.SetFloat("pullbackPos", lerpVal);
    }

    void BallSwing(){
        StartCoroutine(Swing());
        // leftSwingTransition.offset = offset;
        // rightSwingTransition.offset = offset;

    }
    bool GetState(){
        if(astronautAnim.GetCurrentAnimatorStateInfo(0).IsName("Pull Right") || astronautAnim.GetCurrentAnimatorStateInfo(0).IsName("Pull Left"))
        {
            return true;
        }
        else{
            return false;
        }
    }

    IEnumerator Swing(){
        yield return new WaitUntil(() => !astronautAnim.IsInTransition(0) && GetState());
                float offset = Mathf.Lerp(0, 5f/23f, 1-lerpVal);

        if(left){
            astronautAnim.CrossFadeInFixedTime("Swing Left", transitionDuration * animConversionFloat, 0, offset * animConversionFloat);
        }
        else{
            astronautAnim.CrossFadeInFixedTime("Swing Right", transitionDuration * animConversionFloat, 0, offset * animConversionFloat);
        }
    }

    void BallHit(){
        // astronautAnim.SetBool("swing", false);
    }

    void OnEnable(){
        ShootBall.onSwing += BallSwing;
        ShootBall.triggerReset += EndFade;
        ShootBall.onShoot += BallHit;
    }
    void OnDisable(){
        ShootBall.onSwing -= BallSwing;
        ShootBall.triggerReset -= EndFade;
        ShootBall.onShoot -= BallHit;
    }
}
