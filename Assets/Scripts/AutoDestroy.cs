using System.Collections;
using UnityEngine;

namespace BodyTrackingDemo
{
    public class AutoDestroy : MonoBehaviour
    {
        public float duration = 1.6f;
        
        private IEnumerator Start()
        {
            yield return new WaitForSeconds(duration);
            
            Destroy(gameObject);
        }
    }
}