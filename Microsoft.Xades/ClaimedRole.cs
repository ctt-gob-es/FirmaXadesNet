#region

using System.Xml;

#endregion

namespace Microsoft.Xades
{
    /// <summary>
    ///     This class contains a roles claimed by the signer but not it is not a
    ///     certified role
    /// </summary>
    public class ClaimedRole
    {
        #region Constructors

        #endregion

        #region Private variables

        #endregion

        #region Public properties

        /// <summary>
        ///     The generic XML element that represents a claimed role
        /// </summary>
        public XmlElement AnyXmlElement { get; set; }

        /// <summary>
        /// </summary>
        public string InnerText { get; set; }

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

            if (!string.IsNullOrEmpty(InnerText)) retVal = true;

            return retVal;
        }

        /// <summary>
        ///     Load state from an XML element
        /// </summary>
        /// <param name="xmlElement">XML element containing new state</param>
        public void LoadXml(XmlElement xmlElement)
        {
            AnyXmlElement = xmlElement;
            InnerText = xmlElement.InnerText;
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
            retVal = creationXmlDocument.CreateElement(XadesSignedXml.XmlXadesPrefix, "ClaimedRole",
                XadesSignedXml.XadesNamespaceUri);

            if (!string.IsNullOrEmpty(InnerText)) retVal.InnerText = InnerText;

            if (AnyXmlElement != null) retVal.AppendChild(creationXmlDocument.ImportNode(AnyXmlElement, true));

            return retVal;
        }

        #endregion
    }
}