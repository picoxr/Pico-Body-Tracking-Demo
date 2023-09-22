using UnityEngine;
using UnityEngine.UI;

namespace BodyTrackingDemo
{
    public class UIAudioSetting : MonoBehaviour
    {
        [SerializeField] private Toggle toggleMusic;
        [SerializeField] private AudioSource music;
        
        
        private void Start()
        {
            toggleMusic.onValueChanged.AddListener(OnMusic);
        }

        private void OnMusic(bool arg0)
        {
            if (arg0)
            {
                music.Play();    
            }
            else
            {
                music.Pause();
            }
            
        }
    }
}