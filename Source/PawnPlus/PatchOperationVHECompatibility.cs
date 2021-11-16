namespace PawnPlus
{
    using System.Xml;

    using Verse;

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
				XmlAttribute parentNameAttr = xmlNode.OwnerDocument.CreateAttribute("ParentName");
				parentNameAttr.Value = hideMouth ? "FSHumanBeardHideMouth" : "FSHumanBeardShowMouth";
				xmlNode2.Attributes.Append(parentNameAttr);

				XmlElement defNameElem = xmlNode2.OwnerDocument.CreateElement("defName");
				defNameElem.InnerText = xmlNode["defName"].InnerText;
				xmlNode2.AppendChild(defNameElem);
				
				XmlElement labelElem = xmlNode2.OwnerDocument.CreateElement("label");
				labelElem.InnerText = xmlNode["label"].InnerText;
				xmlNode2.AppendChild(labelElem);

				XmlElement partClassElem = xmlNode2.OwnerDocument.CreateElement("partClass");
				XmlElement categoryDefElem = partClassElem.OwnerDocument.CreateElement("categoryDef");
				XmlElement subCategoryElem = partClassElem.OwnerDocument.CreateElement("subcategory");
				categoryDefElem.InnerText = "Beard";
				subCategoryElem.InnerText = subcategory;
				partClassElem.AppendChild(categoryDefElem);
				partClassElem.AppendChild(subCategoryElem);
				xmlNode2.AppendChild(partClassElem);

				xmlNode2.AppendChild(xmlNode["hairTags"]);

				XmlElement defaultTexPathElem = xmlNode2.OwnerDocument.CreateElement("defaultTexPath");
				defaultTexPathElem.InnerText = xmlNode["texPath"].InnerText;
				xmlNode2.AppendChild(defaultTexPathElem);

				xmlNode.ParentNode.InsertBefore(xmlNode2, xmlNode);
				xmlNode.ParentNode.RemoveChild(xmlNode);
			}

			return true;
		}
	}
}
