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

    void Start(){
        emmission.SetColor("_EmissionColor", GameManager.LevelManager.currentTheme.ballColor * 0.25f/6f);    
        emmissionFade.SetColor("_Color", GameManager.LevelManager.currentTheme.ballColor * 0.5f/6f);    
        EndFade();
    }
    void Update(){
        StartCoroutine(GolfSwingPullBack());
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
    
    IEnumerator GolfSwingPullBack(){
        float lerpVal = ShootBall.shootPower/100f;
        float time = Mathf.Lerp(250f/500f, 275f/500f, lerpVal);
        // astronautAnim.Play("Swing Left", 0, time);
        yield return null;
    }
}
