using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private const float MAX_HEALTH = 100;

    //modify this one to the health of the player
    public float healther = MAX_HEALTH;

    private Image healthBar;

    // Start is called before the first frame update
    void Start()
    {
        healthBar = GetComponent<Image>(); //grabs reference to the image or the red part of the main player health bar
    }

    // Update is called once per frame
    void Update()
    {
        healthBar.fillAmount = healther / MAX_HEALTH; //this will change the length of the health bar
    }
}
