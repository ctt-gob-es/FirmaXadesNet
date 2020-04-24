#region

using System;
using System.Collections;
using System.Security.Cryptography.Xml;
using System.Xml;

#endregion

namespace Microsoft.Xades
{
    /// <summary>
    ///     The Transforms element contains a collection of transformations
    /// </summary>
    public class Transforms
    {
        #region Constructors

        /// <summary>
        ///     Default constructor
        /// </summary>
        public Transforms()
        {
            TransformCollection = new TransformCollection();
        }

        #endregion

        #region Public properties

        /// <summary>
        ///     A collection of transforms
        /// </summary>
        public TransformCollection TransformCollection { get; set; }

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

            if (TransformCollection.Count > 0) retVal = true;

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
            Transform newTransform;
            IEnumerator enumerator;
            XmlElement iterationXmlElement;

            if (xmlElement == null) throw new ArgumentNullException("xmlElement");

            xmlNamespaceManager = new XmlNamespaceManager(xmlElement.OwnerDocument.NameTable);
            xmlNamespaceManager.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);

            TransformCollection.Clear();
            xmlNodeList = xmlElement.SelectNodes("ds:Transform", xmlNamespaceManager);
            enumerator = xmlNodeList.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    iterationXmlElement = enumerator.Current as XmlElement;
                    if (iterationXmlElement != null)
                    {
                        newTransform = new Transform();
                        newTransform.LoadXml(iterationXmlElement);
                        TransformCollection.Add(newTransform);
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
            retVal = creationXmlDocument.CreateElement("Transforms", XadesSignedXml.XadesNamespaceUri);

            if (TransformCollection.Count > 0)
                foreach (Transform transform in TransformCollection)
                    if (transform.HasChanged())
                        retVal.AppendChild(creationXmlDocument.ImportNode(transform.GetXml(), true));

            return retVal;
        }

        #endregion
    }
}