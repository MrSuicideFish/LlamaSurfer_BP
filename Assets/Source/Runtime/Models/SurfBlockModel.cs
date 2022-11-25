using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfBlockModel
{
    private SurfBlockView _view;
    
    public SurfBlockModel(SurfBlockView view)
    {
        _view = view;
    }

    public void Destroy()
    {
        _view.DoDestroy();
    }

    public void Detatch()
    {
        _view.transform.SetParent(null, true);
    }
}