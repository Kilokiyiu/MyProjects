namespace MiniWeb_Middleware;

public class ActionFilter
{
    public static List<IMyActionFilter> Filters = new List<IMyActionFilter>();
}

public interface IMyActionFilter
{
    public void Execute();
}