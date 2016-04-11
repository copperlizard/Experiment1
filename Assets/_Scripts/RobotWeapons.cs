using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(UnityStandardAssets.Characters.ThirdPerson.ThirdPersonUserControl))]
public class RobotWeapons : MonoBehaviour
{
    public GameObject m_cam, m_lazerHit;
    public List<GameObject> m_lazers = new List<GameObject>();

    [System.Serializable]
    public class Audio
    {
        public AudioSource m_audioSource;
        public AudioClip m_lazer;
        public float m_lazerPitchSlideRate = 0.1f;
    }
    public Audio m_audio = new Audio();

    public ObjectPool m_pulseBlastAmmo, m_lazerBurns;
    
    public Transform m_camPivot, m_pulseBlastFireTran;
    public Vector3 m_lazerAimOffset, m_pulseBlastAimOffset;
    public float m_lazerForce = 30.0f, m_lazerAimDist, m_lazerBurnSepDist = 0.025f, m_pulseBlastAimDist, m_pulseBlastVelocity = 3.0f, m_minTarDist = 1.0f, 
        m_maxTarDist = 500.0f, m_aimingPlayerTurnSpeed = 20.0f, m_aimingPlayerTurnInertia = 0.1f, m_handAimSpeed = 0.1f;
    public bool m_weaponsIdle = false;

    private List<LineRenderer> m_lazerRends = new List<LineRenderer>();

    private Animator m_anim;
    private Transform m_headBoneTran, m_spineTran;
    private OrbitCam m_camControl;
    private UnityStandardAssets.Characters.ThirdPerson.ThirdPersonUserControl m_playerUserControl;
    private RaycastHit m_hit;
    private Vector3 m_camPivotStartPos, m_headLookTarPos, m_normAtLookTarPos, m_lastBurnPos;
    private float m_prevDist, m_turnCheck, m_turning, m_handAimWeight;
    private int m_wepMode = 0, m_numWepModes = 2;
    private bool m_fire1, m_fire2, m_1, m_2, m_aimed = false, m_pulseBlasted = false, m_headTar;

    // Use this for initialization
    void Start ()
    {
        m_anim = GetComponent<Animator>();
        m_headBoneTran = m_anim.GetBoneTransform(HumanBodyBones.Head);        
        m_spineTran = m_anim.GetBoneTransform(HumanBodyBones.Spine);                             

        m_camControl = m_cam.GetComponent<OrbitCam>();
        m_camPivotStartPos = transform.InverseTransformPoint(m_camPivot.transform.position);
        
        m_playerUserControl = GetComponent<UnityStandardAssets.Characters.ThirdPerson.ThirdPersonUserControl>();

        //Fetch lazer renderers
        for (int i = 0; i < m_lazers.Count; i++)
        {
            LineRenderer rend = m_lazers[i].GetComponent<LineRenderer>();
            m_lazerRends.Add(rend);
        }

        //Set default sound
        m_audio.m_audioSource.clip = m_audio.m_lazer;

        //Fake last burn pos
        m_lastBurnPos = new Vector3(1000.0f, -1000.0f, 1000.0f);
    }
	
	// Update is called once per frame
	void Update ()
    {
        m_fire1 = Input.GetButton("Fire1");
        m_fire2 = Input.GetButton("Fire2");        

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
            RotatePlayer();
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

        m_anim.SetIKPositionWeight(AvatarIKGoal.RightHand, m_handAimWeight);
        m_anim.SetIKRotationWeight(AvatarIKGoal.RightHand, m_handAimWeight);
        m_anim.SetIKPosition(AvatarIKGoal.RightHand, m_headLookTarPos);         
        m_anim.SetIKRotation(AvatarIKGoal.RightHand, m_cam.transform.rotation);

        
        //Twist Spine
        if (m_fire2 && m_wepMode == 1 && m_turnCheck > 0.0f)
        {
            float turnLeftCheck = Mathf.Clamp( Vector3.Dot(m_cam.transform.forward, -transform.right), 0.0f, 1.0f);

            if(turnLeftCheck > 0.0f)
            {                
                m_spineTran.transform.RotateAround(m_spineTran.transform.position, transform.up, m_cam.transform.rotation.eulerAngles.y - transform.rotation.eulerAngles.y);
                m_anim.SetBoneLocalRotation(HumanBodyBones.Spine, m_spineTran.localRotation);
            }            
        }
        
    }

    void HeadLook()
    {
        m_headTar = Physics.Raycast(m_headBoneTran.position, m_cam.transform.forward, out m_hit, m_maxTarDist);
        if (m_headTar)
        {
            m_headLookTarPos = m_hit.point;
            m_normAtLookTarPos = m_hit.normal;
        }
        else
        {
            m_headLookTarPos = m_cam.transform.position + m_cam.transform.forward * m_maxTarDist;
            m_normAtLookTarPos = Vector3.up;
        }

        m_turnCheck = Vector3.Dot((m_headLookTarPos - transform.position).normalized, transform.forward);        
    }

    void RotatePlayer()
    {
        if(m_fire2)
        {
            Vector3 move = m_anim.deltaPosition.normalized;
            
            if(Vector3.Dot(m_cam.transform.forward, move) > 0.0f || move.magnitude == 0.0f)
            {
                if (m_turnCheck < 0.0f)
                {
                    m_turning = Mathf.Lerp(m_turning, 1.0f, m_aimingPlayerTurnInertia);
                }
                else
                {
                    m_turning = Mathf.Lerp(m_turning, 0.0f, m_aimingPlayerTurnInertia);
                }

                if (m_turning > 0.001f)
                {
                    float rigthCheck = Vector3.Dot((m_headLookTarPos - transform.position).normalized, transform.right);

                    if (rigthCheck >= 0.0f)
                    {
                        //Debug.Log("turn right");
                        transform.RotateAround(transform.position, transform.up, Mathf.Max(m_aimingPlayerTurnSpeed * -m_turnCheck, 0.1f));
                    }
                    else
                    {
                        //Debug.Log("turn left");
                        transform.RotateAround(transform.position, transform.up, -Mathf.Max(m_aimingPlayerTurnSpeed * -m_turnCheck, 0.1f));
                    }
                }
            }
        }
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
        m_camPivot.transform.RotateAround(transform.position, transform.up, m_cam.transform.rotation.eulerAngles.y - transform.rotation.eulerAngles.y);

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
            if (!m_aimed)
            {
                //Debug.Log("Aim LazerEyes()");
                m_aimed = true;

                m_prevDist = m_camControl.GetCamDist();
            }

            //Aim    
            CamAim(m_lazerAimOffset, m_lazerAimDist);

            //Rest hand
            m_handAimWeight = Mathf.Lerp(m_handAimWeight, 0.0f, m_handAimSpeed);

            if (fire1 && m_turnCheck > 0.0f)
            {
                //Fire
                //Debug.Log("LAZERS!!!!");

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

                //Visualize hit
                if (m_headTar)
                {                       
                    m_lazerHit.transform.position = m_headLookTarPos;
                    m_lazerHit.transform.rotation = Quaternion.LookRotation(transform.forward, m_normAtLookTarPos);
                    if (!m_lazerHit.activeInHierarchy)
                    {
                        m_lazerHit.SetActive(true);
                    }

                    //Scorch mark
                    if ((m_headLookTarPos - m_lastBurnPos).magnitude > m_lazerBurnSepDist)
                    {
                        m_lastBurnPos = m_headLookTarPos;

                        GameObject burn = m_lazerBurns.GetObject();
                        burn.transform.position = m_headLookTarPos + m_normAtLookTarPos * 0.01f;
                        burn.transform.rotation = Quaternion.LookRotation(-m_normAtLookTarPos, transform.forward);
                        burn.SetActive(true);
                        burn.transform.parent = m_hit.transform;
                    }
                }              

                //LazerSound
                if (!m_audio.m_audioSource.isPlaying)
                {
                    m_audio.m_audioSource.Play();
                }               

                if(m_headTar && m_hit.rigidbody != null)
                {
                    m_hit.collider.attachedRigidbody.AddForceAtPosition((m_headLookTarPos - m_headBoneTran.position).normalized * m_lazerForce, m_headLookTarPos);
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

                if (m_lazerHit.activeInHierarchy)
                {
                    m_lazerHit.SetActive(false);
                }

                if (m_audio.m_audioSource.isPlaying)
                {
                    m_audio.m_audioSource.Stop();
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

            if (m_lazerHit.activeInHierarchy)
            {
                m_lazerHit.SetActive(false);
            }

            if (m_audio.m_audioSource.isPlaying)
            {
                m_audio.m_audioSource.Stop();
            }

            ResetCam();
        }
    }

    void PulseBlast(bool fire1, bool fire2)
    {
        if(fire2)
        {
            if (!m_aimed)
            {
                //Debug.Log("Aim PulseBlast");
                m_aimed = true;

                m_prevDist = m_camControl.GetCamDist();
            }

            //Aim
            CamAim(m_pulseBlastAimOffset, m_pulseBlastAimDist);

            //Lift hand
            m_handAimWeight = Mathf.Lerp(m_handAimWeight, (m_turnCheck > 0.0f) ? 1.0f : 0.0f, m_handAimSpeed);

            if (fire1 && !m_pulseBlasted && m_turnCheck > 0.0f)
            {
                //Fire
                m_pulseBlasted = true;

                GameObject blast = m_pulseBlastAmmo.GetObject();
                blast.transform.position = m_pulseBlastFireTran.position;

                Rigidbody blastRB = blast.GetComponent<Rigidbody>();

                blastRB.velocity = (m_headLookTarPos - m_pulseBlastFireTran.position).normalized * m_pulseBlastVelocity;

                blast.SetActive(true);
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

        m_handAimWeight = Mathf.Lerp(m_handAimWeight, 0.0f, m_handAimSpeed);
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
