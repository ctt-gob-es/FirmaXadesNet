#region

using System;
using System.Security.Cryptography.Xml;
using System.Xml;

#endregion

namespace Microsoft.Xades
{
    /// <summary>
    ///     The Transform element contains a single transformation
    /// </summary>
    public class Transform
    {
        #region Constructors

        #endregion

        #region Private variables

        #endregion

        #region Public properties

        /// <summary>
        ///     Algorithm of the transformation
        /// </summary>
        public string Algorithm { get; set; }

        /// <summary>
        ///     XPath of the transformation
        /// </summary>
        public string XPath { get; set; }

        #endregion

        #region Public methods

        /// <summary>
        ///     Check to see if something has changed in this instance and needs to be serialized
        /// </summary>
        /// <returns>Flag indicating if a member needs serialization</returns>
        public bool HasChanged()
        {
            var retVal = false;

            if (!string.IsNullOrEmpty(Algorithm)) retVal = true;

            if (!string.IsNullOrEmpty(XPath)) retVal = true;

            return retVal;
        }

        /// <summary>
        ///     Load state from an XML element
        /// </summary>
        /// <param name="xmlElement">XML element containing new state</param>
        public void LoadXml(XmlElement xmlElement)
        {
            XmlNodeList xmlNodeList;

            if (xmlElement == null) throw new ArgumentNullException("xmlElement");
            if (xmlElement.HasAttribute("Algorithm"))
                Algorithm = xmlElement.GetAttribute("Algorithm");
            else
                Algorithm = "";

            xmlNodeList = xmlElement.SelectNodes("XPath");
            if (xmlNodeList.Count != 0)
                XPath = xmlNodeList.Item(0).InnerText;
            else
                XPath = "";
        }

        /// <summary>
        ///     Returns the XML representation of the this object
        /// </summary>
        /// <returns>XML element containing the state of this object</returns>
        public XmlElement GetXml()
        {
            XmlDocument creationXmlDocument;
            XmlElement retVal;
            XmlElement bufferXmlElement;

            creationXmlDocument = new XmlDocument();
            retVal = creationXmlDocument.CreateElement("ds", "Transform", SignedXml.XmlDsigNamespaceUrl);

            if (Algorithm != null)
                retVal.SetAttribute("Algorithm", Algorithm);
            else
                retVal.SetAttribute("Algorithm", "");

            if (XPath != null && XPath != "")
            {
                bufferXmlElement = creationXmlDocument.CreateElement("ds", "XPath", SignedXml.XmlDsigNamespaceUrl);
                bufferXmlElement.InnerText = XPath;
                retVal.AppendChild(bufferXmlElement);
            }

            return retVal;
        }

        #endregion
    }
}