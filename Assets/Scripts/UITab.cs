using UnityEngine;
using UnityEngine.UI;

namespace BodyTrackingDemo
{
    public class UITab : MonoBehaviour
    {
        [SerializeField] private Button[] btnTabs;
        [SerializeField] private GameObject[] pages;

        private void Awake()
        {
            for (int i = 0; i < btnTabs.Length; i++)
            {
                int idx = i;
                btnTabs[i].onClick.AddListener(()=>
                {
                    OnTab(idx);
                });
            }

            ChangePage(0);
        }

        private void OnTab(int idx)
        {
            ChangePage(idx);
        }

        private void ChangePage(int idx)
        {
            for (int i = 0; i < btnTabs.Length; i++)
            {
                btnTabs[i].interactable = i != idx;
                pages[i].SetActive(i == idx);
            }
        }
    }
}