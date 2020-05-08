#region

using System;
using System.Security.Cryptography;
using System.Xml;

#endregion

namespace Microsoft.Xades
{
    /// <summary>
    ///     SPUserNotice element is intended for being displayed whenever the
    ///     signature is validated.  The class derives from SigPolicyQualifier.
    /// </summary>
    public class SPUserNotice : SigPolicyQualifier
    {
        #region Constructors

        /// <summary>
        ///     Default constructor
        /// </summary>
        public SPUserNotice()
        {
            NoticeRef = new NoticeRef();
        }

        #endregion

        #region Private variables

        #endregion

        #region Public properties

        /// <summary>
        ///     The NoticeRef element names an organization and identifies by
        ///     numbers a group of textual statements prepared by that organization,
        ///     so that the application could get the explicit notices from a notices file.
        /// </summary>
        public NoticeRef NoticeRef { get; set; }

        /// <summary>
        ///     The	ExplicitText element contains the text of the notice to be displayed
        /// </summary>
        public string ExplicitText { get; set; }

        /// <summary>
        ///     Inherited generic element, not used in the SPUserNotice class
        /// </summary>
        public override XmlElement AnyXmlElement
        {
            get => null; //This does not make sense for SPUserNotice
            set => throw new CryptographicException("Setting AnyXmlElement on a SPUserNotice is not supported");
        }

        #endregion

        #region Public methods

        /// <summary>
        ///     Check to see if something has changed in this instance and needs to be serialized
        /// </summary>
        /// <returns>Flag indicating if a member needs serialization</returns>
        public override bool HasChanged()
        {
            var retVal = false;

            if (!string.IsNullOrEmpty(ExplicitText)) retVal = true;

            if (NoticeRef != null && NoticeRef.HasChanged()) retVal = true;

            return retVal;
        }

        /// <summary>
        ///     Load state from an XML element
        /// </summary>
        /// <param name="xmlElement">XML element containing new state</param>
        public override void LoadXml(XmlElement xmlElement)
        {
            XmlNamespaceManager xmlNamespaceManager;
            XmlNodeList xmlNodeList;

            if (xmlElement == null) throw new ArgumentNullException("xmlElement");

            xmlNamespaceManager = new XmlNamespaceManager(xmlElement.OwnerDocument.NameTable);
            xmlNamespaceManager.AddNamespace("xsd", XadesSignedXml.XadesNamespaceUri);

            xmlNodeList = xmlElement.SelectNodes("xsd:SPUserNotice/xsd:NoticeRef", xmlNamespaceManager);
            if (xmlNodeList.Count != 0)
            {
                NoticeRef = new NoticeRef();
                NoticeRef.LoadXml((XmlElement) xmlNodeList.Item(0));
            }

            xmlNodeList = xmlElement.SelectNodes("xsd:SPUserNotice/xsd:ExplicitText", xmlNamespaceManager);
            if (xmlNodeList.Count != 0) ExplicitText = xmlNodeList.Item(0).InnerText;
        }

        /// <summary>
        ///     Returns the XML representation of the this object
        /// </summary>
        /// <returns>XML element containing the state of this object</returns>
        public override XmlElement GetXml()
        {
            XmlDocument creationXmlDocument;
            XmlElement bufferXmlElement;
            XmlElement bufferXmlElement2;
            XmlElement retVal;

            creationXmlDocument = new XmlDocument();
            retVal = creationXmlDocument.CreateElement("SigPolicyQualifier", XadesSignedXml.XadesNamespaceUri);

            bufferXmlElement = creationXmlDocument.CreateElement("SPUserNotice", XadesSignedXml.XadesNamespaceUri);
            if (NoticeRef != null && NoticeRef.HasChanged())
                bufferXmlElement.AppendChild(creationXmlDocument.ImportNode(NoticeRef.GetXml(), true));
            if (!string.IsNullOrEmpty(ExplicitText))
            {
                bufferXmlElement2 = creationXmlDocument.CreateElement("ExplicitText", XadesSignedXml.XadesNamespaceUri);
                bufferXmlElement2.InnerText = ExplicitText;
                bufferXmlElement.AppendChild(bufferXmlElement2);
            }

            retVal.AppendChild(creationXmlDocument.ImportNode(bufferXmlElement, true));

            return retVal;
        }

        #endregion
    }
}