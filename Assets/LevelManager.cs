using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public List<string> completedLevels { get; private set; }
    private void Awake()
    {
        completedLevels = new List<string>()
        {
        };
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        //print(completedLevels.Count);

    }
    void CompleteCurrentLevel()
    {
        string index = SceneManager.GetActiveScene().name;
        if(completedLevels.Contains(index)) { return; }
        completedLevels.Add(index);
    }
    private void OnEnable()
    {
        ShootBall.onHole += CompleteCurrentLevel;
    }
    private void OnDisable()
    {
        ShootBall.onHole -= CompleteCurrentLevel;
    }
}
