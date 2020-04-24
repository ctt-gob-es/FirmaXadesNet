#region

using System;
using System.Security.Cryptography;
using System.Xml;

#endregion

namespace Microsoft.Xades
{
    /// <summary>
    ///     ObjectIdentifier allows the specification of an unique and permanent
    ///     object of an object and some additional information about the nature of
    ///     the	data object
    /// </summary>
    public class ObjectIdentifier
    {
        #region Private variables

        #endregion

        #region Public properties

        /// <summary>
        ///     The name of the element when serializing
        /// </summary>
        public string TagName { get; set; }

        /// <summary>
        ///     Specification of an unique and permanent identifier
        /// </summary>
        public Identifier Identifier { get; set; }

        /// <summary>
        ///     Textual description of the nature of the data object
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     References to documents where additional information about the
        ///     nature of the data object can be found
        /// </summary>
        public DocumentationReferences DocumentationReferences { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Default constructor
        /// </summary>
        public ObjectIdentifier()
        {
            Identifier = new Identifier();
            DocumentationReferences = new DocumentationReferences();
        }

        /// <summary>
        ///     Constructor with TagName
        /// </summary>
        /// <param name="tagName">Name of the tag when serializing with GetXml</param>
        public ObjectIdentifier(string tagName) : this()
        {
            TagName = tagName;
        }

        #endregion

        #region Public methods

        /// <summary>
        ///     Check to see if something has changed in this instance and needs to be serialized
        /// </summary>
        /// <returns>Flag indicating if a member needs serialization</returns>
        public bool HasChanged()
        {
            var retVal = false;

            if (Identifier != null && Identifier.HasChanged()) retVal = true;

            if (!string.IsNullOrEmpty(Description)) retVal = true;

            if (DocumentationReferences != null && DocumentationReferences.HasChanged()) retVal = true;

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

            xmlNodeList = xmlElement.SelectNodes("xsd:Identifier", xmlNamespaceManager);
            if (xmlNodeList.Count == 0) throw new CryptographicException("Identifier missing");
            Identifier = new Identifier();
            Identifier.LoadXml((XmlElement) xmlNodeList.Item(0));

            xmlNodeList = xmlElement.SelectNodes("xsd:Description", xmlNamespaceManager);
            if (xmlNodeList.Count != 0) Description = xmlNodeList.Item(0).InnerText;

            xmlNodeList = xmlElement.SelectNodes("xsd:DocumentationReferences", xmlNamespaceManager);
            if (xmlNodeList.Count != 0)
            {
                DocumentationReferences = new DocumentationReferences();
                DocumentationReferences.LoadXml((XmlElement) xmlNodeList.Item(0));
            }
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
            retVal = creationXmlDocument.CreateElement(XadesSignedXml.XmlXadesPrefix, TagName,
                XadesSignedXml.XadesNamespaceUri);

            if (Identifier != null && Identifier.HasChanged())
                retVal.AppendChild(creationXmlDocument.ImportNode(Identifier.GetXml(), true));
            else
                throw new CryptographicException("Identifier element missing in OjectIdentifier");

            bufferXmlElement = creationXmlDocument.CreateElement(XadesSignedXml.XmlXadesPrefix, "Description",
                XadesSignedXml.XadesNamespaceUri);
            bufferXmlElement.InnerText = Description;
            retVal.AppendChild(bufferXmlElement);

            if (DocumentationReferences != null && DocumentationReferences.HasChanged())
                retVal.AppendChild(creationXmlDocument.ImportNode(DocumentationReferences.GetXml(), true));

            return retVal;
        }

        #endregion
    }
}