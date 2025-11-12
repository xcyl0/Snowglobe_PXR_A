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
        Debug.Log("Sent this");
        worldLogicTarget.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ToggleSequenceVR.AdvanceStage));
    }
    
    /// <summary>
    /// Example: Toggles a secret door state globally.
    /// </summary>
    public void ToggleSecretDoor()
    {
        // Example logic: Send a synchronized event to all clients
        worldLogicTarget.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(HandleDoorToggle));
        Debug.Log("[GuideControls] Requested door toggle.");
    }

    /// <summary>
    /// Example: Triggers a narrative sound cue globally.
    /// </summary>
    public void TriggerSoundCue()
    {
        worldLogicTarget.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(HandleSoundCue));
        Debug.Log("[GuideControls] Requested sound cue.");
    }
    
    // --- Networked Event Handlers ---
    // These methods run on ALL clients after the guide calls them.
    
    public void HandleDoorToggle()
    {
        // Replace with your actual logic to toggle the door/object
        Debug.Log("[WORLD LOGIC] Secret Door State Changed!");
        // Example: doorObject.SetActive(!doorObject.activeSelf);
    }
    
    public void HandleSoundCue()
    {
        // Replace with your actual logic to play a sound
        Debug.Log("[WORLD LOGIC] Playing narrative sound cue now.");
        // Example: audioSource.Play();
    }
}