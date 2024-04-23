using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BlackHoleGenerator : MonoBehaviour
{
    [Serializable]
    public struct SpaceHoleColorScheme
    {
        public ColorSchemeStaticReference.PlanetType planetType;
        public Color darkColor, lightColor;
        [ColorUsage(true, true)]
        public Color fresnel;
        //[Range(0f, 1f)]
        //public float swirlIntensity;
    }
    [SerializeField] SpaceHoleColorScheme[] schemePool;
    [SerializeField] int schemeIndex;

    MeshRenderer sphereRenderer;
    ParticleSystem swirlSystem;
    MaterialPropertyBlock sphereBlock;
    // Start is called before the first frame update
    void Start()
    {
        NullChecks();
    }

    // Update is called once per frame
    void Update()
    {
        if(schemePool.Length == 0) { return; }
        NullChecks();
        Coloring();
    }
    [ContextMenu("Random Color")]
    void RandomHole()
    {
        schemeIndex = UnityEngine.Random.Range(0, schemePool.Length);
        Coloring();
        swirlSystem.Clear();
        swirlSystem.Play();
    }

    void Coloring()
    {
        schemeIndex = Mathf.Clamp(schemeIndex, 0, schemePool.Length-1);

        if(sphereBlock == null) { sphereBlock = new MaterialPropertyBlock(); }
        sphereBlock.SetColor("_MainColor", Color.black);
        sphereBlock.SetColor("_FresnelColor", (schemePool[schemeIndex].fresnel));
        sphereRenderer.SetPropertyBlock(sphereBlock);

        ParticleSystem.MainModule module = swirlSystem.main;
        Gradient grad = new Gradient();
        grad.SetKeys(new GradientColorKey[] { new GradientColorKey(schemePool[schemeIndex].lightColor, 0.0f), new GradientColorKey(schemePool[schemeIndex].darkColor, 1.0f) }, new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) });
        module.startColor = grad;
    }
    void NullChecks()
    {
        if (sphereRenderer == null) { sphereRenderer = GetComponent<MeshRenderer>(); }
        if (swirlSystem == null) { swirlSystem = GetComponentInChildren<ParticleSystem>(); }
    }
}
