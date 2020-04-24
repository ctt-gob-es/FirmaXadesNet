#region

using System;
using System.Collections;
using System.Xml;

#endregion

namespace Microsoft.Xades
{
    /// <summary>
    ///     The UnsignedDataObjectProperties element may contain properties that
    ///     qualify some of the signed data objects.
    /// </summary>
    public class UnsignedDataObjectProperties
    {
        #region Constructors

        /// <summary>
        ///     Default constructor
        /// </summary>
        public UnsignedDataObjectProperties()
        {
            UnsignedDataObjectPropertyCollection = new UnsignedDataObjectPropertyCollection();
        }

        #endregion

        #region Public properties

        /// <summary>
        ///     A collection of unsigned data object properties
        /// </summary>
        public UnsignedDataObjectPropertyCollection UnsignedDataObjectPropertyCollection { get; set; }

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

            if (UnsignedDataObjectPropertyCollection.Count > 0) retVal = true;

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
            UnsignedDataObjectProperty newUnsignedDataObjectProperty;
            IEnumerator enumerator;
            XmlElement iterationXmlElement;

            if (xmlElement == null) throw new ArgumentNullException("xmlElement");

            xmlNamespaceManager = new XmlNamespaceManager(xmlElement.OwnerDocument.NameTable);
            xmlNamespaceManager.AddNamespace("xsd", XadesSignedXml.XadesNamespaceUri);

            UnsignedDataObjectPropertyCollection.Clear();
            xmlNodeList = xmlElement.SelectNodes("xsd:UnsignedDataObjectProperty", xmlNamespaceManager);
            enumerator = xmlNodeList.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    iterationXmlElement = enumerator.Current as XmlElement;
                    if (iterationXmlElement != null)
                    {
                        newUnsignedDataObjectProperty = new UnsignedDataObjectProperty();
                        newUnsignedDataObjectProperty.LoadXml(iterationXmlElement);
                        UnsignedDataObjectPropertyCollection.Add(newUnsignedDataObjectProperty);
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
            retVal = creationXmlDocument.CreateElement("UnsignedDataObjectProperties",
                XadesSignedXml.XadesNamespaceUri);

            if (UnsignedDataObjectPropertyCollection.Count > 0)
                foreach (UnsignedDataObjectProperty unsignedDataObjectProperty in UnsignedDataObjectPropertyCollection)
                    if (unsignedDataObjectProperty.HasChanged())
                        retVal.AppendChild(creationXmlDocument.ImportNode(unsignedDataObjectProperty.GetXml(), true));

            return retVal;
        }

        #endregion
    }
}