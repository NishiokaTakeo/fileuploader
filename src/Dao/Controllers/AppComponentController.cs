using System;
using System.Linq;
using fileuploader.Dao.Models;
using System.Collections.Generic;

namespace fileuploader.Dao.Controllers
{
    public class AppComponentController : fileuploader.DaoBase, fileuploader.Dao.Interfaces.IAppComponentController
	{
        static String conn ; //= "Data Source=CETJNSQL02;Initial Catalog=CETNEWSMS_imp;Persist Security Info=True;User ID=sa_dev;Password=Slayer6zed";

        public AppComponentController( string connection) : base(connection)
        {
			conn = connection;
        }

		public List<AppComponent> Select(AppComponent table)
		{
			return this.ExecuteSQLDynamic<AppComponent>(table).ToList();
		}

        public int Insert(AppComponent table)
        {
            return this.ExecuteInsertSQL<AppComponent>(table);
        }

        public bool Update(AppComponent table)
        {
            return this.ExecuteUpdateSQL<AppComponent>(table);
        }
        public bool Exists(AppComponent table)
        {
            var records =  this.ExecuteSQLDynamic<AppComponent>(table);

			return records.Count() > 0;
        }

    }
}