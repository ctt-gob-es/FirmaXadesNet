#region

using System;
using System.Collections;
using System.Xml;

#endregion

namespace Microsoft.Xades
{
    /// <summary>
    ///     The SignedDataObjectProperties element contains properties that qualify
    ///     some of the signed data objects
    /// </summary>
    public class SignedDataObjectProperties
    {
        #region Constructors

        /// <summary>
        ///     Default constructor
        /// </summary>
        public SignedDataObjectProperties()
        {
            DataObjectFormatCollection = new DataObjectFormatCollection();
            CommitmentTypeIndicationCollection = new CommitmentTypeIndicationCollection();
            AllDataObjectsTimeStampCollection = new AllDataObjectsTimeStampCollection();
            IndividualDataObjectsTimeStampCollection = new IndividualDataObjectsTimeStampCollection();
        }

        #endregion

        #region Private variables

        #endregion

        #region Public properties

        /// <summary>
        ///     Collection of signed data object formats
        /// </summary>
        public DataObjectFormatCollection DataObjectFormatCollection { get; set; }

        /// <summary>
        ///     Collection of commitment type indications
        /// </summary>
        public CommitmentTypeIndicationCollection CommitmentTypeIndicationCollection { get; set; }

        /// <summary>
        ///     Collection of all data object timestamps
        /// </summary>
        public AllDataObjectsTimeStampCollection AllDataObjectsTimeStampCollection { get; set; }

        /// <summary>
        ///     Collection of individual data object timestamps
        /// </summary>
        public IndividualDataObjectsTimeStampCollection IndividualDataObjectsTimeStampCollection { get; set; }

        #endregion

        #region Public methods

        /// <summary>
        ///     Check to see if something has changed in this instance and needs to be serialized
        /// </summary>
        /// <returns>Flag indicating if a member needs serialization</returns>
        public bool HasChanged()
        {
            var retVal = false;

            if (DataObjectFormatCollection.Count > 0) retVal = true;

            if (CommitmentTypeIndicationCollection.Count > 0) retVal = true;

            if (AllDataObjectsTimeStampCollection.Count > 0) retVal = true;

            if (IndividualDataObjectsTimeStampCollection.Count > 0) retVal = true;

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
            DataObjectFormat newDataObjectFormat;
            CommitmentTypeIndication newCommitmentTypeIndication;
            TimeStamp newTimeStamp;

            if (xmlElement == null) throw new ArgumentNullException("xmlElement");

            xmlNamespaceManager = new XmlNamespaceManager(xmlElement.OwnerDocument.NameTable);
            xmlNamespaceManager.AddNamespace("xsd", XadesSignedXml.XadesNamespaceUri);

            DataObjectFormatCollection.Clear();
            xmlNodeList = xmlElement.SelectNodes("xsd:DataObjectFormat", xmlNamespaceManager);
            enumerator = xmlNodeList.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    iterationXmlElement = enumerator.Current as XmlElement;
                    if (iterationXmlElement != null)
                    {
                        newDataObjectFormat = new DataObjectFormat();
                        newDataObjectFormat.LoadXml(iterationXmlElement);
                        DataObjectFormatCollection.Add(newDataObjectFormat);
                    }
                }
            }
            finally
            {
                var disposable = enumerator as IDisposable;
                if (disposable != null) disposable.Dispose();
            }

            //this.dataObjectFormatCollection.Clear();
            xmlNodeList = xmlElement.SelectNodes("xsd:CommitmentTypeIndication", xmlNamespaceManager);
            enumerator = xmlNodeList.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    iterationXmlElement = enumerator.Current as XmlElement;
                    if (iterationXmlElement != null)
                    {
                        newCommitmentTypeIndication = new CommitmentTypeIndication();
                        newCommitmentTypeIndication.LoadXml(iterationXmlElement);
                        CommitmentTypeIndicationCollection.Add(newCommitmentTypeIndication);
                    }
                }
            }
            finally
            {
                var disposable = enumerator as IDisposable;
                if (disposable != null) disposable.Dispose();
            }

            //this.dataObjectFormatCollection.Clear();
            xmlNodeList = xmlElement.SelectNodes("xsd:AllDataObjectsTimeStamp", xmlNamespaceManager);
            enumerator = xmlNodeList.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    iterationXmlElement = enumerator.Current as XmlElement;
                    if (iterationXmlElement != null)
                    {
                        newTimeStamp = new TimeStamp("AllDataObjectsTimeStamp");
                        newTimeStamp.LoadXml(iterationXmlElement);
                        AllDataObjectsTimeStampCollection.Add(newTimeStamp);
                    }
                }
            }
            finally
            {
                var disposable = enumerator as IDisposable;
                if (disposable != null) disposable.Dispose();
            }

            //this.dataObjectFormatCollection.Clear();
            xmlNodeList = xmlElement.SelectNodes("xsd:IndividualDataObjectsTimeStamp", xmlNamespaceManager);
            enumerator = xmlNodeList.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    iterationXmlElement = enumerator.Current as XmlElement;
                    if (iterationXmlElement != null)
                    {
                        newTimeStamp = new TimeStamp("IndividualDataObjectsTimeStamp");
                        newTimeStamp.LoadXml(iterationXmlElement);
                        IndividualDataObjectsTimeStampCollection.Add(newTimeStamp);
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
            retVal = creationXmlDocument.CreateElement(XadesSignedXml.XmlXadesPrefix, "SignedDataObjectProperties",
                XadesSignedXml.XadesNamespaceUri);

            if (DataObjectFormatCollection.Count > 0)
                foreach (DataObjectFormat dataObjectFormat in DataObjectFormatCollection)
                    if (dataObjectFormat.HasChanged())
                        retVal.AppendChild(creationXmlDocument.ImportNode(dataObjectFormat.GetXml(), true));

            if (CommitmentTypeIndicationCollection.Count > 0)
                foreach (CommitmentTypeIndication commitmentTypeIndication in CommitmentTypeIndicationCollection)
                    if (commitmentTypeIndication.HasChanged())
                        retVal.AppendChild(creationXmlDocument.ImportNode(commitmentTypeIndication.GetXml(), true));

            if (AllDataObjectsTimeStampCollection.Count > 0)
                foreach (TimeStamp timeStamp in AllDataObjectsTimeStampCollection)
                    if (timeStamp.HasChanged())
                        retVal.AppendChild(creationXmlDocument.ImportNode(timeStamp.GetXml(), true));

            if (IndividualDataObjectsTimeStampCollection.Count > 0)
                foreach (TimeStamp timeStamp in IndividualDataObjectsTimeStampCollection)
                    if (timeStamp.HasChanged())
                        retVal.AppendChild(creationXmlDocument.ImportNode(timeStamp.GetXml(), true));

            return retVal;
        }

        #endregion
    }
}