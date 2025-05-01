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
    
    private GameObject currentDraggedObject = null;
    private GameObject placementIndicator = null;
    private Camera mainCamera;
    private bool canPlaceObject = false;
    
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
    
    private void Start()
    {
        mainCamera = Camera.main;
        
        // Create placement indicator if prefab is assigned
        if (placementIndicatorPrefab != null)
        {
            placementIndicator = Instantiate(placementIndicatorPrefab);
            placementIndicator.SetActive(false);
        }
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
        currentDraggedObject.transform.position = mouseScreenPos;
        
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
        if (currentDraggedObject == null || currentDraggedData == null) return;
        
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
                    }
                }
                else
                {
                    Debug.Log("Not enough energy to place organism");
                }
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