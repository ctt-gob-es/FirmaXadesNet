#region

using System;
using System.Collections;
using System.Security.Cryptography;
using System.Xml;

#endregion

namespace Microsoft.Xades
{
    /// <summary>
    ///     The commitment type can be indicated in the electronic signature
    ///     by either explicitly using a commitment type indication in the
    ///     electronic signature or implicitly or explicitly from the semantics
    ///     of the signed data object.
    ///     If the indicated commitment type is explicit by means of a commitment
    ///     type indication in the electronic signature, acceptance of a verified
    ///     signature implies acceptance of the semantics of that commitment type.
    ///     The semantics of explicit commitment types indications shall be
    ///     specified either as part of the signature policy or may be registered
    ///     for	generic use across multiple policies.
    /// </summary>
    public class CommitmentTypeIndication
    {
        #region Constructors

        /// <summary>
        ///     Default constructor
        /// </summary>
        public CommitmentTypeIndication()
        {
            CommitmentTypeId = new ObjectIdentifier("CommitmentTypeId");
            objectReferenceCollection = new ObjectReferenceCollection();
            allSignedDataObjects = true;
            CommitmentTypeQualifiers = new CommitmentTypeQualifiers();
        }

        #endregion

        #region Private variables

        private ObjectReferenceCollection objectReferenceCollection;
        private bool allSignedDataObjects;

        #endregion

        #region Public properties

        /// <summary>
        ///     The CommitmentTypeId element univocally identifies the type of commitment made by the signer.
        ///     A number of commitments have been already identified and assigned corresponding OIDs.
        /// </summary>
        public ObjectIdentifier CommitmentTypeId { get; set; }

        /// <summary>
        ///     Collection of object references
        /// </summary>
        public ObjectReferenceCollection ObjectReferenceCollection
        {
            get => objectReferenceCollection;
            set
            {
                objectReferenceCollection = value;
                if (objectReferenceCollection != null)
                    if (objectReferenceCollection.Count > 0)
                        allSignedDataObjects = false;
            }
        }

        /// <summary>
        ///     If all the signed data objects share the same commitment, the
        ///     AllSignedDataObjects empty element MUST be present.
        /// </summary>
        public bool AllSignedDataObjects
        {
            get => allSignedDataObjects;
            set
            {
                allSignedDataObjects = value;
                if (allSignedDataObjects) objectReferenceCollection.Clear();
            }
        }

        /// <summary>
        ///     The CommitmentTypeQualifiers element provides means to include additional
        ///     qualifying information on the commitment made by the signer.
        /// </summary>
        public CommitmentTypeQualifiers CommitmentTypeQualifiers { get; set; }

        #endregion

        #region Public methods

        /// <summary>
        ///     Check to see if something has changed in this instance and needs to be serialized
        /// </summary>
        /// <returns>Flag indicating if a member needs serialization</returns>
        public bool HasChanged()
        {
            var retVal = false;

            if (CommitmentTypeId != null && CommitmentTypeId.HasChanged()) retVal = true;

            if (objectReferenceCollection.Count > 0) retVal = true;

            if (CommitmentTypeQualifiers != null && CommitmentTypeQualifiers.HasChanged()) retVal = true;

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
            ObjectReference newObjectReference;

            if (xmlElement == null) throw new ArgumentNullException("xmlElement");

            xmlNamespaceManager = new XmlNamespaceManager(xmlElement.OwnerDocument.NameTable);
            xmlNamespaceManager.AddNamespace("xsd", XadesSignedXml.XadesNamespaceUri);

            xmlNodeList = xmlElement.SelectNodes("xsd:CommitmentTypeId", xmlNamespaceManager);
            if (xmlNodeList.Count == 0)
            {
                CommitmentTypeId = null;
                throw new CryptographicException("CommitmentTypeId missing");
            }

            CommitmentTypeId = new ObjectIdentifier("CommitmentTypeId");
            CommitmentTypeId.LoadXml((XmlElement) xmlNodeList.Item(0));

            xmlNodeList = xmlElement.SelectNodes("xsd:ObjectReference", xmlNamespaceManager);
            if (xmlNodeList.Count != 0)
            {
                objectReferenceCollection.Clear();
                allSignedDataObjects = false;
                enumerator = xmlNodeList.GetEnumerator();
                try
                {
                    while (enumerator.MoveNext())
                    {
                        iterationXmlElement = enumerator.Current as XmlElement;
                        if (iterationXmlElement != null)
                        {
                            newObjectReference = new ObjectReference();
                            newObjectReference.LoadXml(iterationXmlElement);
                            objectReferenceCollection.Add(newObjectReference);
                        }
                    }
                }
                finally
                {
                    var disposable = enumerator as IDisposable;
                    if (disposable != null) disposable.Dispose();
                }
            }
            else
            {
                objectReferenceCollection.Clear();
                allSignedDataObjects = true;
            }

            xmlNodeList = xmlElement.SelectNodes("xsd:CommitmentTypeQualifiers", xmlNamespaceManager);
            if (xmlNodeList.Count != 0)
            {
                CommitmentTypeQualifiers = new CommitmentTypeQualifiers();
                CommitmentTypeQualifiers.LoadXml((XmlElement) xmlNodeList.Item(0));
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
            XmlElement bufferXmlElement;

            creationXmlDocument = new XmlDocument();
            retVal = creationXmlDocument.CreateElement("CommitmentTypeIndication", XadesSignedXml.XadesNamespaceUri);

            if (CommitmentTypeId != null && CommitmentTypeId.HasChanged())
                retVal.AppendChild(creationXmlDocument.ImportNode(CommitmentTypeId.GetXml(), true));
            else
                throw new CryptographicException("CommitmentTypeId element missing");

            if (allSignedDataObjects)
            {
                //Add emty element as required
                bufferXmlElement =
                    creationXmlDocument.CreateElement("AllSignedDataObjects", XadesSignedXml.XadesNamespaceUri);
                retVal.AppendChild(bufferXmlElement);
            }
            else
            {
                if (objectReferenceCollection.Count > 0)
                    foreach (ObjectReference objectReference in objectReferenceCollection)
                        if (objectReference.HasChanged())
                            retVal.AppendChild(creationXmlDocument.ImportNode(objectReference.GetXml(), true));
            }

            if (CommitmentTypeQualifiers != null && CommitmentTypeQualifiers.HasChanged())
                retVal.AppendChild(creationXmlDocument.ImportNode(CommitmentTypeQualifiers.GetXml(), true));

            return retVal;
        }

        #endregion
    }
}