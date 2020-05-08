#region

using System;
using System.Collections;
using System.Xml;

#endregion

namespace Microsoft.Xades
{
    /// <summary>
    ///     The ClaimedRoles element contains a sequence of roles claimed by
    ///     the signer but not certified. Additional contents types may be
    ///     defined on a domain application basis and be part of this element.
    ///     The namespaces given to the corresponding XML schemas will allow
    ///     their unambiguous identification in the case these roles use XML.
    /// </summary>
    public class ClaimedRoles
    {
        #region Constructors

        /// <summary>
        ///     Default constructor
        /// </summary>
        public ClaimedRoles()
        {
            ClaimedRoleCollection = new ClaimedRoleCollection();
        }

        #endregion

        #region Public properties

        /// <summary>
        ///     Collection of claimed roles
        /// </summary>
        public ClaimedRoleCollection ClaimedRoleCollection { get; set; }

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

            if (ClaimedRoleCollection.Count > 0) retVal = true;

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
            ClaimedRole newClaimedRole;
            IEnumerator enumerator;
            XmlElement iterationXmlElement;

            if (xmlElement == null) throw new ArgumentNullException("xmlElement");

            xmlNamespaceManager = new XmlNamespaceManager(xmlElement.OwnerDocument.NameTable);
            xmlNamespaceManager.AddNamespace("xsd", XadesSignedXml.XadesNamespaceUri);

            ClaimedRoleCollection.Clear();
            xmlNodeList = xmlElement.SelectNodes("xsd:ClaimedRole", xmlNamespaceManager);
            enumerator = xmlNodeList.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    iterationXmlElement = enumerator.Current as XmlElement;
                    if (iterationXmlElement != null)
                    {
                        newClaimedRole = new ClaimedRole();
                        newClaimedRole.LoadXml(iterationXmlElement);
                        ClaimedRoleCollection.Add(newClaimedRole);
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
            retVal = creationXmlDocument.CreateElement(XadesSignedXml.XmlXadesPrefix, "ClaimedRoles",
                XadesSignedXml.XadesNamespaceUri);

            if (ClaimedRoleCollection.Count > 0)
                foreach (ClaimedRole claimedRole in ClaimedRoleCollection)
                    if (claimedRole.HasChanged())
                        retVal.AppendChild(creationXmlDocument.ImportNode(claimedRole.GetXml(), true));

            return retVal;
        }

        #endregion
    }
}