using System;
using System.Data.SqlClient;

namespace fileuploader.Dao.Models
{

    public class AppFfound: fileuploader.TableBase
	{

        #region Table difinition
        public override string TableName => "AppFfound";

        public override string DBKey => "ID";

        public static string[] columns = new string[] {
													"ID",
													"Name",
													"Stamp",
													"Student",
													"Unit",
													"Student_ID",
													"Date_Created"
										};
        public override string[] GetColumns()
        {
            return columns;
        }

        public int ID {get;set;} = 0;
        public string Name {get;set;} = string.Empty;
        public string Student {get;set;} = "";
        public string Stamp {get;set;} = "";
        public string Unit {get;set;} = "";
        public int Student_ID {get;set;} = 0;
        public DateTime Date_Created {get;set;} = DateTime.Now;

        #endregion

        public AppFfound()
		{

        }

        public AppFfound(SqlDataReader dataReader):base(dataReader)
        {
            this.ID = base.GetFieldValue<int>(dataReader.GetOrdinal("ID"));
            this.Name = base.GetFieldValue<string>(dataReader.GetOrdinal("Name"));
            this.Student = base.GetFieldValue<string>(dataReader.GetOrdinal("Student"));
            this.Stamp = base.GetFieldValue<string>(dataReader.GetOrdinal("Stamp"));
            this.Unit = base.GetFieldValue<string>(dataReader.GetOrdinal("Unit"));
            this.Student_ID = base.GetFieldValue<int>(dataReader.GetOrdinal("Student_ID"));
            this.Date_Created = base.GetFieldValue<DateTime>(dataReader.GetOrdinal("Date_Created"));

        }
    }
}