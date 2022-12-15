
public class GameplayScreenController : GameScreenControllerBase
{
    private GameplayView _gameplayView;
    
    public GameplayScreenController(GameScreenView view) : base(view)
    {
        _gameplayView = view as GameplayView;
    }
}