namespace PaxDeiBotApp;

public static class TaskExtensions
{
    public static async Task Try(this Task task, Action<Exception> exceptionCallback)
    {
        try
        {
            await task;
        }
        catch (Exception e)
        {
            exceptionCallback.Invoke(e);
        }
    }
}