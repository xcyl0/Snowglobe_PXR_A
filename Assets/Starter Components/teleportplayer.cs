using UdonSharp;
using UnityEngine;
using UnityEngine.Experimental.XR.Interaction;
using UnityEngine.InputSystem;
using VRC.SDKBase;

public class teleportplayer : UdonSharpBehaviour
{
    [SerializeField] Transform targetPosition;
    [SerializeField] public GameObject key;
    [SerializeField] public bool interact;
    [SerializeField] public bool useProp;

    [SerializeField] public bool useKey;
    [SerializeField] public bool pickUp;

    public void Start()
    {
        if (interact && !useProp && !pickUp)
            GetComponent<Collider>().isTrigger = false;
        else
            GetComponent<Collider>().isTrigger = true;
    }
    public override void Interact()
    {
        if (interact)
        {
            Networking.LocalPlayer.TeleportTo(targetPosition.position,
                                              targetPosition.rotation,
                                              VRC_SceneDescriptor.SpawnOrientation.Default,
                                              false);
        }
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (!interact && !useProp && !useKey)
        {
            Networking.LocalPlayer.TeleportTo(targetPosition.position,
                                              targetPosition.rotation,
                                              VRC_SceneDescriptor.SpawnOrientation.Default,
                                              false);
        }
    }
    public override void OnPickupUseDown()
    {
        if (!interact && useProp && !useKey)
        {
            Networking.LocalPlayer.TeleportTo(targetPosition.position,
                                              targetPosition.rotation,
                                              VRC_SceneDescriptor.SpawnOrientation.Default,
                                              false);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!interact && !useProp && useKey)
        {
            if (other.gameObject == key)
            {
                Networking.LocalPlayer.TeleportTo(targetPosition.position,
                                              targetPosition.rotation,
                                              VRC_SceneDescriptor.SpawnOrientation.Default,
                                              false);
            }
        }
    }
    public override void OnPickup()
    {
        if (!interact && !useKey && pickUp)
        {
            Networking.LocalPlayer.TeleportTo(targetPosition.position,
                                             targetPosition.rotation,
                                             VRC_SceneDescriptor.SpawnOrientation.Default,
                                             false);
        }
    }
}
