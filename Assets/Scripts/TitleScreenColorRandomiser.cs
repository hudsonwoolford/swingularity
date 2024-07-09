using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScreenColorRandomiser : MonoBehaviour
{
    [SerializeField] BlackHoleGenerator blackHole;
    [SerializeField] ParticleSystem thrust1, thrust2, thrust3;
    [SerializeField] Material spaceShipEmission;

    void Start(){
        UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
        int index = UnityEngine.Random.Range(0, 9);
        while(index == 6){
            index = UnityEngine.Random.Range(0, 9);
        }
        blackHole.RandomHole(index);
    }
}
