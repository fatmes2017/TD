using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
  private  Transform[] orbitPoints;
    public Transform target;
   
    public Transform[] GetCameraOrbitPoints() => orbitPoints;
    private void awake()
    {
       

        foreach (CameraOrbitPoint cop in FindObjectsOfType<CameraOrbitPoint>())
        {
           
           
           // orbitPoints.Append(cop.transform);
          //  orbitPoints.Concat(cop.transform);
            orbitPoints.Append(cop.GetTransform());
        }
       
         target = FindObjectOfType<CameraTarget>().GetCameraTarget();


    }
}
