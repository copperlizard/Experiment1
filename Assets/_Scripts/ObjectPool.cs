using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour
{
    [HideInInspector]
    static public ObjectPool m_thisPool;
    public GameObject m_pooledObject;
    public int m_poolStartSize;
    public bool m_grows = true;

    private List<GameObject> m_objects;    

    void Awake()
    {
        m_thisPool = this;
    }

	// Use this for initialization
	void Start ()
    {
        m_objects = new List<GameObject>();

        for(int i = 0; i < m_poolStartSize; i++)
        {
            GameObject obj = (GameObject)Instantiate(m_pooledObject);
            obj.SetActive(false);
            m_objects.Add(obj);
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    public GameObject GetObject()
    {
        for(int i = 0; i < m_objects.Count; i++)
        {
            if(!m_objects[i].activeInHierarchy)
            {
                return m_objects[i];
            }
        }

        if(m_grows)
        {
            GameObject obj = (GameObject)Instantiate(m_pooledObject);
            m_objects.Add(obj);
            return obj;
        }

        return null;
    }
}
