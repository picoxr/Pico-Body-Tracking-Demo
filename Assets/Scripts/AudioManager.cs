using System;
using System.Collections;
using UnityEngine;

namespace BodyTrackingDemo
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private AudioSource backgroundMusic;
        [SerializeField] private AudioSource vfxTemplate;
        [SerializeField] private AudioBank vfxBank;

        private SimpleObjectPool<AudioSource> _vfxPool;
        
        public AudioSource BackgroundMusic => backgroundMusic;

        public static AudioManager Instance { get; private set; }
        
        private void Awake()
        {
            Instance = this;
            _vfxPool = new SimpleObjectPool<AudioSource>(vfxTemplate);
        }

        private void Start()
        {
            if (PlayerPrefManager.Instance.PlayerPrefData.backgroundMusic)
            {
                PlayMusic();
            }
        }

        public void PlayEffect(AudioSource audioSource, Vector3 targetPos)
        {
            var sfx = Instantiate(audioSource, targetPos, Quaternion.identity);
            sfx.gameObject.SetActive(true);
            sfx.Play();
        }
        
        public void PlayEffect(AudioEffectID effectID)
        {
            foreach (var audioData in vfxBank.audioData)
            {
                if (audioData.id == effectID)
                {
                    var audioSource = _vfxPool.GetObject();
                    audioSource.clip = audioData.audioClip;
                    audioSource.volume = audioData.volume;
                    StartCoroutine(WaitForRecycle(audioSource));
                    break;
                }
            }
        }

        private IEnumerator WaitForRecycle(AudioSource audioSource)
        {
            yield return new WaitForSeconds(audioSource.clip.length);
            _vfxPool.ReturnObject(audioSource);
        }

        public void PlayMusic()
        {
            backgroundMusic.loop = true;
            backgroundMusic.Play();
        }
        
        public void PauseMusic()
        {
            backgroundMusic.Pause();
        }
    }

    [Serializable]
    public class AudioBank
    {
        public AudioData[] audioData;
    }
    
    [Serializable]
    public class AudioData
    {
        public AudioEffectID id;
        public AudioClip audioClip;
        public float volume;
    }
    
    public enum AudioEffectID : int
    {
        FootStepHeel,
        FootStepToe,
    }
}