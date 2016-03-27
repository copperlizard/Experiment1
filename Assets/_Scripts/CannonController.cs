using UnityEngine;
using System.Collections;

public class CannonController : MonoBehaviour
{
    public Transform m_pan, m_tilt;

    private Vector3 m_panEuler, m_tiltEuler;

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

#if UNITY_EDITOR
        Debug.DrawLine(m_pan.position, m_pan.position + m_pan.forward * 5.0f, Color.red, 0.05f, true);
        Debug.DrawLine(m_tilt.position, m_tilt.position + m_tilt.forward * 5.0f, Color.blue, 0.05f, true);
#endif
        
        float yAng = rot.eulerAngles.y;
        float xAng = rot.eulerAngles.x;

        Quaternion pan = Quaternion.Euler(m_panEuler.x, m_panEuler.y + yAng, m_panEuler.z);
        Quaternion tilt = Quaternion.Euler(m_tiltEuler.x - xAng, m_tiltEuler.y + yAng, m_tiltEuler.z);

        m_pan.rotation = Quaternion.RotateTowards(m_pan.rotation, pan, 1.0f);
        m_tilt.rotation = Quaternion.RotateTowards(m_tilt.rotation, tilt, 1.0f);
        
        

        Debug.Log("pan == " + m_pan.rotation.ToString());
        Debug.Log("tilt == " + m_tilt.rotation.ToString());

        //Debug.Log("Fire1 == " + fire1.ToString());
        //Debug.Log("Fire2 == " + fire2.ToString());
    }
}
