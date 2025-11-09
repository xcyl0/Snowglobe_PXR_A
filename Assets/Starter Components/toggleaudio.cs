
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(VRCSpatialAudioSource))]

public class toggleaudio : UdonSharpBehaviour
{
    [SerializeField] public bool synced;
    public AudioSource audioSource;
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
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.priority = 1;

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
            if(_syncedToggle) audioSource.Play();
            else audioSource.Stop();
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
                if (asyncedToggle) audioSource.Play();
                else audioSource.Stop();
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
                if (asyncedToggle) audioSource.Play();
                else audioSource.Stop();
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
                    if (asyncedToggle) audioSource.Play();
                    else audioSource.Stop();
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
                if (asyncedToggle) audioSource.Play();
                else audioSource.Stop();
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
                if (asyncedToggle) audioSource.Play();
                else audioSource.Stop();
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
                if (asyncedToggle) audioSource.Play();
                else audioSource.Stop();
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
                if (asyncedToggle) audioSource.Play();
                else audioSource.Stop();
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
                if (asyncedToggle) audioSource.Play();
                else audioSource.Stop();
            }
        }
    }
}
