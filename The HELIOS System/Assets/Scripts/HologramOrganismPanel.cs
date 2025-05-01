using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class HologramOrganismPanel : MonoBehaviour
{
    [Header("Panel Settings")]
    [SerializeField] private RectTransform panelRectTransform;
    [SerializeField] private float collapsedPosX = 800f; // Match your window width
    [SerializeField] private float expandedPosX = 600f;  // Window width minus panel width
    [SerializeField] private float animationSpeed = 5f;
    
    [Header("Toggle Buttons")]
    [SerializeField] private Button expandButton;
    [SerializeField] private Button collapseButton;
    
    [Header("Content Settings")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject organismButtonPrefab;
    [SerializeField] private int columnCount = 2;
    [SerializeField] private float itemSpacing = 10f;
    [SerializeField] private float itemSize = 80f;
    
    [Header("Hologram Effects")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    [SerializeField] private AudioClip buttonClickSound;
    
    [Header("Organisms")]
    [SerializeField] private List<OrganismData> availableOrganisms = new List<OrganismData>();
    
    private bool isExpanded = false;
    private Vector2 targetPosition;
    private GameManager gameManager;
    private EnergyManager energyManager;
    private DragDropHandler dragDropHandler;
    private AudioSource audioSource;
    
    [System.Serializable]
    public class OrganismData
    {
        public string name;
        public Sprite icon;
        public GameObject prefab;
        public int energyCost;
        public string description;
        public bool isPlant;
    }
    
    private void Awake()
    {
        if (panelRectTransform == null)
            panelRectTransform = GetComponent<RectTransform>();
            
        audioSource = gameObject.AddComponent<AudioSource>();
        
        gameManager = GameManager.Instance;
        energyManager = FindObjectOfType<EnergyManager>();
        dragDropHandler = FindObjectOfType<DragDropHandler>();
        
        if (dragDropHandler == null)
        {
            Debug.LogError("DragDropHandler not found! Make sure it's in the scene.");
        }
        
        // Set initial position
        targetPosition = new Vector2(collapsedPosX, panelRectTransform.anchoredPosition.y);
        panelRectTransform.anchoredPosition = targetPosition;
        
        // Set up buttons
        if (expandButton != null)
            expandButton.onClick.AddListener(ExpandPanel);
            
        if (collapseButton != null)
            collapseButton.onClick.AddListener(CollapsePanel);
            
        // Initialize UI state
        UpdateButtonVisibility();
    }
    
    private void Start()
    {
        // Populate the panel with organisms
        PopulateOrganismPanel();
        
        // Default to expanded in development for easier testing
        #if UNITY_EDITOR
        ExpandPanel();
        #endif
    }
    
    private void Update()
    {
        // Animate panel position
        panelRectTransform.anchoredPosition = Vector2.Lerp(
            panelRectTransform.anchoredPosition, 
            targetPosition, 
            Time.unscaledDeltaTime * animationSpeed
        );
    }
    
    public void ExpandPanel()
    {
        isExpanded = true;
        targetPosition = new Vector2(expandedPosX, panelRectTransform.anchoredPosition.y);
        UpdateButtonVisibility();
        
        // Play sound effect
        if (openSound != null && audioSource != null)
        {
            audioSource.clip = openSound;
            audioSource.Play();
        }
        
        Debug.Log("Panel expanded");
    }
    
    public void CollapsePanel()
    {
        isExpanded = false;
        targetPosition = new Vector2(collapsedPosX, panelRectTransform.anchoredPosition.y);
        UpdateButtonVisibility();
        
        // Play sound effect
        if (closeSound != null && audioSource != null)
        {
            audioSource.clip = closeSound;
            audioSource.Play();
        }
        
        Debug.Log("Panel collapsed");
    }
    
    private void UpdateButtonVisibility()
    {
        if (expandButton != null)
            expandButton.gameObject.SetActive(!isExpanded);
            
        if (collapseButton != null)
            collapseButton.gameObject.SetActive(isExpanded);
    }
    
    private void PopulateOrganismPanel()
    {
        Debug.Log("Populating organism panel with " + availableOrganisms.Count + " organisms");
        
        // Clear existing content
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        
        // Add organism buttons
        for (int i = 0; i < availableOrganisms.Count; i++)
        {
            OrganismData data = availableOrganisms[i];
            
            // Calculate position (2 columns)
            int row = i / columnCount;
            int col = i % columnCount;
            
            GameObject buttonObj = Instantiate(organismButtonPrefab, contentParent);
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            
            // Position within grid
            float xPos = col * (itemSize + itemSpacing);
            float yPos = -row * (itemSize + itemSpacing);
            buttonRect.anchoredPosition = new Vector2(xPos, yPos);
            
            // Set up the button
            Button button = buttonObj.GetComponent<Button>();
            
            // Set the icon image (assuming the button has an IconImage child)
            Transform iconTransform = buttonObj.transform.Find("IconImage");
            if (iconTransform != null && data.icon != null)
            {
                Image iconImage = iconTransform.GetComponent<Image>();
                if (iconImage != null)
                {
                    iconImage.sprite = data.icon;
                    iconImage.color = Color.white;
                    iconImage.preserveAspect = true;
                }
            }
            
            // Set name and cost in child TextMeshPro components
            TextMeshProUGUI[] texts = buttonObj.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length > 0)
            {
                foreach (TextMeshProUGUI text in texts)
                {
                    if (text.gameObject.name.Contains("Name"))
                        text.text = data.name;
                    else if (text.gameObject.name.Contains("Cost"))
                        text.text = data.energyCost.ToString();
                }
            }
            
            // Store the organism data with the button
            HologramButtonData buttonData = buttonObj.AddComponent<HologramButtonData>();
            buttonData.data = data;
            
            // Add drag functionality
            EventTrigger trigger = buttonObj.AddComponent<EventTrigger>();
            
            // Add pointer down event
            EventTrigger.Entry pointerDown = new EventTrigger.Entry();
            pointerDown.eventID = EventTriggerType.PointerDown;
            
            OrganismData capturedData = data; // Capture for lambda
            pointerDown.callback.AddListener((eventData) => { 
                OnOrganismButtonDown(buttonData); 
            });
            
            trigger.triggers.Add(pointerDown);
            
            Debug.Log("Created button for " + data.name);
        }
        
        // Update content parent size
        int rowCount = (availableOrganisms.Count + columnCount - 1) / columnCount;
        float contentHeight = rowCount * (itemSize + itemSpacing) + itemSpacing;
        float contentWidth = columnCount * (itemSize + itemSpacing) + itemSpacing;
        
        RectTransform contentRect = contentParent.GetComponent<RectTransform>();
        if (contentRect != null)
        {
            contentRect.sizeDelta = new Vector2(contentWidth, contentHeight);
        }
    }
    
    private void OnOrganismButtonDown(HologramButtonData buttonData)
    {
        Debug.Log("Button clicked: " + buttonData.data.name);
        
        // Play button click sound
        if (buttonClickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
        
        // Check if player has enough energy
        if (energyManager != null && gameManager != null)
        {
            if (gameManager.energy >= buttonData.data.energyCost)
            {
                // Create a draggable preview
                CreateDraggablePreview(buttonData.data);
            }
            else
            {
                // Not enough energy - show feedback
                StartCoroutine(ShowNotEnoughEnergyFeedback(buttonData.gameObject));
            }
        }
    }
    
    private void CreateDraggablePreview(OrganismData data)
    {
        // Create a preview object that follows the mouse
        GameObject previewObj = new GameObject("DragPreview");
        previewObj.transform.SetParent(transform.root); // Parent to canvas
        
        // Add UI components
        RectTransform rectTransform = previewObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(itemSize, itemSize);
        
        // Add Image for sprite
        Image image = previewObj.AddComponent<Image>();
        image.sprite = data.icon;
        image.raycastTarget = false;
        image.preserveAspect = true;
        
        // Position at mouse
        previewObj.transform.position = Input.mousePosition;
        
        // Create dragged data for DragDropHandler
        DragDropHandler.DraggedOrganismData dragData = new DragDropHandler.DraggedOrganismData
        {
            name = data.name,
            icon = data.icon,
            prefab = data.prefab,
            energyCost = data.energyCost,
            description = data.description,
            isPlant = data.isPlant
        };
        
        // Tell the DragDropHandler about this dragged object
        if (dragDropHandler != null)
        {
            dragDropHandler.StartDragging(previewObj, dragData);
        }
        else
        {
            Debug.LogError("DragDropHandler not found!");
        }
    }
    
    private IEnumerator ShowNotEnoughEnergyFeedback(GameObject button)
    {
        // Flash the button red
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            Color originalColor = buttonImage.color;
            
            buttonImage.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            buttonImage.color = originalColor;
        }
        
        // Play error sound
        if (audioSource != null)
        {
            // Play a simple beep sound
            audioSource.pitch = 0.5f;
            audioSource.PlayOneShot(buttonClickSound);
            audioSource.pitch = 1f;
        }
    }
}

// Helper component to store organism data with buttons
public class HologramButtonData : MonoBehaviour
{
    public HologramOrganismPanel.OrganismData data;
}