using ErrorDataLayer;

namespace ErrorWebApplication
{
    public class SqliteService: ISqliteService
    {
        private static SqLiteManager _sqLiteManager = new SqLiteManager();

        public SqLiteManager DbManager => _sqLiteManager;
    }
}