
/// <summary>
/// 通用单例模式
/// </summary>
/// <typeparam name="T"> 具体单例类型</typeparam>
public class Singletion<T>where T : new()
{
    private static object objStatic = new object();
    private static T instance;
    public static T Instance
    {
        get {
            lock (objStatic)
            {
                if (instance == null)
                    instance = new T();
                return instance;
            }
        }
    }
}
