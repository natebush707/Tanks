using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class tankHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public Slider slider;
    public Image image; 
    public float shellDamage = 25f;

    public ParticleSystem explosionPrefab;
    [SerializeField]
    private float currentHealth;
    private ParticleSystem explosionInstance;
    private AudioSource deathSound;

    private void Update() {
        updateUI();
    }

    private void Awake()
    {
        explosionInstance = Instantiate(explosionPrefab).GetComponent<ParticleSystem>();
        deathSound = explosionInstance.gameObject.GetComponent<AudioSource>();
        explosionInstance.gameObject.SetActive(false);
    }
    private void OnEnable() {
        currentHealth = maxHealth;
    }

    public void takeDamage(float damageTaken){
        this.currentHealth -= damageTaken;
        updateUI();

        if(this.currentHealth <= 0){
            onDeath();
        }
    }
    private void updateUI(){
        slider.value = currentHealth/maxHealth;
        image.color = Color.Lerp(Color.red, Color.green, currentHealth / maxHealth);
    }

    private void onDeath(){
        explosionInstance.transform.position = this.transform.position;
        explosionInstance.gameObject.SetActive(true);
        explosionInstance.Play();
        deathSound.Play();
        Destroy(this.gameObject);
    }

    private void OnTriggerEnter(Collider other) {
        if(other.tag == "enemyShell"){
            takeDamage(shellDamage);
        }
        if(other.tag == "rammer" && !other.gameObject.IsDestroyed())
        {
            onDeath();
        }
    }

}


