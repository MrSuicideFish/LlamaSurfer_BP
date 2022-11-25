using System;
using UnityEngine;

public class SurfBlockView : MonoBehaviour
{
    public Rigidbody rigidBody;
    public Collider collider;
    public SpringJoint joint;

    public MeshRenderer topRenderer;
    public MeshRenderer bottomRenderer;

    public void Init(SurfBlockModel model)
    {
    }

    public void DoDestroy()
    {
    }

    public void Detatch()
    {
        transform.SetParent(null, true);
    }
}