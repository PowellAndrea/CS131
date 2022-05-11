﻿/* Andrea Powell, Spring 2022
 * Capstone project
 * 
 * ToDo:
 *    Use fields from the Record object to generate data grid, instead of addig to .net generated columns.
 *    
 *    Think about pdfDocumentID and pdfInstanceID.  Do I want to search for other instances of the same document?  
 *    Is the new destiation document a different documentID?
 * 
 * *** Multi-Select changes not yet working
 * *** Add validation back or new info is never written?
 * 
 * Define here or in class?  Will be null until the new pdfDocument object is instanciated. 
 *    PdfWriter destinationWriter;
 *    PdfReader sourceReader;
 * 
 *    Look at itext7 GetXmpMetadata(bool createNew)
 *    
 *    Set font if dirty & update when written
 *    Don't update again on exit unless there are changes
 *    Where is my pencil for editing?
 *    Grey out read-only fields
 *    

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

namespace Metadata_Manager.Forms
{
   public partial class Landing : Form
   {

        // Move these into the object class
      //private Record[] arrRecords;

      private PdfRecord Record;

      public Landing()
      {
         InitializeComponent();
      }

      private void openPdfsToolStripMenuItem_Click(object sender, EventArgs e)
      {
         PdfDocument sourceDocument;
         PdfDocumentInfo sourceInfo;

         dataGridMain.Rows.Clear();

          if (openPdfFile.ShowDialog() == DialogResult.OK)
          {
            Record = new ();
            int count = 0;

            foreach (string File in openPdfFile.FileNames)
            {
               // Open Dialog filters out non-PDF files

               Record.FilePath = openPdfFile.FileNames[count];
               Record.FileName = openPdfFile.SafeFileNames[count];

               // fix to use a single reader?
               // move all this stuff to PdfRecordClass
               sourceDocument = new PdfDocument(new PdfReader(Record.FilePath));
               sourceInfo = sourceDocument.GetDocumentInfo();

               Record.Title = testVoid(sourceInfo.GetTitle());
               Record.Author = testVoid(sourceInfo.GetAuthor());
               Record.Published = testVoid(sourceInfo.GetMoreInfo("Published"));
               Record.RecordSeries = testVoid(sourceInfo.GetMoreInfo("RecordSeries"));

               dataGridMain.Rows.Add("...", Record.FileName, Record.Title, Record.Author, Record.Published, Record.RecordSeries, Record.FilePath);
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
         if (e.ColumnIndex == 0) {
            string _filePath = dataGridMain.Rows[e.RowIndex].Cells[6].Value.ToString();

            Record.ShowPdfInBrowser(_filePath); }
      }


      private void dataGridMain_RowValidating(object sender, DataGridViewCellCancelEventArgs e)
      {  
         PdfDocument    sourceDocument;
         PdfDocument    targetDocument;
         PdfDocumentInfo targetInfo;

			Record Record = new();
		 // Locate file to change and reopen for writing
			Record.FileName = dataGridMain.CurrentRow.Cells[1].Value.ToString();
         Record.FilePath = dataGridMain.CurrentRow.Cells[6].Value.ToString();

         sourceDocument = new PdfDocument(new PdfReader(Record.FilePath));


         targetDocument = new PdfDocument(new PdfWriter("./Test" + Record.FileName + ".pdf"));	// Check this - are all of the original properties going too? ** Fix This to duplicate whole file
         sourceDocument.CopyPagesTo(1, sourceDocument.GetNumberOfPages(), targetDocument);
         targetInfo = sourceDocument.GetDocumentInfo();

         Record.Title = dataGridMain.CurrentRow.Cells[2].Value.ToString();
         Record.Author = dataGridMain.CurrentRow.Cells[3].Value.ToString();
         Record.Published = dataGridMain.CurrentRow.Cells[4].Value.ToString();
         Record.RecordSeries = dataGridMain.CurrentRow.Cells[5].Value.ToString();

         targetDocument.GetDocumentInfo().SetTitle(Record.Title + " standard");
         targetInfo.SetAuthor(Record.Author + "standard");

			// Dublin Core namespace
         targetDocument.GetDocumentInfo().SetAuthor(Record.Author);  // dc:creator
         targetDocument.GetDocumentInfo().SetTitle(Record.Title);    //dc:title

         //  Adobe pdfx namespac`e
         targetDocument.GetDocumentInfo().SetMoreInfo("RecordSeries", Record.RecordSeries);
         targetDocument.GetDocumentInfo().SetMoreInfo("Published", Record.Published);

         targetDocument.Close();
         sourceDocument.Close();
         dataGridMain.Refresh();
      }
	}
}
