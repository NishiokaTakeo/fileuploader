using System;
using System.Data.SqlClient;

namespace fileuploader.Dao.Models
{

    public class AppClassList: fileuploader.TableBase
	{

        #region Table difinition
        public override string TableName => "viewAppClassList";

        public override string DBKey => "ClassId";

        public static string[] columns = new string[] {
													"ClassId",
													"ClassName",
													"ComponentCode",
													"ComponentName",
													"ComponentVersion",
													"sdate"
										};


        public override string[] GetColumns()
        {
            return columns;
        }

        public int ClassId {get;set;} = 0;
        public string ClassName {get;set;} = string.Empty;
        public string ComponentCode {get;set;} = "";
        public string ComponentName {get;set;} = "";
        public int ComponentVersion {get;set;} = 0;
        public string sdate {get;set;} = "";

        #endregion

        public AppClassList()
		{

        }

        public AppClassList(SqlDataReader dataReader):base(dataReader)
        {

            this.ClassId = base.GetFieldValue<int>(dataReader.GetOrdinal("ClassId"));
            this.ClassName = base.GetFieldValue<string>(dataReader.GetOrdinal("ClassName"));
            this.ComponentCode = base.GetFieldValue<string>(dataReader.GetOrdinal("ComponentCode"));
            this.ComponentName = base.GetFieldValue<string>(dataReader.GetOrdinal("ComponentName"));
            this.ComponentVersion = base.GetFieldValue<int>(dataReader.GetOrdinal("ComponentVersion"));
            this.sdate = base.GetFieldValue<string>(dataReader.GetOrdinal("sdate"));


        }
    }
}