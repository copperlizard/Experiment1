using UnityEngine;
using System.Collections;

public class PulseBlast : MonoBehaviour
{
    public GameObject m_projectile, m_explosion, m_implosion;
    public float m_fuseTime, m_explosionRadius, m_explosionForce, m_explosionTime, m_implosionRadius, m_implosionForce, m_implosionTime;
    [Range(-1.0f, 1.0f)] public float m_explodeUpMod, m_implodeUpMod;

    [HideInInspector]
    public float m_startTime;

    [System.Serializable]
    public class Audio
    {
        public AudioSource m_source;
        public AudioClip m_projectile, m_explosion, m_implosion;
    }
    public Audio m_audio = new Audio();

    private Rigidbody m_rb;  

    IEnumerator SelfDestructTimer(float time)
    {
        yield return new WaitForSeconds(time);
        StartCoroutine(Detonate());
    }

    IEnumerator Detonate()
    {
        m_rb.detectCollisions = false;
        m_rb.Sleep();

        m_projectile.SetActive(false);

        m_explosion.SetActive(true);
        Explode();
        yield return new WaitForSeconds(m_explosionTime);
        m_explosion.SetActive(false);

        m_implosion.SetActive(true);
        Implode();
        yield return new WaitForSeconds(m_implosionTime);
        m_implosion.SetActive(false);
    }

    void Awake ()
    {
        m_startTime = Time.time;

        m_projectile.SetActive(true);
        m_explosion.SetActive(false);
        m_implosion.SetActive(false);

        m_rb = GetComponent<Rigidbody>();

        m_audio.m_source.clip = m_audio.m_projectile;
        m_audio.m_source.Play();
    }

    // Use this for initialization
    void Start ()
    {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    void OnEnable()
    {
        transform.parent = null;
        m_projectile.SetActive(true);
        m_rb.detectCollisions = true;
        m_rb.WakeUp();
        m_explosion.SetActive(false);
        m_implosion.SetActive(false);

        m_audio.m_source.clip = m_audio.m_projectile;
        m_audio.m_source.Play();

        StartCoroutine(SelfDestructTimer(m_fuseTime));
    }

    /*
    void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.tag != "Player")
        {            
            m_rb.velocity = Vector3.zero;
            m_rb.detectCollisions = false;
            
            if(other.transform != null)
            {                
                transform.parent = other.transform;
                Debug.Log("child of " + other.gameObject.name);
            }
        }
    }
    */

    void Explode()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, m_explosionRadius);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].attachedRigidbody != null)
            {
                hits[i].attachedRigidbody.AddExplosionForce(m_explosionForce, transform.position, m_explosionRadius, m_explodeUpMod, ForceMode.Impulse);
            }
        }

        m_audio.m_source.Stop();
        m_audio.m_source.PlayOneShot(m_audio.m_explosion);
    }

    void Implode()
    {        
        Collider[] hits = Physics.OverlapSphere(transform.position, m_implosionRadius);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].attachedRigidbody != null)
            {
                hits[i].attachedRigidbody.AddExplosionForce(m_implosionForce, transform.position, m_implosionRadius, m_implodeUpMod, ForceMode.Impulse);
            }
        }

        m_audio.m_source.Stop();
        m_audio.m_source.PlayOneShot(m_audio.m_implosion);
    }
}
