using UnityEngine;

public class OpenLevelScripts : MonoBehaviour
{
    public string sceneName;
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == this.gameObject)
            {
                Debug.Log("Clicked on: " + this.gameObject.name);
                LevelSelectGameManager.Instance.LoadScene(sceneName);

            }
        }
    }
}