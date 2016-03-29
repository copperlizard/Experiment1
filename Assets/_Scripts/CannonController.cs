using UnityEngine;
using System.Collections;

public class CannonController : MonoBehaviour
{
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
    }
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    public void Move(Quaternion rot, bool fire1, bool fire2)
    {        
        float yAng = rot.eulerAngles.y;
        float xAng = rot.eulerAngles.x;

        Quaternion pan = Quaternion.Euler(m_panEuler.x, m_panEuler.y + yAng, m_panEuler.z);
        m_pan.rotation = Quaternion.RotateTowards(m_pan.rotation, pan, 1.0f);

        Quaternion tilt = Quaternion.Euler(m_tiltEuler.x - xAng, m_tiltEuler.y + (m_pan.rotation.eulerAngles.y - m_panEuler.y), m_tiltEuler.z);
        m_tilt.rotation = Quaternion.RotateTowards(m_tilt.rotation, tilt, 1.0f);

        CannonFire(fire1, fire2);        
    }

    void CannonFire(bool fire1, bool fire2)
    {
        if(fire1 && !m_fired1)
        {
            m_charge1 = Time.time;
            m_fired1 = true; //fires when released
        }
        else if(!fire1 && m_charge1 > 0.0f)
        {
            Debug.Log("fire1");
            Debug.DrawLine(m_firePos1.position, m_firePos1.position - m_firePos1.forward * 10.0f);

            GameObject cannonball = m_ammo.GetObject();
            cannonball.transform.position = m_firePos1.position - m_firePos1.forward * 5.0f;
            Rigidbody cannonballRB = cannonball.GetComponent<Rigidbody>();
            cannonballRB.velocity = -m_firePos1.forward * m_fireForce * Mathf.Clamp((Time.time - m_charge1 / m_chargeTime), 0.0f, 1.0f);
            cannonball.SetActive(true);

            m_charge1 = 0.0f;
            m_fired1 = false; //fired
        }

        /*
        if(fire1 && !m_fired1)
        {
            m_fired1 = true;

            Debug.Log("fire1");
            Debug.DrawLine(m_firePos1.position, m_firePos1.position - m_firePos1.forward * 10.0f);

            GameObject cannonball = m_ammo.GetObject();
            cannonball.transform.position = m_firePos1.position - m_firePos1.forward * 5.0f;
            Rigidbody cannonballRB = cannonball.GetComponent<Rigidbody>();
            cannonballRB.velocity = -m_firePos1.forward * 10.0f;
            cannonball.SetActive(true);            
        }
        
        if(!fire1)
        {
            m_fired1 = false;
        }

        if(fire2 && !m_fired2)
        {
            m_fired2 = true;

            Debug.Log("fire2");
            Debug.DrawLine(m_firePos2.position, m_firePos2.position - m_firePos2.forward * 10.0f);
        }
        
        if(!fire2)
        {
            m_fired2 = false;
        }
        */
    }
}
