using ErrorDataLayer;

namespace ErrorWebApplication
{
    public class SqliteService: ISqliteService
    {
        private static SqLiteManager _sqLiteManager = new SqLiteManager("Server");

        public SqLiteManager DbManager => _sqLiteManager;
    }
}