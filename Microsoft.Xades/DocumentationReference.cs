#region

using System;
using System.Xml;

#endregion

namespace Microsoft.Xades
{
    /// <summary>
    ///     DocumentationReference points to further explanatory documentation
    ///     of the object identifier
    /// </summary>
    public class DocumentationReference
    {
        #region Public properties

        /// <summary>
        ///     Pointer to further explanatory documentation of the object identifier
        /// </summary>
        public string DocumentationReferenceUri { get; set; }

        #endregion

        #region Private variables

        #endregion

        #region Constructors

        #endregion

        #region Public methods

        /// <summary>
        ///     Check to see if something has changed in this instance and needs to be serialized
        /// </summary>
        /// <returns>Flag indicating if a member needs serialization</returns>
        public bool HasChanged()
        {
            var retVal = false;

            if (!string.IsNullOrEmpty(DocumentationReferenceUri)) retVal = true;

            return retVal;
        }

        /// <summary>
        ///     Load state from an XML element
        /// </summary>
        /// <param name="xmlElement">XML element containing new state</param>
        public void LoadXml(XmlElement xmlElement)
        {
            if (xmlElement == null) throw new ArgumentNullException("xmlElement");

            DocumentationReferenceUri = xmlElement.InnerText;
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
            retVal = creationXmlDocument.CreateElement("DocumentationReference", XadesSignedXml.XadesNamespaceUri);
            retVal.InnerText = DocumentationReferenceUri;

            return retVal;
        }

        #endregion
    }
}