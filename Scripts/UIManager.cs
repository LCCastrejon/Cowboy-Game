using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class UIManager : MonoBehaviour
{

    public TextMeshProUGUI ammoCountText; // Text for displaying total ammo count

    public void UpdateAmmo(int currentAmmo, int totalClipAmount)
    {
        ammoCountText.text = currentAmmo + " / " + totalClipAmount;
    }
}