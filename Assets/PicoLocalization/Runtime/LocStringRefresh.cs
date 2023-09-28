using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;

[RequireComponent(typeof(LocalizeStringEvent))]
public class LocStringRefresh : MonoBehaviour
{
    private LocalizeStringEvent _stringEvent;
    private TMP_Text _text;
    
    private void Awake()
    {
        _stringEvent = GetComponent<LocalizeStringEvent>();
         _text = GetComponent<TMP_Text>();
         _text.alpha = 0;
    }

    void OnEnable()
    {
        _stringEvent.OnUpdateString.AddListener(OnUpdateString);
        _stringEvent.RefreshString();
    }

    void OnDisable()
    {
        _stringEvent.OnUpdateString.RemoveListener(OnUpdateString);
    }
    
    private void OnUpdateString(string value)
    {
        _text.alpha = 1;
        _text.text = value;
    }
}
