// --------------------------------------------------------------------------------------------------------------------
// FrmPrincipal.cs
//
// FirmaXadesNet - Librería la para generación de firmas XADES
// Copyright (C) 2014 Dpto. de Nuevas Tecnologías de la Concejalía de Urbanismo de Cartagena
//
// This program is free software: you can redistribute it and/or modify
// it under the +terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see http://www.gnu.org/licenses/. 
//
// Contact info: J. Arturo Aguado
// Email: informatica@gemuc.es
// 
// --------------------------------------------------------------------------------------------------------------------

using FirmaXadesNet;
using FirmaXadesNet.Clients;
using FirmaXadesNet.Crypto;
using FirmaXadesNet.Signature;
using FirmaXadesNet.Signature.Parameters;
using FirmaXadesNet.Upgraders;
using FirmaXadesNet.Upgraders.Parameters;
using FirmaXadesNet.Utils;
using System;
using System.IO;
using System.Windows.Forms;

namespace TestFirmaXades
{
    public partial class FrmPrincipal : Form
    {
        XadesService _xadesService = new XadesService();
        SignatureDocument _signatureDocument;
        
        public FrmPrincipal()
        {
            InitializeComponent();
        }
        
        private void btnSeleccionarFichero_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtFichero.Text = openFileDialog1.FileName;
            }
        }

        private SignaturePolicyInfo ObtenerPolitica()
        {
            SignaturePolicyInfo spi = new SignaturePolicyInfo();

            spi.PolicyIdentifier = txtIdentificadorPolitica.Text;
            spi.PolicyHash = txtHashPolitica.Text;
            spi.PolicyUri = txtURIPolitica.Text;

            return spi;
        }

        private SignatureParameters ObtenerParametrosFirma()
        {
            SignatureParameters parametros = new SignatureParameters();            
            parametros.SignatureMethod = SignatureMethod.RSAwithSHA256;
            parametros.SigningDate = DateTime.Now;

            return parametros;
        }

        private void btnFirmar_Click(object sender, EventArgs e)
        {

            if (string.IsNullOrEmpty(txtFichero.Text))
            {
                MessageBox.Show("Debe seleccionar un fichero para firmar.");
                return;
            }

            SignatureParameters parametros = ObtenerParametrosFirma();
            
            if (rbInternnallyDetached.Checked)
            {
                // TODO: gestionar correctamente los tipos MIME
                string mimeType = "application/" + 
                    System.IO.Path.GetExtension(txtFichero.Text).ToLower().Replace(".", "");

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

            using(parametros.Signer = new Signer(CertUtil.SelectCertificate()))
            {
                if (parametros.SignaturePackaging != SignaturePackaging.EXTERNALLY_DETACHED)
                {
                    using (FileStream fs = new FileStream(txtFichero.Text, FileMode.Open))
                    {
                        _signatureDocument = _xadesService.Sign(fs, parametros);
                    }
                }
                else
                {
                    _signatureDocument = _xadesService.Sign(null, parametros);
                }
            }

            MessageBox.Show("Firma completada, ahora puede Guardar la firma o ampliarla a Xades-T.", "Test firma XADES", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        private void btnCoFirmar_Click(object sender, EventArgs e)
        {
            SignatureParameters parametros = ObtenerParametrosFirma();

            using (parametros.Signer = new Signer(CertUtil.SelectCertificate()))
            {
                _signatureDocument = _xadesService.CoSign(_signatureDocument, parametros);
            }

            MessageBox.Show("Firma completada correctamente.", "Test firma XADES",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void AmpliarFirma(SignatureFormat formato)
        {
            try
            {
                UpgradeParameters parametros = new UpgradeParameters();

                parametros.TimeStampClient = new TimeStampClient(txtURLSellado.Text);
                parametros.OCSPServers.Add(txtOCSP.Text);

                XadesUpgraderService upgrader = new XadesUpgraderService();
                upgrader.Upgrade(_signatureDocument, formato, parametros);

                MessageBox.Show("Firma ampliada correctamente", "Test firma XADES",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ha ocurrido un error ampliando la firma: " + ex.Message);
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
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _signatureDocument.Save(saveFileDialog1.FileName);

                MessageBox.Show("Firma guardada correctamente.");
            }
        }

        private void btnGuardarFirma_Click(object sender, EventArgs e)
        {
            GuardarFirma();
        }

        private void btnCargarFirma_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (FileStream fs = new FileStream(openFileDialog1.FileName, FileMode.Open))
                {
                    var firmas = _xadesService.Load(fs);

                    FrmSeleccionarFirma frm = new FrmSeleccionarFirma(firmas);

                    if (frm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        _signatureDocument = frm.FirmaSeleccionada;

                        if (!_xadesService.Validate(_signatureDocument).IsValid)
                        {
                            MessageBox.Show("FIRMA NO VÁLIDA");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Debe seleccionar una firma.");
                    }
                }
            }
        }

        private void btnContraFirma_Click(object sender, EventArgs e)
        {
            SignatureParameters parametros = ObtenerParametrosFirma();

            using (parametros.Signer = new Signer(CertUtil.SelectCertificate()))
            {
                _signatureDocument = _xadesService.CounterSign(_signatureDocument, parametros);
            }

            MessageBox.Show("Firma completada correctamente.", "Test firma XADES",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

    }
}
