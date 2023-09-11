using UnityEngine;

namespace BodyTrackingDemo
{
    public class AudioManager : MonoBehaviour
    {
        public void PlayEffect(AudioEffectID effectID)
        {
            
        }

        public void PlayMusic()
        {
            
        }
    }

    public class AudioData
    {
        public AudioEffectID ID;
    }
    
    public enum AudioEffectID : int
    {
        FootStepHeel,
        FootStepToe,
    }
}