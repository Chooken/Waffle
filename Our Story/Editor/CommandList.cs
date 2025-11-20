namespace OurStory.Editor;

public class CommandList
{
    private int _current = -1;
    private int _top = -1;
    private List<ICommand> _commands = new ();

    public void Add(ICommand command)
    {
        _current++;
        
        if (_commands.Count > _current)
        {
            _commands[_current] = command;
        }
        else
        {
            _commands.Add(command);
        }

        _top = _current;
    }
    
    public void Do(ICommand command)
    {
        command.Do();
        Add(command);
    }

    public void Undo()
    {
        if (_current < 0)
        {
            return;
        }
        
        _commands[_current].Undo();
        _current--;
    }

    public void Redo()
    {
        if (_current >= _top)
        {
            return;
        }

        _current++;
        _commands[_current].Do();
    }

    public void Clear()
    {
        _current = -1;
        _top = -1;
    }
}