using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    private Animator m_menuAnimator;

    [SerializeField]
    private CharacterDataSO[] m_characterDatas;

    [SerializeField]
    private AudioClip m_confirmClip;

    // Player name
    [SerializeField]
    private TMP_InputField playerNameInputField; // Add this serialized field
    private bool m_isNameEntered = false; // Add this flag
    private const string k_playerNameKey = "PlayerName";


    private const string k_enterMenuTriggerAnim = "enter_menu";
    private const string k_enterDifficultyChoiceTriggerAnim = "ToDificultyChoice";

    [SerializeField]
    private SceneName nextScene = SceneName.CharacterSelection;

    private IEnumerator Start()
    {
        // -- To test with latency on development builds --
        // To set the latency, jitter and packet-loss percentage values for develop builds we need
        // the following code to execute before NetworkManager attempts to connect (changing the
        // values of the parameters as desired).
        //
        // If you'd like to test without the simulated latency, just set all parameters below to zero(0).
        //
        // More information here:
        // https://docs-multiplayer.unity3d.com/netcode/current/tutorials/testing/testing_with_artificial_conditions#debug-builds
#if DEVELOPMENT_BUILD && !UNITY_EDITOR
        NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>().
            SetDebugSimulatorParameters(
                packetDelay: 50,
                packetJitter: 5,
                dropRate: 3);
#endif

        ClearAllCharacterData();

        if (DifficultyManager.Instance == null)
        {
            // Initialize StaticDifficulty if it is null
            GameObject staticDifficultyObject = new GameObject("DifficultyManager");
            staticDifficultyObject.AddComponent<DifficultyManager>();
        }

        // Wait for the network Scene Manager to start
        yield return new WaitUntil(() => NetworkManager.Singleton.SceneManager != null);

        // Set the events on the loading manager
        // Doing this because every time the network session ends the loading manager stops
        // detecting the events
        LoadingSceneManager.Instance.Init();

        if (PlayerPrefs.HasKey(k_playerNameKey))
        {
            string savedName = PlayerPrefs.GetString(k_playerNameKey);
            playerNameInputField.text = savedName;
            m_isNameEntered = true;
            TriggerMenuTransitionAnimation(k_enterMenuTriggerAnim);
        }

        GameObject playerReportData = new GameObject("playerReportData");
        PlayerReportData reportData = playerReportData.AddComponent<PlayerReportData>();

    }
    private void Update()
    {
        if (PlayerPrefs.HasKey(k_playerNameKey))
        {
            string savedName = PlayerPrefs.GetString(k_playerNameKey);
            playerNameInputField.text = savedName;
            m_isNameEntered = true;
            TriggerMenuTransitionAnimation(k_enterMenuTriggerAnim);
        }
        else if (!m_isNameEntered && Input.GetKeyDown(KeyCode.Return) && !string.IsNullOrEmpty(playerNameInputField.text))
        {
            OnNameEntered(playerNameInputField.text);
        }
    }

    private void OnNameEntered(string playerName)
    {
        if (!m_isNameEntered && !string.IsNullOrEmpty(playerName))
        {
            PlayerPrefs.SetString(k_playerNameKey, playerName); // Save the player's name
            PlayerPrefs.Save();
            TriggerMenuTransitionAnimation(k_enterMenuTriggerAnim);
            m_isNameEntered = true;
        }
    }


    public void OnClickOption(string option)
    {
        if (option == "2")
        {
            // Create and configure the DDAController
            GameObject ddaControllerObject = new GameObject("DDAController");
            DDAController ddaController = ddaControllerObject.AddComponent<DDAController>();
            DifficultyManager.Instance.LoadDifficultySettings();
            DifficultyManager.Instance.SetDifficulty("Dynamic");

            NetworkManager.Singleton.StartHost();
            AudioManager.Instance.PlaySoundEffect(m_confirmClip);
            LoadingSceneManager.Instance.LoadScene(SceneName.CharacterSelection);
        }
        else
        {
            TriggerMenuTransitionAnimation(k_enterDifficultyChoiceTriggerAnim);
        }
    }


    public void OnClickDifficulty(string difficulty)
    {
        // Loading the difficulty (needs to be adjusted to take any of the 3.
        DifficultyManager.Instance.LoadDifficultySettings();
        DifficultyManager.Instance.SetDifficulty(difficulty);


        NetworkManager.Singleton.StartHost();
        AudioManager.Instance.PlaySoundEffect(m_confirmClip);
        LoadingSceneManager.Instance.LoadScene(SceneName.CharacterSelection);
    }

    // TODO: Implement the quit functionality sending a mail to myself with peoples info.
    public void OnClickQuit()
    {
        // Sending data for experiment
        GameObject dataMailerobject = new GameObject("dataMailerobject");
        DataMailer dataMailer = dataMailerobject.AddComponent<DataMailer>();

        PlayerReportData.Instance.playerName = playerNameInputField.text;
        PlayerReportData.Instance.SaveReportToJson();

        PlayerPrefs.DeleteAll();

        AudioManager.Instance.PlaySoundEffect(m_confirmClip);
        Debug.Log("I Quit");
        Application.Quit();
    }

    private void ClearAllCharacterData()
    {
        // Clean the all the data of the characters so we can start with a clean slate
        foreach (CharacterDataSO data in m_characterDatas)
        {
            data.EmptyData();
        }
    }

    private void TriggerMenuTransitionAnimation(string trigger)
    {
        m_menuAnimator.SetTrigger(trigger);
        AudioManager.Instance.PlaySoundEffect(m_confirmClip);
    }

    // We use a coroutine because the server is the one who makes the load
    // we need to make a fade first before calling the start client
    private IEnumerator Join()
    {
        LoadingFadeEffect.Instance.FadeAll();

        yield return new WaitUntil(() => LoadingFadeEffect.s_canLoad);

        NetworkManager.Singleton.StartClient();
    }
}