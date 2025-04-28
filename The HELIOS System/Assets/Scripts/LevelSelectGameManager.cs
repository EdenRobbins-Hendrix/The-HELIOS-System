using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectGameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public static LevelSelectGameManager Instance { get; private set; }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }



}
