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
    private Vector3 m_leftFootTarPos, m_rightFootTarPos, m_ColStartCenter;
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
        m_ColStartCenter = m_playerCol.center;

        m_playerTran = GetComponentsInChildren<Transform>()[1]; //skipping first returned transform
    }
	
	// Update is called once per frame
	void Update ()
    {
        m_baseLayerState = m_anim.GetCurrentAnimatorStateInfo(0);

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
    }

    static float sink = 0.0f;
    void CheckFeet()
    {
        //Find lifted feet pos
        Vector3 leftFootLiftedPos = m_leftFootTarPos + new Vector3(0.0f, m_maxFootLift, 0.0f);
        Vector3 rightFootLiftedPos = m_rightFootTarPos + new Vector3(0.0f, m_maxFootLift, 0.0f);

        //Check feet
        m_leftFootInter = Physics.Raycast(leftFootLiftedPos, Vector3.down, out m_interAtLeftFoot, m_maxFootFall + m_maxFootLift);
        m_rightFootInter = Physics.Raycast(rightFootLiftedPos, Vector3.down, out m_interAtRightFoot, m_maxFootFall + m_maxFootLift);

        //Move left foot target
        if (m_leftFootInter)
        {
            m_leftFootTarPos = m_interAtLeftFoot.point + (m_interAtLeftFoot.normal * m_surfOffset);
            m_leftFootTarRot = Quaternion.FromToRotation(Vector3.up, m_interAtLeftFoot.normal) * transform.rotation;                      
        }        
        
        //Move right foot target
        if (m_rightFootInter)
        {
            m_rightFootTarPos = m_interAtRightFoot.point + (m_interAtRightFoot.normal * m_surfOffset);
            m_rightFootTarRot = Quaternion.FromToRotation(Vector3.up, m_interAtRightFoot.normal) * transform.rotation;
        }      

#if UNITY_EDITOR
        Debug.DrawLine(leftFootLiftedPos, m_leftFootTarPos, (m_leftFootInter) ? Color.red : Color.white, 0.5f, false);
        Debug.DrawLine(rightFootLiftedPos, m_rightFootTarPos, (m_rightFootInter) ? Color.blue : Color.white, 0.5f, false);
#endif

        float feetHeightDif = m_leftFootTarPos.y - m_rightFootTarPos.y;
        if(feetHeightDif < 0.0f)
        {
            feetHeightDif = -feetHeightDif;
        }
        sink = Mathf.Clamp(Mathf.Lerp(sink, feetHeightDif, m_sinkDamp), 0.0f, m_maxFootFall + m_maxFootLift);
        
        m_playerCol.height = m_ColStartHeight - sink;        
        m_playerCol.center = m_ColStartCenter + new Vector3(0.0f, sink, 0.0f);        
    }

    void OnAnimatorIK(int layerIndex)
    {
        //Update feet tar pos and rot
        m_leftFootTarPos = m_playerTran.position + (m_playerTran.rotation * m_leftFootOffset);
        m_rightFootTarPos = m_playerTran.position + (m_playerTran.rotation * m_rightFootOffset);
        m_leftFootTarRot = transform.rotation;
        m_rightFootTarRot = transform.rotation;

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

        //Debug.Log("m_leftFootTarPos == " + m_leftFootTarPos.ToString() + " ; m_rightFootTarPos == " + m_rightFootTarPos.ToString());

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
