using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagneticFieldDraw : MonoBehaviour
{
    const double boltzmannConstant = 1.38064852e-23;
    [SerializeField] Vector2 testPoint = Vector2.zero;
    [SerializeField] Vector2 pole1 = Vector2.zero;
    [SerializeField] Vector2 pole2 = Vector2.zero;
    [SerializeField] float pole1Charge = 10;
    [SerializeField] float pole2Charge = -10;
    [SerializeField] uint xSeg = 10;
    [SerializeField] uint ySeg = 10;
    [SerializeField] float cellSize = 1;
    [SerializeField] float lineLength = 1;
    [SerializeField] ParticleSystem particles1, particles2;
    [SerializeField] float particleSpeed;
    [SerializeField] bool invertPolarity = false;
    ParticleSystem.Particle[] allParticles1, allParticles2;

    // Start is called before the first frame update
    void Start()
    {
        pole1 += new Vector2(transform.position.x, transform.position.y);
        pole2 += new Vector2(transform.position.x, transform.position.y);
        InitializeParticles();

    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if(gameObject.layer == 12) { Destroy(gameObject); }
        UpdateParticleSystem(particles1, ref allParticles1);
        UpdateParticleSystem(particles2, ref allParticles2);
    }
    void UpdateParticleSystem(ParticleSystem particles, ref ParticleSystem.Particle[] allP)
    {
        int numAlive = particles.GetParticles(allP);
        int polarity = !invertPolarity ? 1 : -1;
        for (int i = 0; i < numAlive; i++)
        {
            Vector2 pos = allP[i].position;
            Vector2 wSum = WeightedSum(pos, pole1, pole2);
            float distance = wSum.magnitude;
            Vector2 nextPoint = new Vector2(wSum.x / distance, wSum.y / distance);
            allP[i].velocity =  polarity * nextPoint * Time.fixedDeltaTime * particleSpeed;
            //allP[i].velocity = Vector3.one * 10;
        }

        particles.SetParticles(allP);
    }
    void InitializeParticles()
    {
        allParticles1 = new ParticleSystem.Particle[particles1.main.maxParticles];
        allParticles2 = new ParticleSystem.Particle[particles2.main.maxParticles];
    }
    float FieldMagnitude(Vector2 point, Vector2 polePoint, float chargeProduct)
    {
        return (chargeProduct * (float)boltzmannConstant) / Mathf.Pow((Vector2.Distance(point, polePoint)), 3);
    }
    Vector2 WeightedSum(Vector2 point, Vector2 pole1Point, Vector2 pole2Point)
    {
        float dY =  (point.x - pole1Point.x) * FieldMagnitude(point, pole1Point, pole1Charge) + (point.x - pole2Point.x) * FieldMagnitude(point, pole2Point, pole2Charge);
        float dX = -(point.y - pole1Point.y) * FieldMagnitude(point, pole1Point, pole1Charge) - (point.y - pole2Point.y) * FieldMagnitude(point, pole2Point, pole2Charge);
        return new Vector2(dX, dY);
    }
    private void OnDrawGizmosSelected() //Helpful gizmos in menu
    {
        Gizmos.DrawSphere(testPoint + new Vector2(transform.position.x, transform.position.y), 0.1f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(pole1 + new Vector2(transform.position.x, transform.position.y), 0.1f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(pole2 + new Vector2(transform.position.x, transform.position.y), 0.1f);

    }
    void DrawMagneticField()
    {
        Vector2 offset = new Vector2(-xSeg / 2 * cellSize, -ySeg / 2 * cellSize);
        for (int y = 0; y < ySeg; y++)
        {
            for (int x = 0; x < xSeg; x++)
            {
                Vector2 trackPoint = new Vector2(x * cellSize, y * cellSize) + offset + testPoint + new Vector2(transform.position.x, transform.position.y);
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
}
