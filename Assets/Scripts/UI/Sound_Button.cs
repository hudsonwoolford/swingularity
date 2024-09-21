using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEditor;
using UnityEditor.UI;

public class Sound_Button : Button
{
    public AudioObj clickAudio;
    public AudioObj hoverAudio;
    public override void OnPointerClick(PointerEventData eventData)
    {
        //play button audio
        if(clickAudio != null && GameManager.SFXManager != null) {
            GameManager.SFXManager.PlayInterface(clickAudio);
        }
        Debug.Log("Button Click");

        base.OnPointerClick(eventData);
    }
    public override void OnPointerEnter(PointerEventData eventData)
    {
        //play button audio
        if (hoverAudio != null && GameManager.SFXManager != null)
        {
            GameManager.SFXManager.PlayInterface(hoverAudio);
        }
        Debug.Log("Button Hover");

        base.OnPointerEnter(eventData);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Sound_Button))]
public class Sound_ButtonEditor : ButtonEditor
{
    public override void OnInspectorGUI()
    {
        Sound_Button targetButton = (Sound_Button)target;

        targetButton.clickAudio = (AudioObj)EditorGUILayout.ObjectField("Click Audio", targetButton.clickAudio, typeof(AudioObj), true);
        targetButton.hoverAudio = (AudioObj)EditorGUILayout.ObjectField("Hover Audio", targetButton.hoverAudio, typeof(AudioObj), true);

        base.OnInspectorGUI();
    }
}
#endif
