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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FirmaXadesNet;
using System.IO;

namespace TestFirmaXades
{
    public partial class FrmPrincipal : Form
    {
        FirmaXades _firmaXades = new FirmaXades();
        
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

        private void btnFirmar_Click(object sender, EventArgs e)
        {

            _firmaXades.PolicyIdentifier = txtIdentificadorPolitica.Text;
            _firmaXades.PolicyHash = txtHashPolitica.Text;
            _firmaXades.PolicyUri = txtURIPolitica.Text;

            if (string.IsNullOrEmpty(txtFichero.Text))
            {
                MessageBox.Show("Debe seleccionar un fichero para firmar.");
                return;
            }
            
            if (rbInternnallyDetached.Checked)
            {
                // TODO: gestionar correctamente los tipos MIME
                string mimeType = "application/" + 
                    System.IO.Path.GetExtension(txtFichero.Text).ToLower().Replace(".", "");

                _firmaXades.SetContentInternallyDetached(txtFichero.Text, mimeType);
            }
            else if (rbExternallyDetached.Checked)
            {
                _firmaXades.SetContentExternallyDetached(txtFichero.Text);
            }
            else if (rbEnveloped.Checked)
            {
                _firmaXades.SetContentEnveloped(txtFichero.Text);
            }

            _firmaXades.Sign(_firmaXades.SelectCertificate());

            MessageBox.Show("Firma completada, ahora puede Guardar la firma o ampliarla a Xades-T.", "Test firma XADES", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        private void btnCoFirmar_Click(object sender, EventArgs e)
        {
            _firmaXades.PolicyIdentifier = txtIdentificadorPolitica.Text;
            _firmaXades.PolicyHash = txtHashPolitica.Text;
            _firmaXades.PolicyUri = txtURIPolitica.Text;

            _firmaXades.CoSign(_firmaXades.SelectCertificate());

            MessageBox.Show("Firma completada correctamente.", "Test firma XADES",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnXadesT_Click(object sender, EventArgs e)
        {
            try
            {
                _firmaXades.TSAServer = txtURLSellado.Text;

                _firmaXades.UpgradeToXadesT();

                MessageBox.Show("Sello de tiempo aplicado correctamente.\nAhora puede Guardar la firma o ampliarla a Xades-XL", "Test firma XADES", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ha ocurrido un error ampliando la firma: " + ex.Message);
            }
        }

        private void btnXadesXL_Click(object sender, EventArgs e)
        {
            try
            {
                _firmaXades.TSAServer = txtURLSellado.Text;

                _firmaXades.AddOCSPServer(txtOCSP.Text);

                _firmaXades.UpgradeToXadesXL();

                MessageBox.Show("Firma ampliada correctamente a XADES-XL.", "Test firma XADES", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ha ocurrido un error ampliando la firma: " + ex.Message);
            }
        }

        private void GuardarFirma()
        {
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _firmaXades.Save(saveFileDialog1.FileName);

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
                    var firmas = FirmaXades.Load(fs);

                    FrmSeleccionarFirma frm = new FrmSeleccionarFirma(firmas);

                    if (frm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        _firmaXades = frm.FirmaSeleccionada;
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
            _firmaXades.PolicyIdentifier = txtIdentificadorPolitica.Text;
            _firmaXades.PolicyHash = txtHashPolitica.Text;
            _firmaXades.PolicyUri = txtURIPolitica.Text;

            _firmaXades.CounterSign(_firmaXades.SelectCertificate());

            MessageBox.Show("Firma completada correctamente.", "Test firma XADES",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

    }
}
