using System;
using UnityEngine;

public class PlayerAnimatorController : MonoBehaviour
{
    public Animator controller;
    void Start()
    {
        InputManagerController.OnMove += PlayMoveAnimation;
        PlayerManagerController.OnHit += PlayHitAnimation;
        InputManagerController.OnJump += PlayJumpAnimation;
        PlayerManagerController.OnGround += SetGroundAnimation;
        InputManagerController.OnAttack += PlayerAttackAnim;
    }

    private void PlayerAttackAnim()
    {
        controller.SetTrigger("OnAttack");
    }

    private void SetGroundAnimation (bool value)
    {
        controller.SetBool("IsGrounded", value);
    }

    private void PlayJumpAnimation()
    {
        controller.SetTrigger("OnJump");
    }

    private void PlayMoveAnimation (Vector2 moveInput)
    {
        /*if (moveInput.x != 0)
        {
            controller.SetBool("IsMoving", true;
        }
        else
            controller.SetBool("IsMoving", false;*/
        controller.SetBool("IsMoving", moveInput.x != 0 ? true : false);
        GetComponent<SpriteRenderer>().flipX = (moveInput.x == -1 && moveInput.x != 0) ? true : false; 
    }

    private void PlayHitAnimation()
    {
        controller.SetTrigger("OnHit");
    }

    void Update()
    {
        
    }
}
