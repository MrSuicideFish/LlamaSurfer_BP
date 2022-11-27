using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SBCollect : WorldObjectBase
{
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameSystem.GetGameManager().GivePlayerBlock();
            this.gameObject.SetActive(false);
        }
    }
}
