using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections;

public class LoginWindowView : MonoBehaviour
{
    // Debug Flag to simulate a reset
    public bool ClearPlayerPrefs;
    public bool DisableAutoLogin; // New flag to test manual login

    // Meta fields for objects in the UI
    public InputField Username;
    public InputField Password;
    public InputField ConfirmPassword;
    public Toggle RememberMe;

    public Button LoginButton;
    public Button PlayAsGuestButton;
    public Button RegisterButton;
    public Button CancelRegisterButton;
    public Button ClearSigninButton;
    public Button ResetSampleButton;
    public Button PlayButton; // Play button reference

    // Serialized Image for LoginPanel
    [SerializeField]
    private Image loginPanelImage; // Drag and drop in Inspector

    // Meta references to panels we need to show / hide
    public GameObject RegisterPanel;
    public GameObject SigninPanel;
    public GameObject LoginPanel;
    public GameObject LoggedinPanel;
    public Text StatusText;
    public Text UserName;

    // Settings for what data to get from playfab on login.
    public GetPlayerCombinedInfoRequestParams InfoRequestParams;

    // Reference to our Authentication service
    private PlayFabAuthService _AuthService = PlayFabAuthService.Instance;

    private GameObject playButtonGameObject; // Cache PlayButton GameObject

    public void Awake()
    {
        if (ClearPlayerPrefs)
        {
            _AuthService.UnlinkSilentAuth();
            _AuthService.ClearRememberMe();
            _AuthService.AuthType = Authtypes.None;
            PlayerPrefs.DeleteAll();
            Debug.Log("Awake: PlayerPrefs cleared");
        }

        // Cache PlayButton GameObject
        if (PlayButton != null)
        {
            playButtonGameObject = PlayButton.gameObject;
            playButtonGameObject.SetActive(false);
            Debug.Log("Awake: PlayButton found and set to inactive");
        }
        else
        {
            Debug.LogError("Awake: PlayButton reference is missing in the Inspector. Please assign it.");
        }

        // Check LoginPanel Image
        if (loginPanelImage != null)
        {
            Debug.Log("Awake: LoginPanel Image found");
        }
        else
        {
            Debug.LogError("Awake: LoginPanel Image is missing in the Inspector. Please assign it.");
        }

        // Verify Canvas component
        Canvas canvas = GetComponent<Canvas>();
        Debug.Log($"Awake: LoginWindowView on GameObject: {gameObject.name}, Canvas: {(canvas != null ? "Found" : "Missing")}");

        // Set Remember Me toggle
        RememberMe.isOn = _AuthService.RememberMe;
        RememberMe.onValueChanged.AddListener((toggle) => _AuthService.RememberMe = toggle);
    }

    public void Start()
    {
        // Hide all UI panels initially
        LoginPanel?.SetActive(false);
        LoggedinPanel?.SetActive(false);
        RegisterPanel?.SetActive(false);
        SigninPanel?.SetActive(true);

        // Subscribe to PlayFab authentication events
        PlayFabAuthService.OnDisplayAuthentication += OnDisplayAuthentication;
        PlayFabAuthService.OnLoginSuccess += OnLoginSuccess;
        PlayFabAuthService.OnPlayFabError += OnPlayFabError;

        // Bind UI buttons
        LoginButton.onClick.AddListener(OnLoginClicked);
        PlayAsGuestButton.onClick.AddListener(OnPlayAsGuestClicked);
        RegisterButton.onClick.AddListener(OnRegisterButtonClicked);
        CancelRegisterButton.onClick.AddListener(OnCancelRegisterButtonClicked);
        ResetSampleButton.onClick.AddListener(OnResetSampleButtonClicked);
        ClearSigninButton.onClick.AddListener(OnClearSigninButtonClicked);

        if (PlayButton != null)
        {
            PlayButton.onClick.AddListener(OnPlayButtonClicked);
            Debug.Log("Start: PlayButton found and click listener added");
        }
        else
        {
            Debug.LogError("Start: PlayButton reference is missing. Please assign it in the Inspector.");
        }

        // Set the data we want at login
        _AuthService.InfoRequestParams = InfoRequestParams;

        // Start authentication
        StartCoroutine(DelayedAutoLogin());
    }

    private IEnumerator DelayedAutoLogin()
    {
        yield return new WaitForSeconds(1f); // Ensure UI is ready
        Debug.Log("DelayedAutoLogin: Checking login state");

        if (DisableAutoLogin)
        {
            Debug.Log("DelayedAutoLogin: Auto-login disabled, showing login UI");
            OnDisplayAuthentication();
            yield break;
        }

        if (_AuthService.AuthTicket != null && !string.IsNullOrEmpty(_AuthService.Email))
        {
            Debug.Log($"DelayedAutoLogin: Attempting auto-login with Email: {_AuthService.Email}");
            PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest
            {
                CustomId = _AuthService.Email,
                CreateAccount = false
            }, result =>
            {
                Debug.Log("DelayedAutoLogin: Auto-login succeeded");
                OnLoginSuccess(result);
            }, error =>
            {
                Debug.LogError($"DelayedAutoLogin: Auto-login failed: {error.ErrorMessage}");
                OnPlayFabError(error);
                OnDisplayAuthentication();
            });
        }
        else
        {
            Debug.Log("DelayedAutoLogin: No saved credentials, showing login UI");
            OnDisplayAuthentication();
        }
    }

    /// <summary>
    /// Login Successful - Shows Play Button, disables LoginPanel Image, and hides login UI.
    /// </summary>
    private void OnLoginSuccess(PlayFab.ClientModels.LoginResult result)
    {
        Debug.LogFormat("OnLoginSuccess: Logged in as {0}", result.PlayFabId);
        if (StatusText != null) StatusText.text = "";

        // Enable Play Button
        if (playButtonGameObject != null)
        {
            playButtonGameObject.SetActive(true);
            Canvas buttonCanvas = playButtonGameObject.GetComponentInParent<Canvas>();
            if (buttonCanvas != null)
            {
                buttonCanvas.enabled = true;
                Debug.Log($"OnLoginSuccess: PlayButton Canvas enabled, name: {buttonCanvas.name}, scale: {buttonCanvas.transform.localScale}");
                // Log parent hierarchy
                Transform parent = buttonCanvas.transform.parent;
                string hierarchy = buttonCanvas.name;
                while (parent != null)
                {
                    hierarchy = parent.name + "/" + hierarchy;
                    parent = parent.parent;
                }
                Debug.Log($"OnLoginSuccess: PlayButton Canvas hierarchy: {hierarchy}");
            }
            else
            {
                Debug.LogWarning("OnLoginSuccess: PlayButton has no parent Canvas.");
            }
            RectTransform rect = playButtonGameObject.GetComponent<RectTransform>();
            Debug.Log($"OnLoginSuccess: PlayButton enabled, active: {playButtonGameObject.activeSelf}, position: {rect.anchoredPosition}, scale: {rect.localScale}");
        }
        else
        {
            Debug.LogError("OnLoginSuccess: PlayButton GameObject is missing or destroyed. Please assign PlayButton in the Inspector.");
        }

        // Disable LoginPanel Image
        if (loginPanelImage != null)
        {
            loginPanelImage.enabled = false;
            Debug.Log("OnLoginSuccess: LoginPanel Image disabled");
        }
        else
        {
            Debug.LogWarning("OnLoginSuccess: LoginPanel Image is missing. Please assign it in the Inspector.");
        }

        // Set username (optional)
        if (UserName != null && UserName.gameObject != null)
        {
            UserName.text = result.InfoResultPayload?.AccountInfo?.Username ?? result.PlayFabId;
            Debug.Log("OnLoginSuccess: Username set");
        }

        // Hide login UI
        if (gameObject != null)
        {
            Debug.Log($"OnLoginSuccess: Hiding login UI on GameObject: {gameObject.name}");
            gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("OnLoginSuccess: LoginWindowView GameObject is destroyed.");
        }
    }

    /// <summary>
    /// Play button click event - Loads the game scene.
    /// </summary>
    private void OnPlayButtonClicked()
    {
        Debug.Log("PlayButton clicked");
        SceneManager.LoadScene("game"); // Replace with your actual scene name
    }

    /// <summary>
    /// Error handling for when Login returns errors.
    /// </summary>
    private void OnPlayFabError(PlayFabError error)
    {
        Debug.LogError($"PlayFabError: {error.Error} - {error.ErrorMessage}");
        if (StatusText != null)
        {
            switch (error.Error)
            {
                case PlayFabErrorCode.InvalidEmailAddress:
                case PlayFabErrorCode.InvalidPassword:
                case PlayFabErrorCode.InvalidEmailOrPassword:
                    StatusText.text = "Invalid Email or Password";
                    break;

                case PlayFabErrorCode.AccountNotFound:
                    if (RegisterPanel != null) RegisterPanel.SetActive(true);
                    if (SigninPanel != null) SigninPanel.SetActive(false);
                    break;
                default:
                    StatusText.text = error.GenerateErrorReport();
                    break;
            }
        }
        OnDisplayAuthentication();
    }

    /// <summary>
    /// Choose to display the Auth UI or any other action.
    /// </summary>
    private void OnDisplayAuthentication()
    {
        if (gameObject != null)
        {
            gameObject.SetActive(true);
            Canvas canvas = GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.enabled = true;
                Debug.Log($"OnDisplayAuthentication: Login Canvas enabled on {gameObject.name}");
                // Re-enable LoginPanel Image
                if (loginPanelImage != null)
                {
                    loginPanelImage.enabled = true;
                    Debug.Log("OnDisplayAuthentication: LoginPanel Image enabled");
                }
            }
            else
            {
                Debug.LogWarning("OnDisplayAuthentication: Canvas component not found.");
            }
        }
        else
        {
            Debug.LogWarning("OnDisplayAuthentication: LoginWindowView GameObject is destroyed.");
        }
        if (LoginPanel != null) LoginPanel.SetActive(true);
        if (LoggedinPanel != null) LoggedinPanel.SetActive(false);
        if (SigninPanel != null) SigninPanel.SetActive(true);
        if (StatusText != null) StatusText.text = "";
    }

    private void OnPlayAsGuestClicked()
    {
        if (StatusText != null) StatusText.text = "Logging In As Guest ...";
        _AuthService.Authenticate(Authtypes.Silent);
    }

    private void OnLoginClicked()
    {
        if (StatusText != null) StatusText.text = $"Logging In As {Username.text} ...";
        _AuthService.Email = Username.text;
        _AuthService.Password = Password.text;
        _AuthService.Authenticate(Authtypes.EmailAndPassword);
    }

    private void OnRegisterButtonClicked()
    {
        if (Password.text != ConfirmPassword.text)
        {
            if (StatusText != null) StatusText.text = "Passwords do not match.";
            return;
        }

        if (StatusText != null) StatusText.text = $"Registering User {Username.text} ...";
        _AuthService.Email = Username.text;
        _AuthService.Password = Password.text;
        _AuthService.Authenticate(Authtypes.RegisterPlayFabAccount);
    }

    private void OnCancelRegisterButtonClicked()
    {
        Username.text = Password.text = ConfirmPassword.text = "";
        if (RegisterPanel != null) RegisterPanel.SetActive(false);
        if (SigninPanel != null) SigninPanel.SetActive(true);
    }

    private void OnClearSigninButtonClicked()
    {
        _AuthService.ClearRememberMe();
        if (StatusText != null) StatusText.text = "Signin info cleared";
    }

    private void OnResetSampleButtonClicked()
    {
        PlayFabClientAPI.ForgetAllCredentials();
        _AuthService.Email = _AuthService.Password = _AuthService.AuthTicket = "";
        _AuthService.AuthType = Authtypes.None;
        PlayerPrefs.DeleteAll();
        Debug.Log("ResetSample: Credentials and PlayerPrefs cleared");
        OnDisplayAuthentication();
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        PlayFabAuthService.OnDisplayAuthentication -= OnDisplayAuthentication;
        PlayFabAuthService.OnLoginSuccess -= OnLoginSuccess;
        PlayFabAuthService.OnPlayFabError -= OnPlayFabError;
        Debug.Log("LoginWindowView: Destroyed");
    }
}