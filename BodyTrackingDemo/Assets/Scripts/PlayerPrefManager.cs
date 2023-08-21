using System;
using UnityEngine;

namespace BodyTrackingDemo
{
    public class PlayerPrefManager : MonoBehaviour
    {
        private static PlayerPrefManager _instance;
        public static PlayerPrefManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("PlayerPrefData");
                    _instance = go.AddComponent<PlayerPrefManager>();
                    DontDestroyOnLoad(go);
                }

                return _instance;
            }
        }

        private const string fileName = "PlayerPref";
        
        public PlayerPrefData PlayerPrefData
        {
            get;
            private set;
        }

        #region Field

        private string _userID = "default";

        #endregion

        #region UnityMessage

        private void Awake()
        {
            _instance = this;
            Load("default");
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                Save();
            }
        }

        #endregion

        #region PublicMethod

        public void Init(string userID)
        {
            _userID = userID;
            Load(userID);
        }

        public void Load(string userID)
        {
            var content = PlayerPrefs.GetString($"{fileName}_{userID}");
            if (string.IsNullOrEmpty(content))
            {
                PlayerPrefData = new PlayerPrefData();
            }
            else
            {
                PlayerPrefData = JsonUtility.FromJson<PlayerPrefData>(content);    
            }
            Debug.Log($"PlayerPrefManager.Save: content = {PlayerPrefData}");
        }
        
        public void Save()
        {
            string content = JsonUtility.ToJson(PlayerPrefData);
            PlayerPrefs.SetString($"{fileName}_{_userID}", content);
            PlayerPrefs.Save();
            Debug.Log($"PlayerPrefManager.Save: content = {content}");
        }

        #endregion
    }

    [Serializable]
    public class PlayerPrefData
    {
        public int bodyTrackMode = 1;

        public override string ToString()
        {
            return $"PlayerPrefData: bodyTrackNode = {bodyTrackMode}";
        }
    }
}