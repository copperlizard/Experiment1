using UnityEngine;
using System.Collections;

public class FootFallIKmanager : MonoBehaviour
{
    public GameObject m_camera;
       
    public float m_footHeight, m_maxFootFall = 0.5f, m_maxFootLift = 0.5f, m_surfOffset = 0.1f, m_sinkDamp = 0.05f, m_checkSteps = 100.0f, m_footWeightOffset = 0.1f, m_headTurnSpeed = 10.0f;    

    private Animator m_anim;
    private AnimatorStateInfo m_baseLayerState;

    private CapsuleCollider m_playerCol;

    //private Rigidbody m_playerRB;

    private Transform m_leftFootBone, m_rightFootBone, m_headBone;
    private Vector3 m_leftFootTarPos, m_rightFootTarPos, m_ColStartCenter, m_headLookTarPos;
    private Quaternion m_leftFootTarRot, m_rightFootTarRot;    

    private RaycastHit m_interAtLeftFoot, m_interAtRightFoot;

    private float m_ColStartHeight, m_leftSteps, m_rightSteps, m_sink;   
    private bool m_leftFootInter, m_rightFootInter;

    

    // Use this for initialization
    void Start ()
    {
        m_anim = GetComponent<Animator>();

        m_leftFootBone = m_anim.GetBoneTransform(HumanBodyBones.LeftFoot);
        m_rightFootBone = m_anim.GetBoneTransform(HumanBodyBones.RightFoot);
        m_headBone = m_anim.GetBoneTransform(HumanBodyBones.Head);               

        m_playerCol = GetComponent<CapsuleCollider>();
        m_ColStartHeight = m_playerCol.height;
        m_ColStartCenter = m_playerCol.center;

        //m_playerRB = GetComponent<Rigidbody>(); //for finding player speed...
    }
	
	// Update is called once per frame
	void Update ()
    {
        m_baseLayerState = m_anim.GetCurrentAnimatorStateInfo(0);

        /*
        if (m_baseLayerState.IsName("Base Layer.Airborne"))
        {
            Debug.Log("Base Layer.Airborne");
        }
        else if (m_baseLayerState.IsName("Base Layer.Grounded"))
        {
            Debug.Log("Base Layer.Grounded");
        }
        else if (m_baseLayerState.IsName("Base Layer.Crouching"))
        {
            Debug.Log("Base Layer.Crouching");
        }
        */
    }

    void FixedUpdate()
    {
        AdjustCollider();
    }


    //DEBUG STUFF!!! DELETE LATER!!!
    float lastSink = 0.0f;
    float maxdiff = 0.0f;

    void CheckFeet()
    {
        //Update feet tar pos and rot
        m_leftFootTarPos = m_leftFootBone.position;
        m_leftFootTarRot = m_leftFootBone.rotation;
        m_rightFootTarPos = m_rightFootBone.position;
        m_rightFootTarRot = m_rightFootBone.rotation;

        //Find lifted feet pos
        Vector3 leftFootLiftedPos = m_leftFootTarPos + new Vector3(0.0f, m_maxFootLift + m_surfOffset - m_footHeight, 0.0f);
        Vector3 rightFootLiftedPos = m_rightFootTarPos + new Vector3(0.0f, m_maxFootLift + m_surfOffset - m_footHeight, 0.0f);       

        //Check feet
        m_leftFootInter = Physics.Raycast(leftFootLiftedPos, Vector3.down, out m_interAtLeftFoot, m_maxFootFall + m_maxFootLift);
        m_rightFootInter = Physics.Raycast(rightFootLiftedPos, Vector3.down, out m_interAtRightFoot, m_maxFootFall + m_maxFootLift);
        
        //Move left foot target
        if (m_leftFootInter)
        {
            if(m_leftSteps < m_checkSteps)
            {
                m_leftSteps++;
            }
            m_leftFootTarPos = m_interAtLeftFoot.point + (m_interAtLeftFoot.normal * m_surfOffset);            
            m_leftFootTarRot = Quaternion.FromToRotation(Vector3.up, m_interAtLeftFoot.normal) * transform.rotation;                      
        }
        else
        {
            if(m_leftSteps > 0.0f)
            {
                m_leftSteps = 0.0f;
            }
        }        
        
        //Move right foot target
        if (m_rightFootInter)
        {
            if(m_rightSteps < m_checkSteps)
            {
                m_rightSteps++;
            }
            m_rightFootTarPos = m_interAtRightFoot.point + (m_interAtRightFoot.normal * m_surfOffset);            
            m_rightFootTarRot = Quaternion.FromToRotation(Vector3.up, m_interAtRightFoot.normal) * transform.rotation;
        }
        else
        {
            if(m_rightSteps > 0.0f)
            {
                m_rightSteps = 0.0f;
            }
        } 
        
#if UNITY_EDITOR
        Debug.DrawLine(leftFootLiftedPos, m_leftFootTarPos, (m_leftFootInter) ? Color.red : Color.white, 0.5f, false);
        Debug.DrawLine(rightFootLiftedPos, m_rightFootTarPos, (m_rightFootInter) ? Color.blue : Color.white, 0.5f, false);
#endif         
    }

    void AdjustCollider()
    {
        //Find animation's current foot weights
        float leftFootWeight = m_anim.GetFloat("LeftFoot");
        float rightFootWeight = m_anim.GetFloat("RightFoot");

        //Find "heavier" foot distance from ground (moving)
        if (leftFootWeight > rightFootWeight + m_footWeightOffset && m_leftFootInter)
        {
            m_sink = Mathf.Clamp(Mathf.Lerp(m_sink, m_interAtLeftFoot.distance - m_maxFootLift, m_sinkDamp), 0.0f, m_maxFootFall + m_maxFootLift);
        }
        else if (rightFootWeight > leftFootWeight + m_footWeightOffset && m_rightFootInter)
        {
            m_sink = Mathf.Clamp(Mathf.Lerp(m_sink, m_interAtRightFoot.distance - m_maxFootLift, m_sinkDamp), 0.0f, m_maxFootFall + m_maxFootLift);
        }
        else
        {
            //Find lower foot (stationary)
            if (m_leftFootTarPos.y < m_rightFootTarPos.y && m_leftFootInter)
            {
                m_sink = Mathf.Clamp(Mathf.Lerp(m_sink, m_interAtLeftFoot.distance - m_maxFootLift, m_sinkDamp), 0.0f, m_maxFootFall + m_maxFootLift);
            }
            else if (m_rightFootInter)
            {
                m_sink = Mathf.Clamp(Mathf.Lerp(m_sink, m_interAtRightFoot.distance - m_maxFootLift, m_sinkDamp), 0.0f, m_maxFootFall + m_maxFootLift);
            }
            else
            {
                //Debug.Log("No feet!");

                m_sink = Mathf.Lerp(m_sink, 0.0f, m_sinkDamp);
            }
        }


        float difference = (m_sink - lastSink);

        if (Mathf.Abs(difference) > Mathf.Abs(maxdiff))
        {
            maxdiff = difference;
        }

        //Debug.Log("m_sink == " + m_sink.ToString() + " ; difference of == " + difference.ToString() + " ; max difference == " + maxdiff.ToString());

        lastSink = m_sink;

        //Adjust collider size to allow step 
        float weight = Mathf.Max(leftFootWeight, rightFootWeight);
        if (!m_baseLayerState.IsName("Base Layer.Crouching"))
        {
            m_playerCol.height = m_ColStartHeight - (m_sink * weight) + m_footHeight;
            m_playerCol.center = m_ColStartCenter + new Vector3(0.0f, m_sink * weight - m_footHeight, 0.0f);
        }
        else
        {
            m_playerCol.height = (m_ColStartHeight * 0.5f) - (m_sink * weight) + m_footHeight;
            m_playerCol.center = m_ColStartCenter + new Vector3(0.0f, (m_sink * weight) - (m_ColStartCenter.y * 0.5f) - m_footHeight, 0.0f);
        }
    }

    void HeadLook()
    {        
        m_headLookTarPos = m_headBone.position - (m_camera.transform.position - m_headBone.position);
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (!m_baseLayerState.IsName("Base Layer.Airborne"))
        {
            CheckFeet();
        }

        HeadLook();
        
        //Left foot
        m_anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, m_anim.GetFloat("LeftFoot") * (m_leftSteps / m_checkSteps));
        m_anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, m_anim.GetFloat("LeftFoot") * (m_leftSteps / m_checkSteps));
        m_anim.SetIKPosition(AvatarIKGoal.LeftFoot, m_leftFootTarPos);
        m_anim.SetIKRotation(AvatarIKGoal.LeftFoot, m_leftFootTarRot);

        //Right foot
        m_anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, m_anim.GetFloat("RightFoot") * (m_rightSteps / m_checkSteps));
        m_anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, m_anim.GetFloat("RightFoot") * (m_rightSteps / m_checkSteps));
        m_anim.SetIKPosition(AvatarIKGoal.RightFoot, m_rightFootTarPos);
        m_anim.SetIKRotation(AvatarIKGoal.RightFoot, m_rightFootTarRot);
        
        //Head
        m_anim.SetLookAtWeight(1.0f);
        m_anim.SetLookAtPosition(m_headLookTarPos);
    }


#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(m_leftFootBone.position, 0.05f);
            Gizmos.DrawSphere(m_rightFootBone.position, 0.05f);
        }
    }
#endif
}
