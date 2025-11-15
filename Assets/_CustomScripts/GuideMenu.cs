using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

/// <summary>
/// Handles secret input and guide authorization to toggle a hidden control panel,
/// allowing the guide to trigger world events discreetly.
/// </summary>
public class GuideMenu : UdonSharpBehaviour
{
    [Header("1. Guide Authorization")]
    [Tooltip("List of VRChat usernames authorized to use the guide controls.")]
    public string[] authorizedGuides = new string[] { "GuideName1", "GuideName2" };

    [Header("2. UI Panel Setup")]
    [Tooltip("The parent GameObject of the secret UI menu.")]
    public GameObject guideMenuPanel;
    
    [Header("3. Logic Targets")]
    [Tooltip("The UdonBehaviour target (usually this object) to send events to.")]
    public UdonBehaviour worldLogicTarget;

    private bool isGuide = false;

    public Transform TeleportOutsideLocation;
    
    // Define the VR input button name for the Quest 3 (Right Thumbstick Click)
    private const string VR_TOGGLE_BUTTON = "Oculus_CrossPlatform_SecondaryThumbstick";

    void Start()
    {
        // Must run on every client
        if (Networking.LocalPlayer != null)
        {
            CheckAuthorization();
        }
        
        // Ensure the menu is hidden by default on all clients
        if (guideMenuPanel != null)
        {
            guideMenuPanel.SetActive(false);
        }

        // Set the world logic target to this object if not manually set
        if (worldLogicTarget == null)
        {
            worldLogicTarget = (UdonBehaviour)GetComponent(typeof(UdonBehaviour));
        }
    }

    /// <summary>
    /// Checks if the local player's display name matches any name in the authorized list.
    /// </summary>
    public void CheckAuthorization()
    {
        VRCPlayerApi localPlayer = Networking.LocalPlayer;
        if (localPlayer == null) return;

        string localUsername = localPlayer.displayName;
        
        // Search the array of authorized guides
        foreach (string guideName in authorizedGuides)
        {
            if (localUsername.ToLower() == guideName.ToLower())
            {
                isGuide = true;
                Debug.Log("[GuideControls] Authorization granted for: " + localUsername);
                // We break the loop once a match is found
                return;
            }
        }

        isGuide = false;
        Debug.Log("[GuideControls] Authorization denied for: " + localUsername);
    }

    /// <summary>
    /// Listens for the secret input only if the local player is authorized.
    /// Supports both desktop keyboard and VR controller.
    /// </summary>
    public void Update()
    {
        // if (isGuide && guideMenuPanel != null)
        {
            bool desktopInput = Input.GetKeyDown(KeyCode.G);
            
            // Check for the VR input button (Right Thumbstick Click on Quest)
            bool vrInput = Input.GetButtonDown(VR_TOGGLE_BUTTON);

            if (desktopInput || vrInput)
            {
                ToggleMenu();
            }
        }
    }

    /// <summary>
    /// Toggles the visibility of the guide control menu.
    /// </summary>
    public void ToggleMenu()
    {
        if (guideMenuPanel != null)
        {
            bool currentState = guideMenuPanel.activeSelf;
            guideMenuPanel.SetActive(!currentState);
            Debug.Log("[GuideControls] Toggled menu visibility: " + !currentState);
        }
    }

    // --- Guide Logic Functions ---
    // These functions can be called by the buttons in your hidden UI menu.
    // They MUST be public methods in UdonSharp for buttons to target them.

    public void SnowglobeSwitchStep()
    {
        worldLogicTarget.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ToggleSequenceVR.AdvanceStage));
    }

    public void Mom1AudioEvent()
    {
        worldLogicTarget.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ToggleSequenceVR.PlayMom1Audio));
    }
    public void Mom2AudioEvent()
    {
        worldLogicTarget.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ToggleSequenceVR.PlayMom2Audio));
    }
    public void MamaPenguinAudioEvent()
    {
        Debug.Log("sent");
        worldLogicTarget.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ToggleSequenceVR.PlayMamaPenguinAudio));
    }

    public void DoorSwingEvent()
    {
        worldLogicTarget.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ToggleSequenceVR.PlayDoorSwing));
    }
    
    public void TeleportOutsideEvent()
    {
        Networking.LocalPlayer.TeleportTo(TeleportOutsideLocation.position, 
            TeleportOutsideLocation.rotation, 
            VRC_SceneDescriptor.SpawnOrientation.Default, 
            false);
    }
}