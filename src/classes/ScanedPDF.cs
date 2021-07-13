using System;
// using IronPdf;
using SpiderDocsModule;
using SpiderDocsWebAPIs;
using System.Linq;

// using Syncfusion.Pdf.Parsing;
// using Syncfusion.Windows.Forms.PdfViewer;
// using System.Drawing;
// using System.Drawing.Imaging;

namespace fileuploader
{
    internal class ScanedPDF
    {
		string _path = string.Empty;

		readonly ISDWebClient _client;
		string _parsedOrigin = string.Empty;
		string _parsedbeautify = string.Empty;

		internal string StudentName
		{
			get;set;
		} = "";

		internal string StudentID
		{
			get;set;
		} = "";
		internal string Student
		{
			get;set;
		} = "";

		internal string ComponentName
		{
			get;set;
		} = "";

		internal string FileName
		{
			get
			{
				return System.IO.Path.GetFileName(this._path);
			}
		}

		internal string Extension
		{
			get
			{
				return System.IO.Path.GetExtension(this._path);
			}
		}

		internal int AppFFoundID
		{
			get;set;
		}

		internal ScanedPDF(
				string path,
				SpiderDocsWebAPIs.ISDWebClient client
		)
		{
			_path = path;
			_client = client;
		}


		// internal bool IsValid()
		// {
		// 	return  ( !string.IsNullOrEmpty(StudentID) && !string.IsNullOrEmpty(ComponentName) );
		// }

		internal void InsertRecord()
		{
			throw new NotImplementedException();
		}

		public bool Parse()
		{

			var stream = SaveOnlyCover();


			using (var engine = new Tesseract.TesseractEngine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + @"/assets/tessdata", "eng", Tesseract.EngineMode.Default))
			{
				// using (var img = Tesseract.Pix.LoadFromMemory(this.file))
				// using (var img = Tesseract.Pix.LoadFromFile("C://Dev/fileuploader/test.jpg"))
				using (var img = Tesseract.Pix.LoadFromMemory(stream))
				{
					using (var page = engine.Process(img))
					{
						_parsedOrigin = page.GetText();
					}
				}
			}

			_parsedbeautify = Beautify(_parsedOrigin);

			SaveEach();


			bool allgood = Valid();
			if (!allgood)
			{

			}


			return allgood;
		}

		/// <summary>
		/// Check if necessary information has obtained from PDF file.
		/// </summary>
		/// <returns></returns>
		public bool Valid()
		{
			var number = new System.Text.RegularExpressions.Regex("^[0-9]+$");

			if ( string.IsNullOrWhiteSpace(StudentID) || !number.IsMatch(StudentID))
			{
				return false;
			}

			if ( string.IsNullOrWhiteSpace(StudentName) || string.IsNullOrWhiteSpace(ComponentName))
			{
				return false;
			}



			return true;
		}

		public int GetStudentID()
		{
			return string.IsNullOrWhiteSpace(StudentID) ? 0 : Convert.ToInt32(StudentID);
		}
		internal Document SaveToSpiderDocs()
		{
			Document ans;


			var property = GetSDocProperty();


			_client.SaveDoc(this._path, property, out ans);


			_client.SaveLinkedAttribute(

					(int)SDAttr.StudentID_New,

					this.Student,

					new DocumentAttribute()
					{
						id = (int)SDAttr.Name,
						atbValue = this.StudentName
					});

			return ans;

		}

		internal Document GetSDocProperty()
		{

			int combStudetId = SaveOrGetComboItem4StudentID();
			int combComponentd = SaveOrGetComboItemBy(SDAttr.Unit, this.ComponentName );


			var property = new Document();

			property.id_docType = 9;	// Test Evidence Collection
			property.id_folder = 5;		// Test
			property.title = $"{this.StudentID}-{this.StudentName}-{this.ComponentName}.pdf";
			property.extension = this.Extension;
			property.Attrs.Add(new DocumentAttribute(){ id = 5, id_type = en_AttrType.FixedCombo, atbValue = new System.Collections.Generic.List<int>(){ combStudetId } });	// studentID
			property.Attrs.Add(new DocumentAttribute(){ id = 8, id_type = en_AttrType.Combo , atbValue = new System.Collections.Generic.List<int>{ combComponentd } });		// Component
			property.Attrs.Add(new DocumentAttribute(){ id = 13, id_type = en_AttrType.Text, atbValue =  this.FileName });		// OriginalName
			property.Attrs.Add(new DocumentAttribute(){ id = 20, atbValue = this.Student });
			property.Attrs.Add(new DocumentAttribute(){ id = 22, atbValue = this.ComponentName });

			return property;
		}

		byte[] SaveOnlyCover()
		{
 			var s = new System.IO.MemoryStream();
			using (var collection = new ImageMagick.MagickImageCollection())
			{
				var settings = new ImageMagick.MagickReadSettings();
				settings.FrameIndex = 0; // First page
				settings.FrameCount = 1; // Number of pages
				settings.Depth = 24;
				settings.Density = new ImageMagick.Density(300);
				// Read only the first page of the pdf file
				collection.Read(this._path, settings);

  				foreach (var image in collection)
    			{
					image.Format = ImageMagick.MagickFormat.Jpg;
					// image.Write("C://Dev/fileuploader/test.jpg");
					image.Write(s);
					break;
				}
				// Clear the collection
				collection.Clear();

				// settings.FrameCount = 2; // Number of pages

				// // Read the first two pages of the pdf file
				// collection.Read("Snakeware.pdf", settings);
			}
			return s.ToArray();

		}

		void SaveEach()
		{
			var number = new System.Text.RegularExpressions.Regex("^[0-9]+$");


			var context = _parsedbeautify.Split('\n');

			if( context.Length < 3) return ;

			StudentName = context.Length == 0 ? "" : context[0].Trim();

			StudentID = (context.Length == 0 || !number.IsMatch(context[1])) ? "" : context[1].Trim();

			ComponentName = context.Length == 0 ? "" : context[2].Trim();
		}

		string Beautify(string text)
		{
			string result = System.Text.RegularExpressions.Regex.Replace(text, "(\\n)+", "\n");
			result = System.Text.RegularExpressions.Regex.Replace(result, "(\\n)+(\\s)+(\\n)+", "\n");


			return result;
		}


		int SaveOrGetComboItem4StudentID()
        {
            var client = _client;

            int comboitemId = 0;

			string name = this.StudentName;

			comboitemId = 	SaveOrGetComboItemBy
							(

								SDAttr.StudentID,

								this.Student,

								new System.Collections.Generic.List<SpiderDocsModule.DocumentAttribute>()
								{
									new SpiderDocsModule.DocumentAttribute() { id = (int)SDAttr.Name, atbValue = name  }
								}

							);


            return comboitemId;
        }



		int SaveOrGetComboItemBy(SDAttr attrType, string text,  System.Collections.Generic.List<SpiderDocsModule.DocumentAttribute> children = null)
        {
            var client = _client;

            int comboitemId = 0;

			var combobox = new SpiderDocsModule.DocumentAttributeCombo()
							{
								text = text,
							};

			if (children != null)
			{
				combobox.children = children;
			}

			comboitemId = 	client.EditAttributeComboboxItem
							(

								(int)attrType,

								combobox

							);

            return comboitemId;
        }

        enum SDAttr
        {
            StudentID = 5,
            Group = 6,  // Class
            StudentName = 7, // no longer used.
            Unit = 8,   // Component
            Course = 9,
            CompanyName = 10, //Employer
            AssessmentNo = 11,
            Name = 12,
            OriginalName = 13,
            Pattern = 15,
            Intention = 16,
            ApprenticeObject = 17,
            Campus = 18,
            StaffName = 19,

            /*
             * Below "_New"  attributes are going to be used instead not "_New".
             * Please use both "_New" and non "_New" attributes to save documents.
             */
	        StudentID_New = 20,
	        Class_New = 21,
	        Component_New = 22,
	        Course_New = 23,
	        Employer_New = 24,

            Student_Name = 25
        }


	}
}