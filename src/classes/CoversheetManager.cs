using System;
using fileuploader.Dao.Interfaces;
using fileuploader.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
// using NLog;
using SpiderDocsModule;
using SpiderDocsWebAPIs;
using System.Linq;

namespace fileuploader
{
    public class CoversheetManager
    {
		System.Timers.Timer timer;
		int interval = 30000;

		SpiderDocsWebAPIs.ISDWebClient _client;
		Microsoft.Extensions.Configuration.IConfiguration _appConf;
		private readonly IAppFfoundController _tableCtr;
		private readonly IAppStudentsController _tableCtr2;
		private readonly IAppClassListController _tableCtr3;

		private readonly IMailClient _mailClient;

		ILogger<Worker> _logger;

		public CoversheetManager(
			SpiderDocsWebAPIs.ISDWebClient client,
			Microsoft.Extensions.Configuration.IConfiguration appConf,
			fileuploader.Dao.Interfaces.IAppFfoundController tableCtr,
			fileuploader.Helpers.IMailClient mailClient,
			ILogger<Worker> logger,
			fileuploader.Dao.Interfaces.IAppStudentsController tableCtr2,
			fileuploader.Dao.Interfaces.IAppClassListController tableCtr3 )
		{
			_client = client;
			_appConf = appConf;
			_tableCtr = tableCtr;
			_tableCtr2 = tableCtr2;
			_tableCtr3 = tableCtr3;
			_mailClient = mailClient;
			_logger = logger;

			// timer = new System.Timers.Timer(interval);
			// timer.Elapsed += new System.Timers.ElapsedEventHandler(Run);
			// timer.AutoReset = true;
		}

		// Inject config and parser
		// async public void Run(object sender, System.Timers.ElapsedEventArgs e)

		async public System.Threading.Tasks.Task<string[]> Run()
		{
			var ans =  new string[]{};

			string[] files = System.IO.Directory.GetFiles(_appConf["TargetPath"]);
			if (files.Length > 0)
			{

				foreach (string file in files)
				{
					try
					{

						var ok = await Import(file);

					}
					catch(Exception ex)
					{
						MoveToFTP(file);

						_logger.LogError(ex,"[Failure] API CALL ERROR at {0}",file);

						// keep continue;
					}
				}

			}

			// await System.Threading.Tasks.Task.Delay(1000 * 15);

			return files;

		}


		// public void Start()
		// {
        //     timer.Start();
        //     timer.Enabled = true;
		// }

		// public void Stop()
		// {
        //     timer.Stop();
        //     timer.Enabled = false;
		// }
		System.Threading.Tasks.Task<bool> Import(string file)
		{
			_logger.LogDebug("Work At. {0}", file);

			var coversheet = new ScanedPDF(file, _client);	// Should coversheet


			coversheet.Parse(); // Should obtain Student ID and Component Name

			coversheet.Student = GetStudentWithFundingModel(coversheet.GetStudentID());	// Add Funding Model

			int id = InsertInitialDBRecords( coversheet );

			if( coversheet.Valid() && MatchedWithDBData(coversheet))
			{


				coversheet.SaveToSpiderDocs();

				// If the process suceeded
				// then move coversheet file to backup folder
				MoveToDone(file);

				// update "update AppFfound set student='" . $sn . "', unit='" . $unit . "' where id=" . $rcno;
				UpdateDBRecords( id , coversheet );


				_logger.LogInformation("[Success] A file has been imported successfully. '{0}', AppFfound.id:{1}",FileName(file),id);

				return System.Threading.Tasks.Task.Run( () => true);
			}
			else
			{

				_logger.LogError("[Failure] missing some info. '{0}', AppFfound.id:{1}",FileName(file), id);

				// then copy coversheet file to FTP server.
				MoveToFTP(file);

				return System.Threading.Tasks.Task.Run( () => false);
			}
		}

		bool MatchedWithDBData(ScanedPDF coversheet )
		{
			var studentOK = _tableCtr2.Exists(new Dao.Models.AppStudents() {ID = coversheet.GetStudentID()});
			var componentOK = _tableCtr3.Exists(new Dao.Models.AppClassList(){ ClassName = coversheet.ComponentName });

			if ( studentOK && componentOK )
			{
				return true;
			}
			else
			{
				_logger.LogWarning("[Not Found] '{0}', {1}: {2}, {3}: {4}", coversheet.FileName, coversheet.GetStudentID(), studentOK ? "OK":"NG", coversheet.ComponentName, componentOK? "OK":"NG");

				return false;
			}

		}


		internal int InsertInitialDBRecords( ScanedPDF coversheet )
		{
			int id = 0;
			var has = _tableCtr.Select(new Dao.Models.AppFfound() { Name = coversheet.FileName} ) ;
			if ( has.Count == 0)
			{

				id =
				_tableCtr.Insert(new Dao.Models.AppFfound(){

					Name = coversheet.FileName,

					Stamp = DateTime.Now.ToString("yyMMddHHmmss"),

					Student = coversheet.Student ,

					Unit = coversheet.ComponentName,

					Student_ID = coversheet.GetStudentID()
				});

			}
			else
			{
				id = has[0].ID;
			}

			return id;
		}


		internal bool UpdateDBRecords( int id,  ScanedPDF coversheet )
		{
			var has = _tableCtr.Select(new Dao.Models.AppFfound() { Name = coversheet.FileName} ) ;

			var ok =
			_tableCtr.Update(new Dao.Models.AppFfound(){

				ID = id,

				Stamp = string.IsNullOrEmpty(has[0].Stamp) ? DateTime.Now.ToString("yyMMddHHmmss") : has[0].Stamp,

				Name = coversheet.FileName,

				Student = coversheet.Student ,

				Unit = coversheet.ComponentName,

				Student_ID = Convert.ToInt32(coversheet.StudentID)
			});

			return ok;
		}

		internal string GetStudentWithFundingModel(int studentID)
		{

			var record = _tableCtr2.Select(new Dao.Models.AppStudents(){ ID = studentID}).FirstOrDefault();

			if ( record == null ) return studentID.ToString();

			return string.Format("{0}{1}",record.Styp, studentID);

		}

		bool MoveToDone(string path)
		{
			var to = _appConf["DonePath"];
			try
			{
				if ( ! System.IO.Directory.Exists(to))
				{
					System.IO.Directory.CreateDirectory(to);
				}

				if (System.IO.File.Exists( System.IO.Path.Combine(to, FileName(path) )))
				{
					to = CreateUniquePath( to );

					_logger.LogDebug("[Duplicated] A file has already Done folder. '{0}', Stores to '{1}'", FileName(path), to);
				}

				System.IO.File.Move(path, System.IO.Path.Combine(to, FileName(path) ) );
			}
			catch(Exception ex)
			{
				_logger.LogError(ex,"[Failure] Move a file: {0}", System.IO.Path.Combine(to, FileName(path) ) );

				return false;
			}
			return true;
		}

		bool MoveToFTP(string path)
		{


			if ( ! string.IsNullOrWhiteSpace(_appConf["NoFTP"]) )
			{
				_logger.LogDebug("Skip FTP");

				return true;
			}

			bool ans = false;
			try
			{
				System.Net.FtpWebRequest request = (System.Net.FtpWebRequest)System.Net.WebRequest.Create(_appConf["FTP"] +  FileName(path) );
				request.Method = System.Net.WebRequestMethods.Ftp.UploadFile;
				request.Credentials = new System.Net.NetworkCredential(_appConf["FTPID"], _appConf["FTPPASS"]);
				System.IO.StreamReader sourceStream = new System.IO.StreamReader(path, System.Text.Encoding.GetEncoding(28591));
				System.Text.Encoding enc = System.Text.Encoding.GetEncoding(28591);
				byte[] fileContents = enc.GetBytes(sourceStream.ReadToEnd());
				sourceStream.Close();
				request.ContentLength = fileContents.Length;
				System.IO.Stream requestStream = request.GetRequestStream();
				requestStream.Write(fileContents, 0, fileContents.Length);
				requestStream.Close();
				System.Net.FtpWebResponse response = (System.Net.FtpWebResponse)request.GetResponse();
				// Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription);
				response.Close();

				_logger.LogDebug("[Transfered] A file has been tranfered to FTP. '{0}'", FileName(path));

				ans = true;

				// move a file to done folder if FTP transfer has succeeded.
				MoveToDone(path);
			}
			catch(Exception ex)
			{

				_logger.LogError(ex,"[Failure] FTP CALL ERROR at {0}", path);
			}

			return ans;


		}

        string CreateUniquePath(string basepath)
        {
            basepath = basepath.EndsWith("\\") ? basepath.Substring(0, basepath.Length - 1) : basepath;
            var uniq = Guid.NewGuid().ToString().Substring(0, 10);
            var drivePath = string.Format(@"{0}\{1}\", basepath, uniq);

            System.IO.Directory.CreateDirectory(drivePath);

            return drivePath;
        }

		string FileName(string path)
		{

			return System.IO.Path.GetFileName(path);

		}

		string ToJson(object value)
		{
			var settings = new Newtonsoft.Json.JsonSerializerSettings
			{
                                		NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
										,Error = (serializer,err) => err.ErrorContext.Handled = true
                            		};

 			var json = Newtonsoft.Json.JsonConvert.SerializeObject(value,Newtonsoft.Json.Formatting.None, settings);

			 return json;
		}
	}




}