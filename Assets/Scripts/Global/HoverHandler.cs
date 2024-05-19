using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] TitleScreenButtons titleScreenButtons;
    [SerializeField] GameObject selected;
    public void OnPointerEnter(PointerEventData eventData)
    {
        selected.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        selected.SetActive(false);
    }
}
