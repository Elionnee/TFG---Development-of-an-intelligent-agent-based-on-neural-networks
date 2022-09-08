using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    public CharacterController2D controller;

    public Animator animator;

    public float runSpeed = 22f;

    float horizontalMove = 0f;

    bool crouch = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    // We use this method to get input from our player
    void Update()
    {
        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;
        this.GetComponent<PlayerCombat>().SetAction(0);

        animator.SetFloat("PlayerSpeed", Mathf.Abs(horizontalMove));

        if(Input.GetButtonDown("Crouch"))
        {
            crouch = true;

            this.GetComponent<PlayerCombat>().SetAction(5);

        } else if (Input.GetButtonUp("Crouch"))
        {
            crouch = false;
            this.GetComponent<PlayerCombat>().SetAction(1);
        }

    }


    public void OnCrouching(bool isCrouching)
    {
        animator.SetBool("IsCrouching", crouch);
    }


    /** Update but for physics. It works much in the same way as update, whoever,
     * instead of being called once per frame it's called a fixed amount of
     * times per second, hence the name fixed update
    */
    // This will be the main function to move our character
    // We use this method to apply the input we get from the Update() function
    // to our character
    private void FixedUpdate()
    {
        // We multiply the horizontalMove speed by Time.fixedDeltaTime to
        // ensure that we move the same amount no matter how often this
        // function is called so that our character speed is consistant 
        // through all systems and plataforms
        controller.Move(horizontalMove * Time.fixedDeltaTime, crouch, false);
    }
}
