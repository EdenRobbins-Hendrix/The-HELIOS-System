using UnityEngine;
using UnityEngine.UI;

public class ScanLineEffect : MonoBehaviour
{
    public float scanSpeed = 2.0f;
    public float minPosition = 0.0f;
    public float maxPosition = 1.0f;
    
    private RectTransform rectTransform;
    
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    
    void Update()
    {
        if (rectTransform == null) return;
        
        // Calculate position
        float normalizedPos = Mathf.PingPong(Time.time * scanSpeed, 1.0f);
        float yPos = Mathf.Lerp(minPosition, maxPosition, normalizedPos);
        
        // Update anchor position
        Vector2 anchorPos = rectTransform.anchoredPosition;
        anchorPos.y = yPos * rectTransform.parent.GetComponent<RectTransform>().rect.height - (rectTransform.parent.GetComponent<RectTransform>().rect.height / 2);
        rectTransform.anchoredPosition = anchorPos;
    }
}