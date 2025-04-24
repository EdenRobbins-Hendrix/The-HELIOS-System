using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TooltipSystem : MonoBehaviour
{
    public static TooltipSystem Instance { get; private set; }
    
    [SerializeField] private GameObject tooltipGameObject;
    [SerializeField] private TextMeshProUGUI tooltipText;
    [SerializeField] private RectTransform backgroundRectTransform;
    [SerializeField] private float padding = 10f;
    [SerializeField] private Canvas canvas;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        
        // Initially hide the tooltip
        tooltipGameObject.SetActive(false);
    }
    
    private void Update()
    {
        // Follow mouse position
        if (tooltipGameObject.activeSelf)
        {
            Vector2 mousePosition = Input.mousePosition;
            
            // Convert from screen to canvas space
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                transform.position = mousePosition;
            }
            else if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.transform as RectTransform, mousePosition, canvas.worldCamera, out localPoint);
                transform.position = canvas.transform.TransformPoint(localPoint);
            }
            
            // Adjust position to prevent tooltip from going off-screen
            LayoutRebuilder.ForceRebuildLayoutImmediate(backgroundRectTransform);
            Vector2 pivotOffset = new Vector2(backgroundRectTransform.pivot.x * backgroundRectTransform.rect.width, 
                                             backgroundRectTransform.pivot.y * backgroundRectTransform.rect.height);
            
            Vector2 tooltipSize = backgroundRectTransform.rect.size;
            
            Vector2 screenPosition = mousePosition + new Vector2(20, 20) - pivotOffset;
            
            if (screenPosition.x + tooltipSize.x > Screen.width)
            {
                screenPosition.x = mousePosition.x - tooltipSize.x - 20 - pivotOffset.x;
            }
            
            if (screenPosition.y + tooltipSize.y > Screen.height)
            {
                screenPosition.y = mousePosition.y - tooltipSize.y - 20 - pivotOffset.y;
            }
            
            transform.position = screenPosition;
        }
    }
    
    public void Show(string content)
    {
        tooltipGameObject.SetActive(true);
        tooltipText.text = content;
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(backgroundRectTransform);
    }
    
    public void Hide()
    {
        tooltipGameObject.SetActive(false);
    }
}