// ============================================================
// BluePrismPdfHelper.cs
// PdfPig wrapper for use inside Blue Prism 7.4.x
// Version:      1.0.0
// Target:       net462
// Dependencies: UglyToad.PdfPig 0.1.9
//
// This is the canonical source. The build script
// (build/3_compile_pdf_helper.ps1) compiles this exact file into
// BluePrismPdfHelper.dll. Do not keep a second copy of this code.
// ============================================================

using System;
using System.Text;
using System.Data;
using System.Linq;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace BluePrismPdfHelper
{
    public class PdfHelper
    {
        /// <summary>
        /// Version of the loaded DLL. Useful to confirm which build is deployed.
        /// </summary>
        public static string Version => "1.0.0";

        /// <summary>
        /// Returns the total number of pages in the PDF.
        /// </summary>
        public static int GetPageCount(string pdfPath)
        {
            using (var doc = PdfDocument.Open(pdfPath))
            {
                return doc.NumberOfPages;
            }
        }

        /// <summary>
        /// Extracts all text from the PDF, page by page.
        /// </summary>
        public static string GetAllText(string pdfPath)
        {
            var sb = new StringBuilder();
            using (var doc = PdfDocument.Open(pdfPath))
            {
                foreach (var page in doc.GetPages())
                {
                    sb.AppendLine(string.Join(" ", page.GetWords().Select(w => w.Text)));
                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Extracts the text of a specific page (1-based).
        /// </summary>
        public static string GetPageText(string pdfPath, int pageNumber)
        {
            using (var doc = PdfDocument.Open(pdfPath))
            {
                var page = doc.GetPage(pageNumber);
                return string.Join(" ", page.GetWords().Select(w => w.Text));
            }
        }

        /// <summary>
        /// Returns every page as a DataTable with PageNumber and PageText columns.
        /// </summary>
        public static DataTable GetAllPagesAsTable(string pdfPath)
        {
            var dt = new DataTable();
            dt.Columns.Add("PageNumber", typeof(int));
            dt.Columns.Add("PageText", typeof(string));

            using (var doc = PdfDocument.Open(pdfPath))
            {
                foreach (var page in doc.GetPages())
                {
                    var text = string.Join(" ", page.GetWords().Select(w => w.Text));
                    var row = dt.NewRow();
                    row["PageNumber"] = page.Number;
                    row["PageText"] = text;
                    dt.Rows.Add(row);
                }
            }
            return dt;
        }

        /// <summary>
        /// Searches for a keyword and returns the pages where it was found, with context.
        /// Case-insensitive match.
        /// </summary>
        public static DataTable SearchText(string pdfPath, string keyword)
        {
            var dt = new DataTable();
            dt.Columns.Add("PageNumber", typeof(int));
            dt.Columns.Add("Context", typeof(string));

            using (var doc = PdfDocument.Open(pdfPath))
            {
                foreach (var page in doc.GetPages())
                {
                    var text = string.Join(" ", page.GetWords().Select(w => w.Text));
                    if (text.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        var row = dt.NewRow();
                        row["PageNumber"] = page.Number;
                        row["Context"] = text;
                        dt.Rows.Add(row);
                    }
                }
            }
            return dt;
        }

        /// <summary>
        /// Extracts text from a specific region of a page using coordinates (in points).
        /// Use GetWordsWithCoordinates first to discover the coordinates.
        /// </summary>
        public static string GetTextByRegion(string pdfPath, int pageNumber, double x, double y, double width, double height)
        {
            using (var doc = PdfDocument.Open(pdfPath))
            {
                var page = doc.GetPage(pageNumber);
                var words = page.GetWords()
                    .Where(w =>
                        w.BoundingBox.Left >= x &&
                        w.BoundingBox.Bottom >= y &&
                        w.BoundingBox.Right <= (x + width) &&
                        w.BoundingBox.Top <= (y + height))
                    .Select(w => w.Text);
                return string.Join(" ", words);
            }
        }

        /// <summary>
        /// Returns every word on a page together with its coordinates.
        /// Useful to map the layout and feed GetTextByRegion.
        /// </summary>
        public static DataTable GetWordsWithCoordinates(string pdfPath, int pageNumber)
        {
            var dt = new DataTable();
            dt.Columns.Add("Word", typeof(string));
            dt.Columns.Add("X", typeof(double));
            dt.Columns.Add("Y", typeof(double));
            dt.Columns.Add("Width", typeof(double));
            dt.Columns.Add("Height", typeof(double));

            using (var doc = PdfDocument.Open(pdfPath))
            {
                var page = doc.GetPage(pageNumber);
                foreach (var word in page.GetWords())
                {
                    var row = dt.NewRow();
                    row["Word"] = word.Text;
                    row["X"] = Math.Round(word.BoundingBox.Left, 2);
                    row["Y"] = Math.Round(word.BoundingBox.Bottom, 2);
                    row["Width"] = Math.Round(word.BoundingBox.Width, 2);
                    row["Height"] = Math.Round(word.BoundingBox.Height, 2);
                    dt.Rows.Add(row);
                }
            }
            return dt;
        }
    }
}
