using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointsManager : MonoBehaviour
{
    [SerializeField] Transform[] waypoints;

    public Transform[] GetWaypoints() => waypoints;
}
