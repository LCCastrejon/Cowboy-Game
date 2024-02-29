using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heal : MonoBehaviour
{
    public int heal;
    public PlayerHealth playerHealth;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerHealth.Heal(heal);
            Destroy(gameObject);
        }
    }
}
