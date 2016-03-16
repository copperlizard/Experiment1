using UnityEngine;
using System.Collections;

public class IKmanager : MonoBehaviour
{
    public Transform m_leftFootTran, m_rightFootTran;
    public float m_maxFootFall = 0.5f;
    public float m_maxFootLift = 0.5f;

    private Animator m_anim;
    private AnimatorStateInfo m_baseLayerState;

    private Transform m_parentTran;

    private RaycastHit m_interAtLeftFoot, m_interAtRightFoot;
    private bool m_leftFootInter, m_rightFootInter;

    

    // Use this for initialization
    void Start ()
    {
        m_anim = GetComponent<Animator>();

        m_parentTran = GetComponentInParent<Transform>();        
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
        checkFeet();
    }

    void checkFeet()
    {
        //Check feet
        Vector3 leftFootLiftedPos = new Vector3(m_leftFootTran.position.x, m_parentTran.position.y + m_maxFootLift, m_leftFootTran.position.z);
        Vector3 rightFootLiftedPos = new Vector3(m_rightFootTran.position.x, m_parentTran.position.y + m_maxFootLift, m_rightFootTran.position.z);

        //Debug.Log("leftFootPos == " + m_leftFootTran.position.ToString() + " ; rightFootPos == " + m_rightFootTran.position.ToString());
        //Debug.Log("lifted leftFootPos == " + leftFootLiftedPos.ToString() + " ; lifted rightFootPos == " + rightFootLiftedPos.ToString());

        m_leftFootInter = Physics.Raycast(leftFootLiftedPos, Vector3.down, out m_interAtLeftFoot, m_maxFootFall + m_maxFootLift);
        m_rightFootInter = Physics.Raycast(rightFootLiftedPos, Vector3.down, out m_interAtRightFoot, m_maxFootFall + m_maxFootLift);

        //Debug.Log("m_interAtLeftFoot.distance == " + m_interAtLeftFoot.distance.ToString() + " ; m_interAtRightFoot.distance == " + m_interAtRightFoot.distance.ToString());

        //Move left foot target
        if (m_leftFootInter)
        {            
            Debug.DrawLine(leftFootLiftedPos, leftFootLiftedPos + Vector3.down * m_interAtLeftFoot.distance, Color.red);
            m_leftFootTran.position = leftFootLiftedPos + Vector3.down * m_interAtLeftFoot.distance;            
        }
        else
        {
            m_leftFootTran.position = new Vector3(m_leftFootTran.position.x, m_parentTran.position.y, m_leftFootTran.position.z);
        }

        //Move right foot target
        if (m_rightFootInter)
        {
            Debug.DrawLine(rightFootLiftedPos, rightFootLiftedPos + Vector3.down * m_interAtRightFoot.distance, Color.blue);
        }
        else
        {
            
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
        m_anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, m_anim.GetFloat("LeftFoot"));
        m_anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, m_anim.GetFloat("LeftFoot"));
        m_anim.SetIKPosition(AvatarIKGoal.LeftFoot, m_leftFootTran.position);
        m_anim.SetIKRotation(AvatarIKGoal.LeftFoot, m_leftFootTran.rotation);
    }
}
