#region

using System;
using System.Xml;

#endregion

namespace Microsoft.Xades
{
    /// <summary>
    ///     This class refers to one ds:Reference element of the ds:SignedInfo
    ///     corresponding with one data object qualified by this property.
    ///     If some but not all the signed data objects share the same commitment,
    ///     one ObjectReference element must appear for each one of them.
    ///     However, if all the signed data objects share the same commitment,
    ///     the AllSignedDataObjects empty element must be present.
    /// </summary>
    public class ObjectReference
    {
        #region Public properties

        /// <summary>
        ///     Uri of the object reference
        /// </summary>
        public string ObjectReferenceUri { get; set; }

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

            if (ObjectReferenceUri != null && ObjectReferenceUri != "") retVal = true;

            return retVal;
        }

        /// <summary>
        ///     Load state from an XML element
        /// </summary>
        /// <param name="xmlElement">XML element containing new state</param>
        public void LoadXml(XmlElement xmlElement)
        {
            if (xmlElement == null) throw new ArgumentNullException("xmlElement");

            ObjectReferenceUri = xmlElement.InnerText;
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
            retVal = creationXmlDocument.CreateElement("ObjectReference", XadesSignedXml.XadesNamespaceUri);
            retVal.InnerText = ObjectReferenceUri;

            return retVal;
        }

        #endregion
    }
}