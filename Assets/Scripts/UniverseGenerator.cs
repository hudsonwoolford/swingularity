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
        public float depthScale;
        public float emmissionPower;
        [ColorUsageAttribute(true, true)]
        public Color color;
        [Range(0, 1)]
        public float smoothness, metallic;
        bool isCustom;
        int seed;
    }

    [SerializeField]
    Universe[] possibleUniverses;

    [SerializeField]
    int universeIndex;
    
    MeshRenderer meshRenderer;
    MaterialPropertyBlock pBlock;

    void Awake(){
        meshRenderer = GetComponent<MeshRenderer>();
        pBlock = new MaterialPropertyBlock();
        UnityEngine.Random.InitState(Mathf.RoundToInt(transform.position.x + transform.position.y + transform.position.z));
        universeIndex = UnityEngine.Random.Range(0, possibleUniverses.Length);
        ChangeMaterialBlock();
    }

    void Update(){
    }

    void ChangeMaterialBlock(){
        Universe universe = possibleUniverses[universeIndex];
        pBlock.SetTexture("_Cube", universe.cubemap);
        pBlock.SetFloat("_DepthScale", universe.depthScale);
        pBlock.SetFloat("_EmmissionPower", universe.emmissionPower);
        pBlock.SetColor("_Emmission", universe.color);
        pBlock.SetFloat("_Glossiness", universe.smoothness);
        pBlock.SetFloat("_Metallic", universe.metallic);

        meshRenderer.SetPropertyBlock(pBlock);
    }
}
