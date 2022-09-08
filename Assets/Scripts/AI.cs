using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour
{
    public Animator animator;

    public int maxHealth = 100;
    public int currentHealth;

    public Collider2D collider1;
    public Collider2D collider2;
    public Collider2D collider3;

    public Rigidbody2D gravity;
    public SpriteRenderer rend;


    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage, string trigger)
    {
        currentHealth -= damage;

        Debug.Log("Current health : " + currentHealth);

        // Play Hurt Animation
        animator.SetTrigger(trigger);

        if (currentHealth <= 0)
        {
            Die();
        }

    }

    void Die()
    {

        Debug.Log("Enemy died");

        // Play Die Animation
        animator.SetBool("isDeath", true);

        // Disable enemy

        collider1.enabled = false;
        collider2.enabled = false;
        collider3.enabled = false;

        gravity.Sleep();
        // rend.enabled = false;

        //this.enabled = false;
    }


    public void Recover()
    {
        currentHealth = maxHealth;
        animator.SetBool("isDeath", false);

        collider1.enabled = true;
        collider2.enabled = true;
        collider3.enabled = true;

        gravity.WakeUp();

        Debug.Log("Current health : " + currentHealth);
    }

}
