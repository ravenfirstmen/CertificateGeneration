﻿using System;
using System.Windows.Forms;
using CertificateManager.Services;

namespace CertificateManager.Views
{
    public interface ICACreateViewForm : ICertificateCreateView
    {
        string CrlUri { get; }
        string OcspUri { get; }
    }

    public class CACreateFormPresenter : CertificateCreatePresenter<ICACreateViewForm>
    {
        public override void OnViewReady()
        {
            base.OnViewReady();

            View.AvailableKeyUsage = KeyUsageUtils.All;
            View.DefaultKeyUsage = Constants.DefaulCAKeyUsageMask;
            View.AvailablePurpose = KeyPurposeUtils.All;
            View.DefaultPurpose = Constants.DefaultCAExtendUsages;
        }

        public bool Generate()
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter =
                    @"X.509 Certificate files (*.cer;*.crt;*.cert;*.pem)|*.cer;*.crt;*.cert;*.pem|Pem files (*.pem)|*.pem|Crt files (*.crt)|*.crt|All files (*.*)|*.*";
                dlg.FilterIndex = 0;
                dlg.RestoreDirectory = true;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    var certificateGenerator = new DefaultCAGenerator(new DefaultUrlEvaluator());

                    DateTime beginDate = View.From;
                    DateTime endDate = beginDate.AddDays(View.Duration);
                    CertificateResult result = certificateGenerator.Create(
                        View.Algoritm,
                        View.KeyLength,
                        View.Subject,
                        beginDate,
                        endDate,
                        View.KeyUsage,
                        View.Purpose,
                        new CertificateEndPoints
                        {
                            CrlDistributionEndPoint = View.CrlUri,
                            OcspEndPoint = View.OcspUri
                        }
                        );

                    ResultFileNames files = IOPEMUtils.SaveCertificateAndPrivateKey(result.Certificate, result.KeyPair,
                        dlg.FileName);

                    MessageBox.Show(
                        String.Format("Foram gravados os ficheiros:\nCertificado como '{0}'\nChave privada como '{1}'",
                            files.Certificate,
                            files.PrivateKey), @"Informação",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                        );

                    return true;
                }
            }

            return false;
        }
    }
}