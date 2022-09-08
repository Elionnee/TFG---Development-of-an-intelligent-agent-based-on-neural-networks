using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public Animator animator;

    int action = 1;

    public Transform attackPoint;
    public float attackRange = 0.6f;
    public LayerMask enemyLayer;

    public float lightAtkRate = 2.25f;
    public float hardAtkRate = 2f;
    public float defRate = 1.5f;
    float nextMove = 0f;

    bool isDef = false;


    [SerializeField] private Collider2D BodyCollider1;
    [SerializeField] private Collider2D BodyCollider2;
    [SerializeField] private Collider2D BodyCollider3;

    public Rigidbody2D gravity;

    public int AttackDmg = 5;
    public int HardAttackDmg = 15;

    int playerMissed = 0;

    public int maxHealth = 100;
    public int currentHealth;


    void Start()
    {
        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {

        if (Time.time >= nextMove && isDef)
        {
            isDef = false;
            DefendDown();
        }

        if (Time.time >= nextMove && !isDef) 
        {
            if (Input.GetKeyDown(KeyCode.Space)) // Space
            {
                Attack();
                nextMove = Time.time + 1f / lightAtkRate;
            }
        }

        if (Time.time >= nextMove && !isDef)
        {
            if (Input.GetKeyDown(KeyCode.R)) // R
            {
                AttackHard();
                nextMove = Time.time + 1f / hardAtkRate;
            }
        }

        if (Time.time >= nextMove && !isDef)
        {
            if (Input.GetKeyDown(KeyCode.Q)) // Q
            {
                isDef = true;
                Defend();
                nextMove = Time.time + 1f / defRate;
            }
        }

        
    }


  

    void AttackHard()
    {
        // Play an attack animation
        animator.SetTrigger("AttackHard");

        SetAction(3);

        // Detect enemies in range of attack
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        // Damage them
        if (hitEnemies.Length > 0)
        {
            Debug.Log("We hit " + hitEnemies[hitEnemies.Length - 1].name);

            hitEnemies[hitEnemies.Length - 1].GetComponent<AI>().TakeDamage(HardAttackDmg, "HurtHard");

            SetPlayerMissedHit(0);

        }
        else
        {
            Debug.Log("Missed hit");
            SetPlayerMissedHit(1);
        }

    }

    void Attack()
    {
        // Play an attack animation
        animator.SetTrigger("Attack");

        SetAction(2);

        // Detect enemies in range of attack
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        // Damage them
        if (hitEnemies.Length > 0)
        {
            Debug.Log("We hit " + hitEnemies[hitEnemies.Length - 1].name);

            hitEnemies[hitEnemies.Length - 1].GetComponent<AI>().TakeDamage(AttackDmg, "HurtLight");

            SetPlayerMissedHit(0);

        }
        else
        {
            Debug.Log("Missed hit");

            SetPlayerMissedHit(1);
        }
    }

    void Defend()
    {
        // Play an attack animation
        animator.SetTrigger("Defend");

        SetAction(4);

        if (BodyCollider1 != null)
        {
            BodyCollider1.enabled = false;
            Debug.Log("We defend 1");
        }

        if (BodyCollider2 != null)
        {
            BodyCollider2.enabled = false;
            Debug.Log("We defend 2");
        }

       
    }

    void DefendDown()
    {
        SetAction(1);
        if (BodyCollider1!= null)
        {
            BodyCollider1.enabled = true;
            Debug.Log("We STOP defending 1");
        }

        if (BodyCollider2 != null)
        {
            BodyCollider2.enabled = true;
            Debug.Log("We STOP defending 2");
        }

    }



    public void SetPlayerMissedHit(int n)
    {
        playerMissed = n;
    }

    public int PlayerMissedHit()
    {
        return playerMissed;
    }

    public void TakeDamage(int damage, string trigger)
    {
        currentHealth -= damage;

        Debug.Log("Current player health : " + currentHealth);

        // Play Hurt Animation
        animator.SetTrigger(trigger);

        if (currentHealth <= 0)
        {
            Die();
        }

    }

    void Die()
    {

        Debug.Log("Player died");

        // Play Die Animation
        animator.SetBool("isDeath", true);

        // Disable enemy

        BodyCollider1.enabled = false;
        BodyCollider2.enabled = false;
        BodyCollider3.enabled = false;

        gravity.Sleep();
        // rend.enabled = false;

        //this.enabled = false;
    }


    public void Recover()
    {
        currentHealth = maxHealth;
        animator.SetBool("isDeath", false);

        BodyCollider1.enabled = true;
        BodyCollider2.enabled = true;
        BodyCollider3.enabled = true;

        gravity.WakeUp();

        Debug.Log("Current health : " + currentHealth);
    }


    public void SetAction(int act)
    {
        action = act;
    }
    public int GetAction()
    {
        return action;
    }


    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
        {
            return;
        }
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
