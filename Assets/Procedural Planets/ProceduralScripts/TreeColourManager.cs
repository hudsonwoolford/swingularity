using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode()]
public class TreeColourManager : MonoBehaviour
{
    [Serializable]
    public class PropertyBlock
    {
        public Color _Color;
        public float _Smoothness;
        public float _Metallic;
    }
    public List<PropertyBlock> pBlocks = new List<PropertyBlock>();
    

    MeshRenderer mr;
    public void Initialize()
    { 
        mr = GetComponent<MeshRenderer>();
        pBlocks.Clear();
        for (int i = 0; i < mr.sharedMaterials.Length; i++)
        {
            pBlocks.Add(new PropertyBlock());
        }
    }
    private void Start()
    {
        ApplyPBlocks();
    }
    // Update is called once per frame
    void Update()
    {
        if (!mr.HasPropertyBlock()) { return; }
        ApplyPBlocks();
    }
    void ApplyPBlocks()
    {
        if (mr == null) { mr = GetComponent<MeshRenderer>(); }

        Material[] materials = mr.sharedMaterials;

        for (int i = 0; i < materials.Length; i++)
        {
            MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
            materialPropertyBlock.SetColor("_Color", pBlocks[i]._Color);
            materialPropertyBlock.SetFloat("_Smoothness", pBlocks[i]._Smoothness);
            materialPropertyBlock.SetFloat("_Metallic", pBlocks[i]._Metallic);

            mr.SetPropertyBlock(materialPropertyBlock, i);
        }
    }
}
