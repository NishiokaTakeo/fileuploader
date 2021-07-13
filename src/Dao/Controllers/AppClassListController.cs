using System;
using System.Linq;
using fileuploader.Dao.Models;
using System.Collections.Generic;

namespace fileuploader.Dao.Controllers
{
    public class AppClassListController : fileuploader.DaoBase, fileuploader.Dao.Interfaces.IAppClassListController
	{
        static String conn ; //= "Data Source=CETJNSQL02;Initial Catalog=CETNEWSMS_imp;Persist Security Info=True;User ID=sa_dev;Password=Slayer6zed";

        public AppClassListController( string connection) : base(connection)
        {
			conn = connection;
        }

		public List<AppClassList> Select(AppClassList table)
		{
			return this.ExecuteSQLDynamic<AppClassList>(table).ToList();
		}

        public int Insert(AppClassList table)
        {
            return this.ExecuteInsertSQL<AppClassList>(table);
        }

        public bool Update(AppClassList table)
        {
            return this.ExecuteUpdateSQL<AppClassList>(table);
        }
        public bool Exists(AppClassList table)
        {
            var records =  this.ExecuteSQLDynamic<AppClassList>(table);

			return records.Count() > 0;
        }

    }
}