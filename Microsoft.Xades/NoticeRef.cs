#region

using System;
using System.Security.Cryptography;
using System.Xml;

#endregion

namespace Microsoft.Xades
{
    /// <summary>
    ///     The NoticeRef element names an organization and identifies by
    ///     numbers a group of textual statements prepared by that organization,
    ///     so that the application could get the explicit notices from a notices file.
    /// </summary>
    public class NoticeRef
    {
        #region Constructors

        /// <summary>
        ///     Default constructor
        /// </summary>
        public NoticeRef()
        {
            NoticeNumbers = new NoticeNumbers();
        }

        #endregion

        #region Private variables

        #endregion

        #region Public properties

        /// <summary>
        ///     Organization issuing the signature policy
        /// </summary>
        public string Organization { get; set; }

        /// <summary>
        ///     Numerical identification of textual statements prepared by the organization,
        ///     so that the application can get the explicit notices from a notices file.
        /// </summary>
        public NoticeNumbers NoticeNumbers { get; set; }

        #endregion

        #region Public methods

        /// <summary>
        ///     Check to see if something has changed in this instance and needs to be serialized
        /// </summary>
        /// <returns>Flag indicating if a member needs serialization</returns>
        public bool HasChanged()
        {
            var retVal = false;

            if (!string.IsNullOrEmpty(Organization)) retVal = true;

            if (NoticeNumbers != null && NoticeNumbers.HasChanged()) retVal = true;

            return retVal;
        }

        /// <summary>
        ///     Load state from an XML element
        /// </summary>
        /// <param name="xmlElement">XML element containing new state</param>
        public void LoadXml(XmlElement xmlElement)
        {
            XmlNamespaceManager xmlNamespaceManager;
            XmlNodeList xmlNodeList;

            if (xmlElement == null) throw new ArgumentNullException("xmlElement");

            xmlNamespaceManager = new XmlNamespaceManager(xmlElement.OwnerDocument.NameTable);
            xmlNamespaceManager.AddNamespace("xsd", XadesSignedXml.XadesNamespaceUri);

            xmlNodeList = xmlElement.SelectNodes("xsd:Organization", xmlNamespaceManager);
            if (xmlNodeList.Count == 0) throw new CryptographicException("Organization missing");
            Organization = xmlNodeList.Item(0).InnerText;

            xmlNodeList = xmlElement.SelectNodes("xsd:NoticeNumbers", xmlNamespaceManager);
            if (xmlNodeList.Count == 0) throw new CryptographicException("NoticeNumbers missing");
            NoticeNumbers = new NoticeNumbers();
            NoticeNumbers.LoadXml((XmlElement) xmlNodeList.Item(0));
        }

        /// <summary>
        ///     Returns the XML representation of the this object
        /// </summary>
        /// <returns>XML element containing the state of this object</returns>
        public XmlElement GetXml()
        {
            XmlDocument creationXmlDocument;
            XmlElement bufferXmlElement;
            XmlElement retVal;

            creationXmlDocument = new XmlDocument();
            retVal = creationXmlDocument.CreateElement("NoticeRef", XadesSignedXml.XadesNamespaceUri);

            if (Organization == null) throw new CryptographicException("Organization can't be null");
            bufferXmlElement = creationXmlDocument.CreateElement("Organization", XadesSignedXml.XadesNamespaceUri);
            bufferXmlElement.InnerText = Organization;
            retVal.AppendChild(bufferXmlElement);

            if (NoticeNumbers == null) throw new CryptographicException("NoticeNumbers can't be null");
            retVal.AppendChild(creationXmlDocument.ImportNode(NoticeNumbers.GetXml(), true));

            return retVal;
        }

        #endregion
    }
}