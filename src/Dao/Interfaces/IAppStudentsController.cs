using System;
using System.Linq;
using fileuploader.Dao.Models;
using System.Collections.Generic;

namespace fileuploader.Dao.Interfaces
{
    public interface IAppStudentsController
	{

		List<AppStudents> Select(AppStudents table);
        int Insert(AppStudents table);
        bool Update(AppStudents table);
		bool Exists(AppStudents table);
    }
}