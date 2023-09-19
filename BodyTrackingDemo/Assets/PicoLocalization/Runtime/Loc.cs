using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace PicoPorting.Localization
{
    public static class Loc
    {
        private static readonly TableReference TableReference = (TableReference)"StringTable";
        
        public static string Translate(string key)
        {
            var selectedLocale = LocalizationSettings.SelectedLocale;
            var tableEntryReference = (TableEntryReference)key;

            LocalizationSettings.StringDatabase.NoTranslationFoundMessage = ""; 
            var localizedString = LocalizationSettings.StringDatabase.GetLocalizedString(TableReference, tableEntryReference, selectedLocale);
            
            // Debug.Log($"Loc.Translate: key = {key}, result = {localizedString}");
            return string.IsNullOrEmpty(localizedString) ? key : localizedString;
        }

        public static string Localization(this string target)
        {
            return Translate(target);
        }
    }
}