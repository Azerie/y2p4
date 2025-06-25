using UnityEngine;
using FMODUnity; // Make sure to include the FMODUnity namespace

/// <summary>
/// This script sets a global FMOD parameter when the GameObject it's attached to is destroyed.
/// It's designed to work with the ItemPickupBehaviour script, which destroys the object on interaction.
/// Attach this component to the same GameObject as your ItemPickupBehaviour.
/// </summary>
public class FMODParameterOnPickup : MonoBehaviour
{
    [Header("FMOD Settings")]
    [Tooltip("The exact name of the FMOD Global Parameter you want to control.")]
    [SerializeField] private string parameterName = "YourParameterName";

    [Tooltip("The value you want to set the parameter to when the item is picked up.")]
    [SerializeField] private float parameterValue = 1.0f;

    /// <summary>
    /// OnDestroy is a built-in Unity message that is called when a MonoBehaviour will be destroyed.
    /// Since your ItemPickupBehaviour calls Destroy(gameObject) in its OnInteract() method,
    /// this function will be called automatically right before the object is removed from the scene.
    /// </summary>
    private void OnDestroy()
    {
        // First, check if a parameter name has actually been provided in the Inspector.
        // This prevents errors if the component is not configured correctly.
        if (!string.IsNullOrEmpty(parameterName))
        {
            // This is the core FMOD command. It finds a global parameter by its name
            // and sets it to the desired value.
            RuntimeManager.StudioSystem.setParameterByName(parameterName, parameterValue);

            // It's often helpful to have a debug log to confirm that your script ran correctly.
            // You can remove this line if you don't need it.
            Debug.Log($"FMOD Global Parameter '{parameterName}' was set to '{parameterValue}'.");
        }
        else
        {
            // If the parameter name is missing, this warning will appear in the Unity console.
            Debug.LogWarning("FMODParameterOnPickup: 'Parameter Name' is not set in the Inspector. No parameter was changed.", this.gameObject);
        }
    }
}
