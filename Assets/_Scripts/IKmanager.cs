using UnityEngine;
using System.Collections;

public class IKmanager : MonoBehaviour
{
    //public Transform m_leftFootTran, m_rightFootTran;
    public float m_maxFootFall = 0.5f, m_maxFootLift = 0.5f, m_surfOffset = 0.1f, m_sinkDamp = 0.1f;
    public Vector3 m_leftFootOffset, m_rightFootOffset;

    private Animator m_anim;
    private AnimatorStateInfo m_baseLayerState;

    private CapsuleCollider m_playerCol;

    private Transform m_playerTran, m_leftFootBone, m_rightFootBone;
    private Vector3 m_leftFootTarPos, m_rightFootTarPos;
    private Quaternion m_leftFootTarRot, m_rightFootTarRot;    

    private RaycastHit m_interAtLeftFoot, m_interAtRightFoot;
    private float m_ColStartHeight;   
    private bool m_leftFootInter, m_rightFootInter;

    

    // Use this for initialization
    void Start ()
    {
        m_anim = GetComponent<Animator>();       

        m_playerCol = GetComponent<CapsuleCollider>();
        m_ColStartHeight = m_playerCol.height;

        m_playerTran = GetComponentsInChildren<Transform>()[1]; //skipping first returned transform       

        //Foot Bones appear to move outside of this script...
        //using to establish feet reference positions (from model's T-pose)
        //m_leftFootBone = m_anim.GetBoneTransform(HumanBodyBones.LeftFoot);
        //m_rightFootBone = m_anim.GetBoneTransform(HumanBodyBones.RightFoot);        

        //m_leftFootOffset = m_leftFootBone.position - m_playerTran.position;
        //m_rightFootOffset = m_rightFootBone.position - m_playerTran.position;

        /*
        //for debug, delete later
        m_leftFootTarPos = m_playerTran.position + (m_playerTran.rotation * m_leftFootOffset);
        m_leftFootTarRot = m_leftFootBone.rotation;
        m_rightFootTarPos = m_playerTran.position + (m_playerTran.rotation * m_rightFootOffset);        
        m_rightFootTarRot = m_rightFootBone.rotation;

        Debug.Log("\nStart()");
        Debug.Log("m_leftFootBone.position == " + m_leftFootBone.position.ToString() + " ; m_leftFootBone.rotation == " + m_leftFootBone.rotation.ToString() + " ; m_leftFootOffset == " + m_leftFootOffset.ToString());
        Debug.Log("m_rightFootBone.position == " + m_rightFootBone.position.ToString() + " ; m_rightFootBone.rotation == " + m_rightFootBone.rotation.ToString() + " ; m_rightFootOffset == " + m_rightFootOffset.ToString());
        Debug.Log("m_leftFootTarPos == " + m_leftFootTarPos.ToString() + " ; m_rightFootTarPos == " + m_rightFootTarPos.ToString());
        */
    }
	
	// Update is called once per frame
	void Update ()
    {
        m_baseLayerState = m_anim.GetCurrentAnimatorStateInfo(0);        

        /*
        if (m_baseLayerState.IsName("Base Layer.Grounded"))
        {
            Debug.Log("Grounded!");
        }
        else if (m_baseLayerState.IsName("Base Layer.Airborne"))
        {
            Debug.Log("Airborne!");
        }
        else if (m_baseLayerState.IsName("Base Layer.Crouching"))
        {
            Debug.Log("Crouching!");
        }
        */
        
    }

    static float sink = 0.0f;
    void CheckFeet()
    {
        //Update feet pos
        m_leftFootTarPos = m_playerTran.position + (m_playerTran.rotation * m_leftFootOffset);        
        m_rightFootTarPos = m_playerTran.position + (m_playerTran.rotation * m_rightFootOffset);

        //Debug.Log("m_leftFootTarPos == " + m_leftFootTarPos.ToString() + " ; m_rightFootTarPos == " + m_rightFootTarPos.ToString());

        //Find lifted feet pos
        Vector3 leftFootLiftedPos = m_leftFootTarPos + new Vector3(0.0f, m_maxFootLift, 0.0f);
        Vector3 rightFootLiftedPos = m_rightFootTarPos + new Vector3(0.0f, m_maxFootLift, 0.0f);

        //Debug.Log("leftFootLiftedPos == " + leftFootLiftedPos.ToString() + " ; rightFootLiftedPos == " + rightFootLiftedPos.ToString());

        //Check feet
        m_leftFootInter = Physics.Raycast(leftFootLiftedPos, Vector3.down, out m_interAtLeftFoot, m_maxFootFall + m_maxFootLift);
        m_rightFootInter = Physics.Raycast(rightFootLiftedPos, Vector3.down, out m_interAtRightFoot, m_maxFootFall + m_maxFootLift);
       
        //Move left foot target
        if (m_leftFootInter)
        {
            m_leftFootTarPos = m_interAtLeftFoot.point;

#if UNITY_EDITOR   
            //Debug.DrawLine(leftFootLiftedPos, leftFootLiftedPos + Vector3.down * m_interAtLeftFoot.distance, Color.red);
            Debug.DrawLine(leftFootLiftedPos, m_leftFootTarPos, Color.red);
#endif
            //m_leftFootTarPos = leftFootLiftedPos + Vector3.down * (m_interAtLeftFoot.distance);
            //m_leftFootTran.position += m_interAtLeftFoot.normal * m_surfOffset;                      
            m_leftFootTarRot = Quaternion.FromToRotation(Vector3.up, m_interAtLeftFoot.normal) * transform.rotation;                      
        }
        else
        {
            //m_leftFootTran.position = new Vector3(m_leftFootTran.position.x, m_playerTran.position.y, m_leftFootTran.position.z);
            m_leftFootTarRot = transform.rotation;
        }
        
        //Move right foot target
        if (m_rightFootInter)
        {
            m_rightFootTarPos = m_interAtRightFoot.point;

#if UNITY_EDITOR
            //Debug.DrawLine(rightFootLiftedPos, rightFootLiftedPos + Vector3.down * m_interAtRightFoot.distance, Color.blue);
            Debug.DrawLine(rightFootLiftedPos, m_rightFootTarPos, Color.blue);
#endif
            //m_rightFootTarPos = rightFootLiftedPos + Vector3.down * (m_interAtRightFoot.distance);
            //m_rightFootTran.position += m_interAtRightFoot.normal * m_surfOffset;
            m_rightFootTarRot = Quaternion.FromToRotation(Vector3.up, m_interAtRightFoot.normal) * transform.rotation;
        }
        else
        {
            //m_rightFootTran.position = new Vector3(m_rightFootTran.position.x, m_playerTran.position.y, m_rightFootTran.position.z);
            m_rightFootTarRot = transform.rotation;
        }

        //Debug.Log("m_leftFootTarPos == " + m_leftFootTarPos.ToString() + " ; m_rightFootTarPos == " + m_rightFootTarPos.ToString());

        float footHeightDif = m_leftFootTarPos.y - m_rightFootTarPos.y;
        if (footHeightDif < 0.0f)
        {
            footHeightDif = -footHeightDif;
        }

        if(footHeightDif > m_sinkDamp)
        {
            sink = Mathf.Lerp(sink, footHeightDif, m_sinkDamp);
        }
        else
        {
            if(sink > m_sinkDamp)
            {
                sink = Mathf.Lerp(sink, 0.0f, m_sinkDamp);
            }
            else
            {
                sink = 0.0f;
            }            
        }
        //m_playerCol.height = m_ColStartHeight - sink;
        //m_playerCol.height = m_ColStartHeight - 0.2f;
        

        /*
        Debug.Log("\nCheckFeet()");
        Debug.Log("m_leftFootBone.position == " + m_leftFootBone.position.ToString() + " ; m_leftFootBone.rotation == " + m_leftFootBone.rotation.ToString() + " ; m_leftFootOffset == " + m_leftFootOffset.ToString());
        Debug.Log("m_rightFootBone.position == " + m_rightFootBone.position.ToString() + " ; m_rightFootBone.rotation == " + m_rightFootBone.rotation.ToString() + " ; m_rightFootOffset == " + m_rightFootOffset.ToString());
        Debug.Log("m_leftFootTarPos == " + m_leftFootTarPos.ToString() + " ; m_rightFootTarPos == " + m_rightFootTarPos.ToString());
        */
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (!m_baseLayerState.IsName("Base Layer.Airborne"))
        {
            CheckFeet();
        }

        /*
        Debug.Log("\nOnAnimatorIK()");
        Debug.Log("m_leftFootBone.position == " + m_leftFootBone.position.ToString() + " ; m_leftFootBone.rotation == " + m_leftFootBone.rotation.ToString() + " ; m_leftFootOffset == " + m_leftFootOffset.ToString());
        Debug.Log("m_rightFootBone.position == " + m_rightFootBone.position.ToString() + " ; m_rightFootBone.rotation == " + m_rightFootBone.rotation.ToString() + " ; m_rightFootOffset == " + m_rightFootOffset.ToString());
        Debug.Log("m_leftFootTarPos == " + m_leftFootTarPos.ToString() + " ; m_rightFootTarPos == " + m_rightFootTarPos.ToString());
        */

        Debug.Log("m_leftFootTarPos == " + m_leftFootTarPos.ToString() + " ; m_rightFootTarPos == " + m_rightFootTarPos.ToString());

        //Left foot
        m_anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, m_anim.GetFloat("LeftFoot"));
        m_anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, m_anim.GetFloat("LeftFoot"));
        m_anim.SetIKPosition(AvatarIKGoal.LeftFoot, m_leftFootTarPos);
        m_anim.SetIKRotation(AvatarIKGoal.LeftFoot, m_leftFootTarRot);

        //Right foot
        m_anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, m_anim.GetFloat("RightFoot"));
        m_anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, m_anim.GetFloat("RightFoot"));
        m_anim.SetIKPosition(AvatarIKGoal.RightFoot, m_rightFootTarPos);
        m_anim.SetIKRotation(AvatarIKGoal.RightFoot, m_rightFootTarRot);
    }
}
