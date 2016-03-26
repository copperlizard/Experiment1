using UnityEngine;

namespace UnityStandardAssets.Characters.ThirdPerson
{
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(CapsuleCollider))]
	[RequireComponent(typeof(Animator))]
	public class ThirdPersonCharacter : MonoBehaviour
	{
		[SerializeField] float m_MovingTurnSpeed = 360;
		[SerializeField] float m_StationaryTurnSpeed = 180;
		[SerializeField] float m_JumpPower = 12f;
		[Range(1f, 4f)][SerializeField] float m_GravityMultiplier = 2f;
		[SerializeField] float m_RunCycleLegOffset = 0.2f; //specific to the character in sample assets, will need to be modified to work with others
		[SerializeField] float m_MoveSpeedMultiplier = 1f;
		[SerializeField] float m_AnimSpeedMultiplier = 1f;
		[SerializeField] float m_GroundCheckDistance = 0.1f;
       
		Rigidbody m_Rigidbody;
		Animator m_Animator;
		bool m_IsGrounded;
		float m_OrigGroundCheckDistance;
		const float k_Half = 0.5f;
		float m_TurnAmount;
		float m_ForwardAmount;
		Vector3 m_GroundNormal;
		float m_CapsuleHeight;
		Vector3 m_CapsuleCenter;
		CapsuleCollider m_Capsule;
		bool m_Crouching;

		void Start()
		{
            m_Animator = GetComponent<Animator>();
			m_Rigidbody = GetComponent<Rigidbody>();
			m_Capsule = GetComponent<CapsuleCollider>();
			m_CapsuleHeight = m_Capsule.height;
			m_CapsuleCenter = m_Capsule.center;

			m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
			m_OrigGroundCheckDistance = m_GroundCheckDistance;
		}


		public void Move(Vector3 move, bool crouch, bool jump)
		{
            CheckGroundStatus();

            // control and velocity handling is different when grounded and airborne:
            if (m_IsGrounded)
			{
                move = Vector3.ProjectOnPlane(move, m_GroundNormal);
                HandleGroundedMovement(move, crouch, jump);
			}
			else
			{
				HandleAirborneMovement();
			}

            // convert the world relative moveInput vector into a local-relative
            // turn amount and forward amount required to head in the desired
            // direction.   
            Vector3 loacalMove = transform.InverseTransformDirection(move);

            m_TurnAmount = Mathf.Atan2(loacalMove.x, loacalMove.z);
            m_ForwardAmount = loacalMove.z;

            ApplyExtraTurnRotation();

            ManageCrouching(crouch);			

			// send input and other state parameters to the animator
			UpdateAnimator(loacalMove);
		}

        void ManageCrouching(bool crouch) 
        {
            if (m_IsGrounded && crouch)
            {                 
                m_Crouching = true;
            }
            else
            {
                m_Crouching = PreventStandingInLowHeadroom();
            }
        }

        bool PreventStandingInLowHeadroom()
        {
            if(m_Crouching)
            {
                Ray crouchRay = new Ray(m_Capsule.center + transform.position, Vector3.up);
                float crouchRayLength = m_CapsuleHeight * k_Half;

#if UNITY_EDITOR
                Debug.DrawLine(crouchRay.origin, crouchRay.origin + crouchRay.direction * crouchRayLength, Color.cyan, 0.5f, true);
#endif

                if (Physics.SphereCast(crouchRay, m_Capsule.radius * k_Half, crouchRayLength, ~0, QueryTriggerInteraction.Ignore))
                {
                    return true; //cannot stand
                }
                else
                {
                    return false; //can stand
                }
            }
            return false; //already standing
        }

        void UpdateAnimator(Vector3 move)
		{
			// update the animator parameters
			m_Animator.SetFloat("Forward", m_ForwardAmount, 0.1f, Time.deltaTime);
			m_Animator.SetFloat("Turn", m_TurnAmount, 0.1f, Time.deltaTime);
			m_Animator.SetBool("Crouch", m_Crouching);
			m_Animator.SetBool("OnGround", m_IsGrounded);
			if (!m_IsGrounded)
			{
				m_Animator.SetFloat("Jump", m_Rigidbody.velocity.y);
			}

			// calculate which leg is behind, so as to leave that leg trailing in the jump animation
			// (This code is reliant on the specific run cycle offset in our animations,
			// and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
			float runCycle =
				Mathf.Repeat(
					m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime + m_RunCycleLegOffset, 1);
			float jumpLeg = (runCycle < k_Half ? 1 : -1) * m_ForwardAmount;
			if (m_IsGrounded)
			{
				m_Animator.SetFloat("JumpLeg", jumpLeg);
			}

			// the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
			// which affects the movement speed because of the root motion.
			if (m_IsGrounded && move.magnitude > 0)
			{
				m_Animator.speed = m_AnimSpeedMultiplier;
			}
			else
			{
				// don't use that while airborne
				m_Animator.speed = 1;
			}
		}

		void HandleAirborneMovement()
		{
			// apply extra gravity from multiplier:
			Vector3 extraGravityForce = (Physics.gravity * m_GravityMultiplier) - Physics.gravity;
			m_Rigidbody.AddForce(extraGravityForce);

            //m_Rigidbody.AddForce(move * m_airControl);
            m_GroundCheckDistance = m_Rigidbody.velocity.y < 0 ? m_OrigGroundCheckDistance : 0.01f;
		}


		void HandleGroundedMovement(Vector3 move, bool crouch, bool jump)
		{
            // check whether conditions are right to allow a jump:
            if (jump && !crouch && m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Grounded"))
			{
				// jump!
				m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, m_JumpPower, m_Rigidbody.velocity.z);
				m_IsGrounded = false;
				m_Animator.applyRootMotion = false;
				m_GroundCheckDistance = 0.1f;
			}
		}

		void ApplyExtraTurnRotation()
		{
			// help the character turn faster (this is in addition to root rotation in the animation)
			float turnSpeed = Mathf.Lerp(m_StationaryTurnSpeed, m_MovingTurnSpeed, m_ForwardAmount);
			transform.Rotate(0, m_TurnAmount * turnSpeed * Time.deltaTime, 0);
		}
        
		public void OnAnimatorMove()
		{
			// we implement this function to override the default root motion.
			// this allows us to modify the positional speed before it's applied.
            
			if (m_IsGrounded && Time.deltaTime > 0)
			{
                Vector3 v = (m_Animator.deltaPosition * m_MoveSpeedMultiplier) / Time.deltaTime;
                
                // we preserve the existing y part of the current velocity.
                v.y = m_Rigidbody.velocity.y;
                m_Rigidbody.velocity = v;
            }
        }

        void CheckGroundStatus()
        {   
            RaycastHit hitInfo;
            if (Physics.Raycast(transform.position + m_Capsule.center, Vector3.down, out hitInfo, (m_Capsule.height / 2.0f) + m_GroundCheckDistance))
            {
                m_GroundNormal = hitInfo.normal;
                m_IsGrounded = true;
                m_Animator.applyRootMotion = true;

#if UNITY_EDITOR
                // helper to visualise the ground check ray in the scene view
                Debug.DrawLine(transform.position + m_Capsule.center, hitInfo.point, Color.black, 0.1f, true);
#endif
            }
            else
            {
                m_IsGrounded = false;
                m_GroundNormal = Vector3.up;
                m_Animator.applyRootMotion = false;

#if UNITY_EDITOR
                // helper to visualise the ground check ray in the scene view
                Debug.DrawLine(transform.position + m_Capsule.center, (transform.position + m_Capsule.center) + Vector3.down * ((m_Capsule.height / 2.0f) + m_GroundCheckDistance), Color.white, 0.1f, true);
#endif
            }
        }
    }
}
