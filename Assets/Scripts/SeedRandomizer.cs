using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedRandomizer : MonoBehaviour
{
    [ContextMenu("Randomize All Seeds")]
    void RandomizeAllSeeds()
    {
        MockPlanet[] mp = GetComponentsInChildren<MockPlanet>();

        for (int i = 0; i < mp.Length; i++)
        {
            UnityEngine.Random.InitState(System.DateTime.Now.Millisecond + i);
            mp[i].seed = new Vector3(UnityEngine.Random.Range(-100f, 100f), UnityEngine.Random.Range(-100f, 100f) * 2, UnityEngine.Random.Range(-100f, 100f) * 3);
        }
    }
}
