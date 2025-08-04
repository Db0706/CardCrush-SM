using SweetSugar.Scripts.Level;
using SweetSugar.Scripts.MapScripts;
using SweetSugar.Scripts.System;
using SweetSugar.Scripts.GUI.Boost;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SweetSugar.Scripts.Core
{
    public class InitScript : MonoBehaviour
    {
        public static InitScript Instance;

        // Level and Life Management
        public static int openLevel;
        public static int lifes { get; private set; }
        public int CapOfLife = 0;

        // Life Restoration Timer
        public static float RestLifeTimer;
        public static string DateOfExit;

        // Rewards and Currency (Stubbed but Static for Compatibility)
        public static int Gems = 0;
        public static int waitedPurchaseGems = 0;
        public RewardsType currentReward = RewardsType.NONE;

        // Boosts and Life (Stubbed for Compatibility)
        public static bool losingLifeEveryGame = false;
        public static float TotalTimeForRestLifeHours = 0;
        public static float TotalTimeForRestLifeMin = 0;
        public static float TotalTimeForRestLifeSec = 0;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            Application.runInBackground = true;
            Instance = this;

            RestLifeTimer = PlayerPrefs.GetFloat("RestLifeTimer");
            DateOfExit = PlayerPrefs.GetString("DateOfExit", "");
            lifes = PlayerPrefs.GetInt("Lifes");

            if (PlayerPrefs.GetInt("Launched") == 0)
            {
                lifes = CapOfLife;
                PlayerPrefs.SetInt("Lifes", lifes);
                PlayerPrefs.SetInt("Music", 1);
                PlayerPrefs.SetInt("Sound", 1);
                PlayerPrefs.SetInt("Launched", 1);
                PlayerPrefs.Save();
            }
        }

        public void RestoreLifes()
        {
            lifes = CapOfLife;
            PlayerPrefs.SetInt("Lifes", lifes);
            PlayerPrefs.Save();

            FindObjectOfType<LIFESAddCounter>()?.ResetTimer();
        }

public void SpendLife(int count)
{
    lifes = Mathf.Max(0, lifes - count);
    PlayerPrefs.SetInt("Lifes", lifes);
    PlayerPrefs.Save();

    if (lifes <= 0)
    {
        GameObject liveShop = MenuReference.THIS.LiveShop;
        liveShop.SetActive(true);

        Transform buyButton = liveShop.transform.Find("Image/Buttons/BuyLife");
        if (buyButton != null)
        {
            buyButton.gameObject.SetActive(true);
            var buttonComponent = buyButton.GetComponent<UnityEngine.UI.Button>();
            if (buttonComponent != null)
                buttonComponent.interactable = true;
        }
    }
}




        public void AddLife(int count)
        {
            lifes = Mathf.Min(lifes + count, CapOfLife);
            PlayerPrefs.SetInt("Lifes", lifes);
            PlayerPrefs.Save();
        }

        public int GetLife()
        {
            if (lifes > CapOfLife)
            {
                lifes = CapOfLife;
                PlayerPrefs.SetInt("Lifes", lifes);
                PlayerPrefs.Save();
            }
            return lifes;
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                if (RestLifeTimer > 0)
                {
                    PlayerPrefs.SetFloat("RestLifeTimer", RestLifeTimer);
                }
                PlayerPrefs.SetInt("Lifes", lifes);
                PlayerPrefs.SetString("DateOfExit", ServerTime.THIS.serverTime.ToString());
                PlayerPrefs.Save();
            }
        }

        private void OnApplicationQuit()
        {
            if (RestLifeTimer > 0)
            {
                PlayerPrefs.SetFloat("RestLifeTimer", RestLifeTimer);
            }
            PlayerPrefs.SetInt("Lifes", lifes);
            PlayerPrefs.SetString("DateOfExit", ServerTime.THIS.serverTime.ToString());
            PlayerPrefs.Save();
        }

        public void OnLevelClicked(object sender, LevelReachedEventArgs args)
        {
            if (EventSystem.current.IsPointerOverGameObject(-1))
                return;

            if (!GameObject.Find("CanvasGlobal").transform.Find("MenuPlay").gameObject.activeSelf)
            {
                SoundBase.Instance.PlayOneShot(SoundBase.Instance.click);
                OpenMenuPlay(args.Number);
            }
        }

        public static void OpenMenuPlay(int num)
        {
            PlayerPrefs.SetInt("OpenLevel", num);
            PlayerPrefs.Save();
            LevelManager.THIS.MenuPlayEvent();
            LevelManager.THIS.LoadLevel();
            openLevel = num;
            CrosssceneData.openNextLevel = false;
            MenuReference.THIS.MenuPlay.gameObject.SetActive(true);
        }

        private void OnEnable()
        {
            LevelsMap.LevelSelected += OnLevelClicked;
            LevelsMap.OnLevelReached += OnLevelReached;
        }

        private void OnDisable()
        {
            LevelsMap.LevelSelected -= OnLevelClicked;
            LevelsMap.OnLevelReached -= OnLevelReached;
        }

        private void OnLevelReached()
        {
            var num = PlayerPrefs.GetInt("OpenLevel");
            if (CrosssceneData.openNextLevel && CrosssceneData.totalLevels >= num)
            {
                OpenMenuPlay(num);
            }
        }

        // ---- FIXED STUBS ---- //
        public void SpendBoost(BoostType boostType) { }
        public void BuyBoost(BoostType boostType, int price, int count) { }
        public void SpendGems(int amount) { Gems -= amount; }
        public void AddGems(int amount) { Gems += amount; }
        public void ShowGemsReward(int amount) { AddGems(amount); }
        public void PurchaseSucceded() { Gems += waitedPurchaseGems; waitedPurchaseGems = 0; }
    }

    public class LIFESAddCounter : MonoBehaviour
    {
        public void ResetTimer() { }
    }

    public enum RewardsType
    {
        NONE,
        GetLifes,
        GetGems,
        GetGoOn,
        FreeAction
    }
}
