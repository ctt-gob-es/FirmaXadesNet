#region

using System;
using System.Collections;
using System.Xml;

#endregion

namespace Microsoft.Xades
{
    /// <summary>
    ///     This class contains a collection of OtherRefs
    /// </summary>
    public class OtherRefs
    {
        #region Constructors

        /// <summary>
        ///     Default constructor
        /// </summary>
        public OtherRefs()
        {
            OtherRefCollection = new OtherRefCollection();
        }

        #endregion

        #region Public properties

        /// <summary>
        ///     Collection of other refs
        /// </summary>
        public OtherRefCollection OtherRefCollection { get; set; }

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

            if (OtherRefCollection.Count > 0) retVal = true;

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
            OtherRef newOtherRef;
            IEnumerator enumerator;
            XmlElement iterationXmlElement;

            if (xmlElement == null) throw new ArgumentNullException("xmlElement");

            xmlNamespaceManager = new XmlNamespaceManager(xmlElement.OwnerDocument.NameTable);
            xmlNamespaceManager.AddNamespace("xsd", XadesSignedXml.XadesNamespaceUri);

            OtherRefCollection.Clear();
            xmlNodeList = xmlElement.SelectNodes("xsd:OtherRef", xmlNamespaceManager);
            enumerator = xmlNodeList.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    iterationXmlElement = enumerator.Current as XmlElement;
                    if (iterationXmlElement != null)
                    {
                        newOtherRef = new OtherRef();
                        newOtherRef.LoadXml(iterationXmlElement);
                        OtherRefCollection.Add(newOtherRef);
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
            retVal = creationXmlDocument.CreateElement("OtherRefs", XadesSignedXml.XadesNamespaceUri);

            if (OtherRefCollection.Count > 0)
                foreach (OtherRef otherRef in OtherRefCollection)
                    if (otherRef.HasChanged())
                        retVal.AppendChild(creationXmlDocument.ImportNode(otherRef.GetXml(), true));

            return retVal;
        }

        #endregion
    }
}