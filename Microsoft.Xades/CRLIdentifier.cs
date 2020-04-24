#region

using System;
using System.Xml;

#endregion

namespace Microsoft.Xades
{
    /// <summary>
    ///     This class includes the issuer (Issuer element), the time when the CRL
    ///     was issued (IssueTime element) and optionally the number of the CRL
    ///     (Number element).
    ///     The Identifier element can be dropped if the CRL could be inferred from
    ///     other information. Its URI attribute could serve to	indicate where the
    ///     identified CRL is archived.
    /// </summary>
    public class CRLIdentifier
    {
        #region Private variables

        private DateTime issueTime;

        #endregion

        #region Constructors

        /// <summary>
        ///     Default constructor
        /// </summary>
        public CRLIdentifier()
        {
            issueTime = DateTime.MinValue;
            Number = long.MinValue; //Impossible value
        }

        #endregion

        #region Public properties

        /// <summary>
        ///     The optional URI attribute could serve to indicate where the OCSP
        ///     response identified is archived.
        /// </summary>
        public string UriAttribute { get; set; }

        /// <summary>
        ///     Issuer of the CRL
        /// </summary>
        public string Issuer { get; set; }

        /// <summary>
        ///     Date of issue of the CRL
        /// </summary>
        public DateTime IssueTime
        {
            get => issueTime;
            set => issueTime = value;
        }

        /// <summary>
        ///     Optional number of the CRL
        /// </summary>
        public long Number { get; set; }

        #endregion

        #region Public methods

        /// <summary>
        ///     Check to see if something has changed in this instance and needs to be serialized
        /// </summary>
        /// <returns>Flag indicating if a member needs serialization</returns>
        public bool HasChanged()
        {
            var retVal = false;

            if (!string.IsNullOrEmpty(UriAttribute)) retVal = true;

            if (!string.IsNullOrEmpty(Issuer)) retVal = true;

            if (issueTime != DateTime.MinValue) retVal = true;

            if (Number != long.MinValue) retVal = true;

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

            if (xmlElement == null) throw new ArgumentNullException("xmlElement");
            if (xmlElement.HasAttribute("URI")) UriAttribute = xmlElement.GetAttribute("URI");

            xmlNamespaceManager = new XmlNamespaceManager(xmlElement.OwnerDocument.NameTable);
            xmlNamespaceManager.AddNamespace("xsd", XadesSignedXml.XadesNamespaceUri);

            xmlNodeList = xmlElement.SelectNodes("xsd:Issuer", xmlNamespaceManager);
            if (xmlNodeList.Count != 0) Issuer = xmlNodeList.Item(0).InnerText;

            xmlNodeList = xmlElement.SelectNodes("xsd:IssueTime", xmlNamespaceManager);
            if (xmlNodeList.Count != 0)
                issueTime = XmlConvert.ToDateTime(xmlNodeList.Item(0).InnerText, XmlDateTimeSerializationMode.Local);

            xmlNodeList = xmlElement.SelectNodes("xsd:Number", xmlNamespaceManager);
            if (xmlNodeList.Count != 0) Number = long.Parse(xmlNodeList.Item(0).InnerText);
        }

        /// <summary>
        ///     Returns the XML representation of the this object
        /// </summary>
        /// <returns>XML element containing the state of this object</returns>
        public XmlElement GetXml()
        {
            XmlDocument creationXmlDocument;
            XmlElement retVal;
            XmlElement bufferXmlElement;

            creationXmlDocument = new XmlDocument();
            retVal = creationXmlDocument.CreateElement(XadesSignedXml.XmlXadesPrefix, "CRLIdentifier",
                XadesSignedXml.XadesNamespaceUri);

            retVal.SetAttribute("URI", UriAttribute);

            if (!string.IsNullOrEmpty(Issuer))
            {
                bufferXmlElement = creationXmlDocument.CreateElement(XadesSignedXml.XmlXadesPrefix, "Issuer",
                    XadesSignedXml.XadesNamespaceUri);
                bufferXmlElement.InnerText = Issuer;
                retVal.AppendChild(bufferXmlElement);
            }

            if (issueTime != DateTime.MinValue)
            {
                bufferXmlElement = creationXmlDocument.CreateElement(XadesSignedXml.XmlXadesPrefix, "IssueTime",
                    XadesSignedXml.XadesNamespaceUri);

                var truncatedDateTime = issueTime.AddTicks(-(issueTime.Ticks % TimeSpan.TicksPerSecond));

                bufferXmlElement.InnerText = XmlConvert.ToString(truncatedDateTime, XmlDateTimeSerializationMode.Local);

                retVal.AppendChild(bufferXmlElement);
            }

            if (Number != long.MinValue)
            {
                bufferXmlElement = creationXmlDocument.CreateElement(XadesSignedXml.XmlXadesPrefix, "Number",
                    XadesSignedXml.XadesNamespaceUri);
                bufferXmlElement.InnerText = Number.ToString();
                retVal.AppendChild(bufferXmlElement);
            }

            return retVal;
        }

        #endregion
    }
}