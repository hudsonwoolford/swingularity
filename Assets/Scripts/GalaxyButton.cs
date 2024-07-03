using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyButton : MonoBehaviour
{
    [SerializeField] Animator buttonAnimator;
    Collider buttonCol;
    Vector3 mouseCamPos;
    bool hover;
    // Start is called before the first frame update
    void Start()
    {
        buttonCol = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        mouseCamPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseCamPos.z = Camera.main.transform.position.z;

        if(hover != IsHovering())
        {
            switch (hover)
            {
                case false:
                    buttonAnimator.SetTrigger("Highlighted");
                    break;
                case true:
                    buttonAnimator.SetTrigger("Normal");
                    break;
            }
        }
        hover = IsHovering();
        if(Input.GetMouseButtonDown(0) && hover) {
            buttonAnimator.SetTrigger("Pressed");
        }
    }
    bool IsHovering()
    {
        RaycastHit[] hits = Physics.RaycastAll(mouseCamPos, Camera.main.transform.forward, 1000);
        if (hits.Length > 0)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider == buttonCol)
                {
                    return true;
                }
            }
        }
        return false;
    }
}

