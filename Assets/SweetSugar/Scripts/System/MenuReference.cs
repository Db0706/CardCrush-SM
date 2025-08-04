using System;
using SweetSugar.Scripts.Core;
using UnityEngine;

namespace SweetSugar.Scripts.System
{
    public class MenuReference : MonoBehaviour
    {
        public static MenuReference THIS;
        
        public GameObject PrePlay;
        public GameObject PreCompleteBanner;
        public GameObject MenuPlay;
        public GameObject MenuComplete;
        public GameObject MenuFailed;
        public GameObject PreFailed;
        public GameObject BonusSpin;
        public GameObject Reward;
        public GameObject Daily;
        public GameObject Tutorials;
        public GameObject Settings;

        // Retained shop references, but initialized as null
        public GameObject BoostShop = null;
        public GameObject LiveShop = null;
        public GameObject GemsShop = null;

        private void Awake()
        {
            THIS = this;
            THIS.HideAll();
        }

        private void Start()
        {
            // ShowDailyReward(); // Disabled as part of simplification
        }

        public void HideAll()
        {
            var canvas = THIS.transform;
            foreach (Transform item in canvas)
            {
                if (item.name != "SettingsButton" && item.name != "Tutorials" && item.name != "Orientations" && item.name != "TutorialManager" && !item.name.Contains("Rate"))
                    item.gameObject.SetActive(false);
            }
        }

        // private static void ShowDailyReward()
        // {
        //     if (!ServerTime.THIS.dateReceived)
        //     {
        //         ServerTime.OnDateReceived += ShowDailyReward;
        //         return;
        //     }

        //     var DateReward = PlayerPrefs.GetString("DateReward", default(DateTime).ToString());
        //     var dateTimeReward = DateTime.Parse(DateReward);
        //     DateTime testDate = ServerTime.THIS.serverTime;

        //     if (LevelManager.GetGameStatus() == GameState.Map)
        //     {
        //         if (DateReward == "" || DateReward == default(DateTime).ToString())
        //             InitScript.Instance.DailyMenu.SetActive(true);
        //         else
        //         {
        //             var timePassedDaily = testDate.Subtract(dateTimeReward).TotalDays;
        //             if (timePassedDaily >= 1)
        //                 InitScript.Instance.DailyMenu.SetActive(true);
        //         }
        //     }
        // }
    }
}
