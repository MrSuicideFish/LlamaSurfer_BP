
public static class Events
{
    // where 'int' is new point count
    public static GameEvent<int> OnPointsUpdated = new GameEvent<int>();
    
    // where 'int' is new player block count
    public static GameEvent<int> OnPlayerBlockAdded = new GameEvent<int>();
    
    // where 'int' is new player block count
    public static GameEvent<int> OnPlayerBlockRemoved = new GameEvent<int>();
}