using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tester : MonoBehaviour
{
    [SerializeField] GameObject whiteArrow;
    [SerializeField] int width, height;
    [SerializeField] float cellSize;
    [SerializeField] Vector3 origin;
    [SerializeField] Gradient colorGrad;
    [Range(0.001f, 50)]
    [SerializeField] float colorDivisor;
    [SerializeField] Transform planetTransform;
    [SerializeField] Transform parentTransform;
    Rigidbody ball;
    List<Rigidbody> planets = new List<Rigidbody>();
    private void Awake()
    {
        int i = 0;
        foreach (Transform t in planetTransform)
        {
            planets.Add(t.gameObject.GetComponent<Rigidbody>());
            i++;
        }
        ball = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>();
    }
    // Start is called before the first frame update
    void Start()
    {
        parentTransform.gameObject.SetActive(false);
        Grid grid = new Grid(width, height, cellSize, colorDivisor, origin, planets.ToArray(), ball, whiteArrow, parentTransform, colorGrad);
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.H))
        {
            parentTransform.gameObject.SetActive(!parentTransform.gameObject.activeInHierarchy);
        }
    }
}
