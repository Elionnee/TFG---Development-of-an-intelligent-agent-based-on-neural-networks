using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class PlayerAgent : Agent
{

    public Animator animator;

    public Transform attackPoint;
    public float attackRange = 0.6f;
    public LayerMask playerLayer;

    public float lightAtkRate = 2.25f;
    public float hardAtkRate = 2f;
    public float defRate = 1.5f;
    float nextMove = 0f;

    public int AttackDmg = 5;
    public int HardAttackDmg = 15;


    float moveSpeed = 2f;

    bool isDef = false;

    [SerializeField] private Collider2D BodyCollider1;
    [SerializeField] private Collider2D BodyCollider2;

    [SerializeField] private Transform targetTransform;
    [SerializeField] private Material winMat;
    [SerializeField] private Material loseMat;
    [SerializeField] private SpriteRenderer floorSpriteRender;

    // -13.6, -3.7 Rango de mapa
    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(Random.Range(-13.6f, -3.7f), -17.09f, 2.24f);
        targetTransform.localPosition = new Vector3(Random.Range(-13.6f, -3.7f), -17.09f, 2.24f);
        this.GetComponent<AI>().Recover();
        targetTransform.GetComponent<PlayerCombat>().Recover();
    }

    // How the agent observs its enviroment 
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(targetTransform.localPosition);

        sensor.AddObservation(targetTransform.GetComponent<PlayerCombat>().currentHealth);
        sensor.AddObservation(this.GetComponent<AI>().currentHealth);

        sensor.AddObservation(targetTransform.GetComponent<PlayerCombat>().GetAction());  // Move = 0, Nothing = 1, AttackLight = 2, AttackHard = 3, Defend = 4, Crouch = 5

        sensor.AddObservation(EnemyInRange() ? 1 : 0);

    }


    private bool EnemyInRange() {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);

        // Damage them
        if (hitEnemies.Length > 0)
        {
            return true;
        }
            
        return false;
    }



    // Actions that the agent can take
    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];

        Debug.Log(moveX);

        animator.SetBool("isRun", true);

        transform.localPosition += new Vector3(moveX, 0, 0) * Time.deltaTime * moveSpeed;

        if(isDef && Time.time >= nextMove)
        {
            DefendDown();
        }

        if(targetTransform.position.x > transform.position.x)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }

        if (targetTransform.position.x < transform.position.x)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }



        // MoveLeft = 0, MoveRight = 1, AttackLight = 2, AttackHard = 3, Crouch = 4, Defend = 5
        if (EnemyInRange())
        {
            if (Time.time >= nextMove && !isDef)
            {
                int action = actions.DiscreteActions[0];

                switch (action)
                {
                    case 0:
                        animator.SetBool("isRun", true);
                        transform.localPosition += new Vector3(-1f, 0, 0) * Time.deltaTime * moveSpeed;
                        if (targetTransform.GetComponent<PlayerCombat>().PlayerMissedHit() == 1)
                        {
                            AddReward(1f);
                        }
                        else
                        {
                            //AddReward(-1f / MaxStep);
                        }
                        break;

                    case 1:
                        animator.SetBool("isRun", true);
                        transform.localPosition += new Vector3(1f, 0, 0) * Time.deltaTime * moveSpeed;
                        if (targetTransform.GetComponent<PlayerCombat>().PlayerMissedHit() == 1)
                        {
                            AddReward(1f);
                        }
                        else
                        {
                            //AddReward(-1f / MaxStep);
                        }
                        break;

                    case 2:
                        if (Attack())
                        {
                            AddReward(1f);

                        }
                        else
                        {
                            //AddReward(-1f / MaxStep);
                        }
                        nextMove = Time.time + 1f / lightAtkRate;
                        break;

                    case 3:
                        if (AttackHard())
                        {
                            AddReward(1f);
                        }
                        else
                        {
                            //AddReward(-1f / MaxStep);
                        }
                        nextMove = Time.time + 1f / hardAtkRate;
                        break;

                    case 4:
                        Defend();
                        if (targetTransform.GetComponent<PlayerCombat>().PlayerMissedHit() == 1)
                        {
                            AddReward(1f);
                        }
                        else
                        {
                            //AddReward(-1f / MaxStep);
                        }
                        nextMove = Time.time + 1f / defRate;
                        break;

                }
            }
        }

        if (targetTransform.GetComponent<PlayerCombat>().currentHealth >= this.GetComponent<AI>().currentHealth)
        {
            AddReward(-2f);
        }
        

        if (targetTransform.GetComponent<PlayerCombat>().currentHealth <= 0) {
            AddReward(20f);
            floorSpriteRender.material = winMat;
            EndEpisode();
        }

        if (this.GetComponent<AI>().currentHealth <= 0) {
            AddReward(-20f);
            floorSpriteRender.material = loseMat;
            EndEpisode();
        }
    }

    // Give reward once you detect a collision with the enemy
    private void OnTriggerEnter2D(Collider2D collision)
    {
        /**
        if(collision.TryGetComponent<Enemy>(out Enemy enemy))
        {
            AddReward(10f);
            animator.SetBool("isRun", false);
            floorSpriteRender.material = winMat;
            EndEpisode();
        }
        */
        
        if (collision.TryGetComponent<Wall>(out Wall wall))
        {
            AddReward(-7f);
            floorSpriteRender.material = loseMat;
            EndEpisode();
        }
    }


    // To be able to move the agent manually for testing
    public override void Heuristic(in ActionBuffers actionsOut)
    {

        int act = 0;

        if (Input.GetKeyDown(KeyCode.Space)) act = 2;

        if (Input.GetKeyDown(KeyCode.R)) act = 3;
    
        if (Input.GetKeyDown(KeyCode.Q)) act = 4;

        if (Input.GetKeyDown(KeyCode.LeftArrow)) act = 0;
        
        if (Input.GetKeyDown(KeyCode.RightArrow)) act = 1;

        ActionSegment<float> continousActions = actionsOut.ContinuousActions;
        ActionSegment<int> discriteActions = actionsOut.DiscreteActions;
        
        discriteActions[0] = act;
        continousActions[0] = Input.GetAxisRaw("Horizontal");

    }


    bool Attack()
    {

        // Play an attack animation
        animator.SetTrigger("Attack");
        // Detect enemies in range of attack
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);

        // Damage them
        if (hitEnemies.Length > 0)
        {
            Debug.Log("We hit " + hitEnemies[hitEnemies.Length - 1].name);

            hitEnemies[hitEnemies.Length - 1].GetComponent<PlayerCombat>().TakeDamage(AttackDmg, "HurtLight");

            return true;

        }

        return false;
    }

    bool AttackHard()
    {
        // Play an attack animation
        animator.SetTrigger("AttackHard");

        // Detect enemies in range of attack
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);

        // Damage them
        if (hitEnemies.Length > 0)
        {
            Debug.Log("AI hit " + hitEnemies[hitEnemies.Length - 1].name);

            hitEnemies[hitEnemies.Length - 1].GetComponent<PlayerCombat>().TakeDamage(HardAttackDmg, "HurtHard");

            return true;

        }

        return false;
    }

    void Defend()
    {
        // Play an attack animation
        animator.SetTrigger("Defend");

        if (BodyCollider1 != null)
        {
            BodyCollider1.enabled = false;
            Debug.Log("AI defend 1");
        }

        if (BodyCollider2 != null)
        {
            BodyCollider2.enabled = false;
            Debug.Log("AI defend 2");
        }

        isDef = true;
    }

    void DefendDown()
    {
        if (BodyCollider1 != null)
        {
            BodyCollider1.enabled = true;
            Debug.Log("AI STOP defending 1");
        }

        if (BodyCollider2 != null)
        {
            BodyCollider2.enabled = true;
            Debug.Log("AI STOP defending 2");
        }

        isDef = false;

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
