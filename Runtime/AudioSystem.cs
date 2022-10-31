using System;
using System.Collections;
using System.Linq;
using LiteNinja.SOA.Variables;
using UnityEngine;
using UnityEngine.Audio;

namespace LiteNinja.Audio
{
  [AddComponentMenu("LiteNinja/Systems/Audio System")]
  public class AudioSystem : MonoBehaviour, IAudioSystem
  {
    [Header("Sound Effects")]
    [SerializeField] private FloatVar _sfxVolume;
    [SerializeField] private BoolVar _sfxEnabled;
    [SerializeField] private int _maxAudioSources = 10;
    [SerializeField, HideInInspector] private AudioSource[] fxAudioSources;
    
    [Header("Music")]
    [SerializeField] private FloatVar _musicVolume;
    [SerializeField] private BoolVar _musicEnabled;
    [SerializeField, HideInInspector] private AudioSource musicAudioSource;

    [Header("Mixer")]
    [SerializeField] private AudioMixerGroup musicAudioMixer;
    [SerializeField] private AudioMixerGroup fxAudioMixer;
    [SerializeField] private string _mixerMusicVolumeName = "MusicVolume";
    [SerializeField] private string _mixerSFXVolumeName = "FXVolume";

    

    private Coroutine _fadeoutCoroutine;


    private void Awake()
    {
      OnLoadSystem();
    }

    private void OnDestroy()
    {
      OnUnloadSystem();
    }

    public void SetFXVolume(float volume)
    {
      if (volume != _sfxVolume.Value)
      {
        _sfxVolume.Value = volume;
      }
      var volumeValue =  CalculateVolume(_sfxVolume.Value);
      fxAudioMixer.audioMixer.SetFloat(_mixerSFXVolumeName, volumeValue);
      SaveConfig();
    }

    public void SetMusicVolume(float volume)
    {
      if (volume != _musicVolume.Value)
      {
        _musicVolume.Value = volume;
      }
      var volumeValue = CalculateVolume(_musicVolume.Value);
      musicAudioMixer.audioMixer.SetFloat(_mixerMusicVolumeName, volumeValue);
      SaveConfig();
    }

    public void ToggleSound()
    {
      _sfxEnabled.Value = !_sfxEnabled.Value;

      foreach (var audioSource in fxAudioSources)
      {
        audioSource.mute = !_sfxEnabled.Value;
      }
      
      SaveConfig();
    }

    public void ToggleMusic()
    {
      _musicEnabled.Value = !_musicEnabled.Value;
      musicAudioSource.mute = !_musicEnabled.Value;
      SaveConfig();
    }
    
    public void TurnMusic(bool enabled)
    {
      if (enabled != _musicEnabled.Value)
      {
        _musicEnabled.Value = enabled;
      }
      musicAudioSource.mute = !_musicEnabled.Value;
      SaveConfig();
    }
    
    public void TurnSound(bool enabled)
    {
      if (enabled != _sfxEnabled.Value)
      {
        _sfxEnabled.Value = enabled;
      }
      foreach (var audioSource in fxAudioSources)
      {
        audioSource.mute = !_sfxEnabled.Value;
      }
      SaveConfig();
    }

    public void PlaySound(AudioClip clip)
    {
      //find an available fx audio source
      var source = GetAvailableFxAudioSource();
      source?.PlayOneShot(clip);
    }

    private AudioSource GetAvailableFxAudioSource()
    {
      return fxAudioSources.FirstOrDefault(t => !t.isPlaying && t.enabled);
    }

    public void PlayMusic(AudioClip clip)
    {
      if (musicAudioSource.clip == clip) return;

      if (_fadeoutCoroutine != null)
      {
        StopCoroutine(_fadeoutCoroutine);
      }

      //Fade out current music
      _fadeoutCoroutine = StartCoroutine(FadeOut(1f, () =>
      {
        musicAudioSource.Stop();
        //Play new music
        musicAudioSource.clip = clip;
        musicAudioSource.loop = true;
        if (musicAudioSource.enabled)
        {
          musicAudioSource.Play();
        }
      }));
    }

    private IEnumerator FadeOut(float duration, Action action)
    {
      var audioSource = musicAudioSource;
      var startVolume = _musicVolume.Value;
      if (audioSource.isPlaying && !audioSource.mute )
      {
        var currentVolume = startVolume;
        while (currentVolume > 0)
        {
          currentVolume -= startVolume * Time.deltaTime / duration;
          SetMusicVolume(currentVolume);
          yield return null;
        }

        audioSource.Stop();
      }

      SetMusicVolume(startVolume);
      action?.Invoke();
    }


    public void PlaySoundCollection(SoundCollection soundCollection)
    {
      if (soundCollection == null) return;
      var source = GetAvailableFxAudioSource();
      if (source) soundCollection.Play(source);
    }


    private void OnLoadSystem()
    {
      musicAudioSource = CreateAudioSource("Music Audio Source");
      musicAudioSource.outputAudioMixerGroup = musicAudioMixer;

      fxAudioSources = new AudioSource[_maxAudioSources];
      for (var i = 0; i < _maxAudioSources; i++)
      {
        fxAudioSources[i] = CreateAudioSource("FX Audio Source " + i);
        fxAudioSources[i].outputAudioMixerGroup = fxAudioMixer;
      }

      LoadConfig();
      _sfxVolume.OnValueChanged += SetFXVolume;
      _musicVolume.OnValueChanged += SetMusicVolume;
      _musicEnabled.OnValueChanged += TurnMusic;
      _sfxEnabled.OnValueChanged += TurnSound;

    }

    private void OnUnloadSystem()
    {
      SaveConfig();
      _sfxVolume.OnValueChanged -= SetFXVolume;
      _musicVolume.OnValueChanged -= SetMusicVolume;
      _musicEnabled.OnValueChanged -= TurnMusic;
      _sfxEnabled.OnValueChanged -= TurnSound;
    }


    private AudioSource CreateAudioSource(string name)
    {
      var newSource = new GameObject().AddComponent<AudioSource>();
      newSource.transform.SetParent(transform);
      newSource.name = name;
      return newSource;
    }

    private void LoadConfig()
    {
      _sfxVolume.Load();
      _musicVolume.Load();
      _sfxEnabled.Load();
      _musicEnabled.Load();


      //Should be load if the music and/or fx is enabled
      foreach (var source in fxAudioSources)
      {
        source.enabled = _sfxEnabled.Value;
      }

      musicAudioSource.enabled = _musicEnabled.Value;
      SetFXVolume(_sfxVolume.Value); 
      SetMusicVolume(_musicVolume.Value); 
      
      _sfxVolume.OnValueChanged += SetFXVolume;
      _musicVolume.OnValueChanged += SetMusicVolume;
      _musicEnabled.OnValueChanged += TurnMusic;
      _sfxEnabled.OnValueChanged += TurnSound;
      
    }
    
    private float CalculateVolume(float volume)
    {
      return Mathf.Log10(volume) * 20;
    }

    private void SaveConfig()
    {
      _sfxVolume.Save();
      _musicVolume.Save();
      _sfxEnabled.Save();
      _musicEnabled.Save();
    }
  }
}