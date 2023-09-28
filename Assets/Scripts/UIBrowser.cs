using System;
using System.Collections;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;
#if BROWSER
using Vuplex.WebView;
#endif

namespace BodyTrackingDemo
{
    public class UIBrowser : MonoBehaviour
    {
        public Material Material { get; private set; }
        public Texture Texture  { get; private set; }
        public Vector2Int Size { get; private set; }
        public bool Initialized { get; private set; }
        
        [SerializeField] private Transform webViewRoot;
        [SerializeField] private GameObject navigationBarRoot;
        [SerializeField] private Button btnBack;
        [SerializeField] private Button btnForward;
        [SerializeField] private Button btnRefresh;
        [SerializeField] private Button btnHome;
        [SerializeField] private TMP_InputField inputFieldURL;
        [SerializeField] private string defaultSearchWeb = "https://www.baidu.com/";
        [SerializeField] private string homePage = "https://search.bilibili.com/video?keyword=dance";
#if BROWSER
        
        private CanvasWebViewPrefab _webViewPrefab;
        private float _pointerEnterCounter;
        private float _navigationBarHideTimer;
        private bool _hasFocus = true;

        private void Awake()
        {
            inputFieldURL.onSubmit.AddListener(OnSubmitURL);
            btnBack.onClick.AddListener(OnBack);
            btnForward.onClick.AddListener(OnForward);
            btnRefresh.onClick.AddListener(OnRefresh);
            btnHome.onClick.AddListener(OnHome);

            navigationBarRoot.SetActive(false);
            // Use a desktop User-Agent to request the desktop versions of websites.
            // https://developer.vuplex.com/webview/Web#SetUserAgent
            Web.SetUserAgent(false);
            Web.SetStorageEnabled(true);
            // Web.SetCameraAndMicrophoneEnabled(true);
        }

        async void Start() {
            
            // Create a CanvasWebViewPrefab
            // https://developer.vuplex.com/webview/CanvasWebViewPrefab
            _webViewPrefab = CanvasWebViewPrefab.Instantiate();
            _webViewPrefab.InitialUrl = homePage;
            _webViewPrefab.Resolution = .65f;
            _webViewPrefab.NativeOnScreenKeyboardEnabled = true;
            _webViewPrefab.transform.SetParent(webViewRoot, false);
            _webViewPrefab.transform.localScale = Vector3.one;
            _webViewPrefab.gameObject.SetLayerRecursively(webViewRoot.gameObject.layer);

            var pointerInputDetector = _webViewPrefab.GetComponentInChildren<CanvasPointerInputDetector>();
            pointerInputDetector.PointerEntered += OnPointerEntered;
            pointerInputDetector.PointerExited += OnPointerExited;

            // Wait for the prefab to initialize because its WebView property is null until then.
            // https://developer.vuplex.com/webview/WebViewPrefab#WaitUntilInitialized
            await _webViewPrefab.WaitUntilInitialized();
            
            Material = _webViewPrefab.Material;
            Size = _webViewPrefab.WebView.Size;
            Texture = _webViewPrefab.WebView.Texture;
            Initialized = true;

            // After the prefab has initialized, you can use the IWebView APIs via its WebView property.
            // https://developer.vuplex.com/webview/IWebView
            _webViewPrefab.WebView.UrlChanged += OnUrlChanged;

            _webViewPrefab.WebView.LoadUrl(_webViewPrefab.InitialUrl);
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            _hasFocus = hasFocus;
        }

        private void OnPointerEntered(object sender, EventArgs e)
        {
            if (navigationBarRoot.activeSelf)
            {
                StopAllCoroutines();    
            }
            
            _pointerEnterCounter++;
            navigationBarRoot.SetActive(true);
            Debug.Log($"UIBrowser.OnPointerEntered: sender = {sender}, pointerCount = {_pointerEnterCounter}");
        }

        private void OnPointerExited(object sender, EventArgs e)
        {
            _pointerEnterCounter--;
            if (_pointerEnterCounter <= 0)
            {
                if (navigationBarRoot.activeSelf)
                {
                    _navigationBarHideTimer = 5;
                    StartCoroutine(WaitForHideNavigationBar());    
                }
            }
            
            Debug.Log($"UIBrowser.OnPointerExited: sender = {sender}");
        }

        private IEnumerator WaitForHideNavigationBar()
        {
            yield return new WaitUntil(() =>
            {
                if (_hasFocus)
                {
                    _navigationBarHideTimer -= Time.deltaTime;
                }
                return _navigationBarHideTimer <= 0;
            });
            
            navigationBarRoot.SetActive(false);
        }

        private void OnBack()
        {
            _navigationBarHideTimer = 5;
            _webViewPrefab.WebView.GoBack();
        }

        private void OnForward()
        {
            _navigationBarHideTimer = 5;
            _webViewPrefab.WebView.GoForward();
        }

        private void OnRefresh()
        {
            _navigationBarHideTimer = 5;
            _webViewPrefab.WebView.Reload();
        }

        private void OnHome()
        {
            _navigationBarHideTimer = 5;
            _webViewPrefab.WebView.LoadUrl(_webViewPrefab.InitialUrl);
        }
        
        private void OnSubmitURL(string value)
        {
            _navigationBarHideTimer = 5;
            _webViewPrefab.WebView.LoadUrl(IsURL(value) ? value : $"{defaultSearchWeb}s?wd={value}");
        }
        
        private void OnUrlChanged(object sender, UrlChangedEventArgs e)
        {
            inputFieldURL.text = e.Url;
        }

        private static bool IsURL(string url)
        {
            bool isValidUrl = Uri.TryCreate(url, UriKind.Absolute, out var uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            return isValidUrl;
        }
        
#endif
        
        public Material CreateMaterial()
        {
#if BROWSER
            return _webViewPrefab.WebView.CreateMaterial();
#else
            return null;
#endif
        }
    }
}