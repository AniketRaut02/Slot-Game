using UnityEngine;


public class MainMenuAudioManager : MonoBehaviour
{
    [Header("AudioClips")]
    public AudioClip click_Clip;
    public AudioClip hover_Clip;
    public AudioClip bgm_Clip;

    [Header("AudioSources")]
    public AudioSource sfxSource;
    public AudioSource bgmSource;

    void Start()
    {
        bgmSource.clip = bgm_Clip;
        bgmSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void PlayMenuClick()
    {
        PlayOneShot(click_Clip);
    }

    public void PlayMenuHover()
    {
        PlayOneShot(hover_Clip);
    }
    private void PlayOneShot(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}
