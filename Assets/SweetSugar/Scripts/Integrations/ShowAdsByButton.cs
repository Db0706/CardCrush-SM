using System.Collections;
using SweetSugar.Scripts.AdsEvents;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SweetSugar.Scripts.Integrations
{
    public class ShowAdsByButton : MonoBehaviour
    {
        public UnityEvent OnAdShown;
        public bool checkRewardedAvailable;
        private CanvasGroup canvasGroup;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1;
                canvasGroup.blocksRaycasts = true;

                if (checkRewardedAvailable && GetComponent<Button>().onClick.GetPersistentMethodName(0) == "ShowRewardedAd")
                {
                    canvasGroup.alpha = 0;
                    canvasGroup.blocksRaycasts = false;
                    StartCoroutine(WaitForAds());
                }
            }
        }

        private IEnumerator WaitForAds()
        {
            yield return new WaitUntil(() => AdsManager.THIS.GetRewardedUnityAdsReady());
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;
        }

        private void OnDisable()
        {
            AdsManager.OnRewardedShown -= HandleAdShown;
        }

        private void HandleAdShown()
        {
            OnAdShown?.Invoke();
        }

        public void ShowRewardedAd()
        {
            AdsManager.OnRewardedShown += HandleAdShown;
            AdsManager.THIS.ShowRewardedAds();
        }
    }
}
