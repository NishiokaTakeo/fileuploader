using RestSharp;
using SpiderDocsModule;
using System.Collections.Generic;


namespace SpiderDocsWebAPIs
{
    public interface ISDWebClient
    {
		void login();

		Document[] GetDocument(SearchCriteria c);

		Document[] GetDocument(Document doc);

		int EditAttributeComboboxItem(int AttributeId, DocumentAttributeCombo Item);

		DocumentAttributeCombo[] GetAttributeComboboxItems(int AttributeId = 0, string Text = "");


		string[] GetDownloadUrls(int[] versionIds);
		bool SaveDoc(string filepath, Document doc, out Document result);

		bool CheckOut(int[] DocIds/*, int[] FolderIds*/);

		bool CancelCheckOut(int[] DocIds);

		bool Archive(int[] DocIds);

		bool UnArchive(int[] DocIds);

		History[] GetHistories(SearchCriteria Criteria);



		bool Delete(int[] DocumentIds, string Reason);

		bool UpdateProperty(Document doc);

		bool SaveLinkedAttribute(int keyID, string keyValue, DocumentAttribute linkedAttr);

		List<DocumentAttributeCombo> GetAttributeComboboxItems(int idAttr = 0, int idItem = 0);

		Document ToPDFBy(int[] versionIds, Document doc);

		string GetContentURLA(int id_version);

		string GetContentURL(int id_version);

		Folder SaveFolder(Folder folder);

		List<Folder> GetFoldersL1(int idParent);

		IRestResponse Execute(RestRequest r);

    }

}
