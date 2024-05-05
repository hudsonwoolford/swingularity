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
    [Space()]
    [SerializeField] ParticleSystem swirlSystem;
    [SerializeField] ParticleSystem upperSparks;
    [SerializeField] ParticleSystem lowerSparks;
    [SerializeField] ParticleSystem upperEnd;
    [SerializeField] ParticleSystem lowerEnd;
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
    MaterialPropertyBlock beamBlock;
    MaterialPropertyBlock electroBlock;
    MaterialPropertyBlock darkBlock;
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
        startFF.AddInteractable(lowerEnd.transform);
        endFF.AddInteractable(upperEnd.transform);

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
        RandomInterval();
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
    void RandomInterval()
    {
        bool stop = false;
        intervals.Clear();
        for (int i = 0; i < 5 && !stop; i++)
        {
            intervals.Add(UnityEngine.Random.Range(0.5f, 1.5f));
            stop = (UnityEngine.Random.value+1)/2 > 0.8f ? true : false;
        }
    }
    //Master function
    void ApplyingVariables()
    {
        //Clamping index
        schemeIndex = Mathf.Clamp(schemeIndex, 0, schemePool.Length - 1);

        //Transformations
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, angle);
        lowerBeamParent.localScale = new Vector3(lowerBeamParent.localScale.x, Mathf.Max(radius - 0.255f, 0), lowerBeamParent.localScale.z);
        upperBeamParent.localScale = new Vector3(upperBeamParent.localScale.x, radius, upperBeamParent.localScale.z);
        bCollider.size = new Vector3(1, Mathf.Max(radius * 8.25f, 0.1f), 1);
        bCollider.enabled = radius == 0 ? false : true;
        bool shrinking = maxRadius <= radius ? false : true;
        upperEnd.gameObject.SetActive(!shrinking);
        lowerEnd.gameObject.SetActive(!shrinking);
        

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

        //beam
        beamBlock.SetColor("_Color", schemePool[schemeIndex].beamColor);
        upperRenderer.SetPropertyBlock(beamBlock);
        lowerRenderer.SetPropertyBlock(beamBlock);

        electroBlock.SetColor("_Color", schemePool[schemeIndex].electroColor);
        //electroBlock.SetVector("_MainTex_ST", new Vector4(upperRenderer.transform.lossyScale.y / 1.6f, 1, 0, 0));
        upperElectroRenderer.SetPropertyBlock(electroBlock);
        lowerElectroRenderer.SetPropertyBlock(electroBlock);

        darkBlock.SetColor("_Color", schemePool[schemeIndex].darkColor);
        upperDarkRenderer.SetPropertyBlock(darkBlock);
        lowerDarkRenderer.SetPropertyBlock(darkBlock);

        //Particle system colors
        ParticleSystem.MainModule module;
        Gradient grad = new Gradient();

        //Swirl colors
        module = swirlSystem.main;
        grad.SetKeys(new GradientColorKey[] { new GradientColorKey(schemePool[schemeIndex].lightColor, 0.0f), new GradientColorKey(schemePool[schemeIndex].darkColor, 1.0f) }, new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) });
        module.startColor = grad;

        //End colors
        module = upperEnd.main;
        module.startColor = schemePool[schemeIndex].sparkColor;
        module = lowerEnd.main;
        module.startColor = schemePool[schemeIndex].sparkColor;
        
        //beam colors
        module = upperSparks.main;
        grad.SetKeys(new GradientColorKey[] { new GradientColorKey(schemePool[schemeIndex].sparkColor, 0.0f), new GradientColorKey(schemePool[schemeIndex].sparkColor, 1.0f) }, new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) });
        module.startColor = grad;
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
        while(progress < 1)
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
        if (beamBlock == null) { beamBlock = new MaterialPropertyBlock(); }
        if (darkBlock == null) { darkBlock = new MaterialPropertyBlock(); }
        if (electroBlock == null) { electroBlock = new MaterialPropertyBlock(); }
    }
    private void OnTriggerEnter(Collider collision)
    {
    }
}
