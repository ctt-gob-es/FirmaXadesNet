#region

using System;
using System.Security.Cryptography.Xml;
using System.Xml;

#endregion

namespace Microsoft.Xades
{
    /// <summary>
    ///     This clause defines the XML element containing the sequence of
    ///     references to the full set of CA certificates that have been used
    ///     to validate the electronic signature up to (but not including) the
    ///     signer's certificate. This is an unsigned property that qualifies
    ///     the signature.
    ///     An XML electronic signature aligned with the XAdES standard may
    ///     contain at most one CompleteCertificateRefs element.
    /// </summary>
    public class CompleteCertificateRefs
    {
        #region Constructors

        /// <summary>
        ///     Default constructor
        /// </summary>
        public CompleteCertificateRefs()
        {
            CertRefs = new CertRefs();
        }

        #endregion

        #region Private variables

        #endregion

        #region Public properties

        /// <summary>
        ///     The optional Id attribute can be used to make a reference to the CompleteCertificateRefs element
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///     The CertRefs element contains a sequence of Cert elements, incorporating the
        ///     digest of each certificate and optionally the issuer and serial number identifier.
        /// </summary>
        public CertRefs CertRefs { get; set; }

        #endregion

        #region Public methods

        /// <summary>
        ///     Check to see if something has changed in this instance and needs to be serialized
        /// </summary>
        /// <returns>Flag indicating if a member needs serialization</returns>
        public bool HasChanged()
        {
            var retVal = false;

            if (!string.IsNullOrEmpty(Id)) retVal = true;
            if (CertRefs != null && CertRefs.HasChanged()) retVal = true;

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
            if (xmlElement.HasAttribute("Id"))
                Id = xmlElement.GetAttribute("Id");
            else
                Id = "";

            xmlNamespaceManager = new XmlNamespaceManager(xmlElement.OwnerDocument.NameTable);
            xmlNamespaceManager.AddNamespace("xsd", XadesSignedXml.XadesNamespaceUri);

            xmlNodeList = xmlElement.SelectNodes("xsd:CertRefs", xmlNamespaceManager);
            if (xmlNodeList.Count != 0)
            {
                CertRefs = new CertRefs();
                CertRefs.LoadXml((XmlElement) xmlNodeList.Item(0));
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

            creationXmlDocument = new XmlDocument();
            retVal = creationXmlDocument.CreateElement(XadesSignedXml.XmlXadesPrefix, "CompleteCertificateRefs",
                XadesSignedXml.XadesNamespaceUri);
            retVal.SetAttribute("xmlns:ds", SignedXml.XmlDsigNamespaceUrl);

            if (!string.IsNullOrEmpty(Id)) retVal.SetAttribute("Id", Id);

            if (CertRefs != null && CertRefs.HasChanged())
                retVal.AppendChild(creationXmlDocument.ImportNode(CertRefs.GetXml(), true));

            return retVal;
        }

        #endregion
    }
}