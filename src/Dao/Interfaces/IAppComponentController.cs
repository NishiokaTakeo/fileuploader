using System;
using System.Linq;
using fileuploader.Dao.Models;
using System.Collections.Generic;

namespace fileuploader.Dao.Interfaces
{
    public interface IAppComponentController
	{

		List<AppComponent> Select(AppComponent table);
        int Insert(AppComponent table);
        bool Update(AppComponent table);
		bool Exists(AppComponent table);
    }
}