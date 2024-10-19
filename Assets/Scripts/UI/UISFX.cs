using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

public class UISFX : MonoBehaviour
{
    [SerializeField] AudioObj clickAudio;
    UnityAction callAction = null;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void SubscribeAllButtons()
    {
        List<Button> allButtons = GameObject.FindObjectsOfType<Button>().ToList();

        for (int i = allButtons.Count-1; i >= 0; i--)
        {
            if (allButtons[i].gameObject.GetComponent<NoSoundButton>()!=null)
            {
                allButtons.RemoveAt(i);
            }
        }

        foreach (Button button in allButtons)
        {
            button.onClick.RemoveListener(callAction);
            button.onClick.AddListener(callAction);
        }
    }

    private void OnEnable()
    {
        callAction = () => GameManager.SFXManager.PlaySFX(clickAudio);
        SceneManager.sceneLoaded += (a , b) => SubscribeAllButtons();
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= (a, b) => SubscribeAllButtons();
    }
}
