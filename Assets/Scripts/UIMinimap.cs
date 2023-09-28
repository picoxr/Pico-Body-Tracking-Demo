using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BodyTrackingDemo
{
    public class UIMinimap : MonoBehaviour
    {
        [SerializeField] private RawImage destImage;
        [SerializeField] private UIBrowser browser;

        private RenderTexture _renderTexture;
        private Material _webMaterial;

        private IEnumerator Start()
        {
            destImage.enabled = false;
            yield return new WaitUntil(() => browser.Initialized);
            destImage.enabled = true;

            // Set the material's texture to a RenderTexture to which we'll blit the web content.
            // Note: for CanvasWebViewPrefab, it's important to set material.mainTexture before assigning the material to webViewPrefab.Material.
            var transformImage = destImage.transform as RectTransform;
            var size = transformImage.sizeDelta;

            _renderTexture = new RenderTexture((int)size.x, (int)size.y, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
            _renderTexture.useMipMap = true;
            _renderTexture.filterMode = FilterMode.Bilinear;
            _renderTexture.useDynamicScale = true;    
            destImage.texture = _renderTexture;

                
            
            // var material = new Material(Shader.Find("UI/Default"));
            // material.mainTexture = _renderTexture;
            // destImage.material = material;
            

            _webMaterial = browser.CreateMaterial();
            // destImage.material = _webMaterial;
            
            Debug.Log($"UIMinimap.Start: material = {_webMaterial}, size = {size}");
        }

        private void Update()
        {
            Graphics.Blit(browser.Texture, _renderTexture, _webMaterial);
        }
    }
}