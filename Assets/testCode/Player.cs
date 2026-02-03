using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float xPosition;
    public float zPosition;
    public Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        //    transform.position=new Vector3(xPosition,0,zPosition);
        rb.constraints = RigidbodyConstraints.FreezePosition;
    }

    // Update is called once per frame
    void Update()
    {
        //   Debug.Log("update"); 
        transform.position = new Vector3(xPosition, 0, zPosition);
    }
}
