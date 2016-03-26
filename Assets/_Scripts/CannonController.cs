using UnityEngine;
using System.Collections;

public class CannonController : MonoBehaviour
{

    public Transform m_pan, m_tilt;

	// Use this for initialization
	void Start ()
    {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    public void Move(Quaternion rot, bool fire1, bool fire2)
    {
        float yRot = rot.eulerAngles.y;
        float xRot = rot.eulerAngles.x;

        m_pan.transform.rotation = Quaternion.Euler(0.0f, yRot, 0.0f);
        //m_tilt.transform.rotation = Quaternion.Euler(xRot, 0.0f, 0.0f);

        Debug.Log("Fire1 == " + fire1.ToString());
        Debug.Log("Fire2 == " + fire2.ToString());
    }
}
