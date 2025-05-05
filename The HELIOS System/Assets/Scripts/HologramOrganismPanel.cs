using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    
    [Header("TextMeshPro Settings")]
    [SerializeField] private TMP_FontAsset defaultFont;
    
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
        
        // If default font not assigned, try to load it
        if (defaultFont == null)
        {
            defaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (defaultFont == null)
            {
                Debug.LogWarning("Could not find default TMP font asset. Please assign it manually.");
            }
        }
        
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
        // Clean up any leftover effects before populating
        CleanupResources();
        
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
    
    private void OnDisable()
    {
        // Make sure everything is properly cleaned up
        CleanupResources();
    }

    private void OnDestroy()
    {
        // Make doubly sure everything is cleaned up when the panel is destroyed
        CleanupResources();
    }

    private void CleanupResources()
    {
        // Clean up any objects created at runtime that might not get automatically destroyed
        
        // Find all child objects with specific names that might cause issues
        Transform[] scanLines = GetComponentsInChildren<Transform>().Where(t => t.name.Contains("ScanLine")).ToArray();
        foreach (Transform line in scanLines)
        {
            if (line != null && line.gameObject != null)
            {
                Debug.Log("Cleaning up scan line: " + line.name);
                Destroy(line.gameObject);
            }
        }
        
        // Find all particle systems
        Transform[] particles = GetComponentsInChildren<Transform>().Where(t => t.name.Contains("GlowParticles")).ToArray();
        foreach (Transform p in particles)
        {
            if (p != null && p.gameObject != null)
            {
                Debug.Log("Cleaning up particles: " + p.name);
                Destroy(p.gameObject);
            }
        }
        
        // Also look for any orphaned elements in the scene with our name
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            // Check for scan lines or particles that might be related to this panel
            if ((obj.name.Contains("ScanLine") || obj.name.Contains("GlowParticles")) && 
                obj.name.Contains(gameObject.name))
            {
                Debug.Log("Cleaning up orphaned effect: " + obj.name);
                Destroy(obj);
            }
        }
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
            
            // Create button from prefab
            GameObject buttonObj = Instantiate(organismButtonPrefab, contentParent);
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            
            // Position within grid
            float xPos = col * (itemSize + itemSpacing);
            float yPos = -row * (itemSize + itemSpacing);
            buttonRect.anchoredPosition = new Vector2(xPos, yPos);
            
            // Set up the button
            Button button = buttonObj.GetComponent<Button>();
            
            Debug.Log("Setting up button for: " + data.name);
            
            // Find and set up the icon image
            Transform iconTransform = buttonObj.transform.Find("IconImage");
            if (iconTransform != null && data.icon != null)
            {
                Image iconImage = iconTransform.GetComponent<Image>();
                if (iconImage != null)
                {
                    iconImage.sprite = data.icon;
                    iconImage.color = Color.white;
                    iconImage.preserveAspect = true;
                    Debug.Log("Icon image set up for: " + data.name);
                }
            }
            else
            {
                Debug.LogWarning("IconImage not found in button prefab");
            }
            
            // Start a coroutine to set up the TextMeshPro components safely
            StartCoroutine(SafelySetupTextMeshPro(
                buttonObj, 
                data.name, 
                data.energyCost.ToString()
            ));
            
            // Add organism data to button
            HologramButtonData buttonData = buttonObj.AddComponent<HologramButtonData>();
            buttonData.data = data;
            
            // Add drag functionality
            EventTrigger trigger = buttonObj.AddComponent<EventTrigger>();
            EventTrigger.Entry pointerDown = new EventTrigger.Entry();
            pointerDown.eventID = EventTriggerType.PointerDown;
            pointerDown.callback.AddListener((eventData) => { 
                OnOrganismButtonDown(buttonData); 
            });
            trigger.triggers.Add(pointerDown);
            
            Debug.Log("Button setup complete for: " + data.name);
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
    
    private IEnumerator SafelySetupTextMeshPro(GameObject buttonObj, string nameString, string costString)
    {
        // Wait for a frame to ensure TextMeshPro components are initialized
        yield return null;
        
        // Set up the name text
        Transform nameTextTransform = buttonObj.transform.Find("NameText");
        if (nameTextTransform != null)
        {
            TextMeshProUGUI nameText = nameTextTransform.GetComponent<TextMeshProUGUI>();
            if (nameText != null)
            {
                // Ensure font asset is assigned
                if (nameText.font == null && defaultFont != null)
                {
                    nameText.font = defaultFont;
                }
                
                // Set text (with fallback)
                nameText.text = string.IsNullOrEmpty(nameString) ? "Name" : nameString;
                Debug.Log("Name text set up: " + nameText.text);
            }
        }
        
        // Set up the cost text
        Transform costContainer = buttonObj.transform.Find("CostContainer");
        if (costContainer != null)
        {
            Transform costTextTransform = costContainer.Find("CostText");
            if (costTextTransform != null)
            {
                TextMeshProUGUI costText = costTextTransform.GetComponent<TextMeshProUGUI>();
                if (costText != null)
                {
                    // Ensure font asset is assigned
                    if (costText.font == null && defaultFont != null)
                    {
                        costText.font = defaultFont;
                    }
                    
                    // Set text
                    costText.text = costString;
                    Debug.Log("Cost text set up: " + costText.text);
                }
            }
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
        if (audioSource != null && buttonClickSound != null)
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