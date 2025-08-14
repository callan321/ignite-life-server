namespace IgniteLife.Tools.Commands.Common
{
    public interface ICommand<TSelf> where TSelf : ICommand<TSelf>
    {
        static abstract string Name { get; }
        static abstract void WriteUsage();
        static abstract Task RunAsync(string[] args);
    }
}
