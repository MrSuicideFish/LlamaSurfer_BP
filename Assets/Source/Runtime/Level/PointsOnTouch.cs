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
            GameManager.Instance.AddPoints(value);
            this.gameObject.SetActive(false);
        }
    }
}