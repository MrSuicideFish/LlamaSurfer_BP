using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointsOnTouch : WorldObjectBase
{
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameSystem.GetGameManager().AddPoints(value);
            this.gameObject.SetActive(false);
        }
    }
}