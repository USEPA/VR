using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class TestOver : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool isOver = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"Mouse enter || Time: {Time.time}");
        isOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log($"Mouse exit || Time: {Time.time}");
        isOver = false;
    }
}