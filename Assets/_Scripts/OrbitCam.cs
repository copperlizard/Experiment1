using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OrbitCam : MonoBehaviour
{
    public GameObject m_target;
    public float m_minDist, m_maxDist, m_startDist, m_rotSpeed, m_damp, m_fudge;
    public bool m_HideCursor = true;
    public List<LayerMask> m_ignoreIntersect = new List<LayerMask>();
    
    private RaycastHit m_interAt;
    private Quaternion m_rot;
    private Vector3 m_curVel = Vector3.zero;
    private float m_h, m_v, m_d, m_dist;

	// Use this for initialization
	void Start()
    {            
        m_dist = m_startDist;
        transform.position = m_target.transform.position + new Vector3(0.0f, 0.0f, -m_dist);

        if(m_HideCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.visible = true;
        }
	}
	
	// Update is called once per frame
	void Update()
    {
        GetInput();
	}

    // LateUpdate is called once per frame after Update
    void LateUpdate()
    {
        // Calculate m_rot
        Quaternion deltaRot = Quaternion.Euler(m_v * m_rotSpeed, m_h * m_rotSpeed, 0.0f);
        m_rot = deltaRot;

        // Find new cam position
        m_dist -= m_d;        
        m_dist = Mathf.Clamp(m_dist, m_minDist, m_maxDist);
        Vector3 tarPos = (m_rot * new Vector3(0.0f, 0.0f, -m_dist)) + m_target.transform.position;

        //Check for sight line intersection
        tarPos = IntersectCheck(tarPos);

        //Move camera
        transform.position = Vector3.SmoothDamp(transform.position, tarPos, ref m_curVel, m_damp);
        transform.rotation = m_rot;
    }

    void GetInput()
    {
        m_h += Input.GetAxis("Mouse X");
        m_v -= Input.GetAxis("Mouse Y");
        m_d = Input.GetAxis("Mouse ScrollWheel");        
    }

    Vector3 IntersectCheck(Vector3 target)
    {
        //If intersection (cast ray from player to camera)
        if (Physics.Raycast(m_target.transform.position, target - m_target.transform.position, out m_interAt, m_dist))
        {
            //Ignoring objects tagged player
            if (m_interAt.rigidbody != null)
            {
                if (m_interAt.rigidbody.gameObject.tag == "Player")
                {
                    return target;
                }
            }         

#if UNITY_EDITOR
            Debug.DrawLine(m_target.transform.position, m_interAt.point, Color.yellow, 0.01f, true);
#endif

            float tDist = 0.0f;
            
            //If ignoring object layers...            
            if(m_ignoreIntersect.Count > 0)
            {
                bool ignore = false;
                for (int i = 0; i < m_ignoreIntersect.Count; i++)
                {
                    if(m_interAt.collider.gameObject.layer == m_ignoreIntersect[i])
                    {
                        //If on any ignored layer
                        ignore = true;
                    }
                }

                //If not on ignored layer
                if(!ignore)
                {                     
                    tDist = Mathf.Clamp(m_interAt.distance - m_fudge, m_minDist, m_maxDist);
                    target = (m_rot * new Vector3(0.0f, 0.0f, -tDist)) + m_target.transform.position;
                }
            }
            else
            {                 
                tDist = Mathf.Clamp(m_interAt.distance - m_fudge, m_minDist, m_maxDist);
                target = (m_rot * new Vector3(0.0f, 0.0f, -tDist)) + m_target.transform.position;
            }            
        }

        return target;
    }

    public void SetCamDist(float dist)
    {
        m_dist = Mathf.Clamp(dist, m_minDist, m_maxDist);
    }

    public float GetCamDist()
    {
        return m_dist;
    }

    /*
    float ClampAngle(float ang, float min, float max)
    {
        if (ang < -360.0f)
        {
            ang += 360.0f;
        }
        else if (ang > 360.0f)
        {
            ang -= 360.0f;
        }
        return Mathf.Clamp(ang, min, max);
    }
    */
}
