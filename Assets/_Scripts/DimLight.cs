using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Light))]
public class DimLight : MonoBehaviour
{
    public float m_dimTime = 5.0f;

    private Light m_light;
    private float m_startTime;

	// Use this for initialization
	void Awake ()
    {
        m_light = gameObject.GetComponent<Light>();	
	}
	
	// Update is called once per frame
	void Update ()
    {
        float timePassed = Time.time - m_startTime;

        m_light.intensity = Mathf.Lerp(1.0f, 0.0f, (timePassed / m_dimTime));	
	}

    void OnEnable()
    {
        m_startTime = Time.time;
    }
}
