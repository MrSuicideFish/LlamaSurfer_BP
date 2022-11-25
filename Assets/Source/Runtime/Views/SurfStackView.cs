using System;
using UnityEngine;

public class SurfStackView : MonoBehaviour
{
    private SurfStackModel _model;

    private void Awake()
    {
        _model = new SurfStackModel(this);
    }
}