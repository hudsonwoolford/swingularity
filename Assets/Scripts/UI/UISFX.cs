using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

public class UISFX : MonoBehaviour
{
    [SerializeField] AudioObj clickAudio;
    UnityAction<AudioObj> callAction;
    Dictionary<Button, AudioObj> buttonDict = new Dictionary<Button, AudioObj>();

    void SubscribeAllButtons()
    {
        List<Button> allButtons = GameObject.FindObjectsOfType<Button>().ToList();
        List<Button> newButtons = new List<Button>();

        for (int i = allButtons.Count - 1; i >= 0; i--)
        {
            CustomSoundButton csb;
            if (allButtons[i].gameObject.TryGetComponent(out csb))
            {
                if (csb.customSound != null)
                {
                    if(TryAddNew(allButtons[i], csb.customSound))
                    {
                        newButtons.Add(allButtons[i]);
                    }
                }
                continue;
            }

            if (TryAddNew(allButtons[i], clickAudio))
            {
                newButtons.Add(allButtons[i]);
            }
        }

        foreach (var b in newButtons)
        {
            b.onClick.AddListener(() => callAction.Invoke(buttonDict[b]));
        }

        bool TryAddNew(Button button, AudioObj obj)
        {
            if (buttonDict.ContainsKey(button))
            {
                return false;
            }
            buttonDict.Add(button, obj);
            return true;
        }
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SubscribeAllButtons();
    }
    private void OnEnable()
    {
        callAction = (a) => GameManager.SFXManager.PlaySFX(a);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

}
