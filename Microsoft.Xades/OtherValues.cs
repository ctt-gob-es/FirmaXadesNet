#region

using System;
using System.Collections;
using System.Xml;

#endregion

namespace Microsoft.Xades
{
    /// <summary>
    ///     This class contains a collection of OtherValues
    /// </summary>
    public class OtherValues
    {
        #region Constructors

        /// <summary>
        ///     Default constructor
        /// </summary>
        public OtherValues()
        {
            OtherValueCollection = new OtherValueCollection();
        }

        #endregion

        #region Public properties

        /// <summary>
        ///     Collection of other values
        /// </summary>
        public OtherValueCollection OtherValueCollection { get; set; }

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

            if (OtherValueCollection.Count > 0) retVal = true;

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
            OtherValue newOtherValue;
            IEnumerator enumerator;
            XmlElement iterationXmlElement;

            if (xmlElement == null) throw new ArgumentNullException("xmlElement");

            xmlNamespaceManager = new XmlNamespaceManager(xmlElement.OwnerDocument.NameTable);
            xmlNamespaceManager.AddNamespace("xsd", XadesSignedXml.XadesNamespaceUri);

            OtherValueCollection.Clear();
            xmlNodeList = xmlElement.SelectNodes("xsd:OtherValue", xmlNamespaceManager);
            enumerator = xmlNodeList.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    iterationXmlElement = enumerator.Current as XmlElement;
                    if (iterationXmlElement != null)
                    {
                        newOtherValue = new OtherValue();
                        newOtherValue.LoadXml(iterationXmlElement);
                        OtherValueCollection.Add(newOtherValue);
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
            retVal = creationXmlDocument.CreateElement("OtherValues", XadesSignedXml.XadesNamespaceUri);

            if (OtherValueCollection.Count > 0)
                foreach (OtherValue otherValue in OtherValueCollection)
                    if (otherValue.HasChanged())
                        retVal.AppendChild(creationXmlDocument.ImportNode(otherValue.GetXml(), true));

            return retVal;
        }

        #endregion
    }
}