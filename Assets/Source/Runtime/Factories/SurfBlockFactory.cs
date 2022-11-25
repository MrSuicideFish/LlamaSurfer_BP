using UnityEngine;
using System;
using Object = UnityEngine.Object;

public static class SurfBlockFactory
{
    private const string SurfBlockViewPath = "SurfBlocks/SurfBlock";
    
    public static SurfBlockModel CreateSurfBlock(Transform parent, Vector3 localPosition)
    {
        SurfBlockView newView = Resources.Load<SurfBlockView>(SurfBlockViewPath);
        if (newView != null)
        {
            newView = Object.Instantiate(newView, parent, false);
            newView.transform.localPosition = localPosition;
            SurfBlockModel newModel = new SurfBlockModel(newView);
            newView.Init(newModel);
            newView.gameObject.SetActive(true);
            return newModel;
        }

        return null;
    }
}