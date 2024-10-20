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
    [SerializeField] float transitionDuration;
    [SerializeField] float animConversionFloat;
    ShootBall shootBall;
    public bool isIdle { get; private set; }
    bool left;
    float lerpVal;
    [SerializeField] Material[] whiteMaterials;


    void Start(){
        transform.position = new Vector3(transform.position.x, transform.position.z, -100);
        shootBall = GameObject.FindGameObjectWithTag("Player").GetComponent<ShootBall>();
        emmission.SetColor("_EmissionColor", GameManager.LevelManager.currentTheme.ballColor * 0.25f/6f);    
        emmissionFade.SetColor("_Color", GameManager.LevelManager.currentTheme.ballColor * 0.65f/6f);    

        CustomizeElement.MaterialCustomizationElement mce = SaveLoadManager.currentGlobalSave.cosmetic;
        foreach (Material wm in whiteMaterials)
        {
            wm.SetColor("_Color", mce.matColor);
            wm.SetFloat("_Metallic", mce.metallic);
            wm.SetFloat("_Glossiness", mce.metallic);
        }

        EndFade();
    }
    void Update(){
        isIdle = GetIdleState();
        lerpVal = Mathf.Lerp(10/50, 1, shootBall.shootPower/100f);
        GolfSwingPullBack();
        astronautAnim.SetBool("liningUp", shootBall.liningUpShot && Mathf.CeilToInt(shootBall.shootPower) > 5f); //the 5 is not set in both
                        // astronautAnim.SetBool("swing", (!ShootBall.liningUpShot));
        if(shootBall.astronautFading){
            astronautAnim.Play("Idle", 0);
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
        astronautAnim.Play("Idle", 0);
    }
    
    void GolfSwingPullBack(){
        // if(ShootBall.shootAngle > 1)
        left = shootBall.shootAngle >= 0;
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
    bool GetIdleState()
    {
        return astronautAnim.GetCurrentAnimatorStateInfo(0).IsName("Idle");
    }

    IEnumerator Swing(){

        yield return new WaitUntil(() => !astronautAnim.IsInTransition(0) && GetState());
        float offset = Mathf.Lerp(0, 4.5f/23f, 1-lerpVal);
        shootBall.swingDelay = 4.5f/23f - offset;
        shootBall.pullBack = true;


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
