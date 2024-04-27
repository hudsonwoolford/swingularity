using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

[ExecuteInEditMode()]
public class QuasarGenerator : MonoBehaviour
{
    [Serializable]
    public struct QuasarColorScheme
    {
        public ColorSchemeStaticReference.PlanetType planetType;
        [ColorUsage(true, true)]
        public Color darkColor, lightColor;
        [ColorUsage(true, true)]
        public Color fresnel;
        [ColorUsage(true, true)]
        public Color sparkColor, electroColor, beamColor;
    }
    [SerializeField] QuasarColorScheme[] schemePool;
    [SerializeField] int schemeIndex;
    [SerializeField] float systemRotSpeed = 8;
    [SerializeField] float angle;
    [SerializeField] float radius = 1;
    [Tooltip("First interval is on, after that it switches")]
    [SerializeField] List<float> intervals = new List<float>();
    [SerializeField] AnimationCurve enableCurve;
    [SerializeField] float enableSpeed;
    [Header("Local Variables")]
    [SerializeField] Transform upperBeamParent;
    [SerializeField] Transform lowerBeamParent;
    [SerializeField] Transform upperEnd;
    [SerializeField] Transform lowerEnd;
    [Space()]
    [SerializeField] ParticleSystem swirlSystem;
    [SerializeField] ParticleSystem upperSparks;
    [SerializeField] ParticleSystem lowerSparks;
    [Space()] //Mesh Renderers
    [SerializeField] MeshRenderer sphereRenderer;
    [SerializeField] MeshRenderer distortRenderer;
    [SerializeField] MeshRenderer upperRenderer;
    [SerializeField] MeshRenderer upperElectroRenderer;
    [SerializeField] MeshRenderer upperDarkRenderer;
    [SerializeField] MeshRenderer lowerRenderer;
    [SerializeField] MeshRenderer lowerElectroRenderer;
    [SerializeField] MeshRenderer lowerDarkRenderer;
    //Property Blocks
    MaterialPropertyBlock sphereBlock;
    MaterialPropertyBlock distortBlock;
    MaterialPropertyBlock upperBlock;
    MaterialPropertyBlock upperElectroBlock;
    MaterialPropertyBlock upperDarkBlock;
    MaterialPropertyBlock lowerBlock;
    MaterialPropertyBlock lowerElectroBlock;
    MaterialPropertyBlock lowerDarkBlock;
    //Miscellaneous
    Transform distortionQuad;
    Material distortMat;
    CelestialMass cMass;
    BoxCollider bCollider;

    float intervalTime = 0;
    int currentInterval = 0;
    float maxRadius;
    // Start is called before the first frame update
    void Start()
    {
        maxRadius = radius;
        NullChecks();
    }

    // Update is called once per frame
    void Update()
    {
        if (schemePool.Length == 0) { return; }
        NullChecks();
        ApplyingVariables();
        Interval();
    }
    public void InitRandom(int seed, ColorSchemeStaticReference.PlanetType planetType, Vector3 pos, float ang, float rad, ForceField startFF, ForceField endFF)
    {
        NullChecks();
        startFF.AddInteractable(lowerEnd);
        endFF.AddInteractable(upperEnd);

        maxRadius = rad;
        radius = rad;
        angle = ang;
        transform.position = pos + transform.up * 0f * radius;

        UnityEngine.Random.InitState(seed);

        List<int> typeSchemeIndices = new List<int>();
        for (int i = 0; i < schemePool.Length; i++)
        {
            if (schemePool[i].planetType == planetType)
            {
                typeSchemeIndices.Add(i);
            }
        }
        if (typeSchemeIndices.Count == 0) { return; }
        schemeIndex = typeSchemeIndices[UnityEngine.Random.Range(0, typeSchemeIndices.Count)];

        //Applying
        RandomRotation();
        ApplyingVariables();

        //Reset system
        swirlSystem.Clear();
        swirlSystem.Play();
    }
    [ContextMenu("Random Color")]
    void RandomHole()
    {
        schemeIndex = UnityEngine.Random.Range(0, schemePool.Length);

        //Applying
        RandomRotation();
        ApplyingVariables();

        //Reset system
        swirlSystem.Clear();
        swirlSystem.Play();
    }
    void RandomRotation()
    {
        //maybe changes plane a bit
    }
    //Master function
    void ApplyingVariables()
    {
        //Clamping index
        schemeIndex = Mathf.Clamp(schemeIndex, 0, schemePool.Length - 1);

        //Transformations
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, angle);
        lowerBeamParent.localScale = new Vector3(lowerBeamParent.localScale.x, Mathf.Max(radius-(0.3f), 0), lowerBeamParent.localScale.z); 
        upperBeamParent.localScale = new Vector3(upperBeamParent.localScale.x, radius, upperBeamParent.localScale.z);
        bCollider.size = new Vector3(1, radius * 8.25f, 1);

        //Property blocks
        //Sphere fresnel
        sphereBlock.SetColor("_MainColor", Color.white);
        sphereBlock.SetColor("_FresnelColor", (schemePool[schemeIndex].fresnel));
        sphereBlock.SetFloat("_FresnelStrength", 1.25f);
        sphereRenderer.SetPropertyBlock(sphereBlock);

        //Twirling
        distortMat.SetFloat("_CameraOrthoSize", Camera.main.orthographicSize);
        distortBlock.SetFloat("_Speed", systemRotSpeed);
        distortRenderer.SetPropertyBlock(distortBlock);
        ParticleSystem.RotationOverLifetimeModule rotModule = swirlSystem.rotationOverLifetime;
        rotModule.z = new ParticleSystem.MinMaxCurve(systemRotSpeed);

        //Upper beam
        upperBlock.SetColor("_Color", schemePool[schemeIndex].beamColor);
        upperRenderer.SetPropertyBlock(upperBlock);

        upperElectroBlock.SetColor("_Color", schemePool[schemeIndex].electroColor);
        //upperElectroBlock.SetVector("_MainTex_ST", new Vector4(upperRenderer.transform.lossyScale.y / 1.6f, 1, 0, 0));
        upperElectroRenderer.SetPropertyBlock(upperElectroBlock);

        upperDarkBlock.SetColor("_Color", schemePool[schemeIndex].darkColor);
        upperDarkRenderer.SetPropertyBlock(upperDarkBlock);

        //Lower beam
        lowerBlock.SetColor("_Color", schemePool[schemeIndex].beamColor);
        lowerRenderer.SetPropertyBlock(lowerBlock);

        lowerElectroBlock.SetColor("_Color", schemePool[schemeIndex].electroColor);
        //lowerElectroBlock.SetVector("_MainTex_ST", new Vector4(lowerRenderer.transform.lossyScale.y / 1.6f, 1, 0, 0));
        lowerElectroRenderer.SetPropertyBlock(lowerElectroBlock);

        lowerDarkBlock.SetColor("_Color", schemePool[schemeIndex].darkColor);
        lowerDarkRenderer.SetPropertyBlock(lowerDarkBlock);

        //Particle system colors
        ParticleSystem.MainModule module;
        Gradient grad = new Gradient();

        //Swirl colors
        module = swirlSystem.main;
        grad.SetKeys(new GradientColorKey[] { new GradientColorKey(schemePool[schemeIndex].lightColor, 0.0f), new GradientColorKey(schemePool[schemeIndex].darkColor, 1.0f) }, new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) });
        module.startColor = grad;

        //Upper beam color
        module = upperSparks.main;
        grad.SetKeys(new GradientColorKey[] { new GradientColorKey(schemePool[schemeIndex].sparkColor, 0.0f), new GradientColorKey(schemePool[schemeIndex].sparkColor, 1.0f) }, new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) });
        module.startColor = grad;

        //Lower beam color
        module = lowerSparks.main;
        module.startColor = grad; //same color as upper beam

    }
    void Interval()
    {
        if(!Application.isPlaying) { return; }
        if(intervals.Count <= 0) { return; }
        intervalTime += Time.deltaTime;
        if(intervalTime > intervals[currentInterval]) 
        {

            intervalTime = 0;
            currentInterval++;

            if(currentInterval > intervals.Count-1)
            {
                currentInterval = 0;
            }
            if(currentInterval%2 == 0)
            {
                StartCoroutine(EnableBeam(true));
            }
            else
            {
                StartCoroutine(EnableBeam(false));
            }
        }
    }
    IEnumerator EnableBeam(bool enable)
    {
        float progress = 0;
        float target = enable ? maxRadius : 0;
        float start = radius;
        while(radius != target)
        {
            progress = Mathf.MoveTowards(progress, 1, enableSpeed * Time.deltaTime);
            radius = Mathf.Lerp(start, target, enableCurve.Evaluate(progress));
            yield return null;
        }
    }
    void NullChecks()
    {
        if (distortMat == null) { distortMat = distortRenderer.sharedMaterial; }
        if (distortionQuad == null) { distortionQuad = distortRenderer.transform; }
        if (cMass == null) { cMass = GetComponent<CelestialMass>(); }
        if (bCollider == null) { bCollider = GetComponent<BoxCollider>(); }
        //Property block null checks
        if (sphereBlock == null) { sphereBlock = new MaterialPropertyBlock(); }
        if (distortBlock == null) { distortBlock = new MaterialPropertyBlock(); }
        if (lowerBlock == null) { lowerBlock = new MaterialPropertyBlock(); }
        if (lowerDarkBlock == null) { lowerDarkBlock = new MaterialPropertyBlock(); }
        if (lowerElectroBlock == null) { lowerElectroBlock = new MaterialPropertyBlock(); }
        if (upperBlock == null) { upperBlock = new MaterialPropertyBlock(); }
        if (upperDarkBlock == null) { upperDarkBlock = new MaterialPropertyBlock(); }
        if (upperElectroBlock == null) { upperElectroBlock = new MaterialPropertyBlock(); }
    }
    private void OnTriggerEnter(Collider collision)
    {
    }
}
