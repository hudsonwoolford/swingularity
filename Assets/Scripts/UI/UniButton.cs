using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Events;

public class UniButton : MonoBehaviour
{
    public bool _IsLocked => isLocked;

    [SerializeField] private bool unlockedByInfinite;
    [SerializeField] private GameObject lockVisual;
    [SerializeField] private Animator lockAnimator;
    [SerializeField] private AudioObj lockAudio;
    private bool isLocked;
    private Button uniButton;
    UnityAction lockAction;
    void Start()
    {
        uniButton = GetComponent<Button>();
        lockAction = () => LockError();
        UpdateLock();
    }

    void Update()
    {
        if(Input.anyKey)
        {
            UpdateLock();
        }
    }
    void LockError()
    {
        GameManager.SFXManager.PlaySFX(lockAudio);
        lockAnimator.Play("LockShake");
    }
    private void UpdateLock()
    {
        isLocked = false;
        lockVisual.SetActive(false);
        if (SaveLoadManager.currentGlobalSave != null && unlockedByInfinite)
        {
            if(SaveLoadManager.currentGlobalSave.unlockedInfinite)
            {
                isLocked = false;
                lockVisual.SetActive(false);
            }
            else
            {
                isLocked = true;
                lockVisual.SetActive(true);
            }
        }

        //subscribe button
        if(isLocked)
        {
            uniButton.transition = Selectable.Transition.None;
            uniButton.onClick.RemoveListener(lockAction);
            uniButton.onClick.AddListener(lockAction);
        }
        else
        {
            uniButton.transition = Selectable.Transition.Animation;
            uniButton.onClick.RemoveListener(lockAction);
        }
    }
}
