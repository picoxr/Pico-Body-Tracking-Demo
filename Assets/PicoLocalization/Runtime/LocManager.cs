using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;

namespace PicoPorting.Localization
{
    public class LocManager : MonoBehaviour
    {
        [SerializeField] private string subtitlePrefKey = "subtitlePrefKey";
        
        // private IEnumerator Start()
        // {
        //     // Wait for the localization system to initialize
        //     yield return LocalizationSettings.InitializationOperation;
        //     
        //     var curLocaleCode = LocalizationSettings.SelectedLocale.Identifier.Code;
        //     if (curLocaleCode != "zh-CN")
        //     {
        //         if (Application.systemLanguage == SystemLanguage.Chinese || Application.systemLanguage == SystemLanguage.ChineseSimplified)
        //         {
        //             foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
        //             {
        //                 if (locale.Identifier.Code == "zh-CN")
        //                 {
        //                     LocalizationSettings.SelectedLocale = locale;
        //                     Debug.Log($"LocManager.Start: Change SelectedLocale to {locale.LocaleName}");
        //                     break;
        //                 }
        //             }
        //         }
        //     }
        //
        //     DontDestroyOnLoad(gameObject);
        // }

        private void Awake()
        {
            if (Application.systemLanguage == SystemLanguage.Chinese || Application.systemLanguage == SystemLanguage.ChineseSimplified)
            {
                if (!PlayerPrefs.HasKey(subtitlePrefKey))
                {
                    PlayerPrefs.SetInt(subtitlePrefKey, 1);
                    Debug.Log("LocManager.Awake: subtitlePrefKey = 1");
                }   
            }
#if !DEBUG
            LocalizationSettings.StringDatabase.NoTranslationFoundMessage = ""; 
#endif
        }
    }
}