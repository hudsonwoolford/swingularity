using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomizeElement : MonoBehaviour
{
    [System.Serializable]
    public struct MaterialCustomizationElement
    { 
        public int requiredSupernovae;
        public Color matColor;
        [Range(0f, 1f)]
        public float metallic;
        [Range(0f, 1f)]
        public float smoothness;
    }

    public MaterialCustomizationElement matElement;
    [Header("Element References")]
    [SerializeField] Material[] whiteMaterials;
    [SerializeField] TextMeshProUGUI requiredSupernovaeTxt;
    [SerializeField] Button equipButton;
    [SerializeField] MeshRenderer materialElementRenderer;
    Animator ceAnimator;
    MaterialPropertyBlock mpb;

    public delegate void SetAstronautCustomization(MaterialCustomizationElement materialCustomization);
    public static SetAstronautCustomization setAstronautCustomization;
    public void InitElement()
    {
        UpdateUIElements();
    }
    public void AnimateMaterialBalls(bool enable)
    {
        if(enable) { ceAnimator.Play("ShowBall"); }
        else { ceAnimator.Play("HideBall"); }
    }
    void UpdateUIElements()
    {
        string cost = matElement.requiredSupernovae.ToString("00000");
        requiredSupernovaeTxt.text = cost;
        if(SaveLoadManager.currentGlobalSave!=null)
        {
            equipButton.interactable = SaveLoadManager.currentGlobalSave.totalSupernovae >= matElement.requiredSupernovae;
        }
        if(mpb == null) { mpb = new MaterialPropertyBlock(); }
        mpb.SetColor("_Color", matElement.matColor);
        mpb.SetFloat("_Metallic", matElement.metallic);
        mpb.SetFloat("_Glossiness", matElement.metallic);
        materialElementRenderer.SetPropertyBlock(mpb);
    }
    // Start is called before the first frame update
    void Start()
    {
        mpb = new MaterialPropertyBlock();
        ceAnimator = GetComponent<Animator>();
        equipButton.onClick.AddListener(() => setAstronautCustomization(matElement));
    }
    private void OnValidate()
    {
        UpdateUIElements();
    }
    void SetAstronautMaterials(MaterialCustomizationElement mce)
    {

    }
    private void OnEnable()
    {
        setAstronautCustomization += SetAstronautMaterials;
    }
    private void OnDisable()
    {
        setAstronautCustomization -= SetAstronautMaterials;
    }
}
