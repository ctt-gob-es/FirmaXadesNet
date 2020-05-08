#region

using System.Xml;

#endregion

namespace Microsoft.Xades
{
    /// <summary>
    ///     Alternative forms of validation data can be included in this class
    /// </summary>
    public class OtherRef
    {
        #region Public properties

        /// <summary>
        ///     The generic XML element that represents any other type of ref
        /// </summary>
        public XmlElement AnyXmlElement { get; set; }

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

            if (AnyXmlElement != null) retVal = true;

            return retVal;
        }

        /// <summary>
        ///     Load state from an XML element
        /// </summary>
        /// <param name="xmlElement">XML element containing new state</param>
        public void LoadXml(XmlElement xmlElement)
        {
            AnyXmlElement = xmlElement;
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
            retVal = creationXmlDocument.CreateElement("OtherRef", XadesSignedXml.XadesNamespaceUri);

            if (AnyXmlElement != null) retVal.AppendChild(creationXmlDocument.ImportNode(AnyXmlElement, true));

            return retVal;
        }

        #endregion
    }
}