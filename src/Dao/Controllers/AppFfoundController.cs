using System;
using System.Linq;
using fileuploader.Dao.Models;
using System.Collections.Generic;

namespace fileuploader.Dao.Controllers
{
    public class AppFfoundController : fileuploader.DaoBase, fileuploader.Dao.Interfaces.IAppFfoundController
	{
        static String conn; // = "Data Source=CETJNSQL02;Initial Catalog=CETNEWSMS_imp;Persist Security Info=True;User ID=sa_dev;Password=Slayer6zed";

        public AppFfoundController( string connection ) : base(connection)
        {
			conn = connection;
        }

		public List<AppFfound> Select(AppFfound table)
		{
			return this.ExecuteSQLDynamic<AppFfound>(table).ToList();
		}

        public int Insert(AppFfound table)
        {
            return this.ExecuteInsertSQL<AppFfound>(table);
        }

        public bool Update(AppFfound table)
        {
            return this.ExecuteUpdateSQL<AppFfound>(table);
        }

    }
}