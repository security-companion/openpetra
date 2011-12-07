//
// DO NOT REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
//
// @Authors:
//       timop
//
// Copyright 2004-2011 by OM International
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
//
using System;
using System.Collections.Specialized;
using System.Data;
using System.Data.Odbc;
using System.Xml;
using System.IO;
using GNU.Gettext;
using Ict.Common;
using Ict.Common.IO;
using Ict.Common.DB;
using Ict.Common.Verification;
using Ict.Common.Data;
using Ict.Petra.Shared;
using Ict.Petra.Shared.MFinance.Account.Data;
using Ict.Petra.Shared.MFinance.GL.Data;
using Ict.Petra.Shared.MFinance;
using Ict.Petra.Server.MFinance.Account.Data.Access;
using Ict.Petra.Server.MFinance.GL.Data.Access;
using Ict.Petra.Server.App.Core.Security;
using Ict.Petra.Server.MFinance.GL.WebConnectors;

namespace Ict.Petra.Server.MFinance.Budget.WebConnectors
{
    /// <summary>
    /// maintain the budget
    /// </summary>
    public class TBudgetMaintainWebConnector
    {
        /// <summary>
        /// load budgets
        /// </summary>
        /// <param name="ALedgerNumber"></param>
        /// <returns></returns>
        [RequireModulePermission("FINANCE-1")]
        public static BudgetTDS LoadBudget(Int32 ALedgerNumber)
        {
            BudgetTDS MainDS = new BudgetTDS();

            //TODO: need to filter on Year
            ABudgetAccess.LoadViaALedger(MainDS, ALedgerNumber, null);
            ABudgetRevisionAccess.LoadViaALedger(MainDS, ALedgerNumber, null);
            //TODO: need to filter on ABudgetPeriod using LoadViaBudget or LoadViaUniqueKey
            ABudgetPeriodAccess.LoadAll(MainDS, null);

//            ABudgetPeriodTable BudgetPeriodTable = new ABudgetPeriodTable();
//            ABudgetPeriodRow TemplateRow = (ABudgetPeriodRow)BudgetPeriodTable.NewRow(false);
//
//            TemplateRow.BudgetSequence;
//            ABudgetPeriodAccess.LoadViaABudgetTemplate(MainDS, TemplateRow, null);


            // Accept row changes here so that the Client gets 'unmodified' rows
            MainDS.AcceptChanges();

            // Remove all Tables that were not filled with data before remoting them.
            MainDS.RemoveEmptyTables();

            return MainDS;
        }

        /// <summary>
        /// save modified budgets
        /// </summary>
        /// <param name="AInspectDS"></param>
        /// <param name="AVerificationResult"></param>
        /// <returns></returns>
        [RequireModulePermission("FINANCE-3")]
        public static TSubmitChangesResult SaveBudget(ref BudgetTDS AInspectDS,
            out TVerificationResultCollection AVerificationResult)
        {
            AVerificationResult = null;

            if (AInspectDS != null)
            {
                return BudgetTDSAccess.SubmitChanges(AInspectDS, out AVerificationResult);
            }

            return TSubmitChangesResult.scrError;
        }
        
        
                /// <summary>
        /// import budgets
        /// </summary>
        /// <param name="AImportDS"></param>
        /// <param name="AVerificationResult"></param>
        /// <returns></returns>
        [RequireModulePermission("FINANCE-3")]
        public static bool ImportBudgets(Int32 ALedgerNumber, string ACSVFileName, string[] AFdlgSeparator, ref BudgetTDS AImportDS,
            out TVerificationResultCollection AVerificationResult)
        {
            AVerificationResult = null;

            if (AImportDS != null)
            {
            	bool retVal = ImportBudgetFromCSV(ALedgerNumber, ACSVFileName, AFdlgSeparator, ref AImportDS, ref AVerificationResult);
            	return retVal;
            }

            return false;
        }

                /// <summary>
        /// import budgets
        /// </summary>
        /// <param name="ACSVFileName"></param>
        /// <param name="AImportDS"></param>
        /// <param name="AVerificationResult"></param>
        /// <returns></returns>
        private static bool ImportBudgetFromCSV(Int32 ALedgerNumber, string ACSVFileName, string[] AFdlgSeparator, ref BudgetTDS AImportDS,
            ref TVerificationResultCollection AVerificationResult)
        {

            StreamReader DataFile = new StreamReader(ACSVFileName, System.Text.Encoding.Default);

            string Separator = AFdlgSeparator[0];
            string DateFormat = AFdlgSeparator[1];
            string NumberFormat = AFdlgSeparator[2] == "0" ? "American" : "European";

            //CultureInfo MyCultureInfoDate = new CultureInfo("en-GB");
            //MyCultureInfoDate.DateTimeFormat.ShortDatePattern = DateFormat;

            // To store the From and To currencies
            // Use an array to store these to make for easy
            //   inverting of the two currencies when calculating
            //   the inverse value.
			
	
            string currentBudgetVal = string.Empty;
            string mess = string.Empty;
            string CostCentre = string.Empty;
            string Account = string.Empty;
            string BudgetType = string.Empty;
            int Year = 0;
			decimal[] BudgetPeriods = new decimal[12];

        	int newSequence = -1;

            if (AImportDS.ABudget.Rows.Find(new object[] { newSequence }) != null)
            {
                while (AImportDS.ABudget.Rows.Find(new object[] { newSequence }) != null)
                {
                    newSequence--;
                }
            }

            Int32 rowNumber = 0;
            	
            while (!DataFile.EndOfStream)
            {
                decimal totalBudgetRowAmount = 0;
            	
            	try
                {
	                string Line = DataFile.ReadLine();
	
	                CostCentre = StringHelper.GetNextCSV(ref Line, Separator, false).ToString();
	
	               	if (CostCentre == "Cost Centre")
	                {
	                	//Read the next line
	                	Line = DataFile.ReadLine();
		                CostCentre = StringHelper.GetNextCSV(ref Line, Separator, false).ToString();
		                //newSequence--;
	                }
	                
	               	//Increment row number
	               	rowNumber++;
	               	
	                //Convert separator to a char
	                char Sep = Separator[0];
	                //Turn current line into string array of column values
	                string[] CsvColumns = Line.Split(Sep);
	
	                int NumCols = CsvColumns.Length;

                //If number of columns is not 4 then import csv file is wrongly formed.
//                if (NumCols != 24)
//                {
//                    AVerificationResult. MessageBox.Show(Catalog.GetString("Failed to import the CSV budget file:\r\n\r\n" +
//                            "   " + ADataFilename + "\r\n\r\n" +
//                            "It contains " + NumCols.ToString() + " columns. " +
//                            ), AImportMode + " Exchange Rates Import Error");
//                    return;
//                }

                //Read the values for the current line
	                Account = StringHelper.GetNextCSV(ref Line, Separator, false).ToString();
	                BudgetType = StringHelper.GetNextCSV(ref Line, Separator, false).ToString();
	                if (BudgetType != MFinanceConstants.BUDGET_ADHOC 
	                    && BudgetType != MFinanceConstants.BUDGET_SAME
	                    && BudgetType != MFinanceConstants.BUDGET_INFLATE_N
	                    && BudgetType != MFinanceConstants.BUDGET_SPLIT
	                    && BudgetType != MFinanceConstants.BUDGET_INFLATE_BASE
	                   )
	                {
	                	throw new InvalidOperationException("Budget Type: " + BudgetType + " does not exist.");
	                }
	                
					//TODO: Change this formula
	                Year = Convert.ToInt32(StringHelper.GetNextCSV(ref Line, Separator, false));

	                ALedgerTable LedgerTable = ALedgerAccess.LoadByPrimaryKey(ALedgerNumber);
	                ALedgerRow LedgerRow = (ALedgerRow)LedgerTable.Rows[0];

	                //TAccountingPeriodsWebConnector.GetPeriodEndDate();
	                //Int32  YearForBudgetRevision = LedgerRow.
	                
	                /*
					DateTime currentYearEnd = GetPeriodEndDate(ALedgerNumber,
																LedgerTable[0].CurrentFinancialYear,
																0,
																LedgerTable[0].NumberOfAccountingPeriods);
					currentYearEnd.AddYears(-1 * (LedgerTable[0].CurrentFinancialYear - Convert.ToInt32(row[0]))).ToString("yyyy")
					yearforbudgetrevision=currentYearEnd.Year - YearFromCSV + ledger.currentfinancialyear
					using namespace Ict.Petra.Server.MFinance.GL.WebConnectors
					TAccountingPeriodsWebConnector.GetPeriodEndDate 
					 */
					
					//Read the budgetperiod values to check if valid according to type
										
	                for (int i = 0; i < 12; i++)
	                {
	                	currentBudgetVal = StringHelper.GetNextCSV(ref Line, Separator, false);
	                	BudgetPeriods[i] = Convert.ToDecimal(currentBudgetVal);
	                	totalBudgetRowAmount += BudgetPeriods[i];
	                }
	                
	                bool ErrorInPeriodValues = false;
	                switch (BudgetType)
	                {
	                	case MFinanceConstants.BUDGET_SAME:
	                		if (!CheckBudgetTypeSame(BudgetPeriods)) ErrorInPeriodValues = true;
	                		break;
	                	case MFinanceConstants.BUDGET_SPLIT:
	                		if (!CheckBudgetTypeSplit(BudgetPeriods)) ErrorInPeriodValues = true;
	                		break;
	                	case MFinanceConstants.BUDGET_INFLATE_BASE:
	                		if (!CheckBudgetTypeInflateBase(BudgetPeriods)) ErrorInPeriodValues = true;
	                		break;
	                	case MFinanceConstants.BUDGET_INFLATE_N:
	                		if (!CheckBudgetTypeInflateN(BudgetPeriods)) ErrorInPeriodValues = true;
	                		break;
	                	default: //MFinanceConstants.BUDGET_ADHOC
	                		//Nothing to do
	                		break;
	                }

                    if (ErrorInPeriodValues)
                    {

                    	throw new InvalidOperationException(String.Format("The budget in row {0} for Ledger: {1}, Year: {2}, Cost Centre: {3} and Account: {4}, does not have values consistent with Budget Type: {5}.",
		                			                                            rowNumber,
		                			                                            ALedgerNumber,
		                			                                           	Year,
		                			                                          	CostCentre,
		                			                                         	Account,
		                			                                         	BudgetType));
                    }
	                
                    ABudgetRow BudgetRow = (ABudgetRow)AImportDS.ABudget.NewRowTyped();
		            BudgetRow.BudgetSequence = newSequence;
		            BudgetRow.LedgerNumber = ALedgerNumber;
		            BudgetRow.Year = Year;
		            BudgetRow.CostCentreCode = CostCentre;
		            BudgetRow.AccountCode = Account;
		            BudgetRow.BudgetTypeCode = BudgetType;
	                AImportDS.ABudget.Rows.Add(BudgetRow);

	                //Add the budget periods
	                Array.Clear(BudgetPeriods, 0, 12);

	                for (int i = 0; i < 12; i++)
	                {
		               	ABudgetPeriodRow BudgetPeriodRow = (ABudgetPeriodRow)AImportDS.ABudgetPeriod.NewRowTyped();
		               	BudgetPeriodRow.BudgetSequence = newSequence;
	                	BudgetPeriodRow.PeriodNumber = i;
	                	BudgetPeriodRow.BudgetBase = BudgetPeriods[i];
		                AImportDS.ABudgetPeriod.Rows.Add(BudgetPeriodRow);
	                }

	                newSequence--;
                }
                catch (Exception)
                {
                	throw;
                }
                
//                ADailyExchangeRateTable ExchangeRateDT = (ADailyExchangeRateTable)AExchangeRDT;
//                ADailyExchangeRateRow ExchangeRow = (ADailyExchangeRateRow)ExchangeRateDT.Rows.
//                                                    Find(new object[] { Currencies[x], Currencies[y], DateEffective, 0 });
//
//                if (ExchangeRow == null)                                                                                    // remove 0 for Corporate
//                {
//                    ExchangeRow = (ADailyExchangeRateRow)ExchangeRateDT.NewRowTyped();
//                    ExchangeRow.FromCurrencyCode = Currencies[x];
//                    ExchangeRow.ToCurrencyCode = Currencies[y];
//                    ExchangeRow.DateEffectiveFrom = DateEffective;
//                    ExchangeRateDT.Rows.Add(ExchangeRow);
//                }
            }

            DataFile.Close();
            
            return true;
        }
		
        
        private static bool CheckBudgetTypeSame(decimal[] APeriodValues)
        {
			bool PeriodValuesOK = true;

			decimal Period1Amount = APeriodValues[0];
			
			for (int i = 1; i < 12; i++)
			{
				if (Period1Amount != APeriodValues[i])
				{
					PeriodValuesOK = false;
					break;
				}					
			}

			return PeriodValuesOK;			
        }
        
        private static bool CheckBudgetTypeSplit(decimal[] APeriodValues)
        {
			bool PeriodValuesOK = true;

			decimal Period1Amount = APeriodValues[0];

			for (int i = 1; i < 11; i++)
			{
				if (Period1Amount != APeriodValues[i])
				{
					PeriodValuesOK = false;
					break;
				}					
			}

			if (PeriodValuesOK)
			{
				if (APeriodValues[11] < Period1Amount
				    || (APeriodValues[11]-Period1Amount) >= 12)
				{
					PeriodValuesOK = false;	
				}
			}
			
			return PeriodValuesOK;			
        }

        private static bool CheckBudgetTypeInflateBase(decimal[] APeriodValues)
        {
			bool PeriodValuesOK = true;

			if (APeriodValues[0] == 0)
			{
				PeriodValuesOK = false;	
			}

			return PeriodValuesOK;			
        }

        private static bool CheckBudgetTypeInflateN(decimal[] APeriodValues)
        {
			bool PeriodValuesOK = true;
			bool PeriodAmountHasChanged = false;

			decimal Period1Amount = APeriodValues[0];
			
			if (Period1Amount == 0)
			{
				PeriodValuesOK = false;
				return PeriodValuesOK;
			}
			
			for (int i = 1; i < 12; i++)
			{
				if (Period1Amount != APeriodValues[i])
				{
					if (PeriodAmountHasChanged)
					{
				        PeriodValuesOK = false;	
				        break;
					}
					else
					{
						Period1Amount = APeriodValues[i];
						PeriodAmountHasChanged = true;
					}
				}					
			}

			return PeriodValuesOK;
        }

    }
}