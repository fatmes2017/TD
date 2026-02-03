using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TileSlot : MonoBehaviour
{
    private MeshRenderer meshRenderer => GetComponent<MeshRenderer>();
    private MeshFilter meshFilter => GetComponent<MeshFilter>();
    public Collider myCollider => GetComponent<Collider>();
    public void SwitchTile(GameObject refrenceTile)
    {
        TileSlot newTile = refrenceTile.GetComponent<TileSlot>();

        meshFilter.mesh = newTile.GetMesh();
        meshRenderer.material = newTile.GetMaterial();
        name = newTile.name;

        UpdateCollider(newTile.GetCollider());

        foreach (GameObject obj in GetAllChildren())
        {
            DestroyImmediate(obj);
        }
        foreach (GameObject obj in newTile.GetAllChildren())
        {
            Instantiate(obj, transform);
        }


    }

    public Material GetMaterial() => meshRenderer.sharedMaterial;
    public Mesh GetMesh() => meshFilter.sharedMesh;

    public Collider GetCollider() => myCollider;

    public void UpdateCollider(Collider newCollider)
    {
        DestroyImmediate(myCollider);

        if (newCollider is BoxCollider)
        {
            BoxCollider orginalCollider = newCollider.GetComponent<BoxCollider>();
            BoxCollider myNewCollider = transform.AddComponent<BoxCollider>();


            myNewCollider.center = orginalCollider.center;
            myNewCollider.size = orginalCollider.size;

        }
        if (newCollider is MeshCollider)
        {
            MeshCollider orginalCollider = newCollider.GetComponent<MeshCollider>();
            MeshCollider myNewCollider = transform.AddComponent<MeshCollider>();


            myNewCollider.sharedMesh = orginalCollider.sharedMesh;
            myNewCollider.convex = orginalCollider.convex;

        }

    }

    public List<GameObject> GetAllChildren()
    {
        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in transform)
        {
            children.Add(child.gameObject);

        }
        return children;
    }

    public void RotateTile(int dir) => transform.Rotate(0, 90 * dir, 0);

    public void AdjustY(int verticalDir) => transform.position+=new Vector3(0, 0.1f * verticalDir, 0);


}
