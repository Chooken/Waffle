namespace OurStory.Editor;

public interface ICommand
{
    public void Undo();
    public void Do();
}