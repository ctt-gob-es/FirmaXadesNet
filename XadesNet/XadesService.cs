﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using Microsoft.Xades;
using XadesNet.Crypto;
using XadesNet.Signature;
using XadesNet.Signature.Parameters;
using XadesNet.Utils;
using XadesNet.Validation;
using SignerRole = Microsoft.Xades.SignerRole;

namespace XadesNet
{
    public class XadesService
    {
        public static XmlDocument Sign(string inputPath, X509Certificate2 certificate)
        {
            var _xadesService = new XadesService();
            SignatureDocument _signatureDocument;

            var parameters = new SignatureParameters
            {
                SignatureMethod = SignatureMethod.RSAwithSHA256,
                SigningDate = DateTime.Now,
                SignaturePackaging = SignaturePackaging.ENVELOPED
            };

            using (parameters.Signer = new Signer(certificate))
            {
                using (var fs = new FileStream(inputPath, FileMode.Open))
                {
                    _signatureDocument = _xadesService.Sign(fs, parameters);
                }
            }

            return _signatureDocument.Document;
        }

        public static XmlDocument SignEnveloped(XmlDocument xmlDocument, X509Certificate2 certificate)
        {
            var _xadesService = new XadesService();

            var parametros = new SignatureParameters
            {
                SignatureMethod = SignatureMethod.RSAwithSHA256,
                SigningDate = DateTime.Now,
                SignaturePackaging = SignaturePackaging.ENVELOPED,
                Signer = new Signer(certificate)
            };

            var _signatureDocument = _xadesService.SignEnveloped(xmlDocument, parametros);
            return _signatureDocument.Document;
        }

        #region Private variables

        private Reference _refContent;
        private string _mimeType;
        private string _encoding;

        #endregion

        #region Public methods

        #region Signature methods

        public SignatureDocument SignEnveloped(XmlDocument xmlDocument, SignatureParameters parameters)
        {
            if (parameters.Signer == null) throw new Exception("A valid certificate is required for signature");

            var signatureDocument = new SignatureDocument();

            switch (parameters.SignaturePackaging)
            {
                case SignaturePackaging.ENVELOPED:
                    SetContentEnveloped(signatureDocument, xmlDocument);
                    break;

                case SignaturePackaging.ENVELOPING:
                    SetContentEveloping(signatureDocument, xmlDocument);
                    break;

                case SignaturePackaging.EXTERNALLY_DETACHED:
                    SetContentExternallyDetached(signatureDocument, parameters.ExternalContentUri);
                    break;
            }

            SetSignatureId(signatureDocument.XadesSignature);

            PrepareSignature(signatureDocument, parameters);

            ComputeSignature(signatureDocument);

            signatureDocument.UpdateDocument();

            return signatureDocument;
        }

        /// <summary>
        ///     Realiza el proceso de firmado
        /// </summary>
        /// <param name="input"></param>
        /// <param name="parameters"></param>
        public SignatureDocument Sign(Stream input, SignatureParameters parameters)
        {
            if (parameters.Signer == null) throw new Exception("A valid certificate is required for signature");

            if (input == null && string.IsNullOrEmpty(parameters.ExternalContentUri))
                throw new Exception("No se ha especificado ningún contenido a firmar");

            var signatureDocument = new SignatureDocument();

            switch (parameters.SignaturePackaging)
            {
                case SignaturePackaging.INTERNALLY_DETACHED:
                    if (string.IsNullOrEmpty(parameters.InputMimeType))
                        throw new NullReferenceException("You need to specify the MIME type of the element to sign.");

                    if (!string.IsNullOrEmpty(parameters.ElementIdToSign))
                        SetContentInternallyDetached(signatureDocument, XMLUtil.LoadDocument(input),
                            parameters.ElementIdToSign, parameters.InputMimeType);
                    else
                        SetContentInternallyDetached(signatureDocument, input, parameters.InputMimeType);
                    break;

                case SignaturePackaging.ENVELOPED:
                    SetContentEnveloped(signatureDocument, XMLUtil.LoadDocument(input));
                    break;

                case SignaturePackaging.ENVELOPING:
                    SetContentEveloping(signatureDocument, XMLUtil.LoadDocument(input));
                    break;

                case SignaturePackaging.EXTERNALLY_DETACHED:
                    SetContentExternallyDetached(signatureDocument, parameters.ExternalContentUri);
                    break;
            }

            SetSignatureId(signatureDocument.XadesSignature);

            PrepareSignature(signatureDocument, parameters);

            ComputeSignature(signatureDocument);

            signatureDocument.UpdateDocument();

            return signatureDocument;
        }

        /// <summary>
        ///     Añade una firma al documento
        /// </summary>
        /// <param name="sigDocument"></param>
        /// <param name="parameters"></param>
        public SignatureDocument CoSign(SignatureDocument sigDocument, SignatureParameters parameters)
        {
            SignatureDocument.CheckSignatureDocument(sigDocument);

            _refContent = sigDocument.XadesSignature.GetContentReference();

            if (_refContent == null)
                throw new Exception("Could not find reference for signed content.");

            _mimeType = string.Empty;
            _encoding = string.Empty;

            foreach (DataObjectFormat dof in sigDocument.XadesSignature.XadesObject.QualifyingProperties
                .SignedProperties.SignedDataObjectProperties.DataObjectFormatCollection)
                if (dof.ObjectReferenceAttribute == "#" + _refContent.Id)
                {
                    _mimeType = dof.MimeType;
                    _encoding = dof.Encoding;
                    break;
                }

            var coSignatureDocument = new SignatureDocument {Document = (XmlDocument) sigDocument.Document.Clone()};
            coSignatureDocument.Document.PreserveWhitespace = true;

            coSignatureDocument.XadesSignature = new XadesSignedXml(coSignatureDocument.Document);
            coSignatureDocument.XadesSignature.LoadXml(sigDocument.XadesSignature.GetXml());

            var destination = coSignatureDocument.XadesSignature.GetSignatureElement().ParentNode;

            coSignatureDocument.XadesSignature = new XadesSignedXml(coSignatureDocument.Document);

            _refContent.Id = "Reference-" + Guid.NewGuid();

            if (_refContent.Type != XadesSignedXml.XmlDsigObjectType) _refContent.Type = "";

            coSignatureDocument.XadesSignature.AddReference(_refContent);

            if (destination.NodeType != XmlNodeType.Document)
                coSignatureDocument.XadesSignature.SignatureNodeDestination = (XmlElement) destination;
            else
                coSignatureDocument.XadesSignature.SignatureNodeDestination =
                    ((XmlDocument) destination).DocumentElement;


            SetSignatureId(coSignatureDocument.XadesSignature);

            PrepareSignature(coSignatureDocument, parameters);

            ComputeSignature(coSignatureDocument);

            coSignatureDocument.UpdateDocument();

            return coSignatureDocument;
        }


        /// <summary>
        ///     Realiza la contrafirma de la firma actual
        /// </summary>
        /// <param name="sigDocument"></param>
        /// <param name="parameters"></param>
        public SignatureDocument CounterSign(SignatureDocument sigDocument, SignatureParameters parameters)
        {
            if (parameters.Signer == null) throw new Exception("A valid certificate is required for signing.");

            SignatureDocument.CheckSignatureDocument(sigDocument);

            var counterSigDocument = new SignatureDocument {Document = (XmlDocument) sigDocument.Document.Clone()};
            counterSigDocument.Document.PreserveWhitespace = true;

            var counterSignature = new XadesSignedXml(counterSigDocument.Document);
            SetSignatureId(counterSignature);

            counterSignature.SigningKey = parameters.Signer.SigningKey;

            _refContent = new Reference
            {
                Uri = "#" + sigDocument.XadesSignature.SignatureValueId,
                Id = "Reference-" + Guid.NewGuid(),
                Type = "http://uri.etsi.org/01903#CountersignedSignature"
            };
            _refContent.AddTransform(new XmlDsigC14NTransform());
            counterSignature.AddReference(_refContent);

            _mimeType = "text/xml";
            _encoding = "UTF-8";

            var keyInfo = new KeyInfo {Id = "KeyInfoId-" + counterSignature.Signature.Id};
            keyInfo.AddClause(new KeyInfoX509Data(parameters.Signer.Certificate));
            keyInfo.AddClause(new RSAKeyValue((RSA) parameters.Signer.SigningKey));
            counterSignature.KeyInfo = keyInfo;

            var referenceKeyInfo = new Reference
            {
                Id = "ReferenceKeyInfo-" + counterSignature.Signature.Id,
                Uri = "#KeyInfoId-" + counterSignature.Signature.Id
            };
            counterSignature.AddReference(referenceKeyInfo);

            var counterSignatureXadesObject = new XadesObject
            {
                Id = "CounterSignatureXadesObject-" + Guid.NewGuid(),
                QualifyingProperties =
                {
                    Target = "#" + counterSignature.Signature.Id,
                    SignedProperties = {Id = "SignedProperties-" + counterSignature.Signature.Id}
                }
            };

            AddSignatureProperties(counterSigDocument,
                counterSignatureXadesObject.QualifyingProperties.SignedProperties.SignedSignatureProperties,
                counterSignatureXadesObject.QualifyingProperties.SignedProperties.SignedDataObjectProperties,
                counterSignatureXadesObject.QualifyingProperties.UnsignedProperties.UnsignedSignatureProperties,
                parameters);

            counterSignature.AddXadesObject(counterSignatureXadesObject);

            foreach (Reference signReference in counterSignature.SignedInfo.References)
                signReference.DigestMethod = parameters.DigestMethod.URI;

            counterSignature.SignedInfo.SignatureMethod = parameters.SignatureMethod.URI;

            counterSignature.AddXadesNamespace = true;
            counterSignature.ComputeSignature();

            counterSigDocument.XadesSignature = new XadesSignedXml(counterSigDocument.Document);
            counterSigDocument.XadesSignature.LoadXml(sigDocument.XadesSignature.GetXml());

            var unsignedProperties = counterSigDocument.XadesSignature.UnsignedProperties;
            unsignedProperties.UnsignedSignatureProperties.CounterSignatureCollection.Add(counterSignature);
            counterSigDocument.XadesSignature.UnsignedProperties = unsignedProperties;

            counterSigDocument.UpdateDocument();

            return counterSigDocument;
        }

        #endregion

        #region Carga de firmas

        /// <summary>
        ///     Carga un archivo de firma.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public SignatureDocument[] Load(Stream input)
        {
            return Load(XMLUtil.LoadDocument(input));
        }

        /// <summary>
        ///     Carga un archivo de firma.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public SignatureDocument[] Load(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Open))
            {
                return Load(fs);
            }
        }

        /// <summary>
        ///     Carga un archivo de firma.
        /// </summary>
        /// <param name="xmlDocument"></param>
        public SignatureDocument[] Load(XmlDocument xmlDocument)
        {
            var signatureNodeList = xmlDocument.GetElementsByTagName("Signature", SignedXml.XmlDsigNamespaceUrl);

            if (signatureNodeList.Count == 0) throw new Exception("No se ha encontrado ninguna firma.");

            var firmas = new List<SignatureDocument>();

            foreach (var signatureNode in signatureNodeList)
            {
                var sigDocument = new SignatureDocument {Document = (XmlDocument) xmlDocument.Clone()};
                sigDocument.Document.PreserveWhitespace = true;
                sigDocument.XadesSignature = new XadesSignedXml(sigDocument.Document);
                sigDocument.XadesSignature.LoadXml((XmlElement) signatureNode);

                firmas.Add(sigDocument);
            }

            return firmas.ToArray();
        }

        #endregion

        #region Validación

        /// <summary>
        ///     Realiza la validación de una firma XAdES
        /// </summary>
        /// <param name="sigDocument"></param>
        /// <returns></returns>
        public ValidationResult Validate(SignatureDocument sigDocument)
        {
            SignatureDocument.CheckSignatureDocument(sigDocument);

            var validator = new XadesValidator();

            return validator.Validate(sigDocument);
        }

        #endregion

        #endregion

        #region Private methods

        /// <summary>
        ///     Set the identifier for the signature
        /// </summary>
        private void SetSignatureId(XadesSignedXml xadesSignedXml)
        {
            var id = Guid.NewGuid().ToString();

            xadesSignedXml.Signature.Id = "Signature-" + id;
            xadesSignedXml.SignatureValueId = "SignatureValue-" + id;
        }

        /// <summary>
        ///     Loads the specified XML document and sets to sign the specified element in elementId
        /// </summary>
        /// <param name="sigDocument"></param>
        /// <param name="xmlDocument"></param>
        /// <param name="elementId"></param>
        /// <param name="mimeType"></param>
        private void SetContentInternallyDetached(SignatureDocument sigDocument, XmlDocument xmlDocument,
            string elementId, string mimeType)
        {
            sigDocument.Document = xmlDocument;

            _refContent = new Reference {Uri = "#" + elementId, Id = "Reference-" + Guid.NewGuid()};


            _mimeType = mimeType;

            if (mimeType == "text/xml")
            {
                var transform = new XmlDsigC14NTransform();
                _refContent.AddTransform(transform);

                _encoding = "UTF-8";
            }
            else
            {
                var transform = new XmlDsigBase64Transform();
                _refContent.AddTransform(transform);

                _encoding = transform.Algorithm;
            }

            sigDocument.XadesSignature = new XadesSignedXml(sigDocument.Document);

            sigDocument.XadesSignature.AddReference(_refContent);
        }

        /// <summary>
        ///     Inserta un documento para generar una firma internally detached.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="mimeType"></param>
        /// <param name="sigDocument"></param>
        private void SetContentInternallyDetached(SignatureDocument sigDocument, Stream input, string mimeType)
        {
            sigDocument.Document = new XmlDocument();

            var rootElement = sigDocument.Document.CreateElement("DOCFIRMA");
            sigDocument.Document.AppendChild(rootElement);

            var id = "CONTENT-" + Guid.NewGuid();

            _refContent = new Reference
            {
                Uri = "#" + id, Id = "Reference-" + Guid.NewGuid(), Type = XadesSignedXml.XmlDsigObjectType
            };


            _mimeType = mimeType;

            var contentElement = sigDocument.Document.CreateElement("CONTENT");

            if (mimeType == "text/xml")
            {
                _encoding = "UTF-8";

                var doc = new XmlDocument {PreserveWhitespace = true};
                doc.Load(input);

                contentElement.InnerXml = doc.DocumentElement.OuterXml;

                var transform = new XmlDsigC14NTransform();
                _refContent.AddTransform(transform);
            }
            else
            {
                var transform = new XmlDsigBase64Transform();
                _refContent.AddTransform(transform);

                _encoding = transform.Algorithm;

                if (mimeType == "hash/sha256")
                    using (var sha2 = SHA256.Create())
                    {
                        contentElement.InnerText = Convert.ToBase64String(sha2.ComputeHash(input));
                    }
                else
                    using (var ms = new MemoryStream())
                    {
                        input.CopyTo(ms);
                        contentElement.InnerText = Convert.ToBase64String(ms.ToArray());
                    }
            }

            contentElement.SetAttribute("Id", id);
            contentElement.SetAttribute("MimeType", _mimeType);
            contentElement.SetAttribute("Encoding", _encoding);


            rootElement.AppendChild(contentElement);

            sigDocument.XadesSignature = new XadesSignedXml(sigDocument.Document);

            sigDocument.XadesSignature.AddReference(_refContent);
        }

        /// <summary>
        ///     Insert XML content to generate an enveloping signature.
        /// </summary>
        /// <param name="sigDocument"></param>
        /// <param name="xmlDocument"></param>
        private void SetContentEveloping(SignatureDocument sigDocument, XmlDocument xmlDocument)
        {
            _refContent = new Reference();

            sigDocument.XadesSignature = new XadesSignedXml();

            var doc = (XmlDocument) xmlDocument.Clone();
            doc.PreserveWhitespace = true;

            if (doc.ChildNodes[0].NodeType == XmlNodeType.XmlDeclaration) doc.RemoveChild(doc.ChildNodes[0]);

            //Add an object
            var dataObjectId = "DataObject-" + Guid.NewGuid();
            var dataObject = new DataObject {Data = doc.ChildNodes, Id = dataObjectId};
            sigDocument.XadesSignature.AddObject(dataObject);

            _refContent.Id = "Reference-" + Guid.NewGuid();
            _refContent.Uri = "#" + dataObjectId;
            _refContent.Type = XadesSignedXml.XmlDsigObjectType;

            var transform = new XmlDsigC14NTransform();
            _refContent.AddTransform(transform);

            _mimeType = "text/xml";
            _encoding = "UTF-8";

            sigDocument.XadesSignature.AddReference(_refContent);
        }


        /// <summary>
        ///     Specifies the node where the signature will be added
        /// </summary>
        /// <param name="sigDocument"></param>
        /// <param name="destination"></param>
        private void SetSignatureDestination(SignatureDocument sigDocument, SignatureXPathExpression destination)
        {
            XmlNode nodo;

            if (destination.Namespaces.Count > 0)
            {
                var xmlnsMgr = new XmlNamespaceManager(sigDocument.Document.NameTable);
                foreach (var item in destination.Namespaces) xmlnsMgr.AddNamespace(item.Key, item.Value);

                nodo = sigDocument.Document.SelectSingleNode(destination.XPathExpression, xmlnsMgr);
            }
            else
            {
                nodo = sigDocument.Document.SelectSingleNode(destination.XPathExpression);
            }

            if (nodo == null) throw new Exception("Elemento no encontrado");

            sigDocument.XadesSignature.SignatureNodeDestination = (XmlElement) nodo;
        }


        /// <summary>
        ///     Inserta un documento para generar una firma externally detached.
        /// </summary>
        /// <param name="fileName"></param>
        private void SetContentExternallyDetached(SignatureDocument sigDocument, string fileName)
        {
            _refContent = new Reference();

            sigDocument.Document = new XmlDocument();
            sigDocument.XadesSignature = new XadesSignedXml(sigDocument.Document);

            _refContent.Uri = new Uri(fileName).AbsoluteUri;
            _refContent.Id = "Reference-" + Guid.NewGuid();

            if (_refContent.Uri.EndsWith(".xml") || _refContent.Uri.EndsWith(".XML"))
            {
                _mimeType = "text/xml";
                _refContent.AddTransform(new XmlDsigC14NTransform());
            }


            sigDocument.XadesSignature.AddReference(_refContent);
        }

        /// <summary>
        ///     Añade una transformación XPath al contenido a firmar
        /// </summary>
        /// <param name="XPathString"></param>
        private void AddXPathTransform(SignatureDocument sigDocument, Dictionary<string, string> namespaces,
            string XPathString)
        {
            var document = sigDocument.Document ?? new XmlDocument();

            var xPathElem = document.CreateElement("XPath");

            foreach (var ns in namespaces)
            {
                var attr = document.CreateAttribute("xmlns:" + ns.Key);
                attr.Value = ns.Value;

                xPathElem.Attributes.Append(attr);
            }

            xPathElem.InnerText = XPathString;

            var transform = new XmlDsigXPathTransform();

            transform.LoadInnerXml(xPathElem.SelectNodes("."));

            var reference = sigDocument.XadesSignature.SignedInfo.References[0] as Reference;

            reference.AddTransform(transform);
        }


        /// <summary>
        ///     Inserta un contenido XML para generar una firma enveloped.
        /// </summary>
        /// <param name="xmlDocument"></param>
        private void SetContentEnveloped(SignatureDocument sigDocument, XmlDocument xmlDocument)
        {
            sigDocument.Document = xmlDocument;

            _refContent = new Reference();

            sigDocument.XadesSignature = new XadesSignedXml(sigDocument.Document);

            _refContent.Id = "Reference-" + Guid.NewGuid();
            _refContent.Uri = "";

            _mimeType = "text/xml";
            _encoding = "UTF-8";

            for (var i = 0; i < sigDocument.Document.DocumentElement.Attributes.Count; i++)
                if (sigDocument.Document.DocumentElement.Attributes[i].Name
                    .Equals("id", StringComparison.InvariantCultureIgnoreCase))
                {
                    _refContent.Uri = "#" + sigDocument.Document.DocumentElement.Attributes[i].Value;
                    break;
                }

            var xmlDsigEnvelopedSignatureTransform = new XmlDsigEnvelopedSignatureTransform();
            _refContent.AddTransform(xmlDsigEnvelopedSignatureTransform);


            sigDocument.XadesSignature.AddReference(_refContent);
        }

        private void PrepareSignature(SignatureDocument sigDocument, SignatureParameters parameters)
        {
            sigDocument.XadesSignature.SignedInfo.SignatureMethod = parameters.SignatureMethod.URI;

            AddCertificateInfo(sigDocument, parameters);
            AddXadesInfo(sigDocument, parameters);

            foreach (Reference reference in sigDocument.XadesSignature.SignedInfo.References)
                reference.DigestMethod = parameters.DigestMethod.URI;

            if (parameters.SignatureDestination != null)
                SetSignatureDestination(sigDocument, parameters.SignatureDestination);

            if (parameters.XPathTransformations.Count > 0)
                foreach (var xPathTrans in parameters.XPathTransformations)
                    AddXPathTransform(sigDocument, xPathTrans.Namespaces, xPathTrans.XPathExpression);
        }

        private void ComputeSignature(SignatureDocument sigDocument)
        {
            try
            {
                sigDocument.XadesSignature.ComputeSignature();

                var signatureElement = sigDocument.XadesSignature.GetXml();
                sigDocument.XadesSignature.LoadXml(signatureElement);
            }
            catch (Exception ex)
            {
                throw new Exception("Ha ocurrido un error durante el proceso de firmado", ex);
            }
        }

        #region Sign information and properties

        private void AddXadesInfo(SignatureDocument sigDocument, SignatureParameters parameters)
        {
            var xadesObject = new XadesObject
            {
                Id = "XadesObjectId-" + Guid.NewGuid(),
                QualifyingProperties =
                {
                    Id = "QualifyingProperties-" + Guid.NewGuid(),
                    Target = "#" + sigDocument.XadesSignature.Signature.Id,
                    SignedProperties = {Id = "SignedProperties-" + sigDocument.XadesSignature.Signature.Id}
                }
            };

            AddSignatureProperties(sigDocument,
                xadesObject.QualifyingProperties.SignedProperties.SignedSignatureProperties,
                xadesObject.QualifyingProperties.SignedProperties.SignedDataObjectProperties,
                xadesObject.QualifyingProperties.UnsignedProperties.UnsignedSignatureProperties, parameters);

            sigDocument.XadesSignature.AddXadesObject(xadesObject);
        }


        private void AddCertificateInfo(SignatureDocument sigDocument, SignatureParameters parameters)
        {
            sigDocument.XadesSignature.SigningKey = parameters.Signer.SigningKey;

            var keyInfo = new KeyInfo {Id = "KeyInfoId-" + sigDocument.XadesSignature.Signature.Id};
            keyInfo.AddClause(new KeyInfoX509Data(parameters.Signer.Certificate));
            keyInfo.AddClause(new RSAKeyValue((RSA) parameters.Signer.SigningKey));

            sigDocument.XadesSignature.KeyInfo = keyInfo;

            var reference = new Reference
            {
                Id = "ReferenceKeyInfo", Uri = "#KeyInfoId-" + sigDocument.XadesSignature.Signature.Id
            };


            sigDocument.XadesSignature.AddReference(reference);
        }


        private void AddSignatureProperties(SignatureDocument sigDocument,
            SignedSignatureProperties signedSignatureProperties, SignedDataObjectProperties signedDataObjectProperties,
            UnsignedSignatureProperties unsignedSignatureProperties, SignatureParameters parameters)
        {
            var cert = new Cert
            {
                IssuerSerial =
                {
                    X509IssuerName = parameters.Signer.Certificate.IssuerName.Name,
                    X509SerialNumber = parameters.Signer.Certificate.GetSerialNumberAsDecimalString()
                }
            };
            DigestUtil.SetCertDigest(parameters.Signer.Certificate.GetRawCertData(), parameters.DigestMethod,
                cert.CertDigest);
            signedSignatureProperties.SigningCertificate.CertCollection.Add(cert);

            if (parameters.SignaturePolicyInfo != null)
            {
                if (!string.IsNullOrEmpty(parameters.SignaturePolicyInfo.PolicyIdentifier))
                {
                    signedSignatureProperties.SignaturePolicyIdentifier.SignaturePolicyImplied = false;
                    signedSignatureProperties.SignaturePolicyIdentifier.SignaturePolicyId.SigPolicyId.Identifier
                        .IdentifierUri = parameters.SignaturePolicyInfo.PolicyIdentifier;
                }

                if (!string.IsNullOrEmpty(parameters.SignaturePolicyInfo.PolicyUri))
                {
                    var spq = new SigPolicyQualifier
                    {
                        AnyXmlElement = sigDocument.Document.CreateElement(XadesSignedXml.XmlXadesPrefix, "SPURI",
                            XadesSignedXml.XadesNamespaceUri)
                    };
                    spq.AnyXmlElement.InnerText = parameters.SignaturePolicyInfo.PolicyUri;

                    signedSignatureProperties.SignaturePolicyIdentifier.SignaturePolicyId.SigPolicyQualifiers
                        .SigPolicyQualifierCollection.Add(spq);
                }

                if (!string.IsNullOrEmpty(parameters.SignaturePolicyInfo.PolicyHash))
                {
                    signedSignatureProperties.SignaturePolicyIdentifier.SignaturePolicyId.SigPolicyHash.DigestMethod
                        .Algorithm = parameters.SignaturePolicyInfo.PolicyDigestAlgorithm.URI;
                    signedSignatureProperties.SignaturePolicyIdentifier.SignaturePolicyId.SigPolicyHash.DigestValue =
                        Convert.FromBase64String(parameters.SignaturePolicyInfo.PolicyHash);
                }
            }

            signedSignatureProperties.SigningTime =
                parameters.SigningDate ?? DateTime.Now;

            if (!string.IsNullOrEmpty(_mimeType))
            {
                var newDataObjectFormat = new DataObjectFormat
                {
                    MimeType = _mimeType, Encoding = _encoding, ObjectReferenceAttribute = "#" + _refContent.Id
                };


                signedDataObjectProperties.DataObjectFormatCollection.Add(newDataObjectFormat);
            }

            if (parameters.SignerRole != null &&
                (parameters.SignerRole.CertifiedRoles.Count > 0 || parameters.SignerRole.ClaimedRoles.Count > 0))
            {
                signedSignatureProperties.SignerRole = new SignerRole();

                foreach (var certifiedRole in parameters.SignerRole.CertifiedRoles)
                    signedSignatureProperties.SignerRole.CertifiedRoles.CertifiedRoleCollection.Add(new CertifiedRole
                        {PkiData = certifiedRole.GetRawCertData()});

                foreach (var claimedRole in parameters.SignerRole.ClaimedRoles)
                    signedSignatureProperties.SignerRole.ClaimedRoles.ClaimedRoleCollection.Add(new ClaimedRole
                        {InnerText = claimedRole});
            }
        }

        #endregion

        #endregion
    }
}