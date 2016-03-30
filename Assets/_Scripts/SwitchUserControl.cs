using UnityEngine;
using System.Collections;

public class SwitchUserControl : MonoBehaviour
{
    public GameObject m_cam, m_player, m_camTar1, m_CamTar2;
    
    private UnityStandardAssets.Characters.ThirdPerson.ThirdPersonUserControl m_playerControl;
    private CannonUserControl m_cannonControl;
    private OrbitCam m_camControl;

    private float m_camDist;
    private bool m_inRange, m_switch, m_Tar1, m_Tar2;

    // Use this for initialization
    void Start ()
    {
        m_inRange = false;        
        m_switch = false;
        m_Tar1 = true;
        m_Tar2 = false;

        m_playerControl = m_player.GetComponent<UnityStandardAssets.Characters.ThirdPerson.ThirdPersonUserControl>();
        m_cannonControl = GetComponent<CannonUserControl>();

        m_cannonControl.enabled = false;

        m_camControl = m_cam.GetComponent<OrbitCam>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if(m_inRange)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                m_switch = true;
            }
        } 	
	}

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            m_inRange = true;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if(other.tag == "Player")
        {
            if(m_switch)
            {
                if(!m_Tar1)
                {
                    m_Tar1 = true;
                    m_Tar2 = false;

                    m_playerControl.m_ForceIdle = false;

                    m_cannonControl.enabled = false;

                    m_camControl.SetCamDist(m_camDist);
                    m_camControl.m_target = m_camTar1;
                }
                else if(!m_Tar2)
                {
                    m_Tar2 = true;
                    m_Tar1 = false;

                    m_playerControl.m_ForceIdle = true;

                    m_cannonControl.enabled = true;

                    m_camDist = m_camControl.GetCamDist();
                    m_camControl.SetCamDist(0.0f);
                    m_camControl.m_target = m_CamTar2;
                }

                m_switch = false;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.tag == "Player")
        {
            m_inRange = false;
        }
    }
}
