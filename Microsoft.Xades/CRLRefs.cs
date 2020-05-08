#region

using System;
using System.Collections;
using System.Xml;

#endregion

namespace Microsoft.Xades
{
    /// <summary>
    ///     Class that contains a collection of CRL references
    /// </summary>
    public class CRLRefs
    {
        #region Constructors

        /// <summary>
        ///     Default constructor
        /// </summary>
        public CRLRefs()
        {
            CRLRefCollection = new CRLRefCollection();
        }

        #endregion

        #region Public properties

        /// <summary>
        ///     Collection of
        /// </summary>
        public CRLRefCollection CRLRefCollection { get; set; }

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

            if (CRLRefCollection.Count > 0) retVal = true;

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
            CRLRef newCRLRef;
            IEnumerator enumerator;
            XmlElement iterationXmlElement;

            if (xmlElement == null) throw new ArgumentNullException("xmlElement");

            xmlNamespaceManager = new XmlNamespaceManager(xmlElement.OwnerDocument.NameTable);
            xmlNamespaceManager.AddNamespace("xades", XadesSignedXml.XadesNamespaceUri);

            CRLRefCollection.Clear();
            xmlNodeList = xmlElement.SelectNodes("xades:CRLRef", xmlNamespaceManager);
            enumerator = xmlNodeList.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    iterationXmlElement = enumerator.Current as XmlElement;
                    if (iterationXmlElement != null)
                    {
                        newCRLRef = new CRLRef();
                        newCRLRef.LoadXml(iterationXmlElement);
                        CRLRefCollection.Add(newCRLRef);
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
            retVal = creationXmlDocument.CreateElement("CRLRefs", XadesSignedXml.XadesNamespaceUri);

            if (CRLRefCollection.Count > 0)
                foreach (CRLRef crlRef in CRLRefCollection)
                    if (crlRef.HasChanged())
                        retVal.AppendChild(creationXmlDocument.ImportNode(crlRef.GetXml(), true));

            return retVal;
        }

        #endregion
    }
}