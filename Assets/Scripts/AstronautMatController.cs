using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class AstronautMatController : MonoBehaviour
{
    [SerializeField] SortingGroup[] sortingGroups;
    [SerializeField] SkinnedMeshRenderer[] aluMeshes, blackMeshes, whiteMeshes, emmissionMeshes;
    [Header("Normal Materials")]
    [SerializeField] Material alu, blackGloss, white, emmission;

    [Header("Fade Materials")]
    [SerializeField] Material aluFade, blackFade, whiteFade, emmissionFade;    

    void Start(){
        emmission.SetColor("_EmissionColor", GameManager.LevelManager.currentTheme.ballColor * 0.5f/6f);    
        StartFade();
    }

    public void StartFade(){
        for (int i = 0; i < sortingGroups.Length; i++){
            sortingGroups[i].enabled = true;
        }
        for (int i = 0; i < aluMeshes.Length; i++){
            aluMeshes[i].material = aluFade;
        }
        for (int i = 0; i < blackMeshes.Length; i++){
            blackMeshes[i].material = blackFade;
        }
        for (int i = 0; i < whiteMeshes.Length; i++){
            whiteMeshes[i].material = whiteFade;
        }
        for (int i = 0; i < emmissionMeshes.Length; i++){
            emmissionMeshes[i].material = emmissionFade;
        }
    }
    public void EndFade(){
        for (int i = 0; i < sortingGroups.Length; i++){
            sortingGroups[i].enabled = false;
        }
        for (int i = 0; i < aluMeshes.Length; i++){
            aluMeshes[i].material = alu;
        }
        for (int i = 0; i < blackMeshes.Length; i++){
            blackMeshes[i].material = blackGloss;
        }
        for (int i = 0; i < whiteMeshes.Length; i++){
            whiteMeshes[i].material = white;
        }
        for (int i = 0; i < emmissionMeshes.Length; i++){
            emmissionMeshes[i].material = emmission;
        }
    }
}
