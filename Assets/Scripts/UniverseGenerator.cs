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
        public Cubemap cubemap;
        public Vector3 cubeOffset;
        public float depthScale;
        public float emmissionPower;
        [ColorUsageAttribute(true, true)]
        public Color color;
        [Range(0, 1)]
        public float smoothness, metallic;
        bool isCustom;
        int seed;
    }
    [SerializeField]int indexSeed;
    [SerializeField]
    Universe[] possibleUniverses;

    [SerializeField]
    int universeIndex;
    
    MeshRenderer meshRenderer;
    MaterialPropertyBlock pBlock;

    void Awake(){
        meshRenderer = GetComponent<MeshRenderer>();
        NullCheck();
        UnityEngine.Random.InitState(indexSeed);
        universeIndex = UnityEngine.Random.Range(0, possibleUniverses.Length);
        ChangeMaterialBlock();
    }

    void Update(){
        NullCheck();
        universeIndex = Mathf.Clamp(universeIndex, 0, possibleUniverses.Length - 1);
        ChangeMaterialBlock();
    }

    void ChangeMaterialBlock(){
        Universe universe = possibleUniverses[universeIndex];
        pBlock.SetTexture("_Cube", universe.cubemap);
        pBlock.SetVector("_CubeOffset", universe.cubeOffset);
        pBlock.SetFloat("_DepthScale", universe.depthScale);
        pBlock.SetFloat("_EmissionPower", universe.emmissionPower);
        pBlock.SetColor("_Emission", universe.color);
        pBlock.SetFloat("_Glossiness", universe.smoothness);
        pBlock.SetFloat("_Metallic", universe.metallic);

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
    }
}
