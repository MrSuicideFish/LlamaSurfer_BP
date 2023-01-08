
public class OpeningScreenController : GameScreenControllerBase
{
    private OpeningScreenView _openingScreenView;
    public OpeningScreenController(GameScreenView view) : base(view)
    {
        _openingScreenView = view as OpeningScreenView;
    }
}