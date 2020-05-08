#region

using System;
using System.Collections;
using System.Xml;

#endregion

namespace Microsoft.Xades
{
    /// <summary>
    ///     The CertifiedRoles element contains one or more wrapped attribute
    ///     certificates for the signer
    /// </summary>
    public class CertifiedRoles
    {
        #region Constructors

        /// <summary>
        ///     Default constructor
        /// </summary>
        public CertifiedRoles()
        {
            CertifiedRoleCollection = new CertifiedRoleCollection();
        }

        #endregion

        #region Public properties

        /// <summary>
        ///     Collection of certified roles
        /// </summary>
        public CertifiedRoleCollection CertifiedRoleCollection { get; set; }

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
            var retVal = false;

            if (CertifiedRoleCollection.Count > 0) retVal = true;

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
            EncapsulatedPKIData newCertifiedRole;
            IEnumerator enumerator;
            XmlElement iterationXmlElement;

            if (xmlElement == null) throw new ArgumentNullException("xmlElement");

            xmlNamespaceManager = new XmlNamespaceManager(xmlElement.OwnerDocument.NameTable);
            xmlNamespaceManager.AddNamespace("xsd", XadesSignedXml.XadesNamespaceUri);

            CertifiedRoleCollection.Clear();
            xmlNodeList = xmlElement.SelectNodes("xsd:CertifiedRole", xmlNamespaceManager);
            enumerator = xmlNodeList.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    iterationXmlElement = enumerator.Current as XmlElement;
                    if (iterationXmlElement != null)
                    {
                        newCertifiedRole = new EncapsulatedPKIData("CertifiedRole");
                        newCertifiedRole.LoadXml(iterationXmlElement);
                        CertifiedRoleCollection.Add(newCertifiedRole);
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
            retVal = creationXmlDocument.CreateElement(XadesSignedXml.XmlXadesPrefix, "CertifiedRoles",
                XadesSignedXml.XadesNamespaceUri);

            if (CertifiedRoleCollection.Count > 0)
                foreach (EncapsulatedPKIData certifiedRole in CertifiedRoleCollection)
                    if (certifiedRole.HasChanged())
                        retVal.AppendChild(creationXmlDocument.ImportNode(certifiedRole.GetXml(), true));

            return retVal;
        }

        #endregion
    }
}