using MiniWeb_Middleware;

namespace MiniWeb_API;

public class MyActionFilter
{
    public class MyActionFilter1 : IMyActionFilter
    {
        public void Execute()
        {
            Console.WriteLine("Filter 1执行了");
        }
    }
}