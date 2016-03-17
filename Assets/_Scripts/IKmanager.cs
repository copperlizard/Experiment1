using UnityEngine;
using System.Collections;

public class IKmanager : MonoBehaviour
{
    public Transform m_leftFootTran, m_rightFootTran;
    public float m_maxFootFall = 0.5f, m_maxFootLift = 0.5f, m_feetOffset = 0.1f, m_sinkDamp = 0.1f;

    private Animator m_anim;
    private AnimatorStateInfo m_baseLayerState;

    private CapsuleCollider m_playerCol;

    private Transform m_playerTran;    

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

    void FixedUpdate()
    {
        if(!m_baseLayerState.IsName("Base Layer.Airborne"))
        {
            checkFeet();
        }        
    }

    static float sink = 0.0f;
    void checkFeet()
    {
        //Check feet
        Vector3 leftFootLiftedPos = m_leftFootTran.position + new Vector3(0.0f, m_maxFootLift, 0.0f);
        Vector3 rightFootLiftedPos = m_rightFootTran.position + new Vector3(0.0f, m_maxFootLift, 0.0f);

        m_leftFootInter = Physics.Raycast(leftFootLiftedPos, Vector3.down, out m_interAtLeftFoot, m_maxFootFall + m_maxFootLift);
        m_rightFootInter = Physics.Raycast(rightFootLiftedPos, Vector3.down, out m_interAtRightFoot, m_maxFootFall + m_maxFootLift);
        
        //Body shift
        if (m_interAtLeftFoot.distance > m_maxFootLift + m_feetOffset || m_interAtRightFoot.distance > m_maxFootLift + m_feetOffset)
        {            
            float maxStep = Mathf.Max(m_interAtLeftFoot.distance, m_interAtRightFoot.distance);        
            sink = Mathf.Lerp(sink, maxStep, m_sinkDamp);            
        }
        else if (sink > m_sinkDamp)
        {            
            sink = Mathf.Lerp(sink, 0.0f, m_sinkDamp);            
        }

        m_playerCol.height = m_ColStartHeight - sink;        
        
        //Move left foot target
        if (m_leftFootInter)
        {
#if UNITY_EDITOR   
            Debug.DrawLine(leftFootLiftedPos, leftFootLiftedPos + Vector3.down * m_interAtLeftFoot.distance, Color.red);
#endif
            m_leftFootTran.position = leftFootLiftedPos + Vector3.down * (m_interAtLeftFoot.distance - m_feetOffset);                      
            m_leftFootTran.rotation = Quaternion.FromToRotation(Vector3.up, m_interAtLeftFoot.normal) * transform.rotation;            
        }
        else
        {
            m_leftFootTran.position = new Vector3(m_leftFootTran.position.x, m_playerTran.position.y, m_leftFootTran.position.z);
            m_leftFootTran.rotation = transform.rotation;
        }
        
        //Move right foot target
        if (m_rightFootInter)
        {
#if UNITY_EDITOR
            Debug.DrawLine(rightFootLiftedPos, rightFootLiftedPos + Vector3.down * m_interAtRightFoot.distance, Color.blue);
#endif
            m_rightFootTran.position = rightFootLiftedPos + Vector3.down * (m_interAtRightFoot.distance - m_feetOffset);
            m_rightFootTran.rotation = Quaternion.FromToRotation(Vector3.up, m_interAtRightFoot.normal) * transform.rotation;
        }
        else
        {
            m_rightFootTran.position = new Vector3(m_rightFootTran.position.x, m_playerTran.position.y, m_rightFootTran.position.z);
            m_rightFootTran.rotation = transform.rotation;
        }
        
    }

    void OnAnimatorIK(int layerIndex)
    {
        //Left foot
        m_anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, m_anim.GetFloat("LeftFoot"));
        m_anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, m_anim.GetFloat("LeftFoot"));
        m_anim.SetIKPosition(AvatarIKGoal.LeftFoot, m_leftFootTran.position);
        m_anim.SetIKRotation(AvatarIKGoal.LeftFoot, m_leftFootTran.rotation);

        //Right foot
        m_anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, m_anim.GetFloat("RightFoot"));
        m_anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, m_anim.GetFloat("RightFoot"));
        m_anim.SetIKPosition(AvatarIKGoal.RightFoot, m_rightFootTran.position);
        m_anim.SetIKRotation(AvatarIKGoal.RightFoot, m_rightFootTran.rotation);
    }
}
