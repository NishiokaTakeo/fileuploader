using System;
using System.Data.SqlClient;

namespace fileuploader.Dao.Models
{

    public class AppComponent: fileuploader.TableBase
	{

        #region Table difinition
        public override string TableName => "viewAppComponent";

        public override string DBKey => "ComponentId";

        public static string[] columns = new string[] {
													"ComponentId",
													"ComponentCode",
													"ComponentVersion",
													"ComponentName"
										};


        public override string[] GetColumns()
        {
            return columns;
        }

        public int ComponentId {get;set;} = 0;
        public string ComponentCode {get;set;} = string.Empty;
        public int ComponentVersion {get;set;} = 0;
        public string ComponentName {get;set;} = "";

        #endregion

        public AppComponent()
		{

        }

        public AppComponent(SqlDataReader dataReader):base(dataReader)
        {

            this.ComponentId = base.GetFieldValue<int>(dataReader.GetOrdinal("ComponentId"));
            this.ComponentCode = base.GetFieldValue<string>(dataReader.GetOrdinal("ComponentCode"));
            this.ComponentVersion = base.GetFieldValue<int>(dataReader.GetOrdinal("ComponentVersion"));
            this.ComponentName = base.GetFieldValue<string>(dataReader.GetOrdinal("ComponentName"));

        }
    }
}