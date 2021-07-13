using System;
using System.Linq;
using fileuploader.Dao.Models;
using System.Collections.Generic;

namespace fileuploader.Dao.Interfaces
{
    public interface IAppFfoundController
	{

		List<AppFfound> Select(AppFfound table);
        int Insert(AppFfound table);
        bool Update(AppFfound table);
    }
}