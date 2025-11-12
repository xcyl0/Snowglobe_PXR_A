using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ExpressionMenuToggle : UdonSharpBehaviour
{
	[System.Serializable]
	public class ToggleStep
	{
		public GameObject[] enableObjects;
		public GameObject[] disableObjects;
	}

	public ToggleStep[] steps;

	private int _currentStep;
	private bool _menuOpen;
	public GameObject MenuRoot;

	public void _Udon_MenuOpen()
	{
		_menuOpen = true;
		if (MenuRoot != null) MenuRoot.SetActive(true);
		RunStep();
	}

	public void _Udon_MenuClosed()
	{
		_menuOpen = false;
		if (MenuRoot != null) MenuRoot.SetActive(false);
	}

	private void RunStep()
	{
		if (steps == null || steps.Length == 0) return;

		ToggleStep step = steps[_currentStep];

		for (int i = 0; i < step.enableObjects.Length; i++)
		{
			GameObject go = step.enableObjects[i];
			if (go != null) go.SetActive(true);
		}

		for (int i = 0; i < step.disableObjects.Length; i++)
		{
			GameObject go = step.disableObjects[i];
			if (go != null) go.SetActive(false);
		}

		_currentStep++;
		if (_currentStep >= steps.Length)
			_currentStep = 0;
	}
}
