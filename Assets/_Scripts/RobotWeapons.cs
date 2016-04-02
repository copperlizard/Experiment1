using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(UnityStandardAssets.Characters.ThirdPerson.ThirdPersonUserControl))]
[RequireComponent(typeof(UnityStandardAssets.Characters.ThirdPerson.ThirdPersonCharacter))]
public class RobotWeapons : MonoBehaviour
{
    public List<GameObject> m_lazers = new List<GameObject>();

    public GameObject m_cam;
    public Transform m_camPivot;
    public Vector3 m_lazerAimOffset, m_pulseBlastAimOffset;
    public float m_lazerAimDist, m_pulseBlastAimDist, m_minTarDist = 1.0f, m_maxTarDist = 500.0f;
    public bool m_weaponsIdle = false;

    private List<LineRenderer> m_lazerRends = new List<LineRenderer>();

    private Animator m_anim;
    private Transform m_headBoneTran;    
    private OrbitCam m_camControl;
    private UnityStandardAssets.Characters.ThirdPerson.ThirdPersonCharacter m_playerControl;
    private UnityStandardAssets.Characters.ThirdPerson.ThirdPersonUserControl m_playerUserControl;
    private Vector3 m_camPivotStartPos, m_headLookTarPos;
    private float m_prevDist, m_turnCheck;
    private int m_wepMode = 0, m_numWepModes = 2;
    private bool m_fire1, m_fire2, m_1, m_2, m_jump, m_crouch, m_aimed = false, m_pulseBlasted = false;

    // Use this for initialization
    void Start ()
    {
        m_anim = GetComponent<Animator>();
        m_headBoneTran = m_anim.GetBoneTransform(HumanBodyBones.Head);              

        m_camControl = m_cam.GetComponent<OrbitCam>();
        m_camPivotStartPos = transform.InverseTransformPoint(m_camPivot.transform.position);

        m_playerControl = GetComponent<UnityStandardAssets.Characters.ThirdPerson.ThirdPersonCharacter>();
        m_playerUserControl = GetComponent<UnityStandardAssets.Characters.ThirdPerson.ThirdPersonUserControl>();

        //Fetch lazer renderers
        for (int i = 0; i < m_lazers.Count; i++)
        {
            LineRenderer rend = m_lazers[i].GetComponent<LineRenderer>();
            m_lazerRends.Add(rend);
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        m_fire1 = Input.GetButton("Fire1");
        m_fire2 = Input.GetButton("Fire2");

        if (!m_jump)
        {
            m_jump = Input.GetButtonDown("Jump");
        }

        m_crouch = Input.GetKey(KeyCode.C);

        //1 key press at a time; given to lowest number
        m_1 = Input.GetKeyDown(KeyCode.Alpha1);
        if(m_1)
        {
            SetWeaponMode(0);
        }
        else
        {
            m_2 = Input.GetKeyDown(KeyCode.Alpha2);
            if (m_2)
            {
                SetWeaponMode(1);
            }
        }
    }

    void OnAnimatorMove()
    {   
        if(!m_playerUserControl.m_ForceIdle)
        {
            HeadLook();
            ManageWeapon(m_fire1, m_fire2);

            m_weaponsIdle = false;
        }
        else if(!m_weaponsIdle)
        {
            ManageWeapon(false, false);
            m_weaponsIdle = true;
        }
     
    }

    void OnAnimatorIK(int layerIndex)
    {
        //Head
        m_anim.SetLookAtWeight(1.0f);
        m_anim.SetLookAtPosition(m_headLookTarPos);
    }

    void HeadLook()
    {
        RaycastHit hit;
        if (Physics.Raycast(m_headBoneTran.position, m_cam.transform.forward, out hit, m_maxTarDist))
        {
            m_headLookTarPos = hit.point;
        }
        else
        {
            m_headLookTarPos = m_cam.transform.position + m_cam.transform.forward * m_maxTarDist;
        }

        m_turnCheck = Vector3.Dot((m_headLookTarPos - transform.position), transform.forward);        
    }

    public bool SetWeaponMode(int mode)
    {
        if (mode > m_numWepModes - 1)
        {
            return false;
        }

        m_wepMode = mode;
        return true;
    }

    void CamAim(Vector3 offset, float dist)
    {
        m_camPivot.transform.localPosition = m_camPivotStartPos + offset;
        m_prevDist = m_camControl.GetCamDist();
        m_camControl.SetCamDist(dist);

        m_aimed = true;
    }

    void ResetCam()
    {
        m_camPivot.transform.position = transform.TransformPoint(m_camPivotStartPos);
        m_camControl.SetCamDist(m_prevDist);
    }

    void LazerEyes(bool fire1, bool fire2)
    {
        if(fire2)
        {
            //Aim            
            if(!m_aimed)
            {
                Debug.Log("Aim LazerEyes()");
                m_aimed = true;
                CamAim(m_lazerAimOffset, m_lazerAimDist);
            }
            
            //Ensure facing forward enough
            if(m_turnCheck < 0.0f)
            {
                            
            }            
            
            if(fire1)
            {
                //Fire
                Debug.Log("LAZERS!!!!");

                //Set lazer vert's and enable
                for(int i = 0; i < m_lazers.Count; i++)
                {
                    Debug.DrawLine(m_lazers[i].transform.position, m_headLookTarPos);

                    m_lazerRends[i].SetPosition(0, m_lazers[i].transform.position);
                    m_lazerRends[i].SetPosition(1, m_headLookTarPos);

                    if(!m_lazerRends[i].enabled)
                    {
                        m_lazerRends[i].enabled = true;
                    }                    
                }                
            }
            else
            {
                //Disable lazers
                for (int i = 0; i < m_lazers.Count; i++)
                {
                    if (m_lazerRends[i].enabled)
                    {
                        m_lazerRends[i].enabled = false;
                    }
                }
            }            
        }
        else if(m_aimed)
        {
            m_aimed = false;

            //Disable lazers
            for (int i = 0; i < m_lazers.Count; i++)
            {
                if (m_lazerRends[i].enabled)
                {
                    m_lazerRends[i].enabled = false;
                }
            }

            ResetCam();
        }
    }

    void PulseBlast(bool fire1, bool fire2)
    {
        if(fire2)
        {
            //Aim            
            if (!m_aimed)
            {
                Debug.Log("Aim PulseBlast");
                m_aimed = true;
                CamAim(m_pulseBlastAimOffset, m_pulseBlastAimDist);
            }

            //Ensure facing forward enough


            if (fire1 && !m_pulseBlasted)
            {
                //Fire
                m_pulseBlasted = true;
                Debug.Log("PULSE BLAST!!!");
            }
            else if(!fire1 && m_pulseBlasted)
            {
                m_pulseBlasted = false;
            }
        }
        else if (m_aimed)
        {
            m_aimed = false;
            ResetCam();
        }
    }

    void ManageWeapon(bool fire1, bool fire2)
    {
        switch (m_wepMode)
        {
            case 0:
                LazerEyes(fire1, fire2);
                break;
            case 1:
                PulseBlast(fire1, fire2);
                break;
            default:
                break;
        }
    }
}
