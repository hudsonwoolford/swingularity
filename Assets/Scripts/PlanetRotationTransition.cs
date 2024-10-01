using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetRotationTransition : MonoBehaviour
{
    [SerializeField] AnimationCurve transitionCurve;
    [SerializeField] float rotAmount;
    [SerializeField] float transitionDuration;
    [SerializeField] KeyCode playTransition;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(playTransition))
        {
            StartCoroutine(RotationTransition());
        }
    }
    IEnumerator RotationTransition()
    {
        float startRot = transform.eulerAngles.z;

        float timer = 0;
        while (timer < transitionDuration)
        {
            WhileLoopLogic();
            timer += Time.deltaTime;
            yield return null;
        }

        timer = transitionDuration;
        WhileLoopLogic();

        void WhileLoopLogic()
        {
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, startRot + Mathf.Lerp(0, rotAmount, transitionCurve.Evaluate(timer / transitionDuration)));

        }
    }
}
