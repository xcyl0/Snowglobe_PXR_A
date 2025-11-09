using Newtonsoft.Json.Linq;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

//[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class toggleobj : UdonSharpBehaviour
{
    [SerializeField] public bool synced;
    [SerializeField] public GameObject toggleObject;
    [SerializeField] public GameObject key;
    [SerializeField] public bool interact; 
    [SerializeField] public bool useProp;
    [SerializeField] public bool triggerExits;
    [SerializeField] public bool useKey;
    [SerializeField] public bool pickUp;
    

    [UdonSynced, FieldChangeCallback(nameof(SyncedToggle))]
    private bool _syncedToggle;
    private bool asyncedToggle;

    public void Start()
    {
        _syncedToggle = toggleObject.activeSelf;
        asyncedToggle = toggleObject.activeSelf;

        if (!interact && !useProp && !pickUp)
            GetComponent<Collider>().isTrigger = true;
        else
            GetComponent<Collider>().isTrigger = false;
    }
    public bool SyncedToggle
    {
        set
        {
            _syncedToggle = value;
            toggleObject.SetActive(value);
        }
        get => _syncedToggle;
    }

    public override void Interact()
    {
        if (interact && !useProp && !triggerExits && !useKey)
        {
            if (synced)
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
                SyncedToggle = !SyncedToggle;
            } else
            {
                asyncedToggle = !asyncedToggle;
                toggleObject.SetActive(asyncedToggle);
            }
        }
    }

    public override void OnPickupUseDown()
    {
        if (!interact && useProp && !triggerExits && !useKey)
        {
            if (synced)
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
                SyncedToggle = !SyncedToggle;
            } else
            {
                asyncedToggle = !asyncedToggle;
                toggleObject.SetActive(asyncedToggle);
            }
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!interact && !useProp && !triggerExits && useKey)
        {
            if (other.gameObject == key)
            {
                if (synced)
                {
                    Networking.SetOwner(Networking.LocalPlayer, gameObject);
                    SyncedToggle = true;
                } else
                {
                    asyncedToggle = true;
                    toggleObject.SetActive(asyncedToggle);
                }
            }
        }
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (!interact && !useProp && !useKey)
        {
            if (synced)
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
                SyncedToggle = true;
            } else
            {
                asyncedToggle = true;
                toggleObject.SetActive(asyncedToggle);
            }
        }
    }
    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if (!interact && !useProp && !useKey && triggerExits)
        {
            if (synced)
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
                SyncedToggle = false;
            } else
            {
                asyncedToggle = false;
                toggleObject.SetActive(asyncedToggle);
            }
        }
    }
    public override void OnPlayerCollisionEnter(VRCPlayerApi player)
    {
        if (!interact && !useProp && !useKey)
        {
            if (synced)
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
                SyncedToggle = !SyncedToggle;
            } else
            {
                asyncedToggle = !asyncedToggle;
                toggleObject.SetActive(asyncedToggle);
            }
        }
    }
    public override void OnPickup()
    {
        if (!interact && !useKey && pickUp)
        {
            if (synced)
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
                SyncedToggle = true;
            } else
            {
                asyncedToggle = true;
                toggleObject.SetActive(asyncedToggle);
            }
        }
    }

    public override void OnDrop()
    {
        if (!interact && !useKey && pickUp)
        {
            if (synced)
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
                SyncedToggle = false;
            } else
            {
                asyncedToggle = false;
                toggleObject.SetActive(asyncedToggle);
            }
        }
    }
}