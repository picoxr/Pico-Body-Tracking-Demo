using UnityEngine;
using UnityEngine.UI;

namespace BodyTrackingDemo
{
    public class UIAudioSetting : MonoBehaviour
    {
        [SerializeField] private Toggle toggleMusic;

        private void Start()
        {
            toggleMusic.onValueChanged.AddListener(OnMusic);
        }

        private void OnMusic(bool value)
        {
            if (value)
            {
                AudioManager.Instance.PlayMusic();    
            }
            else
            {
                AudioManager.Instance.PauseMusic();
            }
            
        }
    }
}