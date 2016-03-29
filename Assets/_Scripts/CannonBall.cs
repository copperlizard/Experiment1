using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class CannonBall : MonoBehaviour
{
    public GameObject m_ball, m_explosion;
    public float m_explosionTime = 5.0f, m_maxLife = 30.0f;

    [HideInInspector]
    public float m_startTime;

    IEnumerator DeactivateTimer(float time)
    {
        yield return new WaitForSeconds(time);
        m_explosion.SetActive(false);
        gameObject.SetActive(false);
    }

	// Use this for initialization
	void Start ()
    {
        m_startTime = Time.time;
        m_ball.SetActive(true);
    }

    void OnEnable()
    {
        m_startTime = Time.time;
        m_ball.SetActive(true);
        StartCoroutine(DeactivateTimer(m_maxLife));
    }
	
	// Update is called once per frame
	void Update ()
    {
	    
	}    

    void OnCollisionEnter(Collision other)
    {
        Debug.Log("hit " + other.gameObject.name.ToString());

        m_explosion.transform.position = m_ball.transform.position;
        m_ball.SetActive(false);
        m_explosion.SetActive(true);
        StartCoroutine(DeactivateTimer(m_explosionTime));
    }
}
