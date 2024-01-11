using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntersceneManager : MonoBehaviour
{
    public static IntersceneManager Instance;
    int[] possibleInp = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
    // Start is called before the first frame update
    void Start()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update()
    {
        foreach (int i in possibleInp)
        {
            if(Input.inputString == i.ToString())
            {
                if (SceneManager.sceneCountInBuildSettings <= i)
                {
                    print("No scene at index: " + i);
                    break;
                }
                else
                {
                    SceneManager.LoadScene(i); break;   
                }
            }
        }
    }
}
