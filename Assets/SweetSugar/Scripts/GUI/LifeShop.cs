using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using Solana.Unity.Rpc.Models;
using Solana.Unity.Rpc.Types;
using Solana.Unity.Programs;
using SweetSugar.Scripts.Core;

namespace SweetSugar.Scripts.GUI
{
    public class LifeShop : MonoBehaviour
    {
        public static LifeShop Instance;

        public int CostIfRefill = 1;
        public string recipientAddress = "2X7LGqBYGUQE77x9jM1VSm5rCXVL4M94WyGuKVZr3N7N"; // ✅ known-valid
        public ulong lamportsToSend = 1000000; // 0.001 SOL

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            transform.Find("Image/Buttons/BuyLife/Price")
                     .GetComponent<TextMeshProUGUI>().text = CostIfRefill.ToString();

            if (!LevelManager.THIS.enableInApps)
                transform.Find("Image/Buttons/BuyLife").gameObject.SetActive(false);
        }

        public async void OnBuyLifeButtonClicked()
        {
            Debug.Log("🔌 Attempting to connect Phantom wallet...");

            await Web3.Instance.LoginWalletAdapter(); // deep link logic internal
            Debug.Log("🔗 LoginWalletAdapter called.");

            int waitAttempts = 20;
            while (Web3.Account == null && waitAttempts-- > 0)
                await Task.Delay(250);

            if (Web3.Account != null)
            {
                Debug.Log("👛 Wallet connected: " + Web3.Account.PublicKey);
                await SendBuyLifeTransaction();
            }
            else
            {
                Debug.LogError("❌ Wallet still not connected after waiting.");
            }
        }

        public async Task<bool> SendBuyLifeTransaction()
        {
            var payer = Web3.Account;

            if (payer == null)
            {
                Debug.LogError("❌ Wallet is still null. Cannot proceed.");
                return false;
            }

            var blockHashResult = await Web3.Rpc.GetLatestBlockHashAsync();
            if (!blockHashResult.WasSuccessful)
            {
                Debug.LogError("❌ Failed to fetch recent blockhash.");
                return false;
            }

            PublicKey toPubKey;
            try
            {
                toPubKey = new PublicKey(recipientAddress);
            }
            catch (Exception ex)
            {
                Debug.LogError("❌ Invalid recipient address: " + recipientAddress);
                Debug.LogError("🔍 Exception: " + ex.Message);
                return false;
            }

            var tx = new Transaction
            {
                FeePayer = payer.PublicKey,
                RecentBlockHash = blockHashResult.Result.Value.Blockhash
            };

            tx.Add(SystemProgram.Transfer(payer.PublicKey, toPubKey, lamportsToSend));
            Debug.Log("📦 Sending transaction to wallet adapter for signing...");

            var result = await Web3.Wallet.SignAndSendTransaction(tx);


            if (string.IsNullOrEmpty(result.Result))
            {
                Debug.LogError("❌ Wallet returned empty transaction result.");
                return false;
            }

            if (result.WasSuccessful)
            {
                Debug.Log("✅ Life purchase transaction successful: " + result.Result);
                GrantExtraLife();
                return true;
            }
            else
            {
                Debug.LogError("❌ Transaction failed: " + result.Reason);
                return false;
            }
        }

        private void GrantExtraLife()
        {
            Debug.Log("❤️ Extra life granted!");
            PlayerPrefs.SetInt("ExtraLife", PlayerPrefs.GetInt("ExtraLife", 0) + 1);
        }
    }
}
