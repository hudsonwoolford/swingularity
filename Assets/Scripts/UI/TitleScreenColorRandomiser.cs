using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScreenColorRandomiser : MonoBehaviour
{
    [SerializeField] BlackHoleGenerator blackHole;
    [SerializeField] SpaceShipManager spaceShip;

    void Start(){
        UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
        int index = UnityEngine.Random.Range(0, 9);
        while(index == 6 || index == 5 || index == 8){
            index = UnityEngine.Random.Range(0, 9);
        }
        blackHole.RandomHole(index);
        spaceShip.RandomIndex(index);
    }
}
