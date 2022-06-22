using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

// to read/convert Pdf files
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Parsing;

// for theme
using Syncfusion.SfSkinManager;
using Syncfusion.Themes.FluentLight.WPF;

// to start processes
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;

// to work with images
using System.Drawing;

// to save and read files
using System.IO;
using System.Media;
using Microsoft.Win32;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocToPDFConverter;

namespace WpfPdfConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            // Register Syncfusion license
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NjM3ODE3QDMyMzAyZTMxMmUzMFExSGo4aXNrU2RxSDFWZk1xRDQycFNJRG9JUWViWGZ2Y2RjUUp0N282U3c9");

            FluentTheme fluentTheme = new()
            {
                ThemeName = "FluentLight",
                HoverEffectMode = HoverEffect.None,
                PressedEffectMode = PressedEffect.Glow,
                ShowAcrylicBackground = false
            };

            FluentLightThemeSettings themeSettings = new()
            {
                BodyFontSize = 16,
                FontFamily = new System.Windows.Media.FontFamily("Barlow")
            };

            SfSkinManager.RegisterThemeSettings("FluentLight", themeSettings);
            SfSkinManager.SetTheme(this, fluentTheme);

            InitializeComponent();
        }

        private void ButtonConvert_OnClick(object sender, RoutedEventArgs e)
        {
            var inputFilePath = TextBoxFilePath.Text;

            if (inputFilePath == String.Empty)
            {
                MessageBox.Show("Please choose a file first.");
                return;
            }


            switch (ComboBoxConversionType.SelectedIndex)
            {
                case 0: // Convert Doc to PDF
                    ConvertDocToPdf(inputFilePath);
                    break;

                case 1: // Convert PDF to Doc
                    ConvertPdfToDoc(inputFilePath);
                    break;

                case 2: // Convert PNG to PDF
                    ConvertPngToPdf(inputFilePath);
                    break;

                default:
                    MessageBox.Show("Please select an conversion option first.");
                    return;
            }

            OpenFolder(inputFilePath);
        }

        private void ConvertDocToPdf(string filePath)
        {
            WordDocument wordDocument = new(filePath, FormatType.Automatic);
            DocToPDFConverter converter = new();
            PdfDocument pdfDocument = converter.ConvertToPDF(wordDocument);

            string savePath = filePath.Split('.')[0] + ".pdf";
            pdfDocument.Save(savePath);
            pdfDocument.Close(true);
            wordDocument.Close();
        }

        private void ConvertPdfToDoc(string filePath)
        {
            WordDocument wordDocument = new();
            IWSection section = wordDocument.AddSection();
            section.PageSetup.Margins.All = 0;
            IWParagraph firstParagraph = section.AddParagraph();

            SizeF defaultPageSize = new SizeF(wordDocument.LastSection.PageSetup.PageSize.Width, wordDocument.LastSection.PageSetup.PageSize.Height);

            using PdfLoadedDocument loadedDocument = new PdfLoadedDocument(filePath);
            for (var i = 0; i < loadedDocument.Pages.Count; i++)
            {
                using (var image = loadedDocument.ExportAsImage(i, defaultPageSize, false))
                {
                    IWPicture picture = firstParagraph.AppendPicture(image);
                    picture.Width = image.Width;
                    picture.Height = image.Height;
                }

            }

            string savePath = filePath.Split('.')[0] + ".docx";
            wordDocument.Save(savePath);
            wordDocument.Close();
        }

        private void ConvertPngToPdf(string filePath)
        {
            PdfDocument pdfDocument = new();
            PdfImage pdfImage = PdfImage.FromStream(new FileStream(filePath, FileMode.Open));
            PdfPage pdfPage = new();

            PdfSection pdfSection = pdfDocument.Sections.Add();
            pdfSection.Pages.Insert(0, pdfPage);
            pdfPage.Graphics.DrawImage(pdfImage, 0, 0);

            string savePath = filePath.Split('.')[0] + ".png";
            pdfDocument.Save(savePath);
            pdfDocument.Close(true);
        }

        private void ButtonChooseFile_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new();
            bool? result = openFileDialog.ShowDialog();

            if (result.HasValue && result.Value)
            {
                TextBoxFilePath.Text = openFileDialog.FileName;
            }
        }

        private void OpenFolder(string folderPath)
        {
            ProcessStartInfo startInfo = new()
            {
                Arguments = folderPath.Substring(0, folderPath.LastIndexOf('\\')),
                FileName = "explorer.exe"
            };
            Process.Start(startInfo);
        }
    }
}
