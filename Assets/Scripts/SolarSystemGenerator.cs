using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode()]
public class SolarSystemGenerator : MonoBehaviour
{
    [System.Serializable]
    struct OrbitPlanet
    {
        public float radius;
        public int seed;
        public MockPlanet script;
    }
    [SerializeField] Transform sun;
    [SerializeField] float sunSize = 1;
    [SerializeField] float planetOffset = 0.2f;
    [SerializeField] Transform mockPlanetParent;
    [SerializeField] GameObject mockPlanet;
    [SerializeField] OrbitPlanet[] orbitPlanets;
    // Update is called once per frame
    void Update()
    {
        if(mockPlanet == null || mockPlanetParent == null || sun == null || orbitPlanets.Length <= 0) { return; }
        if(!Application.isPlaying)
        {
            //Spawning and deleting mock planets
            if(mockPlanetParent.childCount != orbitPlanets.Length)
            {
                while(orbitPlanets.Length < mockPlanetParent.childCount)
                {
                    DestroyImmediate(mockPlanetParent.GetChild(0).gameObject);
                }
                while (orbitPlanets.Length > mockPlanetParent.childCount)
                {
                    MockPlanet mp = Instantiate(mockPlanet, mockPlanetParent).GetComponent<MockPlanet>();
                    orbitPlanets[mockPlanetParent.childCount].script = mp;
                }
            }
            //Sun variables
            sun.transform.localScale = Vector3.one * sunSize;
            //Calibrating variables on mock planets, position setting, seed setting, etc
            float totalRadius = sunSize + planetOffset + orbitPlanets[0].radius;
            for (int i = 0; i < orbitPlanets.Length; i++)
            {
                if (orbitPlanets[i].script == null) { orbitPlanets[i].script = mockPlanetParent.GetChild(i).GetComponent<MockPlanet>(); }
                orbitPlanets[i].script.seed = orbitPlanets[i].seed;
                orbitPlanets[i].script.transform.localScale = Vector3.one * orbitPlanets[i].radius;
                orbitPlanets[i].script.transform.position = sun.transform.position + Vector3.right * totalRadius;
                totalRadius += orbitPlanets[i].radius + planetOffset;
            }
        }
    }
}
