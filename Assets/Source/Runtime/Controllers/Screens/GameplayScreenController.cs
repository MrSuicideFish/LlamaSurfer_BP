
public class GameplayScreenController : GameScreenControllerBase
{
    private int points_internal = 0;
    private GameplayView _gameplayView;
    
    public GameplayScreenController(GameScreenView view) : base(view)
    {
        points_internal = 0;
        _gameplayView = view as GameplayView;
        
        Events.OnPointsUpdated.RemoveListener(OnPointsUpdated);
        Events.OnPointsUpdated.AddListener(OnPointsUpdated);
    }


    private void OnPointsUpdated(int points)
    {
        _gameplayView.SetPointsTo(points);
    }
}