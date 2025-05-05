using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragDropHandler : MonoBehaviour
{
    [Header("Visual Feedback")]
    [SerializeField] private GameObject placementIndicatorPrefab;
    [SerializeField] private Color validPlacementColor = Color.green;
    [SerializeField] private Color invalidPlacementColor = Color.red;
    
    // Define game area boundaries
    [Header("Game Area")]
    [SerializeField] private float minX = -8f;
    [SerializeField] private float maxX = 8f;
    [SerializeField] private float minY = -4f;
    [SerializeField] private float maxY = 4f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip placementSuccessSound;
    [SerializeField] private AudioClip placementFailSound;
    
    private GameObject currentDraggedObject = null;
    private GameObject placementIndicator = null;
    private Camera mainCamera;
    private bool canPlaceObject = false;
    private AudioSource audioSource;
    
    // Data class to store organism data during dragging
    [System.Serializable]
    public class DraggedOrganismData
    {
        public string name;
        public Sprite icon;
        public GameObject prefab;
        public int energyCost;
        public string description;
        public bool isPlant;
    }
    
    private DraggedOrganismData currentDraggedData = null;
    
    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }
    
    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("No main camera found! DragDropHandler requires a camera.");
        }
        
        // Create placement indicator if prefab is assigned
        if (placementIndicatorPrefab != null)
        {
            placementIndicator = Instantiate(placementIndicatorPrefab);
            placementIndicator.SetActive(false);
        }
        else
        {
            Debug.LogWarning("No placement indicator prefab assigned. Using a default.");
            CreateDefaultPlacementIndicator();
        }
    }
    
    private void CreateDefaultPlacementIndicator()
    {
        placementIndicator = new GameObject("PlacementIndicator");
        SpriteRenderer renderer = placementIndicator.AddComponent<SpriteRenderer>();
        
        // Create a simple circle sprite if none is available
        renderer.sprite = CreateCircleSprite();
        renderer.color = validPlacementColor;
        
        placementIndicator.SetActive(false);
    }
    
    private Sprite CreateCircleSprite()
    {
        // Create a simple circle texture
        int resolution = 64;
        Texture2D texture = new Texture2D(resolution, resolution);
        
        // Calculate center and radius
        Vector2 center = new Vector2(resolution / 2, resolution / 2);
        float radius = resolution / 2 - 2;
        float radiusSquared = radius * radius;
        
        // Fill the texture with transparent pixels
        Color[] colors = new Color[resolution * resolution];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.clear;
        }
        texture.SetPixels(colors);
        
        // Draw ring
        float ringThickness = 3f;
        float innerRadiusSquared = (radius - ringThickness) * (radius - ringThickness);
        
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float dx = x - center.x;
                float dy = y - center.y;
                float distSquared = dx * dx + dy * dy;
                
                // If pixel is within ring bounds
                if (distSquared <= radiusSquared && distSquared >= innerRadiusSquared)
                {
                    texture.SetPixel(x, y, Color.white);
                }
            }
        }
        
        texture.Apply();
        
        // Create sprite from texture
        return Sprite.Create(texture, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f));
    }
    
    private void Update()
    {
        // If an object is being dragged, update its position
        if (currentDraggedObject != null)
        {
            UpdateDraggedObjectPosition();
            
            // On mouse button release, attempt to place the object
            if (Input.GetMouseButtonUp(0))
            {
                PlaceObject();
            }
        }
    }
    
    // Called by HologramOrganismPanel when starting to drag an organism
    public void StartDragging(GameObject dragObject, DraggedOrganismData data)
    {
        // Store reference to the dragged object and its data
        currentDraggedObject = dragObject;
        currentDraggedData = data;
        
        // Show the placement indicator
        if (placementIndicator != null)
        {
            placementIndicator.SetActive(true);
        }
        
        Debug.Log("Started dragging: " + data.name);
    }
    
    private void UpdateDraggedObjectPosition()
    {
        // Update the screen position to follow the mouse
        Vector3 mouseScreenPos = Input.mousePosition;
        if (currentDraggedObject != null)
        {
            currentDraggedObject.transform.position = mouseScreenPos;
        }
        
        // Convert screen position to world position
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
        worldPos.z = 0; // Set Z to 0 for 2D games
        
        // Check if mouse is over UI
        bool isOverUI = EventSystem.current.IsPointerOverGameObject();
        
        // Position the placement indicator 
        if (placementIndicator != null)
        {
            placementIndicator.transform.position = worldPos;
            
            // Check if the position is valid for placement
            canPlaceObject = !isOverUI && IsValidPlacementPosition(worldPos);
            
            // Update indicator color based on placement validity
            UpdatePlacementIndicatorColor(canPlaceObject);
        }
    }
    
    private void PlaceObject()
    {
        // Ensure we have a dragged object
        if (currentDraggedObject == null || currentDraggedData == null) 
        {
            Debug.LogWarning("Tried to place object but dragged object or data is null");
            return;
        }
        
        if (canPlaceObject)
        {
            // Get placement position from mouse world position
            Vector3 placementPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            placementPosition.z = 0; // Ensure Z is 0 for 2D
            
            // Try to spend energy
            EnergyManager energyManager = FindObjectOfType<EnergyManager>();
            if (energyManager != null)
            {
                bool success = energyManager.SpendEnergy(currentDraggedData.energyCost);
                
                if (success)
                {
                    // Instantiate the actual organism
                    GameObject newOrganism = Instantiate(currentDraggedData.prefab, placementPosition, Quaternion.identity);
                    
                    // Add Tooltip component for hover information if it doesn't already have one
                    Tooltip tooltipComponent = newOrganism.GetComponent<Tooltip>();
                    if (tooltipComponent == null)
                    {
                        tooltipComponent = newOrganism.AddComponent<Tooltip>();
                        Debug.Log("Added Tooltip component to " + newOrganism.name);
                    }
                    
                    // Make sure it has a collider for mouse interaction
                    Collider2D collider = newOrganism.GetComponent<Collider2D>();
                    if (collider == null)
                    {
                        // Add a CircleCollider2D by default
                        CircleCollider2D circleCollider = newOrganism.AddComponent<CircleCollider2D>();
                        
                        // Set a reasonable radius based on the sprite size
                        SpriteRenderer spriteRenderer = newOrganism.GetComponent<SpriteRenderer>();
                        if (spriteRenderer != null && spriteRenderer.sprite != null)
                        {
                            // Get bounds of sprite and use half the average of width and height
                            float width = spriteRenderer.bounds.size.x;
                            float height = spriteRenderer.bounds.size.y;
                            float radius = (width + height) / 4.0f; // Average / 2
                            
                            // Set the collider radius, with a minimum size
                            circleCollider.radius = Mathf.Max(radius, 0.5f);
                        }
                        else
                        {
                            // Default radius if no sprite renderer
                            circleCollider.radius = 0.5f;
                        }
                        
                        // Make sure this collider is not a trigger
                        circleCollider.isTrigger = false;
                        
                        Debug.Log("Added CircleCollider2D to " + newOrganism.name + " for tooltip interaction");
                    }
                    else if (collider.isTrigger)
                    {
                        // If there's already a collider but it's a trigger, add a non-trigger collider for mouse events
                        // Note: OnMouseEnter/Exit only work with non-trigger colliders
                        CircleCollider2D mouseCollider = newOrganism.AddComponent<CircleCollider2D>();
                        mouseCollider.radius = 0.5f;
                        mouseCollider.isTrigger = false;
                        Debug.Log("Added additional non-trigger collider to " + newOrganism.name + " for tooltip interaction");
                    }
                    
                    // Add to GameManager's tracking
                    GameManager gameManager = GameManager.Instance;
                    if (gameManager != null)
                    {
                        string organismName = currentDraggedData.name;
                        
                        if (!gameManager.organisms.ContainsKey(organismName))
                        {
                            gameManager.organisms[organismName] = new List<GameObject>();
                        }
                        
                        gameManager.organisms[organismName].Add(newOrganism);
                        
                        // If it's a plant, add to the plants list
                        if (currentDraggedData.isPlant)
                        {
                            gameManager.plants.Add(newOrganism);
                        }
                        
                        Debug.Log("Successfully placed " + organismName);
                        
                        // Play success sound
                        if (placementSuccessSound != null && audioSource != null)
                        {
                            audioSource.PlayOneShot(placementSuccessSound);
                        }
                    }
                }
                else
                {
                    Debug.Log("Not enough energy to place organism");
                    
                    // Play fail sound
                    if (placementFailSound != null && audioSource != null)
                    {
                        audioSource.PlayOneShot(placementFailSound);
                    }
                }
            }
        }
        else
        {
            // Play fail sound for invalid placement
            if (placementFailSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(placementFailSound);
            }
        }
        
        // Clean up - destroy the preview and hide the indicator
        if (placementIndicator != null)
        {
            placementIndicator.SetActive(false);
        }
        
        Destroy(currentDraggedObject);
        currentDraggedObject = null;
        currentDraggedData = null;
    }
    
    private bool IsValidPlacementPosition(Vector3 position)
    {
        // Check if position is within game boundaries
        if (position.x < minX || position.x > maxX || 
            position.y < minY || position.y > maxY)
        {
            return false;
        }
        
        // Check if there are any obstacles at this position
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, 0.5f);
        
        foreach (Collider2D collider in colliders)
        {
            // Skip triggers
            if (collider.isTrigger)
                continue;
                
            // Skip the placement indicator itself
            if (placementIndicator != null && collider.gameObject == placementIndicator)
                continue;
                
            // Found an obstacle, can't place here
            return false;
        }
        
        // No obstacles found and within boundaries
        return true;
    }
    
    private void UpdatePlacementIndicatorColor(bool isValid)
    {
        if (placementIndicator == null) return;
        
        // Update the color based on placement validity
        SpriteRenderer renderer = placementIndicator.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = isValid ? validPlacementColor : invalidPlacementColor;
        }
    }
}