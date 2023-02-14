using ErrorDataLayer;

namespace ErrorWebApplication
{
    public interface ISqliteService
    {
        static SqLiteManager DbManager { get; }
    }
}