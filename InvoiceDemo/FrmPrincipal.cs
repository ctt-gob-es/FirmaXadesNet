#region

using System;
using System.IO;
using System.Windows.Forms;
using XadesNet;
using XadesNet.Crypto;
using XadesNet.Signature.Parameters;
using XadesNet.Utils;

#endregion

namespace DemoFacturae
{
    public partial class FrmPrincipal : Form
    {
        public FrmPrincipal()
        {
            InitializeComponent();
        }

        private void btnGenerar_Click(object sender, EventArgs e)
        {
            var xadesService = new XadesService();
            var parametros = new SignatureParameters();

            var ficheroFactura = Application.StartupPath + "\\Facturae.xml";

            // E-invoice signature policy 3.1
            parametros.SignaturePolicyInfo = new SignaturePolicyInfo
            {
                PolicyIdentifier =
                    "http://www.facturae.es/politica_de_firma_formato_facturae/politica_de_firma_formato_facturae_v3_1.pdf",
                PolicyHash = "Ohixl6upD6av8N7pEvDABhEL6hM="
            };
            parametros.SignaturePackaging = SignaturePackaging.ENVELOPED;
            parametros.InputMimeType = "text/xml";
            parametros.SignerRole = new SignerRole();
            parametros.SignerRole.ClaimedRoles.Add("issuer");

            using (parametros.Signer = new Signer(CertUtil.SelectCertificate()))
            {
                using (var fs = new FileStream(ficheroFactura, FileMode.Open))
                {
                    var docFirmado = xadesService.Sign(fs, parametros);

                    if (saveFileDialog1.ShowDialog() != DialogResult.OK) return;
                    docFirmado.Save(saveFileDialog1.FileName);
                    MessageBox.Show("File saved correctly.");
                }
            }
        }
    }
}