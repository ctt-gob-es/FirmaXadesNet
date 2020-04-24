#region

using System;
using System.Collections;
using System.Xml;

#endregion

namespace Microsoft.Xades
{
    /// <summary>
    ///     This class contains a collection of CRL values
    /// </summary>
    public class CRLValues
    {
        #region Constructors

        /// <summary>
        ///     Default constructor
        /// </summary>
        public CRLValues()
        {
            CRLValueCollection = new CRLValueCollection();
        }

        #endregion

        #region Public properties

        /// <summary>
        ///     Collection of CRLValues
        /// </summary>
        public CRLValueCollection CRLValueCollection { get; set; }

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

            if (CRLValueCollection.Count > 0) retVal = true;

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
            CRLValue newCRLValue;
            IEnumerator enumerator;
            XmlElement iterationXmlElement;

            if (xmlElement == null) throw new ArgumentNullException("xmlElement");

            xmlNamespaceManager = new XmlNamespaceManager(xmlElement.OwnerDocument.NameTable);
            xmlNamespaceManager.AddNamespace("xsd", XadesSignedXml.XadesNamespaceUri);

            CRLValueCollection.Clear();
            xmlNodeList = xmlElement.SelectNodes("xsd:EncapsulatedCRLValue", xmlNamespaceManager);
            enumerator = xmlNodeList.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    iterationXmlElement = enumerator.Current as XmlElement;
                    if (iterationXmlElement != null)
                    {
                        newCRLValue = new CRLValue();
                        newCRLValue.LoadXml(iterationXmlElement);
                        CRLValueCollection.Add(newCRLValue);
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
            retVal = creationXmlDocument.CreateElement(XadesSignedXml.XmlXadesPrefix, "CRLValues",
                XadesSignedXml.XadesNamespaceUri);

            if (CRLValueCollection.Count > 0)
                foreach (CRLValue crlValue in CRLValueCollection)
                    if (crlValue.HasChanged())
                        retVal.AppendChild(creationXmlDocument.ImportNode(crlValue.GetXml(), true));

            return retVal;
        }

        #endregion
    }
}