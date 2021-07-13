using System;
using System.Linq;
using fileuploader.Dao.Models;
using System.Collections.Generic;

namespace fileuploader.Dao.Controllers
{
    public class AppStudentsController : fileuploader.DaoBase, fileuploader.Dao.Interfaces.IAppStudentsController
	{
        static String conn ; //= "Data Source=CETJNSQL02;Initial Catalog=CETNEWSMS_imp;Persist Security Info=True;User ID=sa_dev;Password=Slayer6zed";

        public AppStudentsController( string connection) : base(connection)
        {
			conn = connection;
        }

		public List<AppStudents> Select(AppStudents table)
		{
			return this.ExecuteSQLDynamic<AppStudents>(table).ToList();
		}

        public int Insert(AppStudents table)
        {
            return this.ExecuteInsertSQL<AppStudents>(table);
        }

        public bool Update(AppStudents table)
        {
            return this.ExecuteUpdateSQL<AppStudents>(table);
        }
        public bool Exists(AppStudents table)
        {
            var records =  this.ExecuteSQLDynamic<AppStudents>(table);

			return records.Count() > 0;
        }

    }
}