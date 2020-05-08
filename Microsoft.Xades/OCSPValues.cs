#region

using System;
using System.Collections;
using System.Security.Cryptography.Xml;
using System.Xml;

#endregion

namespace Microsoft.Xades
{
    /// <summary>
    ///     This class contains a collection of OCSPValues
    /// </summary>
    public class OCSPValues
    {
        #region Constructors

        /// <summary>
        ///     Default constructor
        /// </summary>
        public OCSPValues()
        {
            OCSPValueCollection = new OCSPValueCollection();
        }

        #endregion

        #region Public properties

        /// <summary>
        ///     Collection of OCSP values
        /// </summary>
        public OCSPValueCollection OCSPValueCollection { get; set; }

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

            if (OCSPValueCollection.Count > 0) retVal = true;

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
            OCSPValue newOCSPValue;
            IEnumerator enumerator;
            XmlElement iterationXmlElement;

            if (xmlElement == null) throw new ArgumentNullException("xmlElement");

            xmlNamespaceManager = new XmlNamespaceManager(xmlElement.OwnerDocument.NameTable);
            xmlNamespaceManager.AddNamespace("xades", XadesSignedXml.XadesNamespaceUri);

            OCSPValueCollection.Clear();
            //xmlNodeList = xmlElement.SelectNodes("xades:OCSPValue", xmlNamespaceManager);
            xmlNodeList = xmlElement.SelectNodes("xades:EncapsulatedOCSPValue", xmlNamespaceManager);
            enumerator = xmlNodeList.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    iterationXmlElement = enumerator.Current as XmlElement;
                    if (iterationXmlElement != null)
                    {
                        newOCSPValue = new OCSPValue();
                        newOCSPValue.LoadXml(iterationXmlElement);
                        OCSPValueCollection.Add(newOCSPValue);
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
            retVal = creationXmlDocument.CreateElement(XadesSignedXml.XmlXadesPrefix, "OCSPValues",
                XadesSignedXml.XadesNamespaceUri);
            retVal.SetAttribute("xmlns:ds", SignedXml.XmlDsigNamespaceUrl);


            if (OCSPValueCollection.Count > 0)
                foreach (OCSPValue ocspValue in OCSPValueCollection)
                    if (ocspValue.HasChanged())
                        retVal.AppendChild(creationXmlDocument.ImportNode(ocspValue.GetXml(), true));

            return retVal;
        }

        #endregion
    }
}