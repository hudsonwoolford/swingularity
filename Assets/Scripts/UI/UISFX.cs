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
    UnityAction<AudioObj> callAction;
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
        Dictionary<Button, AudioObj> buttonDict = new Dictionary<Button, AudioObj>();

        for (int i = allButtons.Count-1; i >= 0; i--)
        {
            //no sound / custom sound button
            CustomSoundButton csb;
            if (allButtons[i].gameObject.TryGetComponent<CustomSoundButton>(out csb))
            {
                //if it has a cutsom sound
                if(csb.customSound != null)
                {
                    buttonDict.Add(allButtons[i], csb.customSound);
                    continue;
                }

                //no sound
                allButtons.RemoveAt(i);
                continue;
            }

            buttonDict.Add(allButtons[i], clickAudio);
        }

        foreach (var b in buttonDict.Keys)
        {
            b.onClick.RemoveListener(() => callAction.Invoke(buttonDict[b]));
            b.onClick.AddListener(() => callAction.Invoke(buttonDict[b]));
        }
    }

    private void OnEnable()
    {
        callAction = (a) => GameManager.SFXManager.PlaySFX(a);
        SceneManager.sceneLoaded += (a , b) => SubscribeAllButtons();
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= (a, b) => SubscribeAllButtons();
    }
}
