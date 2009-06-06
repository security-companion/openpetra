﻿/*************************************************************************
 *
 * DO NOT REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
 *
 * @Authors:
 *       timop
 *
 * Copyright 2004-2009 by OM International
 *
 * This file is part of OpenPetra.org.
 *
 * OpenPetra.org is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * OpenPetra.org is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with OpenPetra.org.  If not, see <http://www.gnu.org/licenses/>.
 *
 ************************************************************************/
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Resources;
using System.Windows.Forms;
using System.Threading;
using Ict.Common;
using Ict.Common.Controls;
using Ict.Common.Printing;
using Ict.Petra.Client.CommonForms;
using Ict.Petra.Client.MReporting.Gui;
using Ict.Petra.Client.MReporting.Logic;
using Ict.Petra.Shared.MReporting;


namespace Ict.Petra.Client.MReporting.Gui
{
	/// <summary>
	/// 
	/// </summary>
    public partial class TFrmPrintPreview : System.Windows.Forms.Form, Ict.Petra.Client.CommonForms.IFrmPetra
    {
#region private members

		private TGridPreview FGridPreview;
        private bool FPrinterInstalled;
		
        private TReportPrinterLayout ReportGfxPrinter;
		private TReportPrinterLayout ReportTxtPrinter;
		private TGfxPrinter FGfxPrinter;
        private TTxtPrinter FTxtPrinter;
        private TResultList Results;
        private TParameterList Parameters;
        private string ReportName;
        private TPrintChartCallbackProcedure PrintChartProcedure;
        private bool PrintChartProcedureValid;
        private TFrmPetraUtils FPetraUtilsObject;
        
#endregion

		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="ACallerWindowHandle"></param>
		/// <param name="caption">caption of the dialog</param>
		/// <param name="duration"></param>
		/// <param name="results"></param>
		/// <param name="parameters"></param>
        public TFrmPrintPreview(IntPtr ACallerWindowHandle, String caption, TimeSpan duration, TResultList results, TParameterList parameters)
        	: base()
        {
        	FPetraUtilsObject = new Ict.Petra.Client.CommonForms.TFrmPetraUtils(ACallerWindowHandle, this);
			
            //  
            // Required for Windows Form Designer support 
            //  
            InitializeComponent();
            
			System.Windows.Forms.TabPage SelectedTab;
			String durationText;
			
			this.Text = this.Text + ": " + caption;
            this.ReportName = caption;
            this.Results = results;
            this.Parameters = parameters;
            FTxtPrinter = new TTxtPrinter();
            this.ReportTxtPrinter = new TReportPrinterLayout(Results, Parameters, FTxtPrinter);
            ReportTxtPrinter.PrintReport();
            
            this.txtOutput.Lines = FTxtPrinter.GetArrayOfString();
            FPrinterInstalled = this.PrintDocument.PrinterSettings.IsValid;

            if (FPrinterInstalled)
            {
                this.tabPreview.SelectedTab = tbpPreview;
                FGfxPrinter = new TGfxPrinter(this.PrintDocument);
                this.ReportGfxPrinter = new TReportPrinterLayout(Results, Parameters, FGfxPrinter);
                this.PrintPreviewControl.Document = FGfxPrinter.Document;
                this.PrintPreviewControl.Zoom = 1; // show 100% by default
                this.PrintPreviewControl.UseAntiAlias = true;
                this.lblNoPrinter.Visible = false;
            }
            else
            {
                // PrintPreviewControl.CalculatePageInfo will throw InvalidPrinterException 
                this.tabPreview.SelectedTab = tbpText;
                this.PrintPreviewControl.Visible = false;
                this.CbB_Zoom.Enabled = false;
                this.Btn_PreviousPage.Enabled = false;
                this.Btn_NextPage.Enabled = false;
            }

            this.PrintChartProcedure = null;
            this.PrintChartProcedureValid = false;
            sgGridView.SortableHeaders = false;
            sgGridView.SelectionMode = SourceGrid.GridSelectionMode.Cell;
            this.sgGridView.DoubleClickCell += new TDoubleClickCellEventHandler(this.SgGridView_DoubleClickCell);
            FGridPreview = new TGridPreview(this, FPetraUtilsObject, @PreviewDetailReport, Results, Parameters);

            if (!FGridPreview.PopulateResultGrid(sgGridView))
            {
                SelectedTab = tabPreview.SelectedTab;
                this.tabPreview.TabPages.Clear();
                this.tabPreview.TabPages.AddRange(new TabPage[] { this.tbpText, this.tbpPreview });
                tabPreview.SelectedTab = SelectedTab;
            }
            else
            {
                FGridPreview.PopulateGridContextMenu(ContextMenu1);
            }

            durationText = String.Format("It took {0} to calculate the report", FormatDuration(duration));
            // TODO statusbar this.sbtForm.InstanceDefaultText = durationText;
        }



#region Event Handlers

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void MniFile_Click(System.Object sender, System.EventArgs e)
		{
		    if (sender == mniFileClose)
		    {
		    	FPetraUtilsObject.ExecuteAction(eActionId.eClose);
		    }
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void MniHelpPetra_Click(System.Object sender, System.EventArgs e)
		{
			FPetraUtilsObject.ExecuteAction(eActionId.eHelp);
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void MniHelpAboutPetra_Click(System.Object sender, System.EventArgs e)
		{
			FPetraUtilsObject.ExecuteAction(eActionId.eHelpAbout);
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void MniHelpDevelopmentTeam_Click(System.Object sender, System.EventArgs e)
		{
			FPetraUtilsObject.ExecuteAction(eActionId.eHelpDevelopmentTeam);
		}
	
		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void MniHelpBugReport_Click(System.Object sender, System.EventArgs e)
		{
			FPetraUtilsObject.ExecuteAction(eActionId.eBugReport);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Btn_NextPage_Click(System.Object sender, System.EventArgs e)
        {
            this.PrintPreviewControl.StartPage = this.PrintPreviewControl.StartPage + 1;
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        protected void Btn_PreviousPage_Click(System.Object sender, System.EventArgs e)
        {
            this.PrintPreviewControl.StartPage = this.PrintPreviewControl.StartPage - 1;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void TPrintPreview_Resize(System.Object sender, System.EventArgs e)
        {
            /*System.Int32 origPage;
             * // somehow we need to get the scrollbars activated
             * // seems not to be needed anymore in .net 2.0; the workaround now causes an error
             * origPage = this.PrintPreviewControl.StartPage;
             * this.PrintPreviewControl.StartPage = this.PrintPreviewControl.StartPage - 1;
             * this.PrintPreviewControl.StartPage = origPage;*/
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void tbbCloseClick(object sender, EventArgs e)
	    {
        	FPetraUtilsObject.ExecuteAction(eActionId.eClose);
	    }
	    
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void tbtExportTextClick(System.Object sender, System.EventArgs e)
        {
        	if (dlgSaveTextFile.FileName.Length == 0)
            {
                dlgSaveTextFile.FileName = this.ReportName + '.' + dlgSaveTextFile.DefaultExt;
            }

            if (dlgSaveTextFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FTxtPrinter.WriteToFile(dlgSaveTextFile.FileName);
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void tbtExportCSVClick(System.Object sender, System.EventArgs e)
        {
        	if (!ExportToExcel())
            {
                ExportToCSV();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void tbtGenerateChartClick(System.Object sender, System.EventArgs e)
        {
        	if (PrintChartProcedureValid)
            {
                PrintChartProcedure();
            }
            else
            {
                MessageBox.Show("There are no charts available for this report!");
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void tbtPrintClick(System.Object sender, System.EventArgs e)
        {
        	PrintDialog printerSelectionDlg;

            if (!FPrinterInstalled)
            {
                MessageBox.Show("The program cannot find a printer, and therefore cannot print!", "Problem with printing");
                return;
            }

            printerSelectionDlg = new PrintDialog();
            printerSelectionDlg.Document = FGfxPrinter.Document;

            /* TODO: allow the printing of a range of pages.
             * need to do something in ReportGfxPrinter.Document.Print();
             * as well
             *
             * printerSelectionDlg.PrinterSettings.MinimumPage := 1;
             * printerSelectionDlg.PrinterSettings.MaximumPage := ReportGfxPrinter.NumberOfPages;
             * printerSelectionDlg.PrinterSettings.FromPage := 1;
             * printerSelectionDlg.PrinterSettings.ToPage := ReportGfxPrinter.NumberOfPages;
             *
             * printerSelectionDlg.AllowSomePages := true;
             * printerSelectionDlg.AllowPrintToFile := true;
             * printerSelectionDlg.AllowSelection := true;
             */
            if (printerSelectionDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    FGfxPrinter.Document.Print();
                }
                catch (Exception E)
                {
                    TLogging.Log(E.StackTrace);
                    TLogging.Log(E.Message);
                    System.Console.WriteLine(E.StackTrace);
                }
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ACalculator"></param>
		protected void PreviewDetailReport(TRptCalculator ACalculator)
        {
            TFrmPrintPreview printWindow;

            // show a print window with all kinds of output options 
            printWindow = new TFrmPrintPreview(this.Handle, ACalculator.GetParameters().Get("currentReport").ToString(), ACalculator.GetDuration(), ACalculator.GetResults(
                    ), ACalculator.GetParameters());
            this.AddOwnedForm(printWindow);
            printWindow.Owner = this;

            // printWindow.SetPrintChartProcedure(GenerateChart); 
            printWindow.ShowDialog();

            // EnableDisableToolbar(true); 
        }
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="printChartProcedure"></param>
		public void SetPrintChartProcedure(TPrintChartCallbackProcedure printChartProcedure)
        {
            this.PrintChartProcedure = printChartProcedure;
            this.PrintChartProcedureValid = true;
        }
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="e"></param>
		protected void SgGridView_DoubleClickCell(System.Object Sender, SourceGrid.CellContextEventArgs e)
        {
            // double click on grid row activates the first menu item from the context menu 
            if (ContextMenu1.MenuItems.Count > 0)
            {
                FGridPreview.MenuItemClick(ContextMenu1.MenuItems[0], null);
            }
        }
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void PrintDocument_EndPrint(System.Object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            FGfxPrinter.EndPrint(sender, e);
            EnablePageButtons();
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void CbB_Zoom_SelectedIndexChanged(System.Object sender, System.EventArgs e)
        {
            if (CbB_Zoom.SelectedItem.ToString() == "100%")
            {
                this.PrintPreviewControl.AutoZoom = false;
                this.PrintPreviewControl.Zoom = 1.0;
            }
            else if (CbB_Zoom.SelectedItem.ToString() == "75%")
            {
                this.PrintPreviewControl.AutoZoom = false;
                this.PrintPreviewControl.Zoom = 0.75;
            }
            else if (CbB_Zoom.SelectedItem.ToString() == "50%")
            {
                this.PrintPreviewControl.AutoZoom = false;
                this.PrintPreviewControl.Zoom = 0.5;
            }
            else
            {
                this.PrintPreviewControl.AutoZoom = true;
            }

            EnablePageButtons();
        }

#endregion

        /// <summary>
        /// should this go into StringHelper?
        /// </summary>
        /// <returns>void</returns>
        private String FormatDuration(TimeSpan ADuration)
        {
            String ReturnValue;

            ReturnValue = "";

            if (ADuration.Hours >= 1)
            {
                ReturnValue = ReturnValue + String.Format("{0} hour", ADuration.Hours);

                if (ADuration.Hours != 1)
                {
                    ReturnValue = ReturnValue + 's';
                }

                ReturnValue = ReturnValue + ", ";
            }

            if ((ADuration.Minutes >= 1) || (ADuration.Hours > 0))
            {
                ReturnValue = ReturnValue + String.Format("{0} minute", ADuration.Minutes);

                if (ADuration.Minutes != 1)
                {
                    ReturnValue = ReturnValue + 's';
                }

                ReturnValue = ReturnValue + " and ";
            }

            ReturnValue = ReturnValue + String.Format("{0} second", ADuration.Seconds);

            if (ADuration.Minutes != 1)
            {
                ReturnValue = ReturnValue + 's';
            }

            return ReturnValue;
        }

        /// <summary>
        /// directly export the full result to Excel
        /// </summary>
        /// <returns>false if Excel is not available
        /// </returns>
        public bool ExportToExcel()
        {
            bool ReturnValue;
            TReportExcel myExcel;

            try
            {
                myExcel = new TReportExcel(Results, Parameters);
            }
            catch (Exception)
            {
                return false;
            }
            ReturnValue = true;
            myExcel.Show();
            myExcel.NewSheet("data");
            myExcel.ExportResult();
            myExcel.GiveUserControl();
            return ReturnValue;
        }
		
		/// <summary>
        /// export the full result to a CSV file
        /// 
        /// </summary>
        /// <returns>void</returns>
        public void ExportToCSV()
        {
            System.Diagnostics.Process csvProcess;

            if (dlgSaveCSVFile.FileName.Length == 0)
            {
                dlgSaveCSVFile.FileName = this.ReportName + '.' + dlgSaveCSVFile.DefaultExt;
            }

            if (dlgSaveCSVFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (Results.WriteCSV(Parameters, dlgSaveCSVFile.FileName))
                {
                    try
                    {
                        csvProcess = new System.Diagnostics.Process();
                        csvProcess.EnableRaisingEvents = false;
                        csvProcess.StartInfo.FileName = dlgSaveCSVFile.FileName;
                        csvProcess.Start();
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
        
        /// <summary>
        /// Depending on the setting of zoom, several pages are displayed, or just one 100% or AutoZoom, will display only one page, and enable the previous/next page buttons otherwise display all pages, and scroll through
        /// </summary>
        /// <returns>void</returns>
        private void EnablePageButtons()
        {
            if ((CbB_Zoom.SelectedIndex == -1) || (CbB_Zoom.SelectedItem.ToString() == "Fit to Window")
                || (CbB_Zoom.SelectedItem.ToString() == "100%"))
            {
                Btn_NextPage.Enabled = FGfxPrinter.NumberOfPages > 1;
                Btn_PreviousPage.Enabled = FGfxPrinter.NumberOfPages > 1;
                this.PrintPreviewControl.Rows = 1;
            }
            else
            {
                Btn_NextPage.Enabled = false;
                Btn_PreviousPage.Enabled = false;
                this.PrintPreviewControl.Rows = FGfxPrinter.NumberOfPages;
            }
        }

        
#region interface implementation

		/// <summary>
		/// This function tells the caller whether the window can be closed.
		/// It can be used to find out if something is still edited, for example.
		/// </summary>
		/// <returns>true if window can be closed
		/// </returns>
		public bool CanClose()
		{
		    return true;
		}
		
		/// <summary>
		/// 
		/// </summary>
        public void HookupAllControls()
	    {
        	
	    }
         
        /// <summary>
        /// todoComment
        /// </summary>
        /// <returns></returns>
        public TFrmPetraUtils PetraUtilsObject
        {
            get
            {
                return (TFrmPetraUtils)FPetraUtilsObject;
            }
        }
        
        /// <summary>
        /// todoComment
        /// </summary>
        public void RunOnceOnActivation()
	    {
	
	    }
        
#endregion
    }
    
    /// <summary> </summary>
    public delegate void TPrintChartCallbackProcedure();
}
