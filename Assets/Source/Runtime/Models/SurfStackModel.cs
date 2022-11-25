using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfStackModel
{
    private const float SurfBlockHeight = 1.0f;
    private SurfStackView _view;
    private Queue<SurfBlockModel> _blocks;

    public delegate void SurfStackDel(int numOfBlocks);

    public event SurfStackDel OnBlocksAdded;
    public event SurfStackDel OnBlocksRemoved;

    public SurfStackModel(SurfStackView view)
    {
        _blocks = new Queue<SurfBlockModel>();
        _view = view;
    }
    
    public void AddBlocks(int count)
    {
        for (int i = 0; i < count; i++)
        {
            AddBlock();
        }
    }
    
    public void AddBlock()
    {
        Debug.Log("Add Block");
        _blocks.Enqueue(SurfBlockFactory.CreateSurfBlock(
            this._view.transform,
            new Vector3(0, SurfBlockHeight * _blocks.Count, 0)));
    }

    public void RemoveBlocks(int count, EBlockRemoveType removeType)
    {
        for (int i = 0; i < count; i++)
        {
            RemoveLastBlock(removeType);
        }
    }

    public void RemoveLastBlock(EBlockRemoveType removeType)
    {
        if (removeType == EBlockRemoveType.Destroy)
        {
            _blocks.Dequeue().Destroy();
        }
        else
        {
            _blocks.Dequeue().Detatch();
        }
    }
}