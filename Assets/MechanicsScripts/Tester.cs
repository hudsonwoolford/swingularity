using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tester : MonoBehaviour
{
    [SerializeField] GameObject whiteArrow;
    [SerializeField] int width, height;
    [SerializeField] float cellSize;
    [SerializeField] Vector3 origin;
    [SerializeField] Rigidbody[] planets;
    [SerializeField] Rigidbody ball;
    [SerializeField] Transform parentTransform;
    [SerializeField] Gradient colorGrad;
    [Range(0.001f, 50)]
    [SerializeField] float colorDivisor;
    // Start is called before the first frame update
    void Start()
    {
        parentTransform.gameObject.SetActive(false);
        Grid grid = new Grid(width, height, cellSize, colorDivisor, origin, planets, ball, whiteArrow, parentTransform, colorGrad);
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.H))
        {
            parentTransform.gameObject.SetActive(!parentTransform.gameObject.activeInHierarchy);
        }
    }
}
