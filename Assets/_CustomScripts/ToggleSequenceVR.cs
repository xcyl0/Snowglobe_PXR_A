using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

/// <summary>
/// Manages the sequential stages of a guided experience.
/// Each stage enables one set of objects and disables another set.
/// </summary>
public class ToggleSequenceVR : UdonSharpBehaviour
{
    [Header("Current State")]
    [Tooltip("The index of the current stage. Starts at -1 (initial/off state).")]
    [UdonSynced]
    public int currentStageIndex = 0; 

    [Header("--- Stage Definitions (Configure in Inspector) ---")]
    [Tooltip("The total number of stages defined below (must match your setup).")]
    public int totalStages = 3; 

    // Public arrays for the Inspector. UdonSharp doesn't easily support nested arrays,
    // so we use a set of parallel arrays for easy setup.
    [Header("STAGE 0: Initial Scene Setup")]
    public GameObject[] S0_Enable;
    public GameObject[] S0_Disable;

    [Header("STAGE 1: First Interaction")]
    public GameObject[] S1_Enable;
    public GameObject[] S1_Disable;

    [Header("STAGE 2: Second Interaction")]
    public GameObject[] S2_Enable;
    public GameObject[] S2_Disable;
    public AudioSource TransitionSFX;
    public AudioSource BedroomMusic;
    public AudioSource Mom1Audio;
    public AudioSource Mom2Audio;
    public AudioSource MamaPenguinAudio;
    public AudioSource MusicSnowball;
    public AudioSource MusicMaze;
    public AudioSource MusicCandy;
    public DoorSwing DoorSwing;
    
    // ADD MORE HERE: e.g., public GameObject[] S3_Enable; public GameObject[] S3_Disable;

    [Header("End State")]
    [Tooltip("The GameObject to enable when the final stage is reached.")]
    public GameObject FinalSceneObject;

    // Internal arrays to hold the inspector data dynamically
    private GameObject[][] ObjectsToEnable;
    private GameObject[][] ObjectsToDisable;

    private void Start()
    {
        // 1. Initialize the internal jagged arrays based on the totalStages count.
        ObjectsToEnable = new GameObject[totalStages][];
        ObjectsToDisable = new GameObject[totalStages][];

        // 2. Map the public inspector arrays to the internal jagged arrays.
        // NOTE: If you increase totalStages, you must add more mapping here.
        
        if (totalStages > 0)
        {
            ObjectsToEnable[0] = S0_Enable;
            ObjectsToDisable[0] = S0_Disable;
        }
        if (totalStages > 1)
        {
            ObjectsToEnable[1] = S1_Enable;
            ObjectsToDisable[1] = S1_Disable;
        }
        if (totalStages > 2)
        {
            ObjectsToEnable[2] = S2_Enable;
            ObjectsToDisable[2] = S2_Disable;
        }
        
        Debug.Log("[StageManager] Initialized with " + totalStages + " stages.");

        // Apply the initial state (objects defined in the previous stage are disabled)
        // This is important for late-joiners.
        ApplyStage(); 
    }

    /// <summary>
    /// Networked Event Handler: Advances the stage and applies the logic on all clients.
    /// This method is called by the GuideControls script via a custom network event.
    /// </summary>
    public void AdvanceStage()
    {
        // Ensure the guide is the owner to synchronize the index change
        if (!Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
        
        // Increment the stage index
        currentStageIndex++;

        if (currentStageIndex == 2)
        {
            TransitionSFX.Play();
            MusicSnowball.Play();
            MusicCandy.Play();
            MusicMaze.Play();
            BedroomMusic.Stop();
        }
        
        // Handle completion
        if (currentStageIndex >= totalStages)
        {
            Debug.Log("[StageManager] Experience Complete!");
            // Reset to a known state (optional: currentStageIndex = -1;)
            // if (FinalSceneObject != null) FinalSceneObject.SetActive(true);
            
            // Perform final cleanup of the LAST stage's enabled objects
            ApplyStage();
            currentStageIndex = totalStages; // Keep it at max index to prevent re-trigger
            return;
        }

        Debug.Log("[StageManager] Advancing to Stage Index: " + currentStageIndex);
        ApplyStage();
    }

    /// <summary>
    /// Applies the enable/disable logic based on the currentStageIndex.
    /// </summary>
    private void ApplyStage()
    {
        // 1. First, process the objects from the new stage
        if (currentStageIndex >= 0 && currentStageIndex < totalStages)
        {
            // Enable objects specified for this stage
            GameObject[] toEnable = ObjectsToEnable[currentStageIndex];
            if (toEnable != null)
            {
                foreach (GameObject obj in toEnable)
                {
                    if (obj != null)
                    {
                        obj.SetActive(true);
                        Debug.Log($"enabling: {obj.name}");
                    }
                    
                }
            }
            
            // Disable objects specified for this stage
            GameObject[] toDisable = ObjectsToDisable[currentStageIndex];
            if (toDisable != null)
            {
                foreach (GameObject obj in toDisable)
                {
                    if (obj != null) obj.SetActive(false);
                        Debug.Log($"disabling: {obj.name}");
                    
                }
            }
        }
    }


    public void PlayMom1Audio()
    {
        Mom1Audio.Play();
    }

    public void PlayMom2Audio()
    {
        Mom2Audio.Play();
    }

    public void PlayMamaPenguinAudio()
    {
        MamaPenguinAudio.Play();
    }

    public void PlayDoorSwing()
    {
        DoorSwing.StartDoorSwing();
    }

    // This method is called when the synced variable (currentStageIndex) changes on the network.
    public override void OnDeserialization()
    {
        ApplyStage();
    }
}