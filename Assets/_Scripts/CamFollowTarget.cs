using UnityEngine;
using System.Collections;

public class CamFollowTarget : MonoBehaviour
{
    public GameObject m_target;
    public float m_bob;

    void Awake ()
    {
        transform.position = m_target.transform.position;
        transform.rotation = m_target.transform.rotation;
    }

	// Use this for initialization
	void Start ()
    {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
        transform.position = new Vector3(m_target.transform.position.x, transform.position.y, m_target.transform.position.z);

        float yDif = m_target.transform.position.y - transform.position.y;
        
        if(yDif < -m_bob)
        {
            transform.position = m_target.transform.position + new Vector3(0.0f, m_bob, 0.0f);
        }
        else if(yDif > m_bob)
        {   
            transform.position = m_target.transform.position - new Vector3(0.0f, m_bob, 0.0f);
        }
        
        

       


    }
}
