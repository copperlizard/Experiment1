using UnityEngine;
using System.Collections;

public class CannonController : MonoBehaviour
{
    [System.Serializable]
    public class CannonAudio
    {
        public AudioSource m_audio1, m_audio2, m_audio3;
        public AudioClip m_moveSound, m_fireSound;
    }
    public CannonAudio m_audio = new CannonAudio();

    public Transform m_pan, m_tilt, m_firePos1, m_firePos2;
    public ObjectPool m_ammo;
    public float m_fireForce = 20.0f, m_chargeTime = 1.0f;

    private Vector3 m_panEuler, m_tiltEuler;
    private float m_charge1, m_charge2;
    private bool m_fired1, m_fired2;

    // Use this for initialization
    void Start ()
    {
        m_panEuler = m_pan.rotation.eulerAngles;
        m_tiltEuler = m_tilt.rotation.eulerAngles;

        m_audio.m_audio1.volume = 0.0f;
    }
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    public void Move(Quaternion rot, bool fire1, bool fire2)
    {        
        float yAng = rot.eulerAngles.y;
        float xAng = rot.eulerAngles.x;

        float change = 0.0f;

        Quaternion pan = Quaternion.Euler(m_panEuler.x, m_panEuler.y + yAng, m_panEuler.z);

        change = -Vector3.Dot((pan * Vector3.forward),(m_pan.transform.rotation * Vector3.forward));

        m_pan.rotation = Quaternion.RotateTowards(m_pan.rotation, pan, 1.0f);

        Quaternion tilt = Quaternion.Euler(m_tiltEuler.x - xAng, m_tiltEuler.y + (m_pan.rotation.eulerAngles.y - m_panEuler.y), m_tiltEuler.z);

        change = Mathf.Max(change, -Vector3.Dot((tilt * Vector3.forward), (m_tilt.transform.rotation * Vector3.forward)));

        m_tilt.rotation = Quaternion.RotateTowards(m_tilt.rotation, tilt, 1.0f);

        float volume = GLSLsmoothstep(-0.01f, 0.05f, (change + 1.0f) * 0.5f);

        //Debug.Log(volume.ToString());

        m_audio.m_audio1.volume = volume;

        CannonFire(fire1, fire2);        
    }

    float GLSLsmoothstep(float a, float b, float t)
    {
        float dif = b - a;
        t -= a;
        return Mathf.SmoothStep(0.0f, 1.0f, Mathf.Clamp((t / dif), 0.0f, 1.0f));
    }

    void CannonFire(bool fire1, bool fire2)
    {
        if (fire1 && !m_fired1)
        {
            m_charge1 = Time.time;
            m_fired1 = true; //fires when released
        }
        else if (!fire1 && m_charge1 > 0.0f)
        {
            //Debug.Log("fire1");
            Debug.DrawLine(m_firePos1.position, m_firePos1.position - m_firePos1.forward * 10.0f);

            GameObject cannonball = m_ammo.GetObject();
            cannonball.transform.position = m_firePos1.position - m_firePos1.forward * 5.0f;
            Rigidbody cannonballRB = cannonball.GetComponent<Rigidbody>();
            cannonballRB.velocity = -m_firePos1.forward * (m_fireForce * Mathf.Clamp((Time.time - m_charge1) / m_chargeTime, 0.0f, 1.0f));
            cannonball.SetActive(true);

            m_audio.m_audio2.Play();

            m_charge1 = 0.0f;
            m_fired1 = false; //fired
        }

        if (fire2 && !m_fired2)
        {
            m_charge2 = Time.time;
            m_fired2 = true; //fires when released
        }
        else if (!fire2 && m_charge2 > 0.0f)
        {
            //Debug.Log("fire2");
            Debug.DrawLine(m_firePos2.position, m_firePos2.position - m_firePos2.forward * 10.0f);

            GameObject cannonball = m_ammo.GetObject();
            cannonball.transform.position = m_firePos2.position - m_firePos2.forward * 5.0f;
            Rigidbody cannonballRB = cannonball.GetComponent<Rigidbody>();
            cannonballRB.velocity = -m_firePos2.forward * (m_fireForce * Mathf.Clamp((Time.time - m_charge2) / m_chargeTime, 0.0f, 1.0f));
            cannonball.SetActive(true);

            m_audio.m_audio3.Play();

            m_charge2 = 0.0f;
            m_fired2 = false; //fired
        }


    }
}
