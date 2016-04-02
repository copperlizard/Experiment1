using UnityEngine;
using System.Collections;

public class FootFallIKmanager : MonoBehaviour
{
    public GameObject m_camera;
       
    public float m_footHeight, m_maxFootFall = 0.5f, m_maxFootLift = 0.5f, m_surfOffset = 0.1f, m_sinkDamp = 0.05f, m_checkSteps = 10.0f, m_footWeightOffset = 0.1f;

    [System.Serializable]
    public class Audio
    {
        public AudioSource m_audioSource;
        public AudioClip m_snow, m_ice, m_misc;
        [Range(0.0f, 1.0f)] public float m_stepLiftThreshold = 0.7f, m_stepFallThreshold = 0.8f, m_pitchSlide = 0.2f, m_pitchSlideRate = 0.5f;

        [HideInInspector]
        public bool m_leftStepped = true, m_rightStepped = true;
    }
    public Audio m_audio = new Audio();

    private Animator m_anim;
    private AnimatorStateInfo m_baseLayerState;

    private CapsuleCollider m_playerCol;

    //private Rigidbody m_playerRB;

    private Transform m_leftFootBone, m_rightFootBone;
    private Vector3 m_leftFootTarPos, m_rightFootTarPos, m_ColStartCenter;
    private Quaternion m_leftFootTarRot, m_rightFootTarRot;    

    private RaycastHit m_interAtLeftFoot, m_interAtRightFoot;

    private float m_ColStartHeight, m_leftSteps, m_rightSteps, m_sink, m_lastSink, m_maxDif;   
    private bool m_leftFootInter, m_rightFootInter;

    

    // Use this for initialization
    void Start ()
    {
        m_anim = GetComponent<Animator>();

        m_leftFootBone = m_anim.GetBoneTransform(HumanBodyBones.LeftFoot);
        m_rightFootBone = m_anim.GetBoneTransform(HumanBodyBones.RightFoot);                      

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

    void StepNoise(float left, float right)
    {
        //Left foot
        if(!m_audio.m_leftStepped && left >= m_audio.m_stepFallThreshold)
        {            
            if(m_leftFootInter)
            {
                m_audio.m_leftStepped = true;

                m_audio.m_audioSource.pitch = Mathf.Lerp(m_audio.m_audioSource.pitch, 1.0f + Random.Range(-m_audio.m_pitchSlide, m_audio.m_pitchSlide), m_audio.m_pitchSlideRate);
                
                switch (m_interAtLeftFoot.collider.material.name)
                {
                    case "Snow (Instance)":
                        m_audio.m_audioSource.PlayOneShot(m_audio.m_snow);
                        break;
                    case "Ice (Instance)":
                        m_audio.m_audioSource.PlayOneShot(m_audio.m_ice);
                        break;
                    default:
                        m_audio.m_audioSource.PlayOneShot(m_audio.m_misc);
                        break;
                }
            }
        }
        else if (left < m_audio.m_stepLiftThreshold)
        {
            m_audio.m_leftStepped = false;
        }

        //Right foot
        if (!m_audio.m_rightStepped && right >= m_audio.m_stepFallThreshold)
        {
            if (m_rightFootInter)
            {
                m_audio.m_rightStepped = true;

                m_audio.m_audioSource.pitch = Mathf.Lerp(m_audio.m_audioSource.pitch, 1.0f + Random.Range(-m_audio.m_pitchSlide, m_audio.m_pitchSlide), m_audio.m_pitchSlideRate);

                switch (m_interAtRightFoot.collider.material.name)
                {
                    case "Snow (Instance)":
                        m_audio.m_audioSource.PlayOneShot(m_audio.m_snow);
                        break;
                    case "Ice (Instance)":
                        m_audio.m_audioSource.PlayOneShot(m_audio.m_ice);
                        break;
                    default:
                        m_audio.m_audioSource.PlayOneShot(m_audio.m_misc);
                        break;
                }
            }
        }
        else if (right < m_audio.m_stepLiftThreshold)
        {
            m_audio.m_rightStepped = false;
        }
    }

    void AdjustCollider()
    {
        //Find animation's current foot weights
        float leftFootWeight = m_anim.GetFloat("LeftFoot");
        float rightFootWeight = m_anim.GetFloat("RightFoot");

        StepNoise(leftFootWeight, rightFootWeight);

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
                m_sink = Mathf.Lerp(m_sink, 0.0f, m_sinkDamp);
            }
        }

        float difference = (m_sink - m_lastSink);
        if (Mathf.Abs(difference) > Mathf.Abs(m_maxDif))
        {
            m_maxDif = difference;
        }
        m_lastSink = m_sink;
        //Debug.Log("m_sink == " + m_sink.ToString() + " ; difference of == " + difference.ToString() + " ; max difference == " + maxdiff.ToString());

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

    void OnAnimatorIK(int layerIndex)
    {
        if (!m_baseLayerState.IsName("Base Layer.Airborne"))
        {
            CheckFeet();
        }
        
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
