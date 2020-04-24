#region

using System;
using System.IO;
using System.Windows.Forms;
using XadesNet;
using XadesNet.Clients;
using XadesNet.Crypto;
using XadesNet.Signature;
using XadesNet.Signature.Parameters;
using XadesNet.Upgraders;
using XadesNet.Upgraders.Parameters;
using XadesNet.Utils;

#endregion

namespace TestXades
{
    public partial class FrmPrincipal : Form
    {
        private readonly XadesService _xadesService = new XadesService();
        private SignatureDocument _signatureDocument;

        public FrmPrincipal()
        {
            InitializeComponent();
        }

        private void btnSeleccionarFichero_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK) txtFichero.Text = openFileDialog1.FileName;
        }

        private SignaturePolicyInfo ObtenerPolitica()
        {
            var spi = new SignaturePolicyInfo
            {
                PolicyIdentifier = txtIdentificadorPolitica.Text,
                PolicyHash = txtHashPolitica.Text,
                PolicyUri = txtURIPolitica.Text
            };


            return spi;
        }

        private static SignatureParameters ObtenerParametrosFirma()
        {
            var parametros = new SignatureParameters
            {
                SignatureMethod = SignatureMethod.RSAwithSHA256, SigningDate = DateTime.Now
            };

            return parametros;
        }

        private void btnFirmar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtFichero.Text))
            {
                MessageBox.Show("You must select a file to sign.");
                return;
            }

            var parametros = ObtenerParametrosFirma();

            if (rbInternnallyDetached.Checked)
            {
                // TODO: correctly manage MIME types
                var mimeType = "application/" +
                               Path.GetExtension(txtFichero.Text).ToLower().Replace(".", "");

                parametros.SignaturePolicyInfo = ObtenerPolitica();
                parametros.SignaturePackaging = SignaturePackaging.INTERNALLY_DETACHED;
                parametros.InputMimeType = mimeType;
            }
            else if (rbExternallyDetached.Checked)
            {
                parametros.SignaturePackaging = SignaturePackaging.EXTERNALLY_DETACHED;
                parametros.ExternalContentUri = txtFichero.Text;
            }
            else if (rbEnveloped.Checked)
            {
                parametros.SignaturePackaging = SignaturePackaging.ENVELOPED;
            }

            using (parametros.Signer = new Signer(CertUtil.SelectCertificate()))
            {
                if (parametros.SignaturePackaging != SignaturePackaging.EXTERNALLY_DETACHED)
                    using (var fs = new FileStream(txtFichero.Text, FileMode.Open))
                    {
                        _signatureDocument = _xadesService.Sign(fs, parametros);
                    }
                else
                    _signatureDocument = _xadesService.Sign(null, parametros);
            }

            MessageBox.Show("Signature completed, you can now Save the signature or extend it to Xades-T.",
                "Test signature",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        private void btnCoFirmar_Click(object sender, EventArgs e)
        {
            var parametros = ObtenerParametrosFirma();

            using (parametros.Signer = new Signer(CertUtil.SelectCertificate()))
            {
                _signatureDocument = _xadesService.CoSign(_signatureDocument, parametros);
            }

            MessageBox.Show("Signature completed correctly.", "Test XADES signature",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void AmpliarFirma(SignatureFormat formato)
        {
            try
            {
                var parametros = new UpgradeParameters {TimeStampClient = new TimeStampClient(txtURLSellado.Text)};

                parametros.OCSPServers.Add(txtOCSP.Text);

                var upgrader = new XadesUpgraderService();
                upgrader.Upgrade(_signatureDocument, formato, parametros);

                MessageBox.Show("Signature successfully expanded", "Test XADES signature",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error has occurred during extending the signature: " + ex.Message);
            }
        }

        private void btnXadesT_Click(object sender, EventArgs e)
        {
            AmpliarFirma(SignatureFormat.XAdES_T);
        }

        private void btnXadesXL_Click(object sender, EventArgs e)
        {
            AmpliarFirma(SignatureFormat.XAdES_XL);
        }

        private void GuardarFirma()
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                _signatureDocument.Save(saveFileDialog1.FileName);

                MessageBox.Show("Signature saved correctly.");
            }
        }

        private void btnGuardarFirma_Click(object sender, EventArgs e)
        {
            GuardarFirma();
        }

        private void btnCargarFirma_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                using (var fs = new FileStream(openFileDialog1.FileName, FileMode.Open))
                {
                    var firmas = _xadesService.Load(fs);

                    var frm = new FrmSeleccionarFirma(firmas);

                    if (frm.ShowDialog() == DialogResult.OK)
                    {
                        _signatureDocument = frm.FirmaSeleccionada;

                        if (!_xadesService.Validate(_signatureDocument).IsValid) MessageBox.Show("INVALID SIGNATURE");
                    }
                    else
                    {
                        MessageBox.Show("You must select a signature.");
                    }
                }
        }

        private void btnContraFirma_Click(object sender, EventArgs e)
        {
            var parametros = ObtenerParametrosFirma();

            using (parametros.Signer = new Signer(CertUtil.SelectCertificate()))
            {
                _signatureDocument = _xadesService.CounterSign(_signatureDocument, parametros);
            }

            MessageBox.Show("Signature completed correctly.", "Test XADES signature.",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}