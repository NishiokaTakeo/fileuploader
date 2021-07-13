using System;
using System.Data.SqlClient;

namespace fileuploader.Dao.Models
{

    public class AppStudents: fileuploader.TableBase
	{

        #region Table difinition
        public override string TableName => "viewAppStudents";

        public override string DBKey => "ID";

        public static string[] columns = new string[] {
													"ID",
													"Given",
													"Surname",
													"Studentno",
													"Campus",
													"Dob",
													"Alldata",
													"Styp"
										};


        public override string[] GetColumns()
        {
            return columns;
        }

        public int ID {get;set;} = 0;
        public string Given {get;set;} = string.Empty;
        public string Surname {get;set;} = "";
        public int Studentno {get;set;} = 0;
        public int Campus {get;set;} = 0;
        public string Dob {get;set;} = "";
        public string Alldata {get;set;} = "";
        public string Styp {get;set;} = "";

        #endregion

        public AppStudents()
		{

        }

        public AppStudents(SqlDataReader dataReader):base(dataReader)
        {
            this.ID = base.GetFieldValue<int>(dataReader.GetOrdinal("ID"));
            this.Given = base.GetFieldValue<string>(dataReader.GetOrdinal("Given"));
            this.Surname = base.GetFieldValue<string>(dataReader.GetOrdinal("Surname"));
            this.Studentno = base.GetFieldValue<int>(dataReader.GetOrdinal("Studentno"));
            this.Campus = base.GetFieldValue<int>(dataReader.GetOrdinal("Campus"));
            this.Dob = base.GetFieldValue<string>(dataReader.GetOrdinal("Dob"));
            this.Alldata = base.GetFieldValue<string>(dataReader.GetOrdinal("Alldata"));
            this.Styp = base.GetFieldValue<string>(dataReader.GetOrdinal("Styp"));

        }
    }
}