using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class CannonBall : MonoBehaviour
{
    public GameObject m_ball, m_explosion;
    public float m_explosionTime = 5.0f, m_explosionRadius = 3.0f, m_explosionForce = 10.0f, m_explodeUp = 0.2f, m_maxLife = 30.0f;

    [HideInInspector]
    public float m_startTime;

    private Rigidbody m_rb;   

    IEnumerator DeactivateTimer(float time)
    {
        yield return new WaitForSeconds(time);
        m_explosion.SetActive(false);
        gameObject.SetActive(false);
    }

	// Use this for initialization
	void Awake ()
    {
        m_startTime = Time.time;
        m_ball.SetActive(true);

        m_rb = GetComponent<Rigidbody>();    
    }

    void OnEnable()
    {
        //m_rb = GetComponent<Rigidbody>();
        m_startTime = Time.time;        
        m_rb.detectCollisions = true;        
        m_ball.SetActive(true);
        StartCoroutine(DeactivateTimer(m_maxLife));
    }
	
	// Update is called once per frame
	void Update ()
    {
	    
	}    

    void OnCollisionEnter(Collision other)
    {
        //Debug.Log("hit " + other.gameObject.name.ToString());

        m_explosion.transform.position = m_ball.transform.position;        
        m_rb.detectCollisions = false;
        m_rb.Sleep();
        m_ball.SetActive(false);

        Explode();

        m_explosion.SetActive(true);
        StartCoroutine(DeactivateTimer(m_explosionTime));
    }

    void Explode()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, m_explosionRadius);

        for(int i = 0; i < hits.Length; i++)
        {
            if(hits[i].attachedRigidbody != null)
            {
                hits[i].attachedRigidbody.AddExplosionForce(m_explosionForce, transform.position, m_explosionRadius, m_explodeUp, ForceMode.Impulse);
            }            
        }
    }
}
