using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.Udon;

public class PenguinBehaviour : UdonSharpBehaviour
{
	[UdonSynced] [SerializeField] private bool _collected;
	[SerializeField] private Transform[] _waypoints;
	[SerializeField] private Transform[] _collectedWaypoints;
	[SerializeField] private float _moveSpeed = 2f;
	[SerializeField] private float _waypointThreshold = 0.5f;
	[SerializeField] private float _rotationSpeed = 5f;
	[SerializeField] private GameObject _collectionArea;
	[SerializeField] private AudioSource _penguinAudioSource;
	
	private int _currentWaypointIndex;
	private Transform _currentTarget;
	private VRCPickup _pickup;
	private bool _inCollectionArea;
	private Rigidbody _rigidbody;

	private void Start()
	{
		_pickup = GetComponent<VRCPickup>();
		_rigidbody = GetComponent<Rigidbody>();
		if (_waypoints.Length > 0)
		{
			_currentTarget = _waypoints[0];
		}
	}

	private void Update()
	{
		if (!IsBeingHeld())
		{
			WaypointMove();
			KeepUpright();
		}
	}
	
	private void KeepUpright()
	{
		if (_rigidbody == null) return;
		
		// Force upright rotation
		Vector3 eulerAngles = transform.eulerAngles;
		eulerAngles.x = 0f;
		eulerAngles.z = 0f;
		transform.rotation = Quaternion.Euler(eulerAngles);
		
		// Reset angular velocity to prevent spinning
		_rigidbody.angularVelocity = Vector3.zero;
	}
	
	private bool IsBeingHeld()
	{
		return _pickup != null && _pickup.IsHeld;
	}
	
	private void WaypointMove()
	{
		Transform[] activeWaypoints = _collected ? _collectedWaypoints : _waypoints;
		
		if (_currentTarget == null || activeWaypoints.Length == 0) return;
		
		Vector3 direction = (_currentTarget.position - transform.position).normalized;
		transform.position += direction * _moveSpeed * Time.deltaTime;
		
		if (direction != Vector3.zero)
		{
			Vector3 flatDirection = new Vector3(direction.x, 0, direction.z).normalized;
			Quaternion targetRotation = Quaternion.LookRotation(flatDirection, Vector3.up);
			transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
		}
		
		if (Vector3.Distance(transform.position, _currentTarget.position) <= _waypointThreshold)
		{
			_currentWaypointIndex = (_currentWaypointIndex + 1) % activeWaypoints.Length;
			_currentTarget = activeWaypoints[_currentWaypointIndex];
		}
	}

	public override void OnDrop()
	{
		if (_collectionArea != null && _inCollectionArea)
		{
			_collected = true;
			// Switch to collected waypoints
			if (_collectedWaypoints.Length > 0)
			{
				_currentWaypointIndex = 0;
				_currentTarget = _collectedWaypoints[0];
			}
			RequestSerialization();
		}
	}

	public override void OnPickup()
	{
		base.OnPickup();
		_penguinAudioSource.Play();
	}


	private bool IsInCollectionArea()
	{
		if (_collectionArea == null) return false;
		
		Collider collectionCollider = _collectionArea.GetComponent<Collider>();
		if (collectionCollider != null)
		{
			return collectionCollider.bounds.Contains(transform.position);
		}
		
		return Vector3.Distance(transform.position, _collectionArea.transform.position) <= 2f;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.name == "CollectionArea") //lol
		{
			_inCollectionArea = true;
		}
		else if (other.gameObject.name == "wphDelete")
		{
			Destroy(gameObject);
		}
	}	
	
	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.name == "CollectionArea") 
		{
			_inCollectionArea = false;
		}
	}
}

