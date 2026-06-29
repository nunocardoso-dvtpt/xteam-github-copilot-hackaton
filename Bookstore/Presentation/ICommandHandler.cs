namespace Bookstore.Presentation;

public interface ICommandHandler
{
    void Handle(string commandLine);
}
