using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoPickUp : MonoBehaviour
{
    public int ammoAmount = 10; // Specify the amount of ammo the pickup gives in the Unity Editor

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Get the CharacterController2D component
            CharacterController2D characterController = collision.GetComponent<CharacterController2D>();

            // Check if the component exists
            if (characterController != null)
            {
                // Add ammo from the pickup
                characterController.AddAmmo(ammoAmount);

                // Destroy the pickup object
                Destroy(gameObject);
            }
        }
    }
}