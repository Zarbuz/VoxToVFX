using System;
using UnityEngine;
using VoxToVFXFramework.Scripts.Managers;

public class DestroyOnTouch : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		RuntimeVoxManager.Instance.SetPlayerToWorldCenter();
	}
}
