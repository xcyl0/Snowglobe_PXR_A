using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

/// <summary>
/// This UdonSharpBehaviour automatically selects a random avatar from a list of 
/// VRC_AvatarPedestal components and applies it to the player when they join the world.
/// </summary>
public class WorldAvatarSelector : UdonSharpBehaviour
{
    [Header("Avatar Pedestals (Required)")]
    [Tooltip("Drag all VRC_AvatarPedestal components here. One will be chosen randomly.")]
    public VRC_AvatarPedestal[] avatarPedestals;

    [Tooltip("Delay in seconds before attempting to change the avatar. 1 second is recommended.")]
    public float delayTime = 1.0f;

    /// <summary>
    /// VRChat event that fires on all clients when a player joins the world.
    /// </summary>
    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        // CRITICAL CHECK: We only want the *joining player* to execute the avatar change on *themselves*.
        // If the player who joined is the local client (i.e., this player just entered the world),
        // we trigger the avatar selection logic.
        if (player.isLocal)
        {
            if (avatarPedestals == null || avatarPedestals.Length == 0)
            {
                Debug.LogError("[WorldAvatarSelector] No Avatar Pedestals assigned! Cannot set avatar.");
                return;
            }

            // We use a slight delay (1 second) to ensure the client is fully initialized 
            // and the avatar system is ready before forcing a change.
            SendCustomEventDelayedSeconds(nameof(ApplyRandomAvatar), delayTime);
            Debug.Log("[WorldAvatarSelector] Local player joined. Scheduling random avatar application.");
        }
    }

    /// <summary>
    /// Selects a random pedestal and applies its avatar to the local player.
    /// This method is called after the delay from OnPlayerJoined.
    /// </summary>
    public void ApplyRandomAvatar()
    {
        if (avatarPedestals == null || avatarPedestals.Length == 0) return;

        // 1. Generate a random index within the bounds of the array.
        int randomIndex = Random.Range(0, avatarPedestals.Length);

        // 2. Get the chosen pedestal.
        VRC_AvatarPedestal selectedPedestal = avatarPedestals[randomIndex];
        
        if (selectedPedestal == null)
        {
            Debug.LogError($"[WorldAvatarSelector] Pedestal at index {randomIndex} is null. Skipping change.");
            return;
        }

        // 3. Call the ChangeAvatar method. Since this Udon is running on the local client,
        // this will apply the change to the local player who just joined.
        // selectedPedestal.ChangeAvatar();

        Debug.Log($"[WorldAvatarSelector] Successfully set local player's avatar using Pedestal Index: {randomIndex}");
    }
}