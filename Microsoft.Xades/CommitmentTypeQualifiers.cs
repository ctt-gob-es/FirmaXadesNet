#region

using System;
using System.Collections;
using System.Xml;

#endregion

namespace Microsoft.Xades
{
    /// <summary>
    ///     The CommitmentTypeQualifier element provides means to include
    ///     additional qualifying information on the commitment made by the signer
    /// </summary>
    public class CommitmentTypeQualifiers
    {
        #region Constructors

        /// <summary>
        ///     Default constructor
        /// </summary>
        public CommitmentTypeQualifiers()
        {
            CommitmentTypeQualifierCollection = new CommitmentTypeQualifierCollection();
        }

        #endregion

        #region Public properties

        /// <summary>
        ///     Collection of commitment type qualifiers
        /// </summary>
        public CommitmentTypeQualifierCollection CommitmentTypeQualifierCollection { get; set; }

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

            if (CommitmentTypeQualifierCollection.Count > 0) retVal = true;

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
            IEnumerator enumerator;
            XmlElement iterationXmlElement;
            CommitmentTypeQualifier newCommitmentTypeQualifier;

            if (xmlElement == null) throw new ArgumentNullException("xmlElement");

            xmlNamespaceManager = new XmlNamespaceManager(xmlElement.OwnerDocument.NameTable);
            xmlNamespaceManager.AddNamespace("xsd", XadesSignedXml.XadesNamespaceUri);

            CommitmentTypeQualifierCollection.Clear();
            xmlNodeList = xmlElement.SelectNodes("xsd:CommitmentTypeQualifier", xmlNamespaceManager);
            enumerator = xmlNodeList.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    iterationXmlElement = enumerator.Current as XmlElement;
                    if (iterationXmlElement != null)
                    {
                        newCommitmentTypeQualifier = new CommitmentTypeQualifier();
                        newCommitmentTypeQualifier.LoadXml(iterationXmlElement);
                        CommitmentTypeQualifierCollection.Add(newCommitmentTypeQualifier);
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
            retVal = creationXmlDocument.CreateElement("CommitmentTypeQualifiers", XadesSignedXml.XadesNamespaceUri);

            if (CommitmentTypeQualifierCollection.Count > 0)
                foreach (CommitmentTypeQualifier commitmentTypeQualifier in CommitmentTypeQualifierCollection)
                    if (commitmentTypeQualifier.HasChanged())
                        retVal.AppendChild(creationXmlDocument.ImportNode(commitmentTypeQualifier.GetXml(), true));

            return retVal;
        }

        #endregion
    }
}