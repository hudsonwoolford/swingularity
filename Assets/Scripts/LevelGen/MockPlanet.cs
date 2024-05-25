using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MockPlanet : MonoBehaviour
{
    public ColorSchemeStaticReference.PlanetType planetType;
    public bool specificSeed;
    public int seed;
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
            mPlanet.seed = UnityEngine.Random.Range(-2147483647, 2147483647);
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

