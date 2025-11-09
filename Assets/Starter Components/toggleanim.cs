
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

public class toggleanim : UdonSharpBehaviour
{
    [SerializeField] bool synced;
    public Animator animator;
    
    [SerializeField] string triggerName;
    [SerializeField] bool toggle;
    [SerializeField] public GameObject key;
    [SerializeField] public bool interact;
    [SerializeField] public bool useProp;
    [SerializeField] public bool triggerExits;
    [SerializeField] public bool useKey;
    [SerializeField] public bool pickUp;

    [UdonSynced, FieldChangeCallback(nameof(SyncedToggle))]
    private bool _syncedToggle;
    private bool asyncedToggle;


    public bool SyncedToggle
    {
        set
        {
            Debug.Log("SyncedToggle set to " + value);
            _syncedToggle = value;
            if (!toggle) animator.SetTrigger(triggerName);
            else animator.SetBool(triggerName, _syncedToggle);
        }
        get => _syncedToggle;
    }

    public void Start()
    {
        if (!interact && !useProp && !pickUp)
            GetComponent<Collider>().isTrigger = true;
        else
            GetComponent<Collider>().isTrigger = false;
    }
    public override void Interact()
    {
        if (interact && !useProp && !triggerExits && !useKey)
        {
            Debug.Log("Interact called");
            if (synced)
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
                SyncedToggle = !SyncedToggle;
            } else
            {
                if (!toggle) animator.SetTrigger(triggerName);
                else
                {
                    asyncedToggle = !asyncedToggle;
                    animator.SetBool(triggerName, asyncedToggle);
                }
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
                if (!toggle) animator.SetTrigger(triggerName);
                else
                {
                    asyncedToggle = !asyncedToggle;
                    animator.SetBool(triggerName, asyncedToggle);
                }
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
                    if (!toggle) animator.SetTrigger(triggerName);
                    else
                    {
                        asyncedToggle = true;
                        animator.SetBool(triggerName, asyncedToggle);
                    }
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
                if (!toggle) animator.SetTrigger(triggerName);
                else
                {
                    asyncedToggle = true;
                    animator.SetBool(triggerName, asyncedToggle);
                }
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
                if (!toggle) animator.SetTrigger(triggerName);
                else
                {
                    asyncedToggle = false;
                    animator.SetBool(triggerName, asyncedToggle);
                }
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
                if (!toggle) animator.SetTrigger(triggerName);
                else
                {
                    asyncedToggle = !asyncedToggle;
                    animator.SetBool(triggerName, asyncedToggle);
                }
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
                if (!toggle) animator.SetTrigger(triggerName);
                else
                {
                    asyncedToggle = true;
                    animator.SetBool(triggerName, asyncedToggle);
                }
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
                if (!toggle) animator.SetTrigger(triggerName);
                else
                {
                    asyncedToggle = false;
                    animator.SetBool(triggerName, asyncedToggle);
                }
            }
        }
    }
}

