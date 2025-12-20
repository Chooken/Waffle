namespace OurStory.Editor;

public struct CommandGroup() : ICommand
{
    private List<ICommand> _commands = new ();

    public void Do(ICommand command)
    {
        command.Do();
        _commands.Add(command);
    }
    
    public void Undo()
    {
        for (int i = _commands.Count - 1; i >= 0; i--)
        {
            _commands[i].Undo();
        }
    }

    public void Do()
    {
        foreach (var command in _commands)
        {   
            command.Do();
        }
    }
}