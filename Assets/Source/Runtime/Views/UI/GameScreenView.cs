using System;
using System.Collections;
using UnityEngine;

public class GameScreenView : MonoBehaviour
{
    public virtual IEnumerator OnShow()
    {
        yield break;
    }

    public virtual IEnumerator OnHide()
    {
        yield break;
    }
    
}