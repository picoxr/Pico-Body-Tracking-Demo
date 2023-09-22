using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BodyTrackingDemo
{
    public class UIMinimap : MonoBehaviour
    {
        [SerializeField] private RawImage destImage;
        [SerializeField] private UIBrowser browser;

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => browser.Material != null);
            
            destImage.material = browser.Material;
            destImage.texture = browser.Material.mainTexture;
            Debug.Log($"UIMinimap.Start: material = {browser.Material}");
        }
    }
}