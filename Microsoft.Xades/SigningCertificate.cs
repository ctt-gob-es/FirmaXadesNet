#region

using System;
using System.Collections;
using System.Security.Cryptography;
using System.Xml;

#endregion

namespace Microsoft.Xades
{
    /// <summary>
    ///     This class has as purpose to provide the simple substitution of the
    ///     certificate. It contains references to certificates and digest values
    ///     computed on them
    /// </summary>
    public class SigningCertificate
    {
        #region Constructors

        /// <summary>
        ///     Default constructor
        /// </summary>
        public SigningCertificate()
        {
            CertCollection = new CertCollection();
        }

        #endregion

        #region Public properties

        /// <summary>
        ///     A collection of certs
        /// </summary>
        public CertCollection CertCollection { get; set; }

        #endregion

        #region Private variables

        #endregion

        #region Public methods

        /// <summary>
        ///     Check to see if something has changed in this instance and needs to be serialized
        /// </summary>
        /// <returns>Flag indicating if a member needs serialization</returns>
        public bool HasChanged()
        {
            return true; //Should always be considered dirty
        }

        /// <summary>
        ///     Load state from an XML element
        /// </summary>
        /// <param name="xmlElement">XML element containing new state</param>
        public void LoadXml(XmlElement xmlElement)
        {
            XmlNamespaceManager xmlNamespaceManager;
            XmlNodeList xmlNodeList;
            IEnumerator enumerator;
            XmlElement iterationXmlElement;
            Cert newCert;

            if (xmlElement == null) throw new ArgumentNullException("xmlElement");

            xmlNamespaceManager = new XmlNamespaceManager(xmlElement.OwnerDocument.NameTable);
            xmlNamespaceManager.AddNamespace("xsd", XadesSignedXml.XadesNamespaceUri);

            CertCollection.Clear();
            xmlNodeList = xmlElement.SelectNodes("xsd:Cert", xmlNamespaceManager);
            enumerator = xmlNodeList.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    iterationXmlElement = enumerator.Current as XmlElement;
                    if (iterationXmlElement != null)
                    {
                        newCert = new Cert();
                        newCert.LoadXml(iterationXmlElement);
                        CertCollection.Add(newCert);
                    }
                }
            }
            finally
            {
                var disposable = enumerator as IDisposable;
                if (disposable != null) disposable.Dispose();
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
            retVal = creationXmlDocument.CreateElement(XadesSignedXml.XmlXadesPrefix, "SigningCertificate",
                XadesSignedXml.XadesNamespaceUri);

            if (CertCollection.Count > 0)
            {
                foreach (Cert cert in CertCollection)
                    if (cert.HasChanged())
                        retVal.AppendChild(creationXmlDocument.ImportNode(cert.GetXml(), true));
            }
            else
            {
                throw new CryptographicException("SigningCertificate.Certcollection should have count > 0");
            }

            return retVal;
        }

        #endregion
    }
}