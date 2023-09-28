using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
 
namespace PicoPorting.Localization
{
    [RequireComponent(typeof(TMP_Dropdown))]
    [AddComponentMenu("Localization/Localize Dropdown")]
    public class LocalizeDropdown : MonoBehaviour
    {
        private TMP_Dropdown _dropdown;

        private void Awake()
        {
            _dropdown = GetComponent<TMP_Dropdown>();
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        }

        private void Start()
        {
            UpdateDropdown();
        }

        void OnDestroy() => LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;

        private void OnLocaleChanged(Locale selectedLocale)
        {
            UpdateDropdown();
        }
        
        private void UpdateDropdown()
        {
            foreach (var t in _dropdown.options)
            {
                t.text = t.text.Localization();
            }

            _dropdown.RefreshShownValue();
        }
 
    }
}