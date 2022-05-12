﻿/* Andrea Powell, Spring 2022
 * Capstone project
 * 
 * ToDo:
 *    Use fields from the Record object to generate data grid, instead of addig to .net generated columns.
 *    
 *    Think about pdfDocumentID and pdfInstanceID.  Do I want to search for other instances of the same document?  
 *    Is the new destiation document a different documentID?
 *    
 *    This deseperatly needs to be seperated into class objects
 * 
 * *** Multi-Select changes not yet working
 *    Look at itext7 GetXmpMetadata(bool createNew)
 *    
* The below line works - will use this when updating to use XMP object rather than GetInfo[array].
*    byte[] targetByte = sourceDocument.GetXmpMetadata();
*    

*/

using iText.Kernel.Pdf;
using iText.Kernel.XMP;
using Metadata_Manager.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Packaging;
// needed to target v 5.0.0 of System.IO.Packaging to make DocumentFormat work ?
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Color = System.Drawing.Color;

namespace Metadata_Manager.Forms
{
	public partial class Landing : Form
	{

		private PdfRecord Record;


		public Landing()
		{
			InitializeComponent();

			/// Update this with data binding
			Dictionary<string, Int16> RecordList = new Dictionary<string, Int16>();
			RecordList.Add("Selected", 0);
			RecordList.Add("Details", 1);
			RecordList.Add("FileName", 2);
			RecordList.Add("Title", 3);
			RecordList.Add("Author", 4);
			RecordList.Add("Published", 5);
			RecordList.Add("RecordSeries", 6);
			RecordList.Add("FilePath", 7);
		}

		private void openPdfsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			PdfDocument sourceDocument;
			PdfDocumentInfo sourceInfo;
			List<Record> recordList = new List<Record>();


			dataGridMain.Rows.Clear();

			if (openPdfFile.ShowDialog() == DialogResult.OK)
			{
				// enable Output Window
				ReportMenuItem.Enabled = true;

				recordList = new List<Record>();



				int count = 0;

				foreach (string File in openPdfFile.FileNames)
				{
					Record = new();
					// Open Dialog filters out non-PDF files

					Record.FilePath = openPdfFile.FileNames[count];
					Record.FileName = openPdfFile.SafeFileNames[count];

					// move all this stuff to PdfRecordClass ?
					sourceDocument = new PdfDocument(new PdfReader(Record.FilePath));
					sourceInfo = sourceDocument.GetDocumentInfo();

					Record.Title = testVoid(sourceInfo.GetTitle());
					Record.Author = testVoid(sourceInfo.GetAuthor());
					Record.Published = testVoid(sourceInfo.GetMoreInfo("Published"));
					Record.RecordSeries = testVoid(sourceInfo.GetMoreInfo("RecordSeries"));

					// Fix databinding
					recordList.Add(Record);
					dataGridMain.Rows.Add(false, "...", Record.FileName, Record.Title, Record.Author, Record.Published, Record.RecordSeries, Record.FilePath);
					sourceDocument.Close();
					count++;
				}
				dataGridMain.Refresh();
				dataGridMain.Show();
			}
		}

		private string testVoid(string _value)
		{
			// git rid of this when databound control updated
			if (_value == null || _value.Length == 0) { return " "; }
			return _value;
		}

		private void menuItemExit_Click(object sender, EventArgs e)
		{
			// Need to add verification step to make sure all changes have been saved.
			Close();
		}

		private void dataGridMain_CellClick(object sender, DataGridViewCellEventArgs e)
		{
			// If column clicked is the detail link, open document in Browser
			if (e.ColumnIndex == 1)
			{
				string _filePath = dataGridMain.Rows[e.RowIndex].Cells["FilePath"].Value.ToString();

				Record.ShowPdfInBrowser(_filePath);
			}
		}

		private void dataGridMain_RowValidating(object sender, DataGridViewCellCancelEventArgs e)
		{

			PdfDocument sourceDocument;
			PdfDocument targetDocument;
			PdfDocumentInfo targetInfo;

			Record Record = new();
			// Locate file to change and reopen for writing
			Record.FileName = dataGridMain.CurrentRow.Cells["FileName"].Value.ToString();
			Record.FilePath = dataGridMain.CurrentRow.Cells["FilePath"].Value.ToString();

			sourceDocument = new PdfDocument(new PdfReader(Record.FilePath));


			targetDocument = new PdfDocument(new PdfWriter("./Test" + Record.FileName + ".pdf"));   // Check this - are all of the original properties going too? ** Fix This to duplicate whole file
			sourceDocument.CopyPagesTo(1, sourceDocument.GetNumberOfPages(), targetDocument);
			targetInfo = sourceDocument.GetDocumentInfo();

			Record.Title = dataGridMain.CurrentRow.Cells["Title"].Value.ToString();
			Record.Author = dataGridMain.CurrentRow.Cells["Author"].Value.ToString();
			Record.Published = dataGridMain.CurrentRow.Cells["Published"].Value.ToString();
			Record.RecordSeries = dataGridMain.CurrentRow.Cells["RecordSeries"].Value.ToString();

			/*
			* Do I need the Standard keyword?
			* 			targetDocument.GetDocumentInfo().SetTitle(Record.Title + " standard");
			*		targetInfo.SetAuthor(Record.Author + "standard");
			*/
			targetDocument.GetDocumentInfo().SetTitle(Record.Title);
			targetInfo.SetAuthor(Record.Author);


			// Dublin Core namespace
			targetDocument.GetDocumentInfo().SetAuthor(Record.Author);  // dc:creator
			targetDocument.GetDocumentInfo().SetTitle(Record.Title);    //dc:title

			//  Adobe pdfx namespace
			targetDocument.GetDocumentInfo().SetMoreInfo("RecordSeries", Record.RecordSeries);
			targetDocument.GetDocumentInfo().SetMoreInfo("Published", Record.Published);


			targetDocument.Close();
			sourceDocument.Close();
			dataGridMain.Refresh();
		}

		private void dataGridMain_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			if (dataGridMain.CurrentCell != null)
			{
				dataGridMain.CurrentCell.Style.BackColor = Color.PeachPuff;
				dataGridMain.Refresh();
			}
		}


		private void TurnGreen(object sender, DataGridViewCellEventArgs e)
		{
			// Got to be an easier way to do this at the row level;
			foreach (var cell in dataGridMain.CurrentRow.Cells)
			{
				dataGridMain.CurrentCell.Style.BackColor = Color.Green;
				dataGridMain.Refresh();
			}
		}

		private void menuMain_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{

		}

		private void exportTocsvToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// Export grid data to .csv
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.Filter = "CSV (*.csv) |*.csv| Text (*.txt)|*.txt | Excel (*.xlsx) | *.xlsx";


			if (saveFileDialog.ShowDialog() == DialogResult.OK)
			{
				TextWriter writer = new StreamWriter(saveFileDialog.FileName);
				// Header Cells
				writer.Write("File Name, ");
				writer.Write("Title, ");
				writer.Write("Author, ");
				writer.Write("Published");
				writer.Write("Record Series, ");
				writer.Write("File Path, ");
				writer.WriteLine("");

				foreach (DataGridViewRow row in dataGridMain.Rows)
				{
					writer.Write(row.Cells["FileName"].Value.ToString() + ",");
					writer.Write(row.Cells["Title"].Value.ToString() + ",");
					writer.Write(row.Cells["Author"].Value.ToString() + ",");
					writer.Write(row.Cells["Published"].Value.ToString() + ",");
					writer.Write(row.Cells["RecordSeries"].Value.ToString() + ",");
					writer.Write(row.Cells["FilePath"].Value.ToString() + ",");
					writer.WriteLine("");
				}
				writer.Close();
				MessageBox.Show("File Created");
			}

		}

		// Horray for Spaghetti Code
		public string EscapeComma(string _value)
		{
			return _value.Replace(",", "-");

		}

		private void ExportData(object sender, EventArgs e)
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.Filter = "CSV (*.csv) |*.csv| Text (*.txt)|*.txt | Excel (*.xlsx) | *.xlsx";

			if (saveFileDialog.ShowDialog() == DialogResult.OK)
			{
				// get extention used
				string fileName = saveFileDialog.FileName;
				string ext = fileName.Substring(fileName.Length - 3);
				TextWriter writer = new StreamWriter(saveFileDialog.FileName);
				//StreamWriter writer2 = new StreamWriter(saveFileDialog.FileName);

				switch (ext)
				{
					// this code is ugly - do something - override ToString?  Probably fixed with databinding setup (agh)
					case "csv":
						// Header Cells
						writer.Write("File Name, ");
						writer.Write("Title, ");
						writer.Write("Author, ");
						writer.Write("Published, ");
						writer.Write("Record Series, ");
						writer.Write("File Path,");
						writer.WriteLine("");

						foreach (DataGridViewRow row in dataGridMain.Rows)
						{
							writer.Write(row.Cells["FileName"].Value.ToString().Replace(",", "-") + ",");
							writer.Write(row.Cells["Title"].Value.ToString().Replace(",", "-") + ",");
							writer.Write(row.Cells["Author"].Value.ToString().Replace(",", "-") + ",");
							writer.Write(row.Cells["Published"].FormattedValue.ToString().Replace(",", "-") + ",");
							writer.Write(row.Cells["RecordSeries"].Value.ToString().Replace(",", "-") + ",");
							writer.Write(row.Cells["FilePath"].Value.ToString().Replace(",", "-") + ",");
							writer.WriteLine("");
						}

						break;

					case "txt":
						// Header Cells
						writer.Write("File Name, ");
						writer.Write("Title, ");
						writer.Write("Author, ");
						writer.Write("Published, ");
						writer.Write("Record Series, ");
						writer.Write("File Path,");
						writer.WriteLine("");

						foreach (DataGridViewRow row in dataGridMain.Rows)
						{
							writer.Write(row.Cells["FileName"].Value.ToString().Replace(",", "-") + ",");
							writer.Write(row.Cells["Title"].Value.ToString().Replace(",", "-") + ",");
							writer.Write(row.Cells["Author"].Value.ToString().Replace(",", "-") + ",");
							writer.Write(row.Cells["Published"].FormattedValue.ToString().Replace(",", "-") + ",");
							writer.Write(row.Cells["RecordSeries"].Value.ToString().Replace(",", "-") + ",");
							writer.Write(row.Cells["FilePath"].Value.ToString().Replace(",", "-") + ",");
							writer.WriteLine("");
						}

						break; ;

					case "xlsx":
						DataTable dt = new DataTable();
						dt.Columns.Add("File Name");
						dt.Columns.Add("Title");
						dt.Columns.Add("Author");
						dt.Columns.Add("Published");
						dt.Columns.Add("Record Series");
						dt.Columns.Add("File Path");

						foreach (DataGridViewRow row in dataGridMain.Rows)
						{
							dt.Rows.Add(row.Cells["FileName"].Value.ToString());
							dt.Rows.Add(row.Cells["Title"].Value.ToString());
							dt.Rows.Add(row.Cells["Author"].Value.ToString());
							dt.Rows.Add(row.Cells["Published"].Value.ToString());
							dt.Rows.Add(row.Cells["Record Series"].Value.ToString());
							dt.Rows.Add(row.Cells["File Path"].Value.ToString());
						}
						break;
				};

				writer.Close();
				MessageBox.Show("File Created");
			}
		}
	}
}