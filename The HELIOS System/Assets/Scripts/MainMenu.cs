using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public SoundMixerManager soundMixerManager;

    public void PlayGame()
    {
        SceneManager.LoadScene("Intro");
    }

}
