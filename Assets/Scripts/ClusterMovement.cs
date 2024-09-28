using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClusterMovement : MonoBehaviour
{
    [SerializeField] float delay;
    [SerializeField] float scaleDuration;
    [SerializeField] AnimationCurve scaleCurve;
    Vector3 trueOrigin;
    // Start is called before the first frame update
    IEnumerator Start()
    {
        transform.localScale = Vector3.zero;
        yield return new WaitForSeconds(delay);

        float timer = 0;
        while(timer < scaleDuration)
        {
            MoveLoopLogic();

            timer += Time.deltaTime;
            yield return null;
        }

        timer = scaleDuration;
        MoveLoopLogic();

        void MoveLoopLogic()
        {
            transform.localScale = Vector3.Lerp(Vector3.zero    , Vector3.one, scaleCurve.Evaluate(timer / scaleDuration));
        }
    }

}
