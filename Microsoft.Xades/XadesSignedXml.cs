#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.Xml.Schema;
using Security.Cryptography;

#endregion

namespace Microsoft.Xades
{
    /// <summary>
    ///     Facade class for the XAdES signature library.  The class inherits from
    ///     the System.Security.Cryptography.Xml.SignedXml class and is backwards
    ///     compatible with it, so this class can host xmldsig signatures and XAdES
    ///     signatures.  The property SignatureStandard will indicate the type of the
    ///     signature: XMLDSIG or XAdES.
    /// </summary>
    public class XadesSignedXml : SignedXml
    {
        #region Constants

        /// <summary>
        ///     The XAdES XML namespace URI
        /// </summary>
        public const string XadesNamespaceUri = "http://uri.etsi.org/01903/v1.3.2#";

        /// <summary>
        ///     The XAdES v1.4.1 XML namespace URI
        /// </summary>
        public const string XadesNamespace141Uri = "http://uri.etsi.org/01903/v1.4.1#";

        /// <summary>
        ///     Mandated type name for the Uri reference to the SignedProperties element
        /// </summary>
        public const string SignedPropertiesType = "http://uri.etsi.org/01903#SignedProperties";


        /// <summary>
        /// </summary>
        public const string XmlDsigObjectType = "http://www.w3.org/2000/09/xmldsig#Object";

        #endregion

        #region Private variables

        private static readonly string[] idAttrs =
        {
            "_id",
            "_Id",
            "_ID"
        };

        private XmlDocument cachedXadesObjectDocument;
        private string signedPropertiesIdBuffer;
        private bool validationErrorOccurred;
        private string validationErrorDescription;
        private string signedInfoIdBuffer;
        private readonly XmlDocument signatureDocument;

        #endregion

        #region Public properties

        /// <summary>
        /// </summary>
        public static string XmlDSigPrefix { get; set; }

        /// <summary>
        /// </summary>
        public static string XmlXadesPrefix { get; set; }


        /// <summary>
        ///     Property indicating the type of signature (XmlDsig or XAdES)
        /// </summary>
        public KnownSignatureStandard SignatureStandard { get; private set; }

        /// <summary>
        ///     Read-only property containing XAdES information
        /// </summary>
        public XadesObject XadesObject
        {
            get
            {
                var retVal = new XadesObject();

                retVal.LoadXml(GetXadesObjectElement(GetXml()), GetXml());

                return retVal;
            }
        }

        /// <summary>
        ///     Setting this property will add an ID attribute to the SignatureValue element.
        ///     This is required when constructing a XAdES-T signature.
        /// </summary>
        public string SignatureValueId { get; set; }

        /// <summary>
        ///     This property allows to access and modify the unsigned properties
        ///     after the XAdES object has been added to the signature.
        ///     Because the unsigned properties are part of a location in the
        ///     signature that is not used when computing the signature, it is save
        ///     to modify them even after the XMLDSIG signature has been computed.
        ///     This is needed when XAdES objects that depend on the XMLDSIG
        ///     signature value need to be added to the signature. The
        ///     SignatureTimeStamp element is such a property, it can only be
        ///     created when the XMLDSIG signature has been computed.
        /// </summary>
        public UnsignedProperties UnsignedProperties
        {
            get
            {
                XmlElement dataObjectXmlElement;
                DataObject xadesDataObject;
                XmlNamespaceManager xmlNamespaceManager;
                XmlNodeList xmlNodeList;
                UnsignedProperties retVal;

                retVal = new UnsignedProperties();
                xadesDataObject = GetXadesDataObject();
                if (xadesDataObject != null)
                {
                    dataObjectXmlElement = xadesDataObject.GetXml();
                    xmlNamespaceManager = new XmlNamespaceManager(dataObjectXmlElement.OwnerDocument.NameTable);
                    xmlNamespaceManager.AddNamespace("xades", XadesNamespaceUri);
                    xmlNodeList =
                        dataObjectXmlElement.SelectNodes("xades:QualifyingProperties/xades:UnsignedProperties",
                            xmlNamespaceManager);
                    if (xmlNodeList.Count != 0)
                    {
                        retVal = new UnsignedProperties();
                        retVal.LoadXml((XmlElement) xmlNodeList[0], (XmlElement) xmlNodeList[0]);
                    }
                }
                else
                {
                    throw new CryptographicException(
                        "XAdES object not found. Use AddXadesObject() before accessing UnsignedProperties.");
                }

                return retVal;
            }

            set
            {
                var xadesDataObject = GetXadesDataObject();
                if (xadesDataObject != null)
                {
                    var dataObjectXmlElement = xadesDataObject.GetXml();
                    if (dataObjectXmlElement.OwnerDocument != null)
                    {
                        var xmlNamespaceManager = new XmlNamespaceManager(dataObjectXmlElement.OwnerDocument.NameTable);
                        xmlNamespaceManager.AddNamespace("xades", XadesNamespaceUri);
                        var qualifyingPropertiesXmlNodeList =
                            dataObjectXmlElement.SelectNodes("xades:QualifyingProperties", xmlNamespaceManager);
                        var unsignedPropertiesXmlNodeList = dataObjectXmlElement.SelectNodes(
                            "xades:QualifyingProperties/xades:UnsignedProperties",
                            xmlNamespaceManager);
                        if (unsignedPropertiesXmlNodeList != null && unsignedPropertiesXmlNodeList.Count != 0)
                            qualifyingPropertiesXmlNodeList?[0].RemoveChild(unsignedPropertiesXmlNodeList[0]);

                        var valueXml = value.GetXml();

                        if (dataObjectXmlElement.OwnerDocument != null)
                            qualifyingPropertiesXmlNodeList?[0]
                                .AppendChild(dataObjectXmlElement.OwnerDocument.ImportNode(valueXml, true));
                    }

                    var newXadesDataObject = new DataObject();
                    newXadesDataObject.LoadXml(dataObjectXmlElement);
                    xadesDataObject.Data = newXadesDataObject.Data;
                }
                else
                {
                    throw new CryptographicException(
                        "XAdES object not found. Use AddXadesObject() before accessing UnsignedProperties.");
                }
            }
        }

        /// <summary>
        /// </summary>
        public XmlElement ContentElement { get; set; }

        /// <summary>
        /// </summary>
        public XmlElement SignatureNodeDestination { get; set; }

        /// <summary>
        /// </summary>
        public bool AddXadesNamespace { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Default constructor for the XadesSignedXml class
        /// </summary>
        public XadesSignedXml()
        {
            XmlDSigPrefix = "ds";
            XmlXadesPrefix = "xades";

            cachedXadesObjectDocument = null;
            SignatureStandard = KnownSignatureStandard.XmlDsig;
        }

        /// <summary>
        ///     Constructor for the XadesSignedXml class
        /// </summary>
        /// <param name="signatureElement">XmlElement used to create the instance</param>
        public XadesSignedXml(XmlElement signatureElement)
            : base(signatureElement)
        {
            XmlDSigPrefix = "ds";
            XmlXadesPrefix = "xades";

            cachedXadesObjectDocument = null;
        }

        /// <summary>
        ///     Constructor for the XadesSignedXml class
        /// </summary>
        /// <param name="signatureDocument">XmlDocument used to create the instance</param>
        public XadesSignedXml(XmlDocument signatureDocument)
            : base(signatureDocument)
        {
            XmlDSigPrefix = "ds";
            XmlXadesPrefix = "xades";
            this.signatureDocument = signatureDocument;

            cachedXadesObjectDocument = null;
        }

        #endregion

        #region Public methods

        /// <summary>
        ///     Load state from an XML element
        /// </summary>
        /// <param name="xmlElement">The XML element from which to load the XadesSignedXml state</param>
        public new void LoadXml(XmlElement xmlElement)
        {
            cachedXadesObjectDocument = null;
            SignatureValueId = null;
            base.LoadXml(xmlElement);

            // Get original prefix for namespaces
            foreach (XmlAttribute attr in xmlElement.Attributes)
                if (attr.Name.StartsWith("xmlns"))
                {
                    if (attr.Value.ToUpper() == XadesNamespaceUri.ToUpper())
                        XmlXadesPrefix = attr.Name.Split(':')[1];
                    else if (attr.Value.ToUpper() == XmlDsigNamespaceUrl.ToUpper())
                        XmlDSigPrefix = attr.Name.Split(':')[1];
                }


            var idAttribute = xmlElement.Attributes.GetNamedItem("Id");
            if (idAttribute != null) Signature.Id = idAttribute.Value;
            SetSignatureStandard(xmlElement);

            var xmlNamespaceManager = new XmlNamespaceManager(xmlElement.OwnerDocument.NameTable);

            xmlNamespaceManager.AddNamespace("ds", XmlDsigNamespaceUrl);
            xmlNamespaceManager.AddNamespace("xades", XadesNamespaceUri);

            var xmlNodeList = xmlElement.SelectNodes("ds:SignatureValue", xmlNamespaceManager);
            if (xmlNodeList.Count > 0)
                if (((XmlElement) xmlNodeList[0]).HasAttribute("Id"))
                    SignatureValueId = ((XmlElement) xmlNodeList[0]).Attributes["Id"].Value;

            xmlNodeList = xmlElement.SelectNodes("ds:SignedInfo", xmlNamespaceManager);
            if (xmlNodeList.Count > 0)
            {
                if (((XmlElement) xmlNodeList[0]).HasAttribute("Id"))
                    signedInfoIdBuffer = ((XmlElement) xmlNodeList[0]).Attributes["Id"].Value;
                else
                    signedInfoIdBuffer = null;
            }
        }

        /// <summary>
        ///     Returns the XML representation of the this object
        /// </summary>
        /// <returns>XML element containing the state of this object</returns>
        public new XmlElement GetXml()
        {
            XmlElement retVal;
            XmlNodeList xmlNodeList;
            XmlNamespaceManager xmlNamespaceManager;

            retVal = base.GetXml();

            // Add "ds" namespace prefix to all XmlDsig nodes in the signature
            SetPrefix(XmlDSigPrefix, retVal);

            xmlNamespaceManager = new XmlNamespaceManager(retVal.OwnerDocument.NameTable);
            xmlNamespaceManager.AddNamespace("ds", XmlDsigNamespaceUrl);


            /*if (this.signatureDocument != null)
            {
                                
                XmlNode nodeKeyInfoRetVal = retVal.SelectSingleNode("ds:KeyInfo", xmlNamespaceManager);
                XmlNodeList nodeKeyInfoOrig = this.signatureDocument.DocumentElement.GetElementsByTagName("KeyInfo", SignedXml.XmlDsigNamespaceUrl);

                if (nodeKeyInfoOrig.Count > 0)
                {
                    nodeKeyInfoRetVal.InnerXml = nodeKeyInfoOrig[0].InnerXml;
                }

                XmlNode nodeSignatureValue = retVal.SelectSingleNode("ds:SignatureValue", xmlNamespaceManager);
                XmlNodeList nodeSignatureValueOrign = this.signatureDocument.DocumentElement.GetElementsByTagName("SignatureValue", SignedXml.XmlDsigNamespaceUrl);

                if (nodeSignatureValueOrign.Count > 0)
                {
                    nodeSignatureValue.InnerXml = nodeSignatureValueOrign[0].InnerXml;
                }

                XmlNode nodeSignedInfo = retVal.SelectSingleNode("ds:SignedInfo", xmlNamespaceManager);
                XmlNodeList nodeSignedInfoOrig = this.signatureDocument.DocumentElement.GetElementsByTagName("SignedInfo", SignedXml.XmlDsigNamespaceUrl);

                if (nodeSignedInfoOrig.Count > 0)
                {
                    nodeSignedInfo.InnerXml = nodeSignedInfoOrig[0].InnerXml;
                }
            }*/

            if (SignatureValueId != null && SignatureValueId != "")
            {
                //Id on Signature value is needed for XAdES-T. We inject it here.
                xmlNamespaceManager = new XmlNamespaceManager(retVal.OwnerDocument.NameTable);
                xmlNamespaceManager.AddNamespace("ds", XmlDsigNamespaceUrl);
                xmlNodeList = retVal.SelectNodes("ds:SignatureValue", xmlNamespaceManager);
                if (xmlNodeList.Count > 0) ((XmlElement) xmlNodeList[0]).SetAttribute("Id", SignatureValueId);
            }


            return retVal;
        }

        /// <summary>
        ///     Overridden virtual method to be able to find the nested SignedProperties
        ///     element inside of the XAdES object
        /// </summary>
        /// <param name="xmlDocument">Document in which to find the Id</param>
        /// <param name="idValue">Value of the Id to look for</param>
        /// <returns>XmlElement with requested Id</returns>
        public override XmlElement GetIdElement(XmlDocument xmlDocument, string idValue)
        {
            // check to see if it's a standard ID reference
            XmlElement retVal = null;

            if (xmlDocument != null)
            {
                retVal = base.GetIdElement(xmlDocument, idValue);

                if (retVal != null) return retVal;

                // if not, search for custom ids
                foreach (var idAttr in idAttrs)
                {
                    retVal = xmlDocument.SelectSingleNode("//*[@" + idAttr + "=\"" + idValue + "\"]") as XmlElement;
                    if (retVal != null) break;
                }
            }

            return retVal;
        }

        /// <summary>
        ///     Add a XAdES object to the signature
        /// </summary>
        /// <param name="xadesObject">XAdES object to add to signature</param>
        public void AddXadesObject(XadesObject xadesObject)
        {
            if (SignatureStandard != KnownSignatureStandard.Xades)
            {
                var dataObject = new DataObject {Id = xadesObject.Id, Data = xadesObject.GetXml().ChildNodes};
                AddObject(dataObject); //Add the XAdES object                            

                var reference = new Reference();
                signedPropertiesIdBuffer = xadesObject.QualifyingProperties.SignedProperties.Id;
                reference.Uri = "#" + signedPropertiesIdBuffer;
                reference.Type = SignedPropertiesType;
                AddReference(reference); //Add the XAdES object reference

                cachedXadesObjectDocument = new XmlDocument();
                var bufferXmlElement = xadesObject.GetXml();

                // Add "ds" namespace prefix to all XmlDsig nodes in the XAdES object
                SetPrefix("ds", bufferXmlElement);

                cachedXadesObjectDocument.PreserveWhitespace = true;
                cachedXadesObjectDocument.LoadXml(bufferXmlElement.OuterXml); //Cache to XAdES object for later use

                SignatureStandard = KnownSignatureStandard.Xades;
            }
            else
            {
                throw new CryptographicException(
                    "Can't add XAdES object, the signature already contains a XAdES object");
            }
        }

        /// <summary>
        ///     Additional tests for XAdES signatures.  These tests focus on
        ///     XMLDSIG verification and correct form of the XAdES XML structure
        ///     (schema validation and completeness as defined by the XAdES standard).
        /// </summary>
        /// <remarks>
        ///     Because of the fact that the XAdES library is intentionally
        ///     independent of standards like TSP (RFC3161) or OCSP (RFC2560),
        ///     these tests do NOT include any verification of timestamps nor OCSP
        ///     responses.
        ///     These checks are important and have to be done in the application
        ///     built on top of the XAdES library.
        /// </remarks>
        /// <exception cref="System.Exception">
        ///     Thrown when the signature is not
        ///     a XAdES signature.  SignatureStandard should be equal to
        ///     <see cref="KnownSignatureStandard.Xades">KnownSignatureStandard.Xades</see>.
        ///     Use the CheckSignature method for non-XAdES signatures.
        /// </exception>
        /// <param name="xadesCheckSignatureMasks">
        ///     Bitmask to indicate which
        ///     tests need to be done.  This function will call a public virtual
        ///     methods for each bit that has been set in this mask.
        ///     See the <see cref="XadesCheckSignatureMasks">XadesCheckSignatureMasks</see>
        ///     enum for the bitmask definitions.  The virtual test method associated
        ///     with a bit in the mask has the same name as enum value name.
        /// </param>
        /// <returns>
        ///     If the function returns true the check was OK.  If the
        ///     check fails an exception with a explanatory message is thrown.
        /// </returns>
        public bool XadesCheckSignature(XadesCheckSignatureMasks xadesCheckSignatureMasks)
        {
            bool retVal;

            retVal = true;
            if (SignatureStandard != KnownSignatureStandard.Xades)
                throw new Exception("SignatureStandard is not XAdES.  CheckSignature returned: " + CheckSignature());

            if ((xadesCheckSignatureMasks & XadesCheckSignatureMasks.CheckXmldsigSignature) != 0)
                retVal &= CheckXmldsigSignature();
            if ((xadesCheckSignatureMasks & XadesCheckSignatureMasks.ValidateAgainstSchema) != 0)
                retVal &= ValidateAgainstSchema();
            if ((xadesCheckSignatureMasks & XadesCheckSignatureMasks.CheckSameCertificate) != 0)
                retVal &= CheckSameCertificate();
            if ((xadesCheckSignatureMasks &
                 XadesCheckSignatureMasks.CheckAllReferencesExistInAllDataObjectsTimeStamp) !=
                0) retVal &= CheckAllReferencesExistInAllDataObjectsTimeStamp();
            if ((xadesCheckSignatureMasks &
                 XadesCheckSignatureMasks.CheckAllHashDataInfosInIndividualDataObjectsTimeStamp) !=
                0) retVal &= CheckAllHashDataInfosInIndividualDataObjectsTimeStamp();
            if ((xadesCheckSignatureMasks & XadesCheckSignatureMasks.CheckCounterSignatures) != 0)
                retVal &= CheckCounterSignatures(xadesCheckSignatureMasks);
            if ((xadesCheckSignatureMasks & XadesCheckSignatureMasks.CheckCounterSignaturesReference) != 0)
                retVal &= CheckCounterSignaturesReference();
            if ((xadesCheckSignatureMasks & XadesCheckSignatureMasks.CheckObjectReferencesInCommitmentTypeIndication) !=
                0) retVal &= CheckObjectReferencesInCommitmentTypeIndication();
            if ((xadesCheckSignatureMasks &
                 XadesCheckSignatureMasks.CheckIfClaimedRolesOrCertifiedRolesPresentInSignerRole) !=
                0) retVal &= CheckIfClaimedRolesOrCertifiedRolesPresentInSignerRole();
            if ((xadesCheckSignatureMasks &
                 XadesCheckSignatureMasks.CheckHashDataInfoOfSignatureTimeStampPointsToSignatureValue) !=
                0) retVal &= CheckHashDataInfoOfSignatureTimeStampPointsToSignatureValue();
            if ((xadesCheckSignatureMasks & XadesCheckSignatureMasks.CheckQualifyingPropertiesTarget) != 0)
                retVal &= CheckQualifyingPropertiesTarget();
            if ((xadesCheckSignatureMasks & XadesCheckSignatureMasks.CheckQualifyingProperties) != 0)
                retVal &= CheckQualifyingProperties();
            if ((xadesCheckSignatureMasks & XadesCheckSignatureMasks.CheckSigAndRefsTimeStampHashDataInfos) != 0)
                retVal &= CheckSigAndRefsTimeStampHashDataInfos();
            if ((xadesCheckSignatureMasks & XadesCheckSignatureMasks.CheckRefsOnlyTimeStampHashDataInfos) != 0)
                retVal &= CheckRefsOnlyTimeStampHashDataInfos();
            if ((xadesCheckSignatureMasks & XadesCheckSignatureMasks.CheckArchiveTimeStampHashDataInfos) != 0)
                retVal &= CheckArchiveTimeStampHashDataInfos();
            if ((xadesCheckSignatureMasks & XadesCheckSignatureMasks.CheckXadesCIsXadesT) != 0)
                retVal &= CheckXadesCIsXadesT();
            if ((xadesCheckSignatureMasks & XadesCheckSignatureMasks.CheckXadesXLIsXadesX) != 0)
                retVal &= CheckXadesXLIsXadesX();
            if ((xadesCheckSignatureMasks & XadesCheckSignatureMasks.CheckCertificateValuesMatchCertificateRefs) != 0)
                retVal &= CheckCertificateValuesMatchCertificateRefs();
            if ((xadesCheckSignatureMasks & XadesCheckSignatureMasks.CheckRevocationValuesMatchRevocationRefs) != 0)
                retVal &= CheckRevocationValuesMatchRevocationRefs();

            return retVal;
        }


        /// <summary>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public X509Certificate2 GetSigningCertificate()
        {
            var keyXml = KeyInfo.GetXml().GetElementsByTagName("X509Certificate", XmlDsigNamespaceUrl)[0];

            if (keyXml == null) throw new Exception("Unable to obtain the signing certificate");

            return new X509Certificate2(Convert.FromBase64String(keyXml.InnerText));
        }

        #region XadesCheckSignature routines

        /// <summary>
        ///     Check the signature of the underlying XMLDSIG signature
        /// </summary>
        /// <returns>If the function returns true the check was OK</returns>
        public virtual bool CheckXmldsigSignature()
        {
            var retVal = false;

            if (KeyInfo == null)
            {
                var keyInfo = new KeyInfo();
                X509Certificate xmldsigCert = GetSigningCertificate();
                keyInfo.AddClause(new KeyInfoX509Data(xmldsigCert));
                KeyInfo = keyInfo;
            }

            if (!(CryptoConfig2.CreateFromName(SignedInfo.SignatureMethod) is SignatureDescription description))
                switch (SignedInfo.SignatureMethod)
                {
                    case "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256":
                        CryptoConfig2.AddAlgorithm(typeof(RSAPKCS1SHA256SignatureDescription),
                            "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256");
                        break;
                    case "http://www.w3.org/2001/04/xmldsig-more#rsa-sha512":
                        CryptoConfig2.AddAlgorithm(typeof(RSAPKCS1SHA512SignatureDescription),
                            "http://www.w3.org/2001/04/xmldsig-more#rsa-sha512");
                        break;
                }

            retVal = CheckDigestedReferences();

            if (retVal == false) throw new CryptographicException("CheckXmldsigSignature() failed");

            var key = GetPublicKey();
            retVal = CheckSignedInfo(key);

            if (retVal == false) throw new CryptographicException("CheckXmldsigSignature() failed");

            return retVal;
        }

        /// <summary>
        ///     Validate the XML representation of the signature against the XAdES and XMLDSIG schemas
        /// </summary>
        /// <returns>If the function returns true the check was OK</returns>
        public virtual bool ValidateAgainstSchema()
        {
            var retValue = false;

            var assembly = Assembly.GetExecutingAssembly();
            var schemaSet = new XmlSchemaSet();
            XmlSchema xmlSchema;
            Stream schemaStream;

            NameTable xadesNameTable;
            XmlNamespaceManager xmlNamespaceManager;
            XmlParserContext xmlParserContext;

            validationErrorOccurred = false;
            validationErrorDescription = "";

            try
            {
                schemaStream = assembly.GetManifestResourceStream("Microsoft.Xades.xmldsig-core-schema.xsd");
                xmlSchema = XmlSchema.Read(schemaStream, SchemaValidationHandler);
                schemaSet.Add(xmlSchema);
                schemaStream.Close();


                schemaStream = assembly.GetManifestResourceStream("Microsoft.Xades.XAdES.xsd");
                xmlSchema = XmlSchema.Read(schemaStream, SchemaValidationHandler);
                schemaSet.Add(xmlSchema);
                schemaStream.Close();

                if (validationErrorOccurred)
                    throw new CryptographicException("Schema read validation error: " + validationErrorDescription);
            }
            catch (Exception exception)
            {
                throw new CryptographicException("Problem during access of validation schemas", exception);
            }

            var xmlReaderSettings = new XmlReaderSettings();
            xmlReaderSettings.ValidationEventHandler += XmlValidationHandler;
            xmlReaderSettings.ValidationType = ValidationType.Schema;
            xmlReaderSettings.Schemas = schemaSet;
            xmlReaderSettings.ConformanceLevel = ConformanceLevel.Auto;

            xadesNameTable = new NameTable();
            xmlNamespaceManager = new XmlNamespaceManager(xadesNameTable);
            xmlNamespaceManager.AddNamespace("xsd", XadesNamespaceUri);

            xmlParserContext = new XmlParserContext(null, xmlNamespaceManager, null, XmlSpace.None);

            var txtReader = new XmlTextReader(GetXml().OuterXml, XmlNodeType.Element, xmlParserContext);
            var reader = XmlReader.Create(txtReader, xmlReaderSettings);
            try
            {
                while (reader.Read()) ;
                if (validationErrorOccurred)
                    throw new CryptographicException("Schema validation error: " + validationErrorDescription);
            }
            catch (Exception exception)
            {
                throw new CryptographicException("Schema validation error", exception);
            }
            finally
            {
                reader.Close();
            }

            retValue = true;

            return retValue;
        }

        /// <summary>
        ///     Check to see if first XMLDSIG certificate has same hashvalue as first XAdES SignatureCertificate
        /// </summary>
        /// <returns>If the function returns true the check was OK</returns>
        public virtual bool CheckSameCertificate()
        {
            var retVal = false;

            //KeyInfoX509Data keyInfoX509Data = new KeyInfoX509Data();
            //keyInfoX509Data.LoadXml(this.KeyInfo.GetXml());
            //if (keyInfoX509Data.Certificates.Count <= 0)
            //{
            //    throw new CryptographicException("Certificate not found in XMLDSIG signature while doing CheckSameCertificate()");
            //}
            //string xmldsigCertHash = Convert.ToBase64String(((X509Certificate)keyInfoX509Data.Certificates[0]).GetCertHash());

            X509Certificate xmldsigCert = GetSigningCertificate();
            var xmldsigCertHash = Convert.ToBase64String(xmldsigCert.GetCertHash());

            var xadesSigningCertificateCollection = XadesObject.QualifyingProperties.SignedProperties
                .SignedSignatureProperties.SigningCertificate.CertCollection;
            if (xadesSigningCertificateCollection.Count <= 0)
                throw new CryptographicException(
                    "Certificate not found in SigningCertificate element while doing CheckSameCertificate()");
            var xadesCertHash = Convert.ToBase64String(xadesSigningCertificateCollection[0].CertDigest.DigestValue);


            if (string.Compare(xmldsigCertHash, xadesCertHash, true, CultureInfo.InvariantCulture) != 0)
                throw new CryptographicException(
                    "Certificate in XMLDSIG signature doesn't match certificate in SigningCertificate element");
            retVal = true;

            return retVal;
        }

        /// <summary>
        ///     Check if there is a HashDataInfo for each reference if there is a AllDataObjectsTimeStamp
        /// </summary>
        /// <returns>If the function returns true the check was OK</returns>
        public virtual bool CheckAllReferencesExistInAllDataObjectsTimeStamp()
        {
            var allHashDataInfosExist = true;
            var retVal = false;
            var allDataObjectsTimeStampCollection = XadesObject.QualifyingProperties.SignedProperties
                .SignedDataObjectProperties.AllDataObjectsTimeStampCollection;
            if (allDataObjectsTimeStampCollection.Count > 0)
            {
                int timeStampCounter;
                for (timeStampCounter = 0;
                    allHashDataInfosExist && timeStampCounter < allDataObjectsTimeStampCollection.Count;
                    timeStampCounter++)
                {
                    var timeStamp = allDataObjectsTimeStampCollection[timeStampCounter];
                    allHashDataInfosExist &= CheckHashDataInfosForTimeStamp(timeStamp);
                }

                if (!allHashDataInfosExist)
                    throw new CryptographicException(
                        "At least one HashDataInfo is missing in AllDataObjectsTimeStamp element");
            }

            retVal = true;

            return retVal;
        }

        /// <summary>
        ///     Check if the HashDataInfo of each IndividualDataObjectsTimeStamp points to existing Reference
        /// </summary>
        /// <returns>If the function returns true the check was OK</returns>
        public virtual bool CheckAllHashDataInfosInIndividualDataObjectsTimeStamp()
        {
            var hashDataInfoExists = true;
            var retVal = false;
            var individualDataObjectsTimeStampCollection = XadesObject.QualifyingProperties.SignedProperties
                .SignedDataObjectProperties.IndividualDataObjectsTimeStampCollection;
            if (individualDataObjectsTimeStampCollection.Count > 0)
            {
                int timeStampCounter;
                for (timeStampCounter = 0;
                    hashDataInfoExists && timeStampCounter < individualDataObjectsTimeStampCollection.Count;
                    timeStampCounter++)
                {
                    var timeStamp = individualDataObjectsTimeStampCollection[timeStampCounter];
                    hashDataInfoExists &= CheckHashDataInfosExist(timeStamp);
                }

                if (hashDataInfoExists == false)
                    throw new CryptographicException(
                        "At least one HashDataInfo is pointing to non-existing reference in IndividualDataObjectsTimeStamp element");
            }

            retVal = true;

            return retVal;
        }

        /// <summary>
        ///     Perform XAdES checks on contained counter signatures.  If couter signature is XMLDSIG, only XMLDSIG check
        ///     (CheckSignature()) is done.
        /// </summary>
        /// <param name="counterSignatureMask">Check mask applied to counter signatures</param>
        /// <returns>If the function returns true the check was OK</returns>
        public virtual bool CheckCounterSignatures(XadesCheckSignatureMasks counterSignatureMask)
        {
            CounterSignatureCollection counterSignatureCollection;
            XadesSignedXml counterSignature;
            bool retVal;

            retVal = true;
            counterSignatureCollection = XadesObject.QualifyingProperties.UnsignedProperties.UnsignedSignatureProperties
                .CounterSignatureCollection;
            for (var counterSignatureCounter = 0;
                retVal && counterSignatureCounter < counterSignatureCollection.Count;
                counterSignatureCounter++)
            {
                counterSignature = counterSignatureCollection[counterSignatureCounter];
                //TODO: check if parent signature document is present in counterSignature (maybe a deep copy is required)
                if (counterSignature.SignatureStandard == KnownSignatureStandard.Xades)
                    retVal &= counterSignature.XadesCheckSignature(counterSignatureMask);
                else
                    retVal &= counterSignature.CheckSignature();
            }

            if (retVal == false)
                throw new CryptographicException("XadesCheckSignature() failed on at least one counter signature");
            retVal = true;

            return retVal;
        }

        /// <summary>
        ///     Counter signatures should all contain a reference to the parent signature SignatureValue element
        /// </summary>
        /// <returns>If the function returns true the check was OK</returns>
        public virtual bool CheckCounterSignaturesReference()
        {
            CounterSignatureCollection counterSignatureCollection;
            XadesSignedXml counterSignature;
            string referenceUri;
            ArrayList parentSignatureValueChain;
            bool referenceToParentSignatureFound;
            bool retVal;

            retVal = true;
            parentSignatureValueChain = new ArrayList {"#" + SignatureValueId};
            counterSignatureCollection = XadesObject.QualifyingProperties.UnsignedProperties.UnsignedSignatureProperties
                .CounterSignatureCollection;
            for (var counterSignatureCounter = 0;
                retVal && counterSignatureCounter < counterSignatureCollection.Count;
                counterSignatureCounter++)
            {
                counterSignature = counterSignatureCollection[counterSignatureCounter];
                referenceToParentSignatureFound = false;
                for (var referenceCounter = 0;
                    referenceToParentSignatureFound == false &&
                    referenceCounter < counterSignature.SignedInfo.References.Count;
                    referenceCounter++)
                {
                    referenceUri = ((Reference) counterSignature.SignedInfo.References[referenceCounter]).Uri;
                    if (parentSignatureValueChain.BinarySearch(referenceUri) >= 0)
                        referenceToParentSignatureFound = true;
                    parentSignatureValueChain.Add("#" + counterSignature.SignatureValueId);
                    parentSignatureValueChain.Sort();
                }

                retVal = referenceToParentSignatureFound;
            }

            if (retVal == false)
                throw new CryptographicException(
                    "CheckCounterSignaturesReference() failed on at least one counter signature");
            retVal = true;

            return retVal;
        }

        /// <summary>
        ///     Check if each ObjectReference in CommitmentTypeIndication points to Reference element
        /// </summary>
        /// <returns>If the function returns true the check was OK</returns>
        public virtual bool CheckObjectReferencesInCommitmentTypeIndication()
        {
            CommitmentTypeIndicationCollection commitmentTypeIndicationCollection;
            CommitmentTypeIndication commitmentTypeIndication;
            bool objectReferenceOK;
            bool retVal;

            retVal = true;
            commitmentTypeIndicationCollection = XadesObject.QualifyingProperties.SignedProperties
                .SignedDataObjectProperties.CommitmentTypeIndicationCollection;
            if (commitmentTypeIndicationCollection.Count > 0)
            {
                for (var commitmentTypeIndicationCounter = 0;
                    retVal && commitmentTypeIndicationCounter < commitmentTypeIndicationCollection.Count;
                    commitmentTypeIndicationCounter++)
                {
                    commitmentTypeIndication = commitmentTypeIndicationCollection[commitmentTypeIndicationCounter];
                    objectReferenceOK = true;
                    foreach (ObjectReference objectReference in commitmentTypeIndication.ObjectReferenceCollection)
                        objectReferenceOK &= CheckObjectReference(objectReference);
                    retVal = objectReferenceOK;
                }

                if (retVal == false)
                    throw new CryptographicException(
                        "At least one ObjectReference in CommitmentTypeIndication did not point to a Reference");
            }

            return retVal;
        }

        /// <summary>
        ///     Check if at least ClaimedRoles or CertifiedRoles present in SignerRole
        /// </summary>
        /// <returns>If the function returns true the check was OK</returns>
        public virtual bool CheckIfClaimedRolesOrCertifiedRolesPresentInSignerRole()
        {
            SignerRole signerRole;
            bool retVal;

            retVal = false;
            signerRole = XadesObject.QualifyingProperties.SignedProperties.SignedSignatureProperties.SignerRole;
            if (signerRole != null)
            {
                if (signerRole.CertifiedRoles != null)
                    retVal = signerRole.CertifiedRoles.CertifiedRoleCollection.Count > 0;
                if (retVal == false)
                    if (signerRole.ClaimedRoles != null)
                        retVal = signerRole.ClaimedRoles.ClaimedRoleCollection.Count > 0;
                if (retVal == false)
                    throw new CryptographicException(
                        "SignerRole element must contain at least one CertifiedRole or ClaimedRole element");
            }
            else
            {
                retVal = true;
            }

            return retVal;
        }

        /// <summary>
        ///     Check if HashDataInfo of SignatureTimeStamp points to SignatureValue
        /// </summary>
        /// <returns>If the function returns true the check was OK</returns>
        public virtual bool CheckHashDataInfoOfSignatureTimeStampPointsToSignatureValue()
        {
            SignatureTimeStampCollection signatureTimeStampCollection;
            bool hashDataInfoPointsToSignatureValue;
            TimeStamp timeStamp;
            int timeStampCounter;
            bool retVal;

            hashDataInfoPointsToSignatureValue = true;
            retVal = false;
            signatureTimeStampCollection = XadesObject.QualifyingProperties.UnsignedProperties
                .UnsignedSignatureProperties.SignatureTimeStampCollection;
            if (signatureTimeStampCollection.Count > 0)
            {
                for (timeStampCounter = 0;
                    hashDataInfoPointsToSignatureValue && timeStampCounter < signatureTimeStampCollection.Count;
                    timeStampCounter++)
                {
                    timeStamp = signatureTimeStampCollection[timeStampCounter];
                    hashDataInfoPointsToSignatureValue &= CheckHashDataInfoPointsToSignatureValue(timeStamp);
                }

                if (hashDataInfoPointsToSignatureValue == false)
                    throw new CryptographicException(
                        "HashDataInfo of SignatureTimeStamp doesn't point to signature value element");
            }

            retVal = true;

            return retVal;
        }

        /// <summary>
        ///     Check if the QualifyingProperties Target attribute points to the signature element
        /// </summary>
        /// <returns>If the function returns true the check was OK</returns>
        public virtual bool CheckQualifyingPropertiesTarget()
        {
            string qualifyingPropertiesTarget;
            bool retVal;

            retVal = true;
            qualifyingPropertiesTarget = XadesObject.QualifyingProperties.Target;
            if (Signature.Id == null)
            {
                retVal = false;
            }
            else
            {
                if (qualifyingPropertiesTarget != "#" + Signature.Id) retVal = false;
            }

            if (retVal == false)
                throw new CryptographicException(
                    "Qualifying properties target doesn't point to signature element or signature element doesn't have an Id");

            return retVal;
        }

        /// <summary>
        ///     Check that QualifyingProperties occur in one Object, check that there is only one QualifyingProperties and that
        ///     signed properties occur in one QualifyingProperties element
        /// </summary>
        /// <returns>If the function returns true the check was OK</returns>
        public virtual bool CheckQualifyingProperties()
        {
            XmlElement signatureElement;
            XmlNamespaceManager xmlNamespaceManager;
            XmlNodeList xmlNodeList;

            signatureElement = GetXml();
            xmlNamespaceManager = new XmlNamespaceManager(signatureElement.OwnerDocument.NameTable);
            xmlNamespaceManager.AddNamespace("ds", XmlDsigNamespaceUrl);
            xmlNamespaceManager.AddNamespace("xsd", XadesNamespaceUri);
            xmlNodeList = signatureElement.SelectNodes("ds:Object/xsd:QualifyingProperties", xmlNamespaceManager);
            if (xmlNodeList.Count > 1)
                throw new CryptographicException("More than one Object contains a QualifyingProperties element");

            return true;
        }

        /// <summary>
        ///     Check if all required HashDataInfos are present on SigAndRefsTimeStamp
        /// </summary>
        /// <returns>If the function returns true the check was OK</returns>
        public virtual bool CheckSigAndRefsTimeStampHashDataInfos()
        {
            SignatureTimeStampCollection signatureTimeStampCollection;
            TimeStamp timeStamp;
            bool allRequiredhashDataInfosFound;
            bool retVal;

            retVal = true;
            signatureTimeStampCollection = XadesObject.QualifyingProperties.UnsignedProperties
                .UnsignedSignatureProperties.SigAndRefsTimeStampCollection;
            if (signatureTimeStampCollection.Count > 0)
            {
                allRequiredhashDataInfosFound = true;
                for (var timeStampCounter = 0;
                    allRequiredhashDataInfosFound && timeStampCounter < signatureTimeStampCollection.Count;
                    timeStampCounter++)
                {
                    timeStamp = signatureTimeStampCollection[timeStampCounter];
                    allRequiredhashDataInfosFound &= CheckHashDataInfosOfSigAndRefsTimeStamp(timeStamp);
                }

                if (allRequiredhashDataInfosFound == false)
                    throw new CryptographicException(
                        "At least one required HashDataInfo is missing in a SigAndRefsTimeStamp element");
            }

            return retVal;
        }

        /// <summary>
        ///     Check if all required HashDataInfos are present on RefsOnlyTimeStamp
        /// </summary>
        /// <returns>If the function returns true the check was OK</returns>
        public virtual bool CheckRefsOnlyTimeStampHashDataInfos()
        {
            SignatureTimeStampCollection signatureTimeStampCollection;
            TimeStamp timeStamp;
            bool allRequiredhashDataInfosFound;
            bool retVal;

            retVal = true;
            signatureTimeStampCollection = XadesObject.QualifyingProperties.UnsignedProperties
                .UnsignedSignatureProperties.RefsOnlyTimeStampCollection;
            if (signatureTimeStampCollection.Count > 0)
            {
                allRequiredhashDataInfosFound = true;
                for (var timeStampCounter = 0;
                    allRequiredhashDataInfosFound && timeStampCounter < signatureTimeStampCollection.Count;
                    timeStampCounter++)
                {
                    timeStamp = signatureTimeStampCollection[timeStampCounter];
                    allRequiredhashDataInfosFound &= CheckHashDataInfosOfRefsOnlyTimeStamp(timeStamp);
                }

                if (allRequiredhashDataInfosFound == false)
                    throw new CryptographicException(
                        "At least one required HashDataInfo is missing in a RefsOnlyTimeStamp element");
            }

            return retVal;
        }

        /// <summary>
        ///     Check if all required HashDataInfos are present on ArchiveTimeStamp
        /// </summary>
        /// <returns>If the function returns true the check was OK</returns>
        public virtual bool CheckArchiveTimeStampHashDataInfos()
        {
            SignatureTimeStampCollection signatureTimeStampCollection;
            TimeStamp timeStamp;
            bool allRequiredhashDataInfosFound;
            bool retVal;

            retVal = true;
            signatureTimeStampCollection = XadesObject.QualifyingProperties.UnsignedProperties
                .UnsignedSignatureProperties.ArchiveTimeStampCollection;
            if (signatureTimeStampCollection.Count > 0)
            {
                allRequiredhashDataInfosFound = true;
                for (var timeStampCounter = 0;
                    allRequiredhashDataInfosFound && timeStampCounter < signatureTimeStampCollection.Count;
                    timeStampCounter++)
                {
                    timeStamp = signatureTimeStampCollection[timeStampCounter];
                    allRequiredhashDataInfosFound &= CheckHashDataInfosOfArchiveTimeStamp(timeStamp);
                }

                if (allRequiredhashDataInfosFound == false)
                    throw new CryptographicException(
                        "At least one required HashDataInfo is missing in a ArchiveTimeStamp element");
            }

            return retVal;
        }

        /// <summary>
        ///     Check if a XAdES-C signature is also a XAdES-T signature
        /// </summary>
        /// <returns>If the function returns true the check was OK</returns>
        public virtual bool CheckXadesCIsXadesT()
        {
            UnsignedSignatureProperties unsignedSignatureProperties;
            bool retVal;

            retVal = true;
            unsignedSignatureProperties =
                XadesObject.QualifyingProperties.UnsignedProperties.UnsignedSignatureProperties;
            if (unsignedSignatureProperties.CompleteCertificateRefs != null &&
                unsignedSignatureProperties.CompleteCertificateRefs.HasChanged()
                || unsignedSignatureProperties.CompleteCertificateRefs != null &&
                unsignedSignatureProperties.CompleteCertificateRefs.HasChanged())
                if (unsignedSignatureProperties.SignatureTimeStampCollection.Count == 0)
                    throw new CryptographicException(
                        "XAdES-C signature should also contain a SignatureTimeStamp element");

            return retVal;
        }

        /// <summary>
        ///     Check if a XAdES-XL signature is also a XAdES-X signature
        /// </summary>
        /// <returns>If the function returns true the check was OK</returns>
        public virtual bool CheckXadesXLIsXadesX()
        {
            UnsignedSignatureProperties unsignedSignatureProperties;
            bool retVal;

            retVal = true;
            unsignedSignatureProperties =
                XadesObject.QualifyingProperties.UnsignedProperties.UnsignedSignatureProperties;
            if (unsignedSignatureProperties.CertificateValues != null &&
                unsignedSignatureProperties.CertificateValues.HasChanged()
                || unsignedSignatureProperties.RevocationValues != null &&
                unsignedSignatureProperties.RevocationValues.HasChanged())
                if (unsignedSignatureProperties.SigAndRefsTimeStampCollection.Count == 0 &&
                    unsignedSignatureProperties.RefsOnlyTimeStampCollection.Count == 0)
                    throw new CryptographicException("XAdES-XL signature should also contain a XAdES-X element");

            return retVal;
        }

        /// <summary>
        ///     Check if CertificateValues match CertificateRefs
        /// </summary>
        /// <returns>If the function returns true the check was OK</returns>
        public virtual bool CheckCertificateValuesMatchCertificateRefs()
        {
            SHA1Managed sha1Managed;
            UnsignedSignatureProperties unsignedSignatureProperties;
            ArrayList certDigests;
            byte[] certDigest;
            int index;
            bool retVal;

            //TODO: Similar test should be done for XML based (Other) certificates, but as the check needed is not known, there is no implementation
            retVal = true;
            unsignedSignatureProperties =
                XadesObject.QualifyingProperties.UnsignedProperties.UnsignedSignatureProperties;
            if (unsignedSignatureProperties.CompleteCertificateRefs != null &&
                unsignedSignatureProperties.CompleteCertificateRefs.CertRefs != null &&
                unsignedSignatureProperties.CertificateValues != null)
            {
                certDigests = new ArrayList();
                foreach (Cert cert in unsignedSignatureProperties.CompleteCertificateRefs.CertRefs.CertCollection)
                    certDigests.Add(Convert.ToBase64String(cert.CertDigest.DigestValue));
                certDigests.Sort();
                foreach (EncapsulatedX509Certificate encapsulatedX509Certificate in unsignedSignatureProperties
                    .CertificateValues.EncapsulatedX509CertificateCollection)
                {
                    sha1Managed = new SHA1Managed();
                    certDigest = sha1Managed.ComputeHash(encapsulatedX509Certificate.PkiData);
                    index = certDigests.BinarySearch(Convert.ToBase64String(certDigest));
                    if (index >= 0) certDigests.RemoveAt(index);
                }

                if (certDigests.Count != 0)
                    throw new CryptographicException("Not all CertificateRefs correspond to CertificateValues");
            }


            return retVal;
        }

        /// <summary>
        ///     Check if RevocationValues match RevocationRefs
        /// </summary>
        /// <returns>If the function returns true the check was OK</returns>
        public virtual bool CheckRevocationValuesMatchRevocationRefs()
        {
            SHA1Managed sha1Managed;
            UnsignedSignatureProperties unsignedSignatureProperties;
            ArrayList crlDigests;
            byte[] crlDigest;
            int index;
            bool retVal;

            //TODO: Similar test should be done for XML based (Other) revocation information and OCSP responses, but to keep the library independent of these technologies, this test is left to appliactions using the library
            retVal = true;
            unsignedSignatureProperties =
                XadesObject.QualifyingProperties.UnsignedProperties.UnsignedSignatureProperties;
            if (unsignedSignatureProperties.CompleteRevocationRefs != null &&
                unsignedSignatureProperties.CompleteRevocationRefs.CRLRefs != null &&
                unsignedSignatureProperties.RevocationValues != null)
            {
                crlDigests = new ArrayList();
                foreach (CRLRef crlRef in unsignedSignatureProperties.CompleteRevocationRefs.CRLRefs.CRLRefCollection)
                    crlDigests.Add(Convert.ToBase64String(crlRef.CertDigest.DigestValue));
                crlDigests.Sort();
                foreach (CRLValue crlValue in unsignedSignatureProperties.RevocationValues.CRLValues.CRLValueCollection)
                {
                    sha1Managed = new SHA1Managed();
                    crlDigest = sha1Managed.ComputeHash(crlValue.PkiData);
                    index = crlDigests.BinarySearch(Convert.ToBase64String(crlDigest));
                    if (index >= 0) crlDigests.RemoveAt(index);
                }

                if (crlDigests.Count != 0)
                    throw new CryptographicException("Not all RevocationRefs correspond to RevocationValues");
            }

            return retVal;
        }

        #endregion

        #endregion

        #region Fix to add a namespace prefix for all XmlDsig nodes

        private void SetPrefix(string prefix, XmlNode node)
        {
            if (node.NamespaceURI == XmlDsigNamespaceUrl) node.Prefix = prefix;

            foreach (XmlNode child in node.ChildNodes) SetPrefix(prefix, child);
        }


        private SignatureDescription GetSignatureDescription()
        {
            if (CryptoConfig2.CreateFromName(SignedInfo.SignatureMethod) is SignatureDescription description)
                return description;
            switch (SignedInfo.SignatureMethod)
            {
                case "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256":
                    CryptoConfig2.AddAlgorithm(typeof(RSAPKCS1SHA256SignatureDescription),
                        "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256");
                    break;
                case "http://www.w3.org/2001/04/xmldsig-more#rsa-sha512":
                    CryptoConfig2.AddAlgorithm(typeof(RSAPKCS1SHA512SignatureDescription),
                        "http://www.w3.org/2001/04/xmldsig-more#rsa-sha512");
                    break;
            }

            description = CryptoConfig2.CreateFromName(SignedInfo.SignatureMethod) as SignatureDescription;

            return description;
        }

        /// <summary>
        /// </summary>
        /// <exception cref="CryptographicException"></exception>
        public new void ComputeSignature()
        {
            BuildDigestedReferences();

            var signingKey = SigningKey;
            if (signingKey == null) throw new CryptographicException("Cryptography_Xml_LoadKeyFailed");
            if (SignedInfo.SignatureMethod == null)
            {
                if (!(signingKey is DSA))
                {
                    if (!(signingKey is RSA)) throw new CryptographicException("Cryptography_Xml_CreatedKeyFailed");
                    if (SignedInfo.SignatureMethod == null)
                        SignedInfo.SignatureMethod = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";
                }
                else
                {
                    SignedInfo.SignatureMethod = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";
                }
            }

            var description = GetSignatureDescription();
            if (description == null)
                throw new CryptographicException("Cryptography_Xml_SignatureDescriptionNotCreated");

            var hash = description.CreateDigest();
            if (hash == null) throw new CryptographicException("Cryptography_Xml_CreateHashAlgorithmFailed");
            //this.GetC14NDigest(hash);
            var hashValue = GetC14NDigest(hash, "ds");

            m_signature.SignatureValue = description.CreateFormatter(signingKey).CreateSignature(hash);
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public Reference GetContentReference()
        {
            XadesObject xadesObject;

            if (cachedXadesObjectDocument != null)
            {
                xadesObject = new XadesObject();
                xadesObject.LoadXml(cachedXadesObjectDocument.DocumentElement, null);
            }
            else
            {
                xadesObject = XadesObject;
            }

            if (xadesObject.QualifyingProperties.SignedProperties.SignedDataObjectProperties.DataObjectFormatCollection
                .Count > 0)
            {
                var referenceId = xadesObject.QualifyingProperties.SignedProperties.SignedDataObjectProperties
                    .DataObjectFormatCollection[0].ObjectReferenceAttribute.Substring(1);

                foreach (var reference in SignedInfo.References)
                    if (((Reference) reference).Id == referenceId)
                        return (Reference) reference;
            }

            return (Reference) SignedInfo.References[0];
        }

        /// <summary>
        /// </summary>
        public void FindContentElement()
        {
            var contentRef = GetContentReference();

            if (!string.IsNullOrEmpty(contentRef.Uri) &&
                contentRef.Uri.StartsWith("#"))
                ContentElement = GetIdElement(signatureDocument, contentRef.Uri.Substring(1));
            else
                ContentElement = signatureDocument.DocumentElement;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public XmlElement GetSignatureElement()
        {
            var signatureElement = GetIdElement(signatureDocument, Signature.Id);

            if (signatureElement != null) return signatureElement;

            if (SignatureNodeDestination != null) return SignatureNodeDestination;

            if (ContentElement == null) return null;

            if (ContentElement.ParentNode.NodeType != XmlNodeType.Document)
                return (XmlElement) ContentElement.ParentNode;
            return ContentElement;
        }


        /// <summary>
        /// </summary>
        /// <param name="fromElement"></param>
        /// <returns></returns>
        public List<XmlAttribute> GetAllNamespaces(XmlElement fromElement)
        {
            var namespaces = new List<XmlAttribute>();

            if (fromElement != null &&
                fromElement.ParentNode.NodeType == XmlNodeType.Document)
            {
                foreach (XmlAttribute attr in fromElement.Attributes)
                    if (attr.Name.StartsWith("xmlns") && !namespaces.Exists(f => f.Name == attr.Name))
                        namespaces.Add(attr);

                return namespaces;
            }

            XmlNode currentNode = fromElement;

            while (currentNode != null && currentNode.NodeType != XmlNodeType.Document)
            {
                foreach (XmlAttribute attr in currentNode.Attributes)
                    if (attr.Name.StartsWith("xmlns") && !namespaces.Exists(f => f.Name == attr.Name))
                        namespaces.Add(attr);

                currentNode = currentNode.ParentNode;
            }

            return namespaces;
        }

        /// <summary>
        ///     Copy of System.Security.Cryptography.Xml.SignedXml.BuildDigestedReferences() which will add a "ds"
        ///     namespace prefix to all XmlDsig nodes
        /// </summary>
        private void BuildDigestedReferences()
        {
            var references = SignedInfo.References;

            //this.m_refProcessed = new bool[references.Count];
            var SignedXml_Type = typeof(SignedXml);
            var SignedXml_m_refProcessed =
                SignedXml_Type.GetField("m_refProcessed", BindingFlags.NonPublic | BindingFlags.Instance);
            SignedXml_m_refProcessed.SetValue(this, new bool[references.Count]);
            //

            //this.m_refLevelCache = new int[references.Count];
            var SignedXml_m_refLevelCache =
                SignedXml_Type.GetField("m_refLevelCache", BindingFlags.NonPublic | BindingFlags.Instance);
            SignedXml_m_refLevelCache.SetValue(this, new int[references.Count]);
            //

            //ReferenceLevelSortOrder comparer = new ReferenceLevelSortOrder();
            var System_Security_Assembly =
                Assembly.Load("System.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            var ReferenceLevelSortOrder_Type =
                System_Security_Assembly.GetType("System.Security.Cryptography.Xml.SignedXml+ReferenceLevelSortOrder");
            var ReferenceLevelSortOrder_Constructor = ReferenceLevelSortOrder_Type.GetConstructor(new Type[] { });
            var comparer = ReferenceLevelSortOrder_Constructor.Invoke(null);
            //

            //comparer.References = references;
            var ReferenceLevelSortOrder_References =
                ReferenceLevelSortOrder_Type.GetProperty("References", BindingFlags.Public | BindingFlags.Instance);
            ReferenceLevelSortOrder_References.SetValue(comparer, references, null);
            //

            var list2 = new ArrayList();
            foreach (Reference reference in references) list2.Add(reference);

            list2.Sort((IComparer) comparer);

            var CanonicalXmlNodeList_Type =
                System_Security_Assembly.GetType("System.Security.Cryptography.Xml.CanonicalXmlNodeList");
            var CanonicalXmlNodeList_Constructor =
                CanonicalXmlNodeList_Type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
                    new Type[] { }, null);

            // refList is a list of elements that might be targets of references
            var refList = CanonicalXmlNodeList_Constructor.Invoke(null);

            var CanonicalXmlNodeList_Add =
                CanonicalXmlNodeList_Type.GetMethod("Add", BindingFlags.Public | BindingFlags.Instance);

            //
            var SignedXml_m_containingDocument = SignedXml_Type.GetField("m_containingDocument",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var Reference_Type = typeof(Reference);
            var Reference_UpdateHashValue =
                Reference_Type.GetMethod("UpdateHashValue", BindingFlags.NonPublic | BindingFlags.Instance);
            //

            var m_containingDocument = SignedXml_m_containingDocument.GetValue(this);

            if (ContentElement == null) FindContentElement();

            var signatureParentNodeNameSpaces = GetAllNamespaces(GetSignatureElement());

            if (AddXadesNamespace)
            {
                var attr = signatureDocument.CreateAttribute("xmlns:xades");
                attr.Value = XadesNamespaceUri;

                signatureParentNodeNameSpaces.Add(attr);
            }

            foreach (Reference reference2 in list2)
            {
                XmlDocument xmlDoc = null;
                var addSignatureNamespaces = false;

                if (reference2.Uri.StartsWith("#KeyInfoId-"))
                {
                    var keyInfoXml = KeyInfo.GetXml();
                    SetPrefix(XmlDSigPrefix, keyInfoXml);

                    xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(keyInfoXml.OuterXml);

                    addSignatureNamespaces = true;
                }
                else if (reference2.Type == SignedPropertiesType)
                {
                    xmlDoc = (XmlDocument) cachedXadesObjectDocument.Clone();

                    addSignatureNamespaces = true;
                }
                else if (reference2.Type == XmlDsigObjectType)
                {
                    var dataObjectId = reference2.Uri.Substring(1);
                    XmlElement dataObjectXml = null;

                    foreach (DataObject dataObject in m_signature.ObjectList)
                        if (dataObjectId == dataObject.Id)
                        {
                            dataObjectXml = dataObject.GetXml();

                            SetPrefix(XmlDSigPrefix, dataObjectXml);

                            addSignatureNamespaces = true;

                            xmlDoc = new XmlDocument();
                            xmlDoc.LoadXml(dataObjectXml.OuterXml);

                            break;
                        }

                    // If no DataObject found, search on document
                    if (dataObjectXml == null)
                    {
                        dataObjectXml = GetIdElement(signatureDocument, dataObjectId);

                        if (dataObjectXml != null)
                        {
                            xmlDoc = new XmlDocument {PreserveWhitespace = true};
                            xmlDoc.LoadXml(dataObjectXml.OuterXml);
                        }
                        else
                        {
                            throw new Exception("No reference target found");
                        }
                    }
                }
                else
                {
                    xmlDoc = (XmlDocument) m_containingDocument;
                }


                if (addSignatureNamespaces)
                    foreach (var attr in signatureParentNodeNameSpaces)
                    {
                        var newAttr = xmlDoc.CreateAttribute(attr.Name);
                        newAttr.Value = attr.Value;

                        xmlDoc.DocumentElement.Attributes.Append(newAttr);
                    }

                if (xmlDoc != null) CanonicalXmlNodeList_Add.Invoke(refList, new object[] {xmlDoc.DocumentElement});

                Reference_UpdateHashValue.Invoke(reference2, new[] {xmlDoc, refList});

                if (reference2.Id != null)
                {
                    var xml = reference2.GetXml();

                    SetPrefix(XmlDSigPrefix, xml);
                }
            }
        }


        /// <summary>
        /// </summary>
        /// <returns></returns>
        protected override AsymmetricAlgorithm GetPublicKey()
        {
            var SignedXml_Type = typeof(SignedXml);

            var SignedXml_Type_GetPublicKey =
                SignedXml_Type.GetMethod("GetPublicKey", BindingFlags.NonPublic | BindingFlags.Instance);

            return SignedXml_Type_GetPublicKey.Invoke(this, null) as AsymmetricAlgorithm;
        }


        private bool CheckDigestedReferences()
        {
            var SignedXml_Type = typeof(SignedXml);

            var SignedXml_Type_CheckDigestedReferences = SignedXml_Type.GetMethod("CheckDigestedReferences",
                BindingFlags.NonPublic | BindingFlags.Instance);

            return Convert.ToBoolean(SignedXml_Type_CheckDigestedReferences.Invoke(this, null));
        }


        private bool CheckSignedInfo(AsymmetricAlgorithm key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            if (!(CryptoConfig2.CreateFromName(SignatureMethod) is SignatureDescription signatureDescription))
                throw new CryptographicException("signature description can't be created");

            // Let's see if the key corresponds with the SignatureMethod
            var ta = Type.GetType(signatureDescription.KeyAlgorithm);
            var tb = key.GetType();
            if (ta != tb && !ta.IsSubclassOf(tb) && !tb.IsSubclassOf(ta))
                // Signature method key mismatch
                return false;

            var hashAlgorithm = signatureDescription.CreateDigest();
            if (hashAlgorithm == null)
                throw new CryptographicException("signature description can't be created");

            // NECESSARY FOR THE CORRECT CALCULATION
            var hashval = GetC14NDigest(hashAlgorithm, "ds");

            var asymmetricSignatureDeformatter = signatureDescription.CreateDeformatter(key);

            return asymmetricSignatureDeformatter.VerifySignature(hashval, m_signature.SignatureValue);
        }


        /// <summary>
        ///     We won't call System.Security.Cryptography.Xml.SignedXml.GetC14NDigest(), as we want to use our own.
        /// </summary>
        private byte[] GetC14NDigest(HashAlgorithm hash)
        {
            return null;
        }

        /// <summary>
        ///     Copy of System.Security.Cryptography.Xml.SignedXml.GetC14NDigest() which will add a
        ///     namespace prefix to all XmlDsig nodes
        /// </summary>
        private byte[] GetC14NDigest(HashAlgorithm hash, string prefix)
        {
            //if (!this.bCacheValid || !this.SignedInfo.CacheValid)
            //{
            var SignedXml_Type = typeof(SignedXml);
            var SignedXml_bCacheValid =
                SignedXml_Type.GetField("bCacheValid", BindingFlags.NonPublic | BindingFlags.Instance);
            var bCacheValid = (bool) SignedXml_bCacheValid.GetValue(this);
            var SignedInfo_Type = typeof(SignedInfo);
            var SignedInfo_CacheValid =
                SignedInfo_Type.GetProperty("CacheValid", BindingFlags.NonPublic | BindingFlags.Instance);
            var CacheValid = (bool) SignedInfo_CacheValid.GetValue(SignedInfo, null);

            var SignedXml__digestedSignedInfo =
                SignedXml_Type.GetField("_digestedSignedInfo", BindingFlags.NonPublic | BindingFlags.Instance);

            if (!bCacheValid || !CacheValid)
            {
                //
                //string securityUrl = (this.m_containingDocument == null) ? null : this.m_containingDocument.BaseURI;
                var SignedXml_m_containingDocument = SignedXml_Type.GetField("m_containingDocument",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var m_containingDocument = (XmlDocument) SignedXml_m_containingDocument.GetValue(this);
                var securityUrl = m_containingDocument?.BaseURI;
                //

                //XmlResolver xmlResolver = this.m_bResolverSet ? this.m_xmlResolver : new XmlSecureResolver(new XmlUrlResolver(), securityUrl);
                var SignedXml_m_bResolverSet =
                    SignedXml_Type.GetField("m_bResolverSet", BindingFlags.NonPublic | BindingFlags.Instance);
                var m_bResolverSet = (bool) SignedXml_m_bResolverSet.GetValue(this);
                var SignedXml_m_xmlResolver =
                    SignedXml_Type.GetField("m_xmlResolver", BindingFlags.NonPublic | BindingFlags.Instance);
                var m_xmlResolver = (XmlResolver) SignedXml_m_xmlResolver.GetValue(this);
                var xmlResolver = m_bResolverSet
                    ? m_xmlResolver
                    : new XmlSecureResolver(new XmlUrlResolver(), securityUrl);
                //

                //XmlDocument document = Utils.PreProcessElementInput(this.SignedInfo.GetXml(), xmlResolver, securityUrl);
                var System_Security_Assembly =
                    Assembly.Load("System.Security, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                var Utils_Type = System_Security_Assembly.GetType("System.Security.Cryptography.Xml.Utils");
                var Utils_PreProcessElementInput = Utils_Type.GetMethod("PreProcessElementInput",
                    BindingFlags.NonPublic | BindingFlags.Static);

                var xml = SignedInfo.GetXml();
                SetPrefix(prefix, xml); // <---

                var document =
                    (XmlDocument) Utils_PreProcessElementInput.Invoke(null,
                        new object[] {xml, xmlResolver, securityUrl});

                var docNamespaces = GetAllNamespaces(GetSignatureElement());

                if (AddXadesNamespace)
                {
                    var attr = signatureDocument.CreateAttribute("xmlns:xades");
                    attr.Value = XadesNamespaceUri;

                    docNamespaces.Add(attr);
                }


                foreach (var attr in docNamespaces)
                {
                    var newAttr = document.CreateAttribute(attr.Name);
                    newAttr.Value = attr.Value;

                    document.DocumentElement.Attributes.Append(newAttr);
                }

                //CanonicalXmlNodeList namespaces = (this.m_context == null) ? null : Utils.GetPropagatedAttributes(this.m_context);
                var SignedXml_m_context =
                    SignedXml_Type.GetField("m_context", BindingFlags.NonPublic | BindingFlags.Instance);
                var Utils_GetPropagatedAttributes = Utils_Type.GetMethod("GetPropagatedAttributes",
                    BindingFlags.NonPublic | BindingFlags.Static);
                var m_context = SignedXml_m_context.GetValue(this);
                var namespaces = m_context == null
                    ? null
                    : Utils_GetPropagatedAttributes.Invoke(null, new[] {m_context});


                //

                // Utils.AddNamespaces(document.DocumentElement, namespaces);
                var CanonicalXmlNodeList_Type =
                    System_Security_Assembly.GetType("System.Security.Cryptography.Xml.CanonicalXmlNodeList");
                var Utils_AddNamespaces = Utils_Type.GetMethod("AddNamespaces",
                    BindingFlags.NonPublic | BindingFlags.Static, null,
                    new[] {typeof(XmlElement), CanonicalXmlNodeList_Type}, null);
                Utils_AddNamespaces.Invoke(null, new[] {document.DocumentElement, namespaces});
                //

                //Transform canonicalizationMethodObject = this.SignedInfo.CanonicalizationMethodObject;
                var canonicalizationMethodObject = SignedInfo.CanonicalizationMethodObject;
                //

                canonicalizationMethodObject.Resolver = xmlResolver;

                //canonicalizationMethodObject.BaseURI = securityUrl;
                var Transform_Type = typeof(System.Security.Cryptography.Xml.Transform);
                var Transform_BaseURI =
                    Transform_Type.GetProperty("BaseURI", BindingFlags.NonPublic | BindingFlags.Instance);
                Transform_BaseURI.SetValue(canonicalizationMethodObject, securityUrl, null);
                //

                canonicalizationMethodObject.LoadInput(document);

                //this._digestedSignedInfo = canonicalizationMethodObject.GetDigestedOutput(hash);
                SignedXml__digestedSignedInfo.SetValue(this, canonicalizationMethodObject.GetDigestedOutput(hash));
                //

                //this.bCacheValid = true;
                SignedXml_bCacheValid.SetValue(this, true);
                //
            }

            //return this._digestedSignedInfo;
            var _digestedSignedInfo = (byte[]) SignedXml__digestedSignedInfo.GetValue(this);
            return _digestedSignedInfo;
            //
        }

        #endregion

        #region Private methods

        private XmlElement GetXadesObjectElement(XmlElement signatureElement)
        {
            XmlElement retVal = null;

            var xmlNamespaceManager =
                new XmlNamespaceManager(signatureElement.OwnerDocument
                    .NameTable); //Create an XmlNamespaceManager to resolve namespace
            xmlNamespaceManager.AddNamespace("ds", XmlDsigNamespaceUrl);
            xmlNamespaceManager.AddNamespace("xades", XadesNamespaceUri);

            var xmlNodeList = signatureElement.SelectNodes("ds:Object/xades:QualifyingProperties", xmlNamespaceManager);
            if (xmlNodeList.Count > 0)
                retVal = (XmlElement) xmlNodeList.Item(0).ParentNode;
            else
                retVal = null;

            return retVal;
        }

        private void SetSignatureStandard(XmlElement signatureElement)
        {
            if (GetXadesObjectElement(signatureElement) != null)
                SignatureStandard = KnownSignatureStandard.Xades;
            else
                SignatureStandard = KnownSignatureStandard.XmlDsig;
        }

        private DataObject GetXadesDataObject()
        {
            DataObject retVal = null;

            for (var dataObjectCounter = 0; dataObjectCounter < Signature.ObjectList.Count; dataObjectCounter++)
            {
                var dataObject = (DataObject) Signature.ObjectList[dataObjectCounter];
                var dataObjectXmlElement = dataObject.GetXml();
                var xmlNamespaceManager = new XmlNamespaceManager(dataObjectXmlElement.OwnerDocument.NameTable);
                xmlNamespaceManager.AddNamespace("xades", XadesNamespaceUri);
                var xmlNodeList = dataObjectXmlElement.SelectNodes("xades:QualifyingProperties", xmlNamespaceManager);
                if (xmlNodeList.Count != 0)
                {
                    retVal = dataObject;

                    break;
                }
            }

            return retVal;
        }

        private void SchemaValidationHandler(object sender, ValidationEventArgs validationEventArgs)
        {
            validationErrorOccurred = true;
            validationErrorDescription += "Validation error:\n";
            validationErrorDescription += "\tSeverity: " + validationEventArgs.Severity + "\n";
            validationErrorDescription += "\tMessage: " + validationEventArgs.Message + "\n";
        }

        private void XmlValidationHandler(object sender, ValidationEventArgs validationEventArgs)
        {
            if (validationEventArgs.Severity != XmlSeverityType.Warning)
            {
                validationErrorOccurred = true;
                validationErrorDescription += "Validation error:\n";
                validationErrorDescription += "\tSeverity: " + validationEventArgs.Severity + "\n";
                validationErrorDescription += "\tMessage: " + validationEventArgs.Message + "\n";
            }
        }

        private bool CheckHashDataInfosForTimeStamp(TimeStamp timeStamp)
        {
            var retVal = true;

            for (var referenceCounter = 0; retVal && referenceCounter < SignedInfo.References.Count; referenceCounter++)
            {
                var referenceId = ((Reference) SignedInfo.References[referenceCounter]).Id;
                var referenceUri = ((Reference) SignedInfo.References[referenceCounter]).Uri;
                if (referenceUri != "#" + XadesObject.QualifyingProperties.SignedProperties.Id)
                {
                    var hashDataInfoFound = false;
                    for (var hashDataInfoCounter = 0;
                        hashDataInfoFound == false && hashDataInfoCounter < timeStamp.HashDataInfoCollection.Count;
                        hashDataInfoCounter++)
                    {
                        var hashDataInfo = timeStamp.HashDataInfoCollection[hashDataInfoCounter];
                        hashDataInfoFound = "#" + referenceId == hashDataInfo.UriAttribute;
                    }

                    retVal = hashDataInfoFound;
                }
            }

            return retVal;
        }

        private bool CheckHashDataInfosExist(TimeStamp timeStamp)
        {
            var retVal = true;

            for (var hashDataInfoCounter = 0;
                retVal && hashDataInfoCounter < timeStamp.HashDataInfoCollection.Count;
                hashDataInfoCounter++)
            {
                var hashDataInfo = timeStamp.HashDataInfoCollection[hashDataInfoCounter];
                var referenceFound = false;
                string referenceId;

                for (var referenceCounter = 0;
                    referenceFound == false && referenceCounter < SignedInfo.References.Count;
                    referenceCounter++)
                {
                    referenceId = ((Reference) SignedInfo.References[referenceCounter]).Id;
                    if ("#" + referenceId == hashDataInfo.UriAttribute) referenceFound = true;
                }

                retVal = referenceFound;
            }

            return retVal;
        }


        private bool CheckObjectReference(ObjectReference objectReference)
        {
            var retVal = false;

            for (var referenceCounter = 0;
                retVal == false && referenceCounter < SignedInfo.References.Count;
                referenceCounter++)
            {
                var referenceId = ((Reference) SignedInfo.References[referenceCounter]).Id;
                if ("#" + referenceId == objectReference.ObjectReferenceUri) retVal = true;
            }

            return retVal;
        }

        private bool CheckHashDataInfoPointsToSignatureValue(TimeStamp timeStamp)
        {
            var retVal = true;
            foreach (HashDataInfo hashDataInfo in timeStamp.HashDataInfoCollection)
                retVal &= hashDataInfo.UriAttribute == "#" + SignatureValueId;

            return retVal;
        }

        private bool CheckHashDataInfosOfSigAndRefsTimeStamp(TimeStamp timeStamp)
        {
            UnsignedSignatureProperties unsignedSignatureProperties;
            var signatureValueHashDataInfoFound = false;
            var allSignatureTimeStampHashDataInfosFound = false;
            var completeCertificateRefsHashDataInfoFound = false;
            var completeRevocationRefsHashDataInfoFound = false;

            var signatureTimeStampIds = new ArrayList();

            var retVal = true;

            unsignedSignatureProperties =
                XadesObject.QualifyingProperties.UnsignedProperties.UnsignedSignatureProperties;

            foreach (TimeStamp signatureTimeStamp in unsignedSignatureProperties.SignatureTimeStampCollection)
                signatureTimeStampIds.Add("#" + signatureTimeStamp.EncapsulatedTimeStamp.Id);
            signatureTimeStampIds.Sort();
            foreach (HashDataInfo hashDataInfo in timeStamp.HashDataInfoCollection)
            {
                if (hashDataInfo.UriAttribute == "#" + SignatureValueId) signatureValueHashDataInfoFound = true;
                var signatureTimeStampIdIndex = signatureTimeStampIds.BinarySearch(hashDataInfo.UriAttribute);
                if (signatureTimeStampIdIndex >= 0) signatureTimeStampIds.RemoveAt(signatureTimeStampIdIndex);
                if (hashDataInfo.UriAttribute == "#" + unsignedSignatureProperties.CompleteCertificateRefs.Id)
                    completeCertificateRefsHashDataInfoFound = true;
                if (hashDataInfo.UriAttribute == "#" + unsignedSignatureProperties.CompleteRevocationRefs.Id)
                    completeRevocationRefsHashDataInfoFound = true;
            }

            if (signatureTimeStampIds.Count == 0) allSignatureTimeStampHashDataInfosFound = true;
            retVal = signatureValueHashDataInfoFound && allSignatureTimeStampHashDataInfosFound &&
                     completeCertificateRefsHashDataInfoFound && completeRevocationRefsHashDataInfoFound;

            return retVal;
        }

        private bool CheckHashDataInfosOfRefsOnlyTimeStamp(TimeStamp timeStamp)
        {
            var completeCertificateRefsHashDataInfoFound = false;
            var completeRevocationRefsHashDataInfoFound = false;
            var retVal = true;

            var unsignedSignatureProperties =
                XadesObject.QualifyingProperties.UnsignedProperties.UnsignedSignatureProperties;
            foreach (HashDataInfo hashDataInfo in timeStamp.HashDataInfoCollection)
            {
                if (hashDataInfo.UriAttribute == "#" + unsignedSignatureProperties.CompleteCertificateRefs.Id)
                    completeCertificateRefsHashDataInfoFound = true;
                if (hashDataInfo.UriAttribute == "#" + unsignedSignatureProperties.CompleteRevocationRefs.Id)
                    completeRevocationRefsHashDataInfoFound = true;
            }

            retVal = completeCertificateRefsHashDataInfoFound && completeRevocationRefsHashDataInfoFound;

            return retVal;
        }

        private bool CheckHashDataInfosOfArchiveTimeStamp(TimeStamp timeStamp)
        {
            var allReferenceHashDataInfosFound = false;
            var signedInfoHashDataInfoFound = false;
            var signedPropertiesHashDataInfoFound = false;
            var signatureValueHashDataInfoFound = false;
            var allSignatureTimeStampHashDataInfosFound = false;
            var completeCertificateRefsHashDataInfoFound = false;
            var completeRevocationRefsHashDataInfoFound = false;
            var certificatesValuesHashDataInfoFound = false;
            var revocationValuesHashDataInfoFound = false;
            var allSigAndRefsTimeStampHashDataInfosFound = false;
            var allRefsOnlyTimeStampHashDataInfosFound = false;
            var allArchiveTimeStampHashDataInfosFound = false;
            var allOlderArchiveTimeStampsFound = false;

            var referenceIds = new ArrayList();
            var signatureTimeStampIds = new ArrayList();
            var sigAndRefsTimeStampIds = new ArrayList();
            var refsOnlyTimeStampIds = new ArrayList();
            var archiveTimeStampIds = new ArrayList();

            var unsignedSignatureProperties =
                XadesObject.QualifyingProperties.UnsignedProperties.UnsignedSignatureProperties;
            var signedProperties = XadesObject.QualifyingProperties.SignedProperties;

            foreach (Reference reference in Signature.SignedInfo.References)
                if (reference.Uri != "#" + signedProperties.Id)
                    referenceIds.Add(reference.Uri);
            referenceIds.Sort();
            foreach (TimeStamp signatureTimeStamp in unsignedSignatureProperties.SignatureTimeStampCollection)
                signatureTimeStampIds.Add("#" + signatureTimeStamp.EncapsulatedTimeStamp.Id);
            signatureTimeStampIds.Sort();
            foreach (TimeStamp sigAndRefsTimeStamp in unsignedSignatureProperties.SigAndRefsTimeStampCollection)
                sigAndRefsTimeStampIds.Add("#" + sigAndRefsTimeStamp.EncapsulatedTimeStamp.Id);
            sigAndRefsTimeStampIds.Sort();
            foreach (TimeStamp refsOnlyTimeStamp in unsignedSignatureProperties.RefsOnlyTimeStampCollection)
                refsOnlyTimeStampIds.Add("#" + refsOnlyTimeStamp.EncapsulatedTimeStamp.Id);
            refsOnlyTimeStampIds.Sort();
            allOlderArchiveTimeStampsFound = false;
            for (var archiveTimeStampCounter = 0;
                !allOlderArchiveTimeStampsFound && archiveTimeStampCounter <
                unsignedSignatureProperties.ArchiveTimeStampCollection.Count;
                archiveTimeStampCounter++)
            {
                var archiveTimeStamp = unsignedSignatureProperties.ArchiveTimeStampCollection[archiveTimeStampCounter];
                if (archiveTimeStamp.EncapsulatedTimeStamp.Id == timeStamp.EncapsulatedTimeStamp.Id)
                    allOlderArchiveTimeStampsFound = true;
                else
                    archiveTimeStampIds.Add("#" + archiveTimeStamp.EncapsulatedTimeStamp.Id);
            }

            archiveTimeStampIds.Sort();
            foreach (HashDataInfo hashDataInfo in timeStamp.HashDataInfoCollection)
            {
                var index = referenceIds.BinarySearch(hashDataInfo.UriAttribute);
                if (index >= 0) referenceIds.RemoveAt(index);
                if (hashDataInfo.UriAttribute == "#" + signedInfoIdBuffer) signedInfoHashDataInfoFound = true;
                if (hashDataInfo.UriAttribute == "#" + signedProperties.Id) signedPropertiesHashDataInfoFound = true;
                if (hashDataInfo.UriAttribute == "#" + SignatureValueId) signatureValueHashDataInfoFound = true;
                index = signatureTimeStampIds.BinarySearch(hashDataInfo.UriAttribute);
                if (index >= 0) signatureTimeStampIds.RemoveAt(index);
                if (hashDataInfo.UriAttribute == "#" + unsignedSignatureProperties.CompleteCertificateRefs.Id)
                    completeCertificateRefsHashDataInfoFound = true;
                if (hashDataInfo.UriAttribute == "#" + unsignedSignatureProperties.CompleteRevocationRefs.Id)
                    completeRevocationRefsHashDataInfoFound = true;
                if (hashDataInfo.UriAttribute == "#" + unsignedSignatureProperties.CertificateValues.Id)
                    certificatesValuesHashDataInfoFound = true;
                if (hashDataInfo.UriAttribute == "#" + unsignedSignatureProperties.RevocationValues.Id)
                    revocationValuesHashDataInfoFound = true;
                index = sigAndRefsTimeStampIds.BinarySearch(hashDataInfo.UriAttribute);
                if (index >= 0) sigAndRefsTimeStampIds.RemoveAt(index);
                index = refsOnlyTimeStampIds.BinarySearch(hashDataInfo.UriAttribute);
                if (index >= 0) refsOnlyTimeStampIds.RemoveAt(index);
                index = archiveTimeStampIds.BinarySearch(hashDataInfo.UriAttribute);
                if (index >= 0) archiveTimeStampIds.RemoveAt(index);
            }

            if (referenceIds.Count == 0) allReferenceHashDataInfosFound = true;
            if (signatureTimeStampIds.Count == 0) allSignatureTimeStampHashDataInfosFound = true;
            if (sigAndRefsTimeStampIds.Count == 0) allSigAndRefsTimeStampHashDataInfosFound = true;
            if (refsOnlyTimeStampIds.Count == 0) allRefsOnlyTimeStampHashDataInfosFound = true;
            if (archiveTimeStampIds.Count == 0) allArchiveTimeStampHashDataInfosFound = true;

            var retVal = allReferenceHashDataInfosFound && signedInfoHashDataInfoFound &&
                         signedPropertiesHashDataInfoFound &&
                         signatureValueHashDataInfoFound && allSignatureTimeStampHashDataInfosFound &&
                         completeCertificateRefsHashDataInfoFound &&
                         completeRevocationRefsHashDataInfoFound && certificatesValuesHashDataInfoFound &&
                         revocationValuesHashDataInfoFound &&
                         allSigAndRefsTimeStampHashDataInfosFound && allRefsOnlyTimeStampHashDataInfosFound &&
                         allArchiveTimeStampHashDataInfosFound;

            return retVal;
        }

        #endregion
    }
}