using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsManager : MonoBehaviour
{
    [SerializeField] Animator creditsAnim;
    [SerializeField] Material skybox;
    [SerializeField] float rotation, speed;

    void Start(){
        LevelGenerator.levelInit?.Invoke(LevelManager.allGalaxyData[0]);
        creditsAnim.Play("RollCredits");
        StartCoroutine(EndCredits());
    }

    void Update(){
        if (Input.GetKeyDown(KeyCode.Escape)){
            GameManager.SceneHandling.LoadScene("Main Menu", false);
        }
        rotation += Time.deltaTime * speed;
        if (rotation > 360) {
            rotation = 0;
        }
        skybox.SetFloat("_Rotation", rotation);
    }

    IEnumerator EndCredits(){
        yield return new WaitForSeconds(30f);
        GameManager.SceneHandling.LoadScene("Main Menu", false);
    }
}
