using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MockPlanet : MonoBehaviour
{
    [SerializeField] public ColorSchemeStaticReference.PlanetType planetType;
    [SerializeField] public bool specificSeed;
    [SerializeField] public bool darkMatter;
    [SerializeField] public Vector3 seed;
}
#if UNITY_EDITOR
[CustomEditor(typeof(MockPlanet))]
[CanEditMultipleObjects]
public class MockPlanetEditor : Editor
{
    MockPlanet mPlanet;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector();

        if (GUILayout.Button("Randomize Seed"))
        {
            foreach (var obj in targets)
            {
                MockPlanet planet = (MockPlanet)obj;

                // Generate a unique seed per object
                int seed = System.DateTime.Now.Ticks.GetHashCode() ^ planet.transform.GetSiblingIndex();
                UnityEngine.Random.InitState(seed);

                // Apply random seed
                planet.seed = new Vector3(
                    UnityEngine.Random.Range(-100f, 100f),
                    UnityEngine.Random.Range(-100f, 100f) * 2,
                    UnityEngine.Random.Range(-100f, 100f) * 3
                );

                EditorUtility.SetDirty(planet);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    protected virtual void OnEnable()
    {
        mPlanet = (MockPlanet)target;
    }
}

#endif

