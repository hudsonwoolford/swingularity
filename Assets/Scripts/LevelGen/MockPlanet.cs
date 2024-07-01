using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MockPlanet : MonoBehaviour
{
    [SerializeField] public ColorSchemeStaticReference.PlanetType planetType;
    [SerializeField] public bool specificSeed;
    [SerializeField] public Vector3 seed;
}
#if UNITY_EDITOR
[CustomEditor(typeof(MockPlanet))]
[CanEditMultipleObjects()]
public class MockPlanetEditor : Editor
{
    MockPlanet mPlanet;
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector();
        if(GUILayout.Button("Randomize Seed"))
        {
            UnityEngine.Random.InitState(Mathf.RoundToInt(Time.deltaTime * 1000000) + System.DateTime.Now.Millisecond * System.DateTime.Now.Second * (int)UnityEngine.Random.value * (System.DateTime.Now.Millisecond * 100000) + mPlanet.transform.GetSiblingIndex());
            mPlanet.seed = new Vector3(UnityEngine.Random.Range(-100f, 100f), UnityEngine.Random.Range(-100f, 100f) * 2, UnityEngine.Random.Range(-100f, 100f) * 3);
        }
        serializedObject.ApplyModifiedProperties();
    }
    protected virtual void OnEnable()
    {
        mPlanet = (MockPlanet)target;
    }
    private void OnDisable()
    {
        
    }
}
#endif

