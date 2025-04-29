using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    // [SerializeField] private AudioClip buttonClickSound;

    // public SoundMixerManager soundMixerManager;

    public void PlayGame()
    {
        SceneManager.LoadScene("LevelSelect");
    }

    // public void ButtonPressed()
    // {
    //     SoundFXManager.instance.PlayButtonClick(buttonClickSound, transform, 1f);
    // }

}
