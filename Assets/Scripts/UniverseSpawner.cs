using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode()]
public class UniverseSpawner : MonoBehaviour
{
    [SerializeField] GameObject universeBall;
    [SerializeField] uint quantity;
    [SerializeField] Vector3 spawnSize;

    [ContextMenu("Spawn Balls")]
    void SpawnBalls()
    {
        for (int i = 0; i < quantity; i++) 
        {
            Transform spawnedBall = Instantiate(universeBall, transform, true).transform;
            UnityEngine.Random.InitState(i * System.DateTime.Now.Millisecond + i);
            float[] randValue = new float[] { UnityEngine.Random.value * 2 - 1, UnityEngine.Random.value * 2 - 1, UnityEngine.Random.value * 2 - 1 };
            spawnedBall.position = transform.position + new Vector3(randValue[0] * spawnSize.x, randValue[1] * spawnSize.y, randValue[2] * spawnSize.z)/2;
            spawnedBall.GetComponent<UniverseGenerator>().Init(i * System.DateTime.Now.Millisecond + i);
        }
    }
    [ContextMenu("Clear Balls")]
    void ClearBalls()
    {
        GameObject[] destroyList = new GameObject[transform.childCount];
        for(int i = 0;i < transform.childCount;i++) 
        {
            destroyList[i] = transform.GetChild(i).gameObject;
        }
        for(int i = 0;i<destroyList.Length;i++)
        {
            DestroyImmediate(destroyList[i]);
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.2f);
        Gizmos.DrawCube(transform.position, spawnSize);
    }
}
