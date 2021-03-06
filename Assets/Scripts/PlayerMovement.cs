﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent( typeof( BoxCollider2D ) )]
public class PlayerMovement : MonoBehaviour {
	public float m_MaxHorizontalSpeed;
	public float m_MoveForce;
	public float m_JumpVelocity;
	public float m_StopThreshold;
	public float m_AirborneReductionFactor;
	public float m_ReverseAirborneReductionFactor;
	public LayerMask m_PlatformMask = 0;
	private float m_HorizontalForce;
	private Rigidbody2D m_RigidBody;
	private BoxCollider2D m_BoxCollider;
	private Animator m_Anim;
	private bool m_IsGrounded = true;
	private bool m_IsFacingRight = true;
	private bool m_IsCrouching = false;
	private float m_CrouchHeight = 0.96f;
	private float m_StandHeight = 1.17f;
	private float m_Epsilon = 0.1f;
	// Use this for initialization
	void Start () {
		m_RigidBody = GetComponent<Rigidbody2D>();
		m_Anim = GetComponent<Animator>();
		m_BoxCollider = GetComponent<BoxCollider2D>();
	}
	
	// Update is called once per frame
	void Update () {
		m_HorizontalForce = Input.GetAxis("Horizontal") * m_MoveForce;
		if(Input.GetButtonDown("Crouch")){
			Crouch();
		}
		else if(Input.GetButtonUp("Crouch")){
			Uncrouch();
		}
	}
	void Crouch(){
		m_IsCrouching = true;
		Set2DBoxColliderHeight(m_CrouchHeight);
		m_Anim.SetBool("Crouch", m_IsCrouching);
	}

	void Set2DBoxColliderHeight(float height){
		m_BoxCollider.size = new Vector2(m_BoxCollider.size.x, height);
	}
	void CheckGrounded(){
		Vector3 topLeft = transform.position + new Vector3(-m_BoxCollider.size.x / 2f, -m_BoxCollider.size.y / 2f);
		Vector3 bottomRight = transform.position + new Vector3(m_BoxCollider.size.x / 2f, -((m_BoxCollider.size.y / 2f) + m_Epsilon));
		m_IsGrounded = Physics2D.OverlapArea(topLeft,
			bottomRight,
			m_PlatformMask,
			-m_Epsilon,
			m_Epsilon
			);
	}
	void Uncrouch(){
		m_IsCrouching = false;
		Set2DBoxColliderHeight(m_StandHeight);
		m_Anim.SetBool("Crouch", m_IsCrouching);
	}
	/// <summary>
	/// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
	/// </summary>
	void FixedUpdate()
	{
		if(!m_IsCrouching){
			if(m_IsGrounded){
				m_RigidBody.AddForce(new Vector2(m_HorizontalForce, 0f));
			}
			else{
				if(m_HorizontalForce * m_RigidBody.velocity.x < 0f){
					m_RigidBody.AddForce(new Vector2(m_HorizontalForce * m_ReverseAirborneReductionFactor, 0f));
				}
				else{
					m_RigidBody.AddForce(new Vector2(m_HorizontalForce * m_AirborneReductionFactor, 0f));
				}
			}
		}

		m_Anim.SetFloat("Speed", Mathf.Abs(m_RigidBody.velocity.x));
		CheckGrounded();
		m_Anim.SetBool("Ground", m_IsGrounded);
		// Make sure Player doesn't accelerate to insane levels
		if(Mathf.Abs(m_RigidBody.velocity.x) > m_MaxHorizontalSpeed){
			m_RigidBody.velocity = new Vector2(Mathf.Clamp(m_RigidBody.velocity.x, -m_MaxHorizontalSpeed, m_MaxHorizontalSpeed), m_RigidBody.velocity.y);
		}
		// Stop once horizontal speed reaches a low enough point for more controllability.
		else if(Mathf.Abs(m_RigidBody.velocity.x) < m_StopThreshold && Mathf.Abs(m_HorizontalForce) < m_Epsilon){
			m_RigidBody.velocity = new Vector2(0f, m_RigidBody.velocity.y);
		}
		if((m_HorizontalForce < -m_Epsilon && m_IsFacingRight) || (m_HorizontalForce > m_Epsilon && !m_IsFacingRight)){
			Flip();
		}
		// Player jumps
		if(Input.GetButtonDown("Jump") && m_IsGrounded){
			m_RigidBody.velocity = new Vector2(m_RigidBody.velocity.x, m_JumpVelocity);
		}
	}

	private void Flip()
        {
            // Switch the way the player is labelled as facing.
            m_IsFacingRight = !m_IsFacingRight;

            // Multiply the player's x local scale by -1.
            Vector3 theScale = transform.localScale;
            theScale.x *= -1;
            transform.localScale = theScale;
        }
}
