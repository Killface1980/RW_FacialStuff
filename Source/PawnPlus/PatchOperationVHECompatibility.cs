using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Verse;

namespace PawnPlus
{
	public class PatchOperationVHECompatibility : PatchOperationPathed
	{
		public string subcategory;
		public bool hideMouth;

		protected override bool ApplyWorker(XmlDocument xml)
		{
			foreach(object item in xml.SelectNodes(xpath))
			{
				XmlNode xmlNode = item as XmlNode;
				XmlNode xmlNode2 = xmlNode.OwnerDocument.CreateElement("PawnPlus.Defs.PartDef");
				var parentNameAttr = xmlNode.OwnerDocument.CreateAttribute("ParentName");
				parentNameAttr.Value = hideMouth ? "FSHumanBeardHideMouth" : "FSHumanBeardShowMouth";
				xmlNode2.Attributes.Append(parentNameAttr);

				var defNameElem = xmlNode2.OwnerDocument.CreateElement("defName");
				defNameElem.InnerText = xmlNode["defName"].InnerText;
				xmlNode2.AppendChild(defNameElem);
				
				var labelElem = xmlNode2.OwnerDocument.CreateElement("label");
				labelElem.InnerText = xmlNode["label"].InnerText;
				xmlNode2.AppendChild(labelElem);

				var partClassElem = xmlNode2.OwnerDocument.CreateElement("partClass");
				var categoryDefElem = partClassElem.OwnerDocument.CreateElement("categoryDef");
				var subCategoryElem = partClassElem.OwnerDocument.CreateElement("subcategory");
				categoryDefElem.InnerText = "Beard";
				subCategoryElem.InnerText = subcategory;
				partClassElem.AppendChild(categoryDefElem);
				partClassElem.AppendChild(subCategoryElem);
				xmlNode2.AppendChild(partClassElem);

				xmlNode2.AppendChild(xmlNode["hairTags"]);

				var defaultTexPathElem = xmlNode2.OwnerDocument.CreateElement("defaultTexPath");
				defaultTexPathElem.InnerText = xmlNode["texPath"].InnerText;
				xmlNode2.AppendChild(defaultTexPathElem);

				xmlNode.ParentNode.InsertBefore(xmlNode2, xmlNode);
				xmlNode.ParentNode.RemoveChild(xmlNode);
			}
			return true;
		}
	}
}
