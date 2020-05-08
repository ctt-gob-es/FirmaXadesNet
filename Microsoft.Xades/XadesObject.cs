#region

using System;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;

#endregion

namespace Microsoft.Xades
{
    /// <summary>
    ///     This class represents the unique object of a XAdES signature that
    ///     contains all XAdES information
    /// </summary>
    public class XadesObject
    {
        #region Constructors

        /// <summary>
        ///     Default constructor
        /// </summary>
        public XadesObject()
        {
            QualifyingProperties = new QualifyingProperties();
        }

        #endregion

        #region Private variable

        #endregion

        #region Public properties

        /// <summary>
        ///     Id attribute of the XAdES object
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///     The QualifyingProperties element acts as a container element for
        ///     all the qualifying information that should be added to an XML
        ///     signature.
        /// </summary>
        public QualifyingProperties QualifyingProperties { get; set; }

        #endregion

        #region Public methods

        /// <summary>
        ///     Check to see if something has changed in this instance and needs to be serialized
        /// </summary>
        /// <returns>Flag indicating if a member needs serialization</returns>
        public bool HasChanged()
        {
            var retVal = false;

            if (Id != null && Id != "") retVal = true;

            if (QualifyingProperties != null && QualifyingProperties.HasChanged()) retVal = true;

            return retVal;
        }

        /// <summary>
        ///     Load state from an XML element
        /// </summary>
        /// <param name="xmlElement">XML element containing new state</param>
        /// <param name="counterSignedXmlElement">Element containing parent signature (needed if there are counter signatures)</param>
        public void LoadXml(XmlElement xmlElement, XmlElement counterSignedXmlElement)
        {
            XmlNamespaceManager xmlNamespaceManager;
            XmlNodeList xmlNodeList;

            if (xmlElement == null) throw new ArgumentNullException("xmlElement");
            if (xmlElement.HasAttribute("Id"))
                Id = xmlElement.GetAttribute("Id");
            else
                Id = "";

            xmlNamespaceManager = new XmlNamespaceManager(xmlElement.OwnerDocument.NameTable);
            xmlNamespaceManager.AddNamespace("xades", XadesSignedXml.XadesNamespaceUri);

            xmlNodeList = xmlElement.SelectNodes("xades:QualifyingProperties", xmlNamespaceManager);
            if (xmlNodeList.Count == 0) throw new CryptographicException("QualifyingProperties missing");
            QualifyingProperties = new QualifyingProperties();
            QualifyingProperties.LoadXml((XmlElement) xmlNodeList.Item(0), counterSignedXmlElement);

            xmlNodeList = xmlElement.SelectNodes("xades:QualifyingPropertiesReference", xmlNamespaceManager);
            if (xmlNodeList.Count != 0)
                throw new CryptographicException(
                    "Current implementation can't handle QualifyingPropertiesReference element");
        }

        /// <summary>
        ///     Returns the XML representation of the this object
        /// </summary>
        /// <returns>XML element containing the state of this object</returns>
        public XmlElement GetXml()
        {
            XmlDocument creationXmlDocument;
            XmlElement retVal;

            creationXmlDocument = new XmlDocument();
            retVal = creationXmlDocument.CreateElement("ds", "Object", SignedXml.XmlDsigNamespaceUrl);
            if (Id != null && Id != "") retVal.SetAttribute("Id", Id);

            if (QualifyingProperties != null && QualifyingProperties.HasChanged())
                retVal.AppendChild(creationXmlDocument.ImportNode(QualifyingProperties.GetXml(), true));

            return retVal;
        }

        #endregion
    }
}