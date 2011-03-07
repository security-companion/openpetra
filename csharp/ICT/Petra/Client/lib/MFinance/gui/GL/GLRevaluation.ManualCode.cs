﻿//
// DO NOT REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
//
// @Authors:
//       wolfgangu
//
// Copyright 2004-2010 by OM International
//
// This file is part of OpenPetra.org.
//
// OpenPetra.org is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// OpenPetra.org is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with OpenPetra.org.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Data;
using System.Collections.Generic;

using Ict.Petra.Client.MFinance.Logic;
using Ict.Petra.Client.App.Core;

using Ict.Petra.Client.MFinance.Gui.Setup;
using Ict.Petra.Client.CommonControls;

using Ict.Petra.Shared.MFinance;
using Ict.Petra.Shared.MFinance.Account.Data;

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;


using Ict.Petra.Shared;
using Ict.Petra.Shared.MPartner.Partner.Data;
using Ict.Petra.Shared.RemotedExceptions;
using Ict.Common;
using Ict.Petra.Shared.MCommon;
using Ict.Petra.Shared.MCommon.Data;



namespace Ict.Petra.Client.MFinance.Gui.GL
{
	/// <summary>
	/// Description of GLRevaluation_ManualCode.
	/// </summary>
	public partial class TGLRevaluation
	{
		
		private const string REVALUATIONCOSTCENTRE = "REVALUATIONCOSTCENTRE";


		private Int32 FLedgerNumber;
        
        private DateTime DefaultDate;
        private DateTime StartDateCurrentPeriod;
        private DateTime EndDateLastForwardingPeriod;
        
        private string strBaseCurrency;
        private string strLedgerName;
        private string strCountryCode;
        
        private string strRevaluationCurrencies;
        
        TFrmSetupDailyExchangeRate tFrmSetupDailyExchangeRate;

        LinkClickDelete linkClickDelete = new LinkClickDelete();

        /// <summary>
        /// use this ledger
        /// </summary>
        public Int32 LedgerNumber
        {
            set
            {
                FLedgerNumber = value;
                
                Ict.Petra.Client.CommonControls.TCmbAutoPopulated cmbAccountList;
                cmbAccountList = new Ict.Petra.Client.CommonControls.TCmbAutoPopulated();
                TFinanceControls.InitialiseCostCentreList(ref cmbCostCenter, FLedgerNumber,
                                                   true, false, true, false);
                
                TLedgerSelection.GetCurrentPostingRangeDates(FLedgerNumber,
                                                             out StartDateCurrentPeriod,
                                                             out EndDateLastForwardingPeriod,
                                                             out DefaultDate);

                CreateDataGridHeader();
                GetListOfRevaluationCurrencies();

			
                LoadUserDefaults();

                this.lblAccountText.Text = Catalog.GetString("Account:");

                GetLedgerInfos(FLedgerNumber);

                lblAccountValue.Text = FLedgerNumber.ToString() + " - " +
                	strLedgerName + " [" + strBaseCurrency + "]";
                
                lblDateStart.Text = Catalog.GetString("Start Date:");
                lblDateStartValue.Text = StartDateCurrentPeriod.ToLongDateString();
                lblDateEnd.Text = Catalog.GetString("End Date (=Revaluation Date):");
                lblDateEndValue.Text = EndDateLastForwardingPeriod.ToLongDateString();
                
                lblRevCur.Text = Catalog.GetString("Revaluation Currencies:");
                lblRevCurValue.Text = strRevaluationCurrencies;
                
            }
        }
        
        private void GetListOfRevaluationCurrencies()
        {
        	
        	TFrmSetupDailyExchangeRate frmExchangeRate =
        		new TFrmSetupDailyExchangeRate(this.Handle);

                DataTable table = TDataCache.TMFinance.GetCacheableFinanceTable(
                	TCacheableFinanceTablesEnum.AccountList, FLedgerNumber);
                
                int ic = 0;

                foreach (DataRow row in table.Rows)
                {
                	bool blnIsLedger = (FLedgerNumber == (int)row["a_ledger_number_i"]);
                	bool blnAccountActive = (bool)row["a_account_active_flag_l"];
                	bool blnAccountForeign = (bool)row["a_foreign_currency_flag_l"];
                	bool blnAccountHasPostings = (bool)row["a_posting_status_l"];
                	
                	if (blnIsLedger && blnAccountActive && 
                	    blnAccountForeign && blnAccountHasPostings)
                	{
                		++ic;
                		if (strRevaluationCurrencies == null) {
                			strRevaluationCurrencies =  
                				"[" + (string)row["a_foreign_currency_code_c"];
                		} else
                		{
                			strRevaluationCurrencies = strRevaluationCurrencies + 
                				"|" + row["a_foreign_currency_code_c"];
                		}

                		string strCurrencyCode = (string)row["a_foreign_currency_code_c"];
                		decimal decExchangeRate = frmExchangeRate.GetLastExchangeValueOfIntervall(
                			StartDateCurrentPeriod, EndDateLastForwardingPeriod, strCurrencyCode);
                		AddADataRow(ic, strCurrencyCode, decExchangeRate);
                	}
                }
                
                if (strRevaluationCurrencies != null) {
                	strRevaluationCurrencies = strRevaluationCurrencies + "]";
                }
        }
        
        private void CreateDataGridHeader()
        {
			grdDetails.BorderStyle = BorderStyle.FixedSingle;


			grdDetails.Columns.Add("DoRevaluation", "...",
			                       typeof(bool)).Width = 30;
			grdDetails.Columns.Add("Currency", "[CUR]",
			                       typeof(string)).Width = 50;
			grdDetails.Columns.Add("ExchangeRate", Catalog.GetString("Exchange Rate"),
			                       typeof(decimal)).Width = 200;
			grdDetails.Columns.Add("Status", Catalog.GetString("Status"),
			                       typeof(string)).Width = 200;
			
			grdDetails.SelectionMode = SourceGrid.GridSelectionMode.Row;
			
			
			SourceGrid.DataGridColumn gridColumn;

			gridColumn = grdDetails.Columns.Add(
				null, "", new SourceGrid.Cells.Button("..."));
			linkClickDelete.InitFrmData(this, StartDateCurrentPeriod, EndDateLastForwardingPeriod);
			gridColumn.DataCell.AddController(linkClickDelete);

        }
        
        private void AddADataRow(int AIndex, string ACurrencyValue, decimal AExchangeRate)
        {
			CurrencyExchange ce = new CurrencyExchange(ACurrencyValue, AExchangeRate);
        	currencyExchangeList.Add(ce);
        	mBoundList = new DevAge.ComponentModel.BoundList<CurrencyExchange>
        		(currencyExchangeList);
			grdDetails.DataSource = mBoundList;

			mBoundList.AllowNew = false;
			mBoundList.AllowDelete = false;
			linkClickDelete.SetDataList(currencyExchangeList);
        }

        private void GetLedgerInfos(Int32 ALedgerNumber)
        {

        	ALedgerRow ledger =
                    ((ALedgerTable)TDataCache.TMFinance.GetCacheableFinanceTable(
                         TCacheableFinanceTablesEnum.LedgerDetails, ALedgerNumber))[0];

        	strBaseCurrency = ledger.BaseCurrency;
        	strCountryCode = ledger.CountryCode;
        	

        	PCountryTable DataCacheCountryDT = 
        		(PCountryTable)TDataCache.TMCommon.GetCacheableCommonTable(
        			TCacheableCommonTablesEnum.CountryList);
        	PCountryRow CountryDR = 
        		(PCountryRow)DataCacheCountryDT.Rows.Find(strCountryCode);
        	
            if (CountryDR != null)
            {
                strLedgerName = CountryDR.CountryName;
            }
            else
            {
                strLedgerName ="";
            }
        	
        }
	
		
        private void SaveUserDefaults()
        {
        	TUserDefaults.SetDefault(REVALUATIONCOSTCENTRE, cmbCostCenter.GetSelectedString());
        }
        
        private void LoadUserDefaults()
        {
        	try {
        		cmbCostCenter.SetSelectedString(
        			TUserDefaults.GetStringDefault(REVALUATIONCOSTCENTRE));
        		} catch (Exception) {}        	
        }
		
        private void CancelRevaluation(object btn, EventArgs e)
		{
			this.Close();
		}
		
		private void RunRevaluation(object btn, EventArgs e)
		{
			SaveUserDefaults();
			this.Close();
		}

        public class CurrencyExchange
        {
        	private string strMessageNotInitialized = Catalog.GetString("Not initialzed");
        	private string strMessageRunRevaluation = Catalog.GetString("Revaluation");
        	private string strMessageRunNoRevaluation = Catalog.GetString("No Revaluation");
        	
        	public const int IS_NOT_INITIALIZED = 0;
        	public const int DO_REVALUATION = 1;
        	public const int DO_NO_REVALUATION = 2;
        	
            private bool mDoRevaluation = true;
            private string mCurrency = "?";
            private decimal mExchangeRate = 1.0m;
            private string mStatus = "?";
            private int intStatus;
            
            private void SetRateAndStatus(decimal ANewExchangeRate)
            {
            	if (ANewExchangeRate == 0) 
            	{
            		if (mDoRevaluation) {
            			intStatus = DO_REVALUATION;
            		} else {
            			intStatus = DO_NO_REVALUATION;
            		}
            	} else {
            		mExchangeRate = ANewExchangeRate;
            		if (mExchangeRate == 1.0m) {
            			intStatus = IS_NOT_INITIALIZED;
            			mDoRevaluation = false;
            		}  else {
            			intStatus = DO_REVALUATION;
            			mDoRevaluation = true;
            		}
            	}
            	if (intStatus == IS_NOT_INITIALIZED) 
            	{
            		mStatus = strMessageNotInitialized;
            	} else if (intStatus == DO_REVALUATION)
            	{
            		mStatus = strMessageRunRevaluation;
            	} else if (intStatus == DO_NO_REVALUATION)
            	{
            		mStatus = strMessageRunNoRevaluation;
            	}
            }

            public CurrencyExchange(string ACurrency, decimal AExchangeRate)
        	{
        		mCurrency = ACurrency;
        		SetRateAndStatus(AExchangeRate);
        	}
        	
            public bool DoRevaluation
            {
                get { return mDoRevaluation; }
                set { 
                	if (intStatus != IS_NOT_INITIALIZED)
                	{
                		mDoRevaluation = value;
                		SetRateAndStatus(0.0m);
                	}
                }
            }

            public string Currency
            {
                get { return mCurrency; }
            }

            public decimal ExchangeRate
            {
                get { return mExchangeRate; }
            }

            public string Status
            {
                get { return mStatus; }
            }
            
            public void updateExchangeRate(decimal newRate)
            {
            	if (newRate != mExchangeRate) {
            		SetRateAndStatus(newRate);
            	}
            }
            
        }

        private List<CurrencyExchange> currencyExchangeList = new List<CurrencyExchange>();
        private DevAge.ComponentModel.BoundList<CurrencyExchange> mBoundList;
        
		private class LinkClickDelete : SourceGrid.Cells.Controllers.ControllerBase
		{
			int ix = 0;
			
			TGLRevaluation mainForm;
			DateTime dteStart;
			DateTime dteEnd;
			
			List<CurrencyExchange> currencyExchangeList;
						
						
			public override void OnClick(SourceGrid.CellContext sender, EventArgs e)
			{
				base.OnClick(sender, e);

				SourceGrid.DataGrid grid = (SourceGrid.DataGrid)sender.Grid;

				++ix;
				System.Diagnostics.Debug.WriteLine(sender.Position.Row.ToString());
				
				int iRow = sender.Position.Row-1;
				
				TFrmSetupDailyExchangeRate frmExchangeRate = 
					new TFrmSetupDailyExchangeRate(mainForm.Handle);
				frmExchangeRate.LedgerNumber = mainForm.FLedgerNumber;
				frmExchangeRate.SetDataFilters(dteStart, dteEnd, 
				                               currencyExchangeList[iRow].Currency,
				                               currencyExchangeList[iRow].ExchangeRate);
				frmExchangeRate.ShowDialog(mainForm);
				
				currencyExchangeList[iRow].updateExchangeRate(
					Decimal.Parse(frmExchangeRate.CurrencyExchangeRate));
				
				
			}
			
			public void InitFrmData(TGLRevaluation AMain, DateTime ADateStart, DateTime ADateEnd)
			{
				mainForm = AMain;
				dteStart = ADateStart;
				dteEnd = ADateEnd;
			}
			
			public void SetDataList(List<CurrencyExchange> ACurrencyExchangeList)
			{
				currencyExchangeList = ACurrencyExchangeList;
			}
		}
	}
}
