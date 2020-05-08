#region

using System;
using System.Collections;
using System.Security.Cryptography.Xml;
using System.Xml;

#endregion

namespace Microsoft.Xades
{
    /// <summary>
    ///     This class contains a collection of OCSPRefs
    /// </summary>
    public class OCSPRefs
    {
        #region Constructors

        /// <summary>
        ///     Default constructor
        /// </summary>
        public OCSPRefs()
        {
            OCSPRefCollection = new OCSPRefCollection();
        }

        #endregion

        #region Public properties

        /// <summary>
        ///     Collection of OCSP refs
        /// </summary>
        public OCSPRefCollection OCSPRefCollection { get; set; }

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

            if (OCSPRefCollection.Count > 0) retVal = true;

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
            OCSPRef newOCSPRef;
            IEnumerator enumerator;
            XmlElement iterationXmlElement;

            if (xmlElement == null) throw new ArgumentNullException("xmlElement");

            xmlNamespaceManager = new XmlNamespaceManager(xmlElement.OwnerDocument.NameTable);
            xmlNamespaceManager.AddNamespace("xsd", XadesSignedXml.XadesNamespaceUri);

            OCSPRefCollection.Clear();
            xmlNodeList = xmlElement.SelectNodes("xsd:OCSPRef", xmlNamespaceManager);
            enumerator = xmlNodeList.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    iterationXmlElement = enumerator.Current as XmlElement;
                    if (iterationXmlElement != null)
                    {
                        newOCSPRef = new OCSPRef();
                        newOCSPRef.LoadXml(iterationXmlElement);
                        OCSPRefCollection.Add(newOCSPRef);
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
            retVal = creationXmlDocument.CreateElement(XadesSignedXml.XmlXadesPrefix, "OCSPRefs",
                XadesSignedXml.XadesNamespaceUri);
            retVal.SetAttribute("xmlns:ds", SignedXml.XmlDsigNamespaceUrl);

            if (OCSPRefCollection.Count > 0)
                foreach (OCSPRef ocspRef in OCSPRefCollection)
                    if (ocspRef.HasChanged())
                        retVal.AppendChild(creationXmlDocument.ImportNode(ocspRef.GetXml(), true));

            return retVal;
        }

        #endregion
    }
}