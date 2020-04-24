#region

using System;
using System.Collections;
using System.Xml;

#endregion

namespace Microsoft.Xades
{
    /// <summary>
    ///     This class contains a collection of SigPolicyQualifiers
    /// </summary>
    public class SigPolicyQualifiers
    {
        #region Constructors

        /// <summary>
        ///     Default constructor
        /// </summary>
        public SigPolicyQualifiers()
        {
            SigPolicyQualifierCollection = new SigPolicyQualifierCollection();
        }

        #endregion

        #region Public properties

        /// <summary>
        ///     A collection of sig policy qualifiers
        /// </summary>
        public SigPolicyQualifierCollection SigPolicyQualifierCollection { get; set; }

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

            if (SigPolicyQualifierCollection.Count > 0) retVal = true;

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
            SPUri newSPUri;
            SPUserNotice newSPUserNotice;
            SigPolicyQualifier newSigPolicyQualifier;
            IEnumerator enumerator;
            XmlElement iterationXmlElement;
            XmlElement subElement;

            if (xmlElement == null) throw new ArgumentNullException("xmlElement");

            xmlNamespaceManager = new XmlNamespaceManager(xmlElement.OwnerDocument.NameTable);
            xmlNamespaceManager.AddNamespace("xsd", XadesSignedXml.XadesNamespaceUri);

            SigPolicyQualifierCollection.Clear();
            xmlNodeList = xmlElement.SelectNodes("xsd:SigPolicyQualifier", xmlNamespaceManager);
            enumerator = xmlNodeList.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    iterationXmlElement = enumerator.Current as XmlElement;
                    if (iterationXmlElement != null)
                    {
                        subElement =
                            (XmlElement) iterationXmlElement.SelectSingleNode("xsd:SPURI", xmlNamespaceManager);
                        if (subElement != null)
                        {
                            newSPUri = new SPUri();
                            newSPUri.LoadXml(iterationXmlElement);
                            SigPolicyQualifierCollection.Add(newSPUri);
                        }
                        else
                        {
                            subElement =
                                (XmlElement) iterationXmlElement.SelectSingleNode("xsd:SPUserNotice",
                                    xmlNamespaceManager);
                            if (subElement != null)
                            {
                                newSPUserNotice = new SPUserNotice();
                                newSPUserNotice.LoadXml(iterationXmlElement);
                                SigPolicyQualifierCollection.Add(newSPUserNotice);
                            }
                            else
                            {
                                newSigPolicyQualifier = new SigPolicyQualifier();
                                newSigPolicyQualifier.LoadXml(iterationXmlElement);
                                SigPolicyQualifierCollection.Add(newSigPolicyQualifier);
                            }
                        }
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
            retVal = creationXmlDocument.CreateElement(XadesSignedXml.XmlXadesPrefix, "SigPolicyQualifiers",
                XadesSignedXml.XadesNamespaceUri);

            if (SigPolicyQualifierCollection.Count > 0)
                foreach (SigPolicyQualifier sigPolicyQualifier in SigPolicyQualifierCollection)
                    if (sigPolicyQualifier.HasChanged())
                        retVal.AppendChild(creationXmlDocument.ImportNode(sigPolicyQualifier.GetXml(), true));

            return retVal;
        }

        #endregion
    }
}