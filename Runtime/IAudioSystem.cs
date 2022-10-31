using UnityEngine;

namespace LiteNinja.Audio
{
  public interface IAudioSystem
  {
    void SetFXVolume(float volume);
    void SetMusicVolume(float volume);
    void ToggleSound();
    void ToggleMusic();
    void PlaySound(AudioClip clip);
    void PlayMusic(AudioClip clip);
    void PlaySoundCollection(SoundCollection soundCollection);
  }
}