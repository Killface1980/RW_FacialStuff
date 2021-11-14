﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Verse;

namespace PawnPlus
{
	public class PatchOperationHairDefConversion : PatchOperationPathed
	{
		protected override bool ApplyWorker(XmlDocument xml)
		{
			foreach(object item in xml.SelectNodes(xpath))
			{
				XmlNode xmlNode = item as XmlNode;
				XmlNode modExtHairNode = xmlNode["modExtensions"]?.SelectSingleNode("/li[@Class=\"PawnPlus.Parts.ModExtensionHair\"");
				// The PartDef for the HairDef already exists, so skip it.
				if(modExtHairNode != null)
				{
					continue;
				}

				XmlNode xmlNode2 = xmlNode.OwnerDocument.CreateElement("PawnPlus.Defs.PartDef");

				// Create PartDef for the HairDef
				var defNameElem = xmlNode2.OwnerDocument.CreateElement("defName");
				defNameElem.InnerText = "FSHumanHair" + xmlNode["defName"].InnerText;
				xmlNode2.AppendChild(defNameElem);

				var labelElem = xmlNode2.OwnerDocument.CreateElement("label");
				labelElem.InnerText = xmlNode["label"].InnerText;
				xmlNode2.AppendChild(labelElem);

				var partClassElem = xmlNode2.OwnerDocument.CreateElement("partClass");
				var categoryDefElem = partClassElem.OwnerDocument.CreateElement("categoryDef");
				categoryDefElem.InnerText = "Hair";
				partClassElem.AppendChild(categoryDefElem);
				xmlNode2.AppendChild(partClassElem);

				var hairTagsElem = xmlNode2.OwnerDocument.CreateElement("hairTags");
				hairTagsElem.InnerXml = xmlNode["hairTags"].InnerXml;
				xmlNode2.AppendChild(hairTagsElem);

				var hairGenderElem = xmlNode2.OwnerDocument.CreateElement("hairGender");
				hairGenderElem.InnerXml = xmlNode["hairGender"].InnerXml;
				xmlNode2.AppendChild(hairGenderElem);

				var partRendererElem = xmlNode2.OwnerDocument.CreateElement("partRenderer");
				var partRendererClassAttr = partRendererElem.OwnerDocument.CreateAttribute("Class");
				partRendererClassAttr.Value = "PawnPlus.Parts.HumanHairRenderer";
				partRendererElem.Attributes.Append(partRendererClassAttr);
				var additionalOffsetElem = partRendererElem.OwnerDocument.CreateElement("additionalOffset");
				// Constant is "YOffsetIntervalClothes". Adding this will ensure that hair is above the head 
				// and apparel regardless of the head's orientation, but also ensure that it remains below headwear.
				// Kind of arbitrary, but at least it works.
				const float yOffset = 0.00306122447f;
				additionalOffsetElem.InnerText = "(0, " + yOffset + ", 0)";
				partRendererElem.AppendChild(additionalOffsetElem);
				xmlNode2.AppendChild(partRendererElem);

				var raceBodyDefElem = xmlNode2.OwnerDocument.CreateElement("raceBodyDef");
				raceBodyDefElem.InnerText = "Human";
				xmlNode2.AppendChild(raceBodyDefElem);

				var partsElem = xmlNode2.OwnerDocument.CreateElement("parts");
				var partsLiElem = partsElem.OwnerDocument.CreateElement("li");
				var renderNodeNameElem = partsLiElem.OwnerDocument.CreateElement("renderNodeName");
				renderNodeNameElem.InnerText = "HumanHead";
				partsLiElem.AppendChild(renderNodeNameElem);
				partsElem.AppendChild(partsLiElem);
				xmlNode2.AppendChild(partsElem);

				var defaultTexPathElem = xmlNode2.OwnerDocument.CreateElement("defaultTexPath");
				defaultTexPathElem.InnerText = xmlNode["texPath"].InnerText;
				xmlNode2.AppendChild(defaultTexPathElem);

				var modExtElem = xmlNode["modExtensions"];
				if(modExtElem == null)
				{
					modExtElem = xmlNode.OwnerDocument.CreateElement("modExtensions");
					xmlNode.AppendChild(modExtElem);
				}
				var modExtLiElem = modExtElem.OwnerDocument.CreateElement("li");
				var modExtLiClassAttrib = modExtLiElem.OwnerDocument.CreateAttribute("Class");
				modExtLiClassAttrib.Value = "PawnPlus.Parts.ModExtensionHair";
				modExtLiElem.Attributes.Append(modExtLiClassAttrib);
				var partDefElem = modExtLiElem.OwnerDocument.CreateElement("partDef");
				partDefElem.InnerText = defNameElem.InnerText;
				modExtLiElem.AppendChild(partDefElem);
				modExtElem.AppendChild(modExtLiElem);

				xmlNode.ParentNode.InsertBefore(xmlNode2, xmlNode);
				xmlNode.ParentNode.AppendChild(xmlNode2);
			}
			return true;
		}
	}
}
