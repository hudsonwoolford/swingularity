using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class NeutronStarGenerator : SunGenerator
{
    const double boltzmannConstant = 1.38064852e-23;
    [Header("Neutron Variables")]
    [Space()]
    [Header("Poles")]
    [SerializeField] float poleAngle = 0;
    [SerializeField] float pole1Charge = 1;
    [SerializeField] float pole2Charge = -1;
    [Header("Particle Visualisation")]
    [SerializeField] Transform particleParent;
    [SerializeField] ParticleSystem[] particleSystems;
    [SerializeField]ParticleSystem atmosphericParticles;
    [SerializeField] float particleSpeed = 1100;
    [SerializeField] bool invertPolarity = false;
    [Header("Magnetic Field Grid")]
    [SerializeField] uint xSeg = 50;
    [SerializeField] uint ySeg = 50;
    [SerializeField] float cellSize = 0.2f;
    [SerializeField] float lineLength = 0.1f;
    ParticleSystem.Particle[][] allParticles;

    [HideInInspector][SerializeField]Vector2 pole1;
    [HideInInspector][SerializeField] Vector2 pole2;
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        cMass.mass = Mathf.Abs(cMass.mass) * -1.5f;
        InitializeParticleArrays();
        InitializeParticles();
        //DrawMagneticField();
        particleSpeed = transform.localScale.x / 10;
    }
    // Update is called once per frame
    private void Update()
    {
        Vector2 angleVector = new Vector2(Mathf.Cos(poleAngle * Mathf.Deg2Rad), Mathf.Sin(poleAngle * Mathf.Deg2Rad));
        pole1 = -angleVector * transform.localScale.x + new Vector2(transform.position.x, transform.position.y);
        pole2 = angleVector * transform.localScale.x + new Vector2(transform.position.x, transform.position.y);
        particleParent.eulerAngles = new Vector3(0, 0, poleAngle);
    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (gameObject.layer == 7)
        {
            for (int i = 0; i < particleSystems.Length; i++)
            {
                Destroy(particleSystems[i]);
            }
        }

        for (int i = 0; i < particleSystems.Length; i++)
        {
            UpdateParticleSystem(particleSystems[i], ref allParticles[i]);
        }
        particleSpeed = transform.localScale.x / 10;
    }
    public override void RandomSun(int seed, ColorSchemeStaticReference.PlanetType type)
    {
        base.RandomSun(seed, type);

        UnityEngine.Random.InitState(seed);
        poleAngle = UnityEngine.Random.value * 180;
        cMass.mass = Mathf.Abs(cMass.mass) * -1.5f;
        InitializeParticleArrays();
        InitializeParticles();
    }
    #region Field Visualisations
    void InitializeParticles()
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(new GradientColorKey[] { new GradientColorKey(schemePool[starSchemeIndex].neutronParticleColor, 0), new GradientColorKey(schemePool[starSchemeIndex].neutronParticleColor, 1) }, new GradientAlphaKey[] { new GradientAlphaKey(1, 1), new GradientAlphaKey(1, 1) });
        ParticleSystem.MainModule main = atmosphericParticles.main;
        main.startColor = gradient;
        for (int i = 0; i < particleSystems.Length; i++)
        {
            main = particleSystems[i].main;
            main.startColor = gradient;
            float lifetimeMult = i == 0 || i == 1 ? 2.2f : 2.8f;
            main.startLifetime = lifetimeMult;
        }
        atmosphericParticles.Clear();
        atmosphericParticles.Play();
    }
    void UpdateParticleSystem(ParticleSystem particles, ref ParticleSystem.Particle[] allP)
    {
        allP =new ParticleSystem.Particle[particles.particleCount];
        int numAlive = particles.GetParticles(allP);
        int polarity = !invertPolarity ? 1 : -1;
        for (int i = 0; i < numAlive; i++)
        {
            //Vector2 pos = transform.position + transform.InverseTransformPoint(allP[i].position);
            Vector2 pos = allP[i].position;
            if (allP[i].remainingLifetime < allP[i].startLifetime / 1.5f && Vector2.Distance(pos, transform.position) < transform.localScale.x / 2)
            {
                allP[i].velocity = Vector2.zero;
                continue;
            }
            Vector2 wSum = WeightedSum(pos, pole1, pole2);
            float distance = wSum.magnitude;
            Vector2 nextPoint = new Vector2(wSum.x / distance, wSum.y / distance);
            //allP[i].velocity = polarity * nextPoint * Time.fixedDeltaTime * particleSpeed;
            //allP[i].position = transform.TransformPoint(new Vector3(wSum.x * particleSpeed / distance + pos.x, wSum.y * particleSpeed / distance + pos.y, transform.position.z)-transform.position);
            allP[i].position = new Vector3(wSum.x * particleSpeed / distance + pos.x, wSum.y * particleSpeed / distance + pos.y, transform.position.z);
            //allP[i].velocity = Vector3.one * 10;
        }

        particles.SetParticles(allP);
    }
    void InitializeParticleArrays()
    {
        allParticles = new ParticleSystem.Particle[particleSystems.Length][];
        for (int i = 0; i < particleSystems.Length; i++)
        {
            allParticles[i] = new ParticleSystem.Particle[particleSystems[i].main.maxParticles];
        }
    }

    void DrawMagneticField()
    {
        Vector2 offset = new Vector2(-xSeg / 2 * cellSize, -ySeg / 2 * cellSize);
        for (int y = 0; y < ySeg; y++)
        {
            for (int x = 0; x < xSeg; x++)
            {
                Vector2 trackPoint = new Vector2(x * cellSize, y * cellSize) + offset + new Vector2(transform.position.x, transform.position.y);
                float t = -lineLength;
                Vector2 wSum = WeightedSum(trackPoint, pole1, pole2);
                float distance = wSum.magnitude;

                Vector2 point1 = new Vector2(wSum.x * t / distance + trackPoint.x, wSum.y * t / distance + trackPoint.y);

                t = lineLength;
                Vector2 point2 = new Vector2(wSum.x * t / distance + trackPoint.x, wSum.y * t / distance + trackPoint.y);

                Debug.DrawLine(point1, point2, Color.red, 50, false);
            }
        }
    }
    #endregion
    #region Field Calculations
    float FieldMagnitude(Vector2 point, Vector2 polePoint, float charge)
    {
        return (charge * (float)boltzmannConstant) / Mathf.Pow((Vector2.Distance(point, polePoint)), 3);
    }
    Vector2 WeightedSum(Vector2 point, Vector2 pole1Point, Vector2 pole2Point)
    {

        float dY =  (point.x - pole1Point.x) * FieldMagnitude(point, pole1Point, pole1Charge) + (point.x - pole2Point.x) * FieldMagnitude(point, pole2Point, pole2Charge);
        float dX = -(point.y - pole1Point.y) * FieldMagnitude(point, pole1Point, pole1Charge) - (point.y - pole2Point.y) * FieldMagnitude(point, pole2Point, pole2Charge);
        return new Vector2(dX, dY);
    }
    #endregion
    private void OnDrawGizmosSelected() //Helpful gizmos in menu
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(pole1, 0.1f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(pole2, 0.1f);

    }
}
