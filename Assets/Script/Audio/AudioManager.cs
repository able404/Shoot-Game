using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    int activeMusicSourceIndex;

    AudioSource[] musicSources;
    AudioSource sfxSource;
    Transform audioListener;
    Transform playerT;
    SoundLib library;

    public AudioMixer mainMixer;
    public AudioMixerGroup musicMixerGroup;
    public AudioMixerGroup sfxMixerGroup;

    void Awake()
    {
        if (instance!= null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            library = GetComponent<SoundLib>();

            // Music Source
            musicSources = new AudioSource[2];
            for (int i = 0; i < 2; i++)
            {
                GameObject newMusicSource = new GameObject("Music Source " + (i + 1));
                musicSources[i] = newMusicSource.AddComponent<AudioSource>();
                newMusicSource.transform.parent = transform;
                musicSources[i].outputAudioMixerGroup = musicMixerGroup;
                musicSources[i].loop = true;
            }

            // SFX Source
            GameObject newSfxSource = new GameObject("SFX Source");
            sfxSource = newSfxSource.AddComponent<AudioSource>();
            newSfxSource.transform.parent = transform;
            sfxSource.outputAudioMixerGroup = sfxMixerGroup;

            // Find player and listener
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                playerT = playerObject.transform;
            }
            audioListener = FindFirstObjectByType<AudioListener>()?.transform;
        }
    }

    void Update()
    {
        if (playerT != null && audioListener != null)
        {
            audioListener.position = playerT.position;
        }
    }

    public void PlayMusic(AudioClip clip, float fadeDuration = 1f)
    {
        activeMusicSourceIndex = 1 - activeMusicSourceIndex;
        musicSources[activeMusicSourceIndex].clip = clip;
        musicSources[activeMusicSourceIndex].Play();

        StartCoroutine(AnimateMusicCrossFade(fadeDuration));
    }

    public void PlaySound(AudioClip clip, Vector3 pos)
    {
        if (clip != null)
        {
            sfxSource.transform.position = pos;
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlaySound(string soundName, Vector3 pos)
    {
        PlaySound(library.GetClipFromName(soundName), pos);
    }

    public float GetVolume(string name)
    {
        if (mainMixer.GetFloat(name, out float value))
        {
            // 使用 Mathf.InverseLerp 将分贝值(-80 to 20)转换回线性值(0-1)
            return Mathf.InverseLerp(-80, 20, value);
        }
        return 0f;
    }

    public void SetMasterVolume(float value)
    {
        mainMixer.SetFloat("Master", Mathf.Lerp(-80, 20, value));
    }

    public void SetMusicVolume(float value)
    {
        mainMixer.SetFloat("Music", Mathf.Lerp(-80, 20, value));
    }

    public void SetSFXVolume(float value)
    {
        mainMixer.SetFloat("SFX", Mathf.Lerp(-80, 20, value));
    }

    IEnumerator AnimateMusicCrossFade(float duration)
    {
        float percent = 0f;

        while (percent < 1f)
        {
            percent += Time.deltaTime * 1f / duration;
            musicSources[activeMusicSourceIndex].volume = Mathf.Lerp(0f, 1f, percent);
            musicSources[1 - activeMusicSourceIndex].volume = Mathf.Lerp(1f, 0f, percent);
            yield return null;
        }
    }
}
