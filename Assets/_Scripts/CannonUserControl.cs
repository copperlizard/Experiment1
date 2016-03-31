using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CannonController))]
public class CannonUserControl : MonoBehaviour
{

    private CannonController m_cannon;
    private Transform m_Cam;                  // A reference to the main camera in the scenes transform
    private bool m_fire1, m_fire2;

    // Use this for initialization
    void Awake ()
    {
        // get the transform of the main camera
        if (Camera.main != null)
        {
            m_Cam = Camera.main.transform;            
        }
        else
        {
            Debug.LogWarning(
                "Warning: no main camera found. cannon needs a Camera tagged \"MainCamera\", for camera-relative controls.");            
        }

        // get the third person character ( this should never be null due to require component )
        m_cannon = GetComponent<CannonController>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        m_fire1 = Input.GetButton("Fire1"); 
        m_fire2 = Input.GetButton("Fire2");
    }

    void FixedUpdate()
    {
        m_cannon.Move(m_Cam.transform.rotation, m_fire1, m_fire2);
    }

    void OnDisable()
    {
        m_cannon.m_audio.m_audio1.Stop();
    }

    void OnEnable()
    {
        m_cannon.m_audio.m_audio1.Play();
    }
}
