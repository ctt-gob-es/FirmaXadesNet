#region

using System;
using System.Collections;
using System.Xml;

#endregion

namespace Microsoft.Xades
{
    /// <summary>
    ///     This class contains a collection of DocumentationReferences
    /// </summary>
    public class DocumentationReferences
    {
        #region Constructors

        /// <summary>
        ///     Default constructor
        /// </summary>
        public DocumentationReferences()
        {
            DocumentationReferenceCollection = new DocumentationReferenceCollection();
        }

        #endregion

        #region Public properties

        /// <summary>
        ///     Collection of documentation references
        /// </summary>
        public DocumentationReferenceCollection DocumentationReferenceCollection { get; set; }

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

            if (DocumentationReferenceCollection.Count > 0) retVal = true;

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
            DocumentationReference newDocumentationReference;
            IEnumerator enumerator;
            XmlElement iterationXmlElement;

            if (xmlElement == null) throw new ArgumentNullException("xmlElement");

            xmlNamespaceManager = new XmlNamespaceManager(xmlElement.OwnerDocument.NameTable);
            xmlNamespaceManager.AddNamespace("xsd", XadesSignedXml.XadesNamespaceUri);

            DocumentationReferenceCollection.Clear();
            xmlNodeList = xmlElement.SelectNodes("xsd:DocumentationReference", xmlNamespaceManager);
            enumerator = xmlNodeList.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    iterationXmlElement = enumerator.Current as XmlElement;
                    if (iterationXmlElement != null)
                    {
                        newDocumentationReference = new DocumentationReference();
                        newDocumentationReference.LoadXml(iterationXmlElement);
                        DocumentationReferenceCollection.Add(newDocumentationReference);
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
            retVal = creationXmlDocument.CreateElement("DocumentationReferences", XadesSignedXml.XadesNamespaceUri);

            if (DocumentationReferenceCollection.Count > 0)
                foreach (DocumentationReference documentationReference in DocumentationReferenceCollection)
                    if (documentationReference.HasChanged())
                        retVal.AppendChild(creationXmlDocument.ImportNode(documentationReference.GetXml(), true));

            return retVal;
        }

        #endregion
    }
}