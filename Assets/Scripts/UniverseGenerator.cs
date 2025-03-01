using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteInEditMode]
public class UniverseGenerator : MonoBehaviour
{
    [Serializable]
    public struct Universe{
        [ColorUsageAttribute(true, true)]
        public Color nebulaColorA, nebulaColorB;
        [ColorUsageAttribute(true, true)]
        public Color fresnelColor, glitterColor, volumetricStarColor;
    }
    [SerializeField] int universeIndex;
    [SerializeField] int indexSeed;
    [SerializeField] Universe[] possibleUniverses;
    [SerializeField] Vector3 rotSpeed;
    [SerializeField] bool rotate = false;
    MeshRenderer meshRenderer;
    MaterialPropertyBlock pBlock;

    [SerializeField] bool filming = false;

    [SerializeField] Vector3 offset, warpOffset;

    void Awake(){
        meshRenderer = GetComponent<MeshRenderer>();
        NullCheck();
        UnityEngine.Random.InitState(indexSeed);
        if(!filming){
        universeIndex = UnityEngine.Random.Range(0, possibleUniverses.Length);  
        }   
        ChangeMaterialBlock();
    }

    void Update(){
        NullCheck();
        if(!filming){
            universeIndex = Mathf.Clamp(universeIndex, 0, possibleUniverses.Length - 1);
        }
        ChangeMaterialBlock();
        if (rotate)
        {
            if(!filming){
                transform.Rotate(rotSpeed * Time.deltaTime);
            }
            else{
                transform.localEulerAngles += rotSpeed * Time.deltaTime;
            }
        }
    }
    void ChangeMaterialBlock(){
        Universe universe = possibleUniverses[universeIndex];
        pBlock.SetColor("_NebulaColorA", universe.nebulaColorA);
        pBlock.SetColor("_NebulaColorB", universe.nebulaColorB);
        pBlock.SetColor("_FresnelColor", universe.fresnelColor);
        pBlock.SetColor("_StarColor", universe.glitterColor);
        pBlock.SetColor("_VolumetricStarsColor", universe.volumetricStarColor);

        pBlock.SetVector("_Offset", offset);
        pBlock.SetVector("_WarpOffset", warpOffset);

        meshRenderer.SetPropertyBlock(pBlock);
    }
    void NullCheck()
    {
        if (pBlock == null) { pBlock = new MaterialPropertyBlock(); }
    }
    public void Init(int indexSeed)
    {
        this.indexSeed = indexSeed;
        UnityEngine.Random.InitState(indexSeed);
        universeIndex = UnityEngine.Random.Range(0, possibleUniverses.Length);
        offset = new Vector3(UnityEngine.Random.Range(0f, 2e+3f), UnityEngine.Random.Range(0f, 2e+3f), UnityEngine.Random.Range(0f, 2e+3f)); 
        warpOffset = new Vector3(UnityEngine.Random.Range(0f, 2e+3f), UnityEngine.Random.Range(0f, 2e+3f), UnityEngine.Random.Range(0f, 2e+3f)); 
    }
    public void ChangeUniverse(string s)
    {
        UnityEngine.Random.InitState(LevelSettingsManager.ConvertTextToSeed(s));
        offset = new Vector3(UnityEngine.Random.Range(0f, 2e+3f), UnityEngine.Random.Range(0f, 2e+3f), UnityEngine.Random.Range(0f, 2e+3f)); 
        warpOffset = new Vector3(UnityEngine.Random.Range(0f, 2e+3f), UnityEngine.Random.Range(0f, 2e+3f), UnityEngine.Random.Range(0f, 2e+3f)); 
        universeIndex = UnityEngine.Random.Range(0, possibleUniverses.Length);
    }
    public void ChangeUniverse(int s)
    {
        UnityEngine.Random.InitState(s);
        offset = new Vector3(UnityEngine.Random.Range(0f, 2e+3f), UnityEngine.Random.Range(0f, 2e+3f), UnityEngine.Random.Range(0f, 2e+3f));
        warpOffset = new Vector3(UnityEngine.Random.Range(0f, 2e+3f), UnityEngine.Random.Range(0f, 2e+3f), UnityEngine.Random.Range(0f, 2e+3f));
        universeIndex = UnityEngine.Random.Range(0, possibleUniverses.Length);
        ChangeMaterialBlock();
    }
}
