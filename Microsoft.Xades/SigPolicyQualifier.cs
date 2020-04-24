#region

using System.Xml;

#endregion

namespace Microsoft.Xades
{
    /// <summary>
    ///     This class can contain additional information qualifying the signature
    ///     policy identifier
    /// </summary>
    public class SigPolicyQualifier
    {
        #region Private variables

        private XmlElement anyXmlElement;

        #endregion

        #region Public properties

        /// <summary>
        ///     The generic XML element that represents a sig policy qualifier
        /// </summary>
        public virtual XmlElement AnyXmlElement
        {
            get => anyXmlElement;
            set => anyXmlElement = value;
        }

        #endregion

        #region Constructors

        #endregion

        #region Public methods

        /// <summary>
        ///     Check to see if something has changed in this instance and needs to be serialized
        /// </summary>
        /// <returns>Flag indicating if a member needs serialization</returns>
        public virtual bool HasChanged()
        {
            var retVal = false;

            if (anyXmlElement != null) retVal = true;

            return retVal;
        }

        /// <summary>
        ///     Load state from an XML element
        /// </summary>
        /// <param name="xmlElement">XML element containing new state</param>
        public virtual void LoadXml(XmlElement xmlElement)
        {
            anyXmlElement = xmlElement;
        }

        /// <summary>
        ///     Returns the XML representation of the this object
        /// </summary>
        /// <returns>XML element containing the state of this object</returns>
        public virtual XmlElement GetXml()
        {
            XmlDocument creationXmlDocument;
            XmlElement retVal;

            creationXmlDocument = new XmlDocument();
            retVal = creationXmlDocument.CreateElement(XadesSignedXml.XmlXadesPrefix, "SigPolicyQualifier",
                XadesSignedXml.XadesNamespaceUri);

            if (anyXmlElement != null) retVal.AppendChild(creationXmlDocument.ImportNode(anyXmlElement, true));

            return retVal;
        }

        #endregion
    }
}