using WaffleEngine;

namespace Editor;

public class TestQuery : Query<Camera>
{
    private Modifier _modifer1 = new Modifier(Keycode.LeftShift);
    private Modifier _modifier2 = new Modifier(Keycode.LeftShift, Keycode.LeftControl);
    
    public override void Run(ref Camera camera)
    {
        if (Input.GetDefaultEventSpace.KeyPressed(Keycode.K))
            Log.Info("Key Pressed from Global Input");

        if (Input.GetEventSpace(_modifer1).KeyPressed(Keycode.K))
            Log.Info("Key Pressed with modifier Left Shift");

        if (Input.GetEventSpace(_modifier2).KeyPressed(Keycode.K))
            Log.Info("Key Pressed with modifiers Left Shift and Left Control");
    }
}