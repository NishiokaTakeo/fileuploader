using System;
using System.Linq;
using fileuploader.Dao.Models;
using System.Collections.Generic;

namespace fileuploader.Dao.Interfaces
{
    public interface IAppClassListController
	{
		List<AppClassList> Select(AppClassList table);
        int Insert(AppClassList table);
        bool Update(AppClassList table);
		bool Exists(AppClassList table);
    }
}