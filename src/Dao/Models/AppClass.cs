using System;
using System.Data.SqlClient;

namespace fileuploader.Dao.Models
{

    public class AppClass: fileuploader.TableBase
	{

        #region Table difinition
        public override string TableName => "viewAppClass";

        public override string DBKey => "ID";

        public static string[] columns = new string[] {
													"ID",
													"Classid",
													"Moduleid",
													"Modulename",
													"Campus",
													"Group",
													"Ctyp",
													"Sdate",
										};


        public override string[] GetColumns()
        {
            return columns;
        }

        public int ID {get;set;} = 0;
        public string Classid {get;set;} = string.Empty;
        public int Moduleid {get;set;} = 0;
        public string Modulename {get;set;} = "";
        public string Campus {get;set;} = "";
        public string Group {get;set;} = "";
        public string Ctyp {get;set;} = "";
        public string Sdate {get;set;} = "";

        #endregion

        public AppClass()
		{

        }

        public AppClass(SqlDataReader dataReader):base(dataReader)
        {

            this.ID = base.GetFieldValue<int>(dataReader.GetOrdinal("ID"));
            this.Classid = base.GetFieldValue<string>(dataReader.GetOrdinal("Classid"));
            this.Moduleid = base.GetFieldValue<int>(dataReader.GetOrdinal("Moduleid"));
            this.Modulename = base.GetFieldValue<string>(dataReader.GetOrdinal("Modulename"));
            this.Campus = base.GetFieldValue<string>(dataReader.GetOrdinal("Campus"));
            this.Group = base.GetFieldValue<string>(dataReader.GetOrdinal("Group"));
            this.Ctyp = base.GetFieldValue<string>(dataReader.GetOrdinal("Ctyp"));
            this.Sdate = base.GetFieldValue<string>(dataReader.GetOrdinal("Sdate"));


        }
    }
}