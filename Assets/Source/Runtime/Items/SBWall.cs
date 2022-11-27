using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SBWall : WorldObjectBase
{
    private List<Collision> _collisions = new List<Collision>();
    private bool wasHit = false;
    
    public void OnCollisionEnter(Collision collision)
    {
        var other = collision.gameObject;
        if (GameSystem.GetGameManager().gameHasStarted 
            && !GameSystem.GetGameManager().gameHasEnded
            && other.CompareTag("Player")
            && !wasHit)
        {
            int contactCount = collision.contactCount;
            _collisions.Add(collision);
            wasHit = true;
            
            if (other.transform.position.y <= transform.position.y + 0.3f)
            {
                SurfBlockView sb = other.GetComponent<SurfBlockView>();
                GameSystem.GetGameManager().RemovePlayerBlock(sb);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (_collisions.Count > 0)
        {
            for (int i = 0; i < _collisions.Count; i++)
            {
                Collision c = _collisions[i];
                if (c != null)
                {
                    int contactCount = _collisions[i].contactCount;
                    for (int j = 0; j < contactCount; j++)
                    {
                        ContactPoint cp = c.contacts[j];
                        DrawContactPoint(cp);
                    }
                }
            }
        }
    }

    private void DrawContactPoint(ContactPoint cp)
    {
        Ray normalRay = new Ray(cp.point, cp.normal);
        
        Gizmos.DrawRay(normalRay);
        Gizmos.DrawCube(cp.point, Vector3.one * 0.3f);
    }

    private void DrawCollision(Collision c, Vector3 origin)
    {
        Vector3 fwd = transform.forward;
        Vector3 impulse = c.impulse;
        Vector3 cross = Vector3.Cross(fwd, impulse);
            
        Ray impulseRay = new Ray(origin, impulse);
        Ray crossRay = new Ray(origin, cross);

        Gizmos.color = Color.white;
        Gizmos.DrawRay(impulseRay);
        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(crossRay);
    }
}