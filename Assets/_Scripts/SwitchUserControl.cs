using UnityEngine;
using System.Collections;

public class SwitchUserControl : MonoBehaviour
{
    public GameObject m_cam, m_player, m_camTar1, m_CamTar2;
    

    private UnityStandardAssets.Characters.ThirdPerson.ThirdPersonUserControl m_playerControl;
    private CannonUserControl m_cannonControl;
    private OrbitCam m_camControl;

    private float m_lastCamDist;
    private bool m_switch, m_pressed, m_Tar1, m_Tar2;

    // Use this for initialization
    void Start ()
    {        
        m_switch = false;
        m_pressed = false;
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
        if(Input.GetKeyDown(KeyCode.E) && !m_pressed)
        {            
            Debug.Log("tick");

            m_switch = !m_switch;
            m_pressed = true;
                        
        }
        else if(m_pressed)
        {
            Debug.Log("tock");
            
            m_pressed = false;
        }        	
	}

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {

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

                    m_cannonControl.enabled = false;
                    m_playerControl.enabled = true;                    

                    m_camControl.m_target = m_camTar1;
                }
                else if(!m_Tar2)
                {
                    m_Tar2 = true;
                    m_Tar1 = false;

                    m_cannonControl.enabled = true;
                    m_playerControl.enabled = false;

                    m_camControl.m_target = m_CamTar2;
                }
            }
        }
    }
}
