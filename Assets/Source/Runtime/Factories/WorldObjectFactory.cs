public enum EWorldObjectType
{
    Apple_Single
}


public static class WorldObjectFactory
{
    public static WorldObjectBase Create(EWorldObjectType objectType)
    {
        WorldObjectBase newObject = null;
        
        switch (objectType)
        {
            case EWorldObjectType.Apple_Single:
                break;
        }

        return newObject;
    }
}