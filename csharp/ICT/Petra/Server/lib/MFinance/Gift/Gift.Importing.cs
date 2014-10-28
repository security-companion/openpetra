//
// DO NOT REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
//
// @Authors:
//       matthiash, timop, dougm
//
// Copyright 2004-2012 by OM International
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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;

using Ict.Common;
using Ict.Common.DB;
using Ict.Common.Verification;
using Ict.Petra.Server.App.Core;
using Ict.Petra.Server.MCommon;
using Ict.Petra.Server.MFinance.Account.Data.Access;
using Ict.Petra.Server.MFinance.Gift.Data.Access;
using Ict.Petra.Shared.MFinance.Gift.Validation;
using Ict.Petra.Server.MFinance.GL.WebConnectors;
using Ict.Petra.Shared.MFinance.Validation;
using Ict.Petra.Shared;
using Ict.Petra.Shared.MFinance;
using Ict.Petra.Shared.MFinance.Account.Data;
using Ict.Petra.Shared.MFinance.Gift.Data;
using Ict.Petra.Server.MSysMan.Maintenance.SystemDefaults.WebConnectors;


//using Ict.Petra.Server.MFinance.Account.Data.Access;
//using Ict.Petra.Shared.MFinance.Account.Data;


namespace Ict.Petra.Server.MFinance.Gift
{
    /// <summary>
    /// Import a Gift Batch
    /// </summary>
    public class TGiftImporting
    {
        String FDelimiter;
        Int32 FLedgerNumber;
        String FDateFormatString;
        GiftBatchTDS FMainDS;
        CultureInfo FCultureInfoNumberFormat;
        CultureInfo FCultureInfoDate;

        private String FImportLine;
        private String FNewLine;
        private string FLedgerBaseCurrency = String.Empty;

        private String InferCostCentre(AGiftDetailRow AgiftDetails)
        {
            String costCentre = "";

            if (!Common.Common.HasPartnerCostCentreLink(AgiftDetails.RecipientKey, out costCentre))
            {
                // There's no helpful entry in a_valid_ledger_number - I'll see about using the MotivationDetail.
                AMotivationDetailRow mdRow = FMainDS.AMotivationDetail.NewRowTyped(false);
                mdRow.LedgerNumber = AgiftDetails.LedgerNumber;
                mdRow.MotivationGroupCode = AgiftDetails.MotivationGroupCode;
                mdRow.MotivationDetailCode = AgiftDetails.MotivationDetailCode;
                AMotivationDetailTable tempTbl = null;

                TDBTransaction Transaction = null;
                DBAccess.GDBAccessObj.GetNewOrExistingAutoReadTransaction(IsolationLevel.ReadCommitted,
                    TEnforceIsolationLevel.eilMinimum,
                    ref Transaction,
                    delegate
                    {
                        tempTbl = AMotivationDetailAccess.LoadUsingTemplate(mdRow, Transaction);
                    });

                if (tempTbl.Rows.Count > 0)
                {
                    costCentre = tempTbl[0].CostCentreCode;
                }
            }

            return costCentre;
        }

        private bool UpdateDailyExchangeRateTable(ADailyExchangeRateTable DailyExchangeTable, string AFromCurrencyCode, string AToCurrencyCode,
            decimal AExchangeRate, DateTime AEffectiveDate)
        {
            string SortByTimeDescending = ADailyExchangeRateTable.GetTimeEffectiveFromDBName() + " DESC";

            string filter = String.Format("{0}='{1}' AND {2}='{3}' AND {4}=#{5}#",
                ADailyExchangeRateTable.GetFromCurrencyCodeDBName(), AFromCurrencyCode,
                ADailyExchangeRateTable.GetToCurrencyCodeDBName(), AToCurrencyCode,
                ADailyExchangeRateTable.GetDateEffectiveFromDBName(),
                    AEffectiveDate.ToString("d", CultureInfo.InvariantCulture));

            DataView dvTo = new DataView(DailyExchangeTable, filter, SortByTimeDescending, DataViewRowState.CurrentRows);
            bool foundMatch = false;

            for (int i = 0; i < dvTo.Count; i++)
            {
                if (((ADailyExchangeRateRow)dvTo[i].Row).RateOfExchange == AExchangeRate)
                {
                    foundMatch = true;
                    break;
                }
            }

            if (!foundMatch)
            {
                int newTime = 3600;
                if (dvTo.Count > 0)
                {
                    newTime = ((ADailyExchangeRateRow)dvTo[0].Row).TimeEffectiveFrom + 600;
                }

                ADailyExchangeRateRow newRow = DailyExchangeTable.NewRowTyped();
                newRow.DateEffectiveFrom = AEffectiveDate;
                newRow.FromCurrencyCode = AFromCurrencyCode;
                newRow.ToCurrencyCode = AToCurrencyCode;
                newRow.TimeEffectiveFrom = newTime;
                newRow.RateOfExchange = AExchangeRate;
                DailyExchangeTable.Rows.Add(newRow);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Import Gift batch data
        /// The data file contents from the client is sent as a string, imported in the database
        /// and committed immediately
        /// </summary>
        /// <param name="ARequestParams">Hashtable containing the given params </param>
        /// <param name="AImportString">Big parts of the export file as a simple String</param>
        /// <param name="AMessages">Additional messages to display in a messagebox</param>
        /// <returns>false if error</returns>
        public bool ImportGiftBatches(
            Hashtable ARequestParams,
            String AImportString,
            out TVerificationResultCollection AMessages
            )
        {
            TProgressTracker.InitProgressTracker(DomainManager.GClientID.ToString(),
                Catalog.GetString("Importing Gift Batches"),
                100);

            TProgressTracker.SetCurrentState(DomainManager.GClientID.ToString(),
                Catalog.GetString("Initialising"),
                0);

            AMessages = new TVerificationResultCollection();
            FMainDS = new GiftBatchTDS();
            StringReader sr = new StringReader(AImportString);

            // Parse the supplied parameters
            FDelimiter = (String)ARequestParams["Delimiter"];
            FLedgerNumber = (Int32)ARequestParams["ALedgerNumber"];
            FDateFormatString = (String)ARequestParams["DateFormatString"];
            String NumberFormat = (String)ARequestParams["NumberFormat"];
            FNewLine = (String)ARequestParams["NewLine"];

            // Set culture from parameters
            FCultureInfoNumberFormat = new CultureInfo(NumberFormat.Equals("American") ? "en-US" : "de-DE");
            FCultureInfoDate = new CultureInfo("en-GB");
            FCultureInfoDate.DateTimeFormat.ShortDatePattern = FDateFormatString;

            bool TaxDeductiblePercentageEnabled = Convert.ToBoolean(
                TSystemDefaults.GetSystemDefault(SharedConstants.SYSDEFAULT_TAXDEDUCTIBLEPERCENTAGE, "FALSE"));

            // Initialise our working variables
            TDBTransaction Transaction = null;
            AGiftBatchRow giftBatch = null;
            decimal totalBatchAmount = 0;
            Int32 RowNumber = 0;
            Int32 BatchDetailCounter = 0;
            bool ok = false;

            string ImportMessage = Catalog.GetString("Initialising");

            // Create some validation dictionaries
            TValidationControlsDict ValidationControlsDictBatch = new TValidationControlsDict();
            TValidationControlsDict ValidationControlsDictGift = new TValidationControlsDict();
            TValidationControlsDict ValidationControlsDictGiftDetail = new TValidationControlsDict();

            try
            {
                // This needs to be initialised because we will be calling the method
                TSharedFinanceValidationHelper.GetValidPeriodDatesDelegate = @TAccountingPeriodsWebConnector.GetPeriodDates;

                // Get a new transaction
                Transaction = DBAccess.GDBAccessObj.BeginTransaction(IsolationLevel.Serializable);

                // If we did not succeed there is something wrong (a transaction is already dangling somewhere?)
                if (Transaction == null)
                {
                    throw new Exception(Catalog.GetString(
                            "Could not create a new import transaction because an existing transaction has not completed."));
                }

                // Load supplementary tables that we are going to need for validation
                ALedgerTable LedgerTable = ALedgerAccess.LoadByPrimaryKey(FLedgerNumber, Transaction);
                AAccountTable AccountTable = AAccountAccess.LoadViaALedger(FLedgerNumber, Transaction);
                ACostCentreTable CostCentreTable = ACostCentreAccess.LoadViaALedger(FLedgerNumber, Transaction);

                if (LedgerTable.Rows.Count == 0)
                {
                    throw new Exception(String.Format(Catalog.GetString("Ledger {0} doesn't exist."), FLedgerNumber));
                }

                FLedgerBaseCurrency = ((ALedgerRow)LedgerTable.Rows[0]).BaseCurrency;
                string LedgerIntlCurrency = ((ALedgerRow)LedgerTable.Rows[0]).IntlCurrency;

                ACorporateExchangeRateTable CorporateExchangeToLedgerTable = ACorporateExchangeRateAccess.LoadViaACurrencyToCurrencyCode(FLedgerBaseCurrency, Transaction);
                ADailyExchangeRateTable DailyExchangeToLedgerTable = ADailyExchangeRateAccess.LoadViaACurrencyToCurrencyCode(FLedgerBaseCurrency, Transaction);
                ADailyExchangeRateTable DailyExchangeToIntlTable = ADailyExchangeRateAccess.LoadViaACurrencyToCurrencyCode(LedgerIntlCurrency, Transaction);

                ImportMessage = Catalog.GetString("Parsing first line");
                AGiftRow previousGift = null;

                // Go round a loop reading the file line by line
                FImportLine = sr.ReadLine();

                while (FImportLine != null)
                {
                    RowNumber++;

                    // skip empty lines and commented lines
                    if ((FImportLine.Trim().Length > 0) && !FImportLine.StartsWith("/*") && !FImportLine.StartsWith("#"))
                    {
                        int numberOfElements = StringHelper.GetCSVList(FImportLine, FDelimiter).Count;

                        // Read the row analysisType - there is no 'validation' on this so we can make the call with null parameters
                        string RowType = ImportString(Catalog.GetString("row type"), null, null);

                        if (RowType == "B")
                        {
                            // It is a Batch row
                            if (numberOfElements != 9)
                            {
                                AMessages.Add(new TVerificationResult(String.Format(MCommonConstants.StrParsingErrorInLine, RowNumber),
                                        Catalog.GetString("Wrong number of batch columns.  The correct number is 9 columns."),
                                        TResultSeverity.Resv_Critical));

                                FImportLine = sr.ReadLine();
                                continue;
                            }

                            //Check if this is the start of a new batch (i.e. not the first batch)
                            if ((previousGift != null) && (giftBatch != null))
                            {
                                //New batch so set total amount of Batch for previous batch
                                giftBatch.BatchTotal = totalBatchAmount;

                                if (TVerificationHelper.IsNullOrOnlyNonCritical(AMessages))
                                {
                                    AGiftBatchAccess.SubmitChanges(FMainDS.AGiftBatch, Transaction);
                                    // no need to AcceptChanges() because we will create a new TDS below
                                }

                                // We use a new TDS for each new batch
                                FMainDS = new GiftBatchTDS();
                                previousGift = null;
                            }

                            totalBatchAmount = 0;

                            // Parse the complete line and validate it
                            ParseBatchLine(ref giftBatch, ref Transaction, ref LedgerTable, ref ImportMessage, RowNumber,
                                AMessages, ValidationControlsDictBatch, AccountTable, CostCentreTable, CorporateExchangeToLedgerTable);

                            if (TVerificationHelper.IsNullOrOnlyNonCritical(AMessages))
                            {
                                // This row passes validation so we can do final actions if the batch is not in the ledger currency
                                if (giftBatch.CurrencyCode != FLedgerBaseCurrency)
                                {
                                    // Validation will have ensured that we have a corporate rate for the effective date
                                    // We need to know what that rate is...
                                    DateTime firstOfMonth = new DateTime(giftBatch.GlEffectiveDate.Year, giftBatch.GlEffectiveDate.Month, 1);
                                    ACorporateExchangeRateRow corporateRateRow = (ACorporateExchangeRateRow)CorporateExchangeToLedgerTable.Rows.Find(
                                        new object[] { giftBatch.CurrencyCode, FLedgerBaseCurrency, firstOfMonth});
                                    decimal corporateRate = corporateRateRow.RateOfExchange;

                                    if (Math.Abs((giftBatch.ExchangeRateToBase - corporateRate) / corporateRate) > 0.20m)
                                    {
                                        AMessages.Add(new TVerificationResult(String.Format(MCommonConstants.StrImportValidationWarningInLine, RowNumber),
                                            String.Format(Catalog.GetString("The exchange rate of {0} differs from the Corporate Rate of {1} for the month commencing {2} by more than 20 percent."),
                                                giftBatch.ExchangeRateToBase, corporateRate, StringHelper.DateToLocalizedString(firstOfMonth)),
                                            TResultSeverity.Resv_Noncritical));
                                    }

                                    // we need to create a daily exchange rate pair for the transaction date
                                    // start with To Ledger currency
                                    if (UpdateDailyExchangeRateTable(DailyExchangeToLedgerTable, giftBatch.CurrencyCode, FLedgerBaseCurrency, giftBatch.ExchangeRateToBase, giftBatch.GlEffectiveDate))
                                    {
                                        ADailyExchangeRateAccess.SubmitChanges(DailyExchangeToLedgerTable, Transaction);
                                        DailyExchangeToLedgerTable.AcceptChanges();

                                        AMessages.Add(new TVerificationResult(String.Format(MCommonConstants.StrImportInformationForLine, RowNumber),
                                            String.Format(Catalog.GetString("Added exchange rate of {0} to Daily Exchange Rate table for {1}"),
                                                giftBatch.ExchangeRateToBase, StringHelper.DateToLocalizedString(giftBatch.GlEffectiveDate)),
                                            TResultSeverity.Resv_Info));
                                    }

                                    // Now the inverse for From Ledger currency
                                    ADailyExchangeRateTable DailyExchangeFromTable =
                                        ADailyExchangeRateAccess.LoadViaACurrencyFromCurrencyCode(giftBatch.CurrencyCode, Transaction);
                                    decimal inverseRate = Math.Round(1 / giftBatch.ExchangeRateToBase, 10);

                                    if (UpdateDailyExchangeRateTable(DailyExchangeFromTable, FLedgerBaseCurrency, giftBatch.CurrencyCode,
                                        inverseRate, giftBatch.GlEffectiveDate))
                                    {
                                        ADailyExchangeRateAccess.SubmitChanges(DailyExchangeFromTable, Transaction);
                                    }
                                }

                                // Now we can commit the change
                                ImportMessage = Catalog.GetString("Saving gift batch");

                                AGiftBatchAccess.SubmitChanges(FMainDS.AGiftBatch, Transaction);
                                FMainDS.AGiftBatch.AcceptChanges();
                            }

                            BatchDetailCounter = 0;
                            TProgressTracker.SetCurrentState(DomainManager.GClientID.ToString(),
                                string.Format(Catalog.GetString("Batch {0}"), giftBatch.BatchNumber),
                                10);
                        }
                        else if (RowType == "T")
                        {
                            // It is a Transaction row
                            if (numberOfElements < 12) // Perhaps this CSV file is a summary, and can't be imported?
                            {
                                AMessages.Add(new TVerificationResult(String.Format(MCommonConstants.StrParsingErrorInLine, RowNumber),
                                        Catalog.GetString("Wrong number of gift columns. (This may be a summary?)"), TResultSeverity.Resv_Critical));
                                FImportLine = sr.ReadLine();
                                continue;
                            }
                            else if ((numberOfElements != 21) && (numberOfElements != 27))
                            {
                                AMessages.Add(new TVerificationResult(String.Format(MCommonConstants.StrParsingErrorInLine, RowNumber),
                                    Catalog.GetString("Wrong number of gift columns. Expected either 21 or 27."), TResultSeverity.Resv_Critical));
                                FImportLine = sr.ReadLine();
                                continue;
                            }

                            if (giftBatch == null)
                            {
                                AMessages.Add(new TVerificationResult(String.Format(MCommonConstants.StrParsingErrorInLine, RowNumber),
                                    Catalog.GetString(
                                        "Expected a GiftBatch line, but found a Gift. Will create a dummy working batch for the current period."),
                                    TResultSeverity.Resv_Critical));

                                // in order to carry on we will make a dummy batch and force the date to fit
                                giftBatch = TGiftBatchFunctions.CreateANewGiftBatchRow(ref FMainDS,
                                    ref Transaction,
                                    ref LedgerTable,
                                    FLedgerNumber,
                                    DateTime.Today);
                            }

                            // Parse the line into a new row
                            AGiftRow gift = FMainDS.AGift.NewRowTyped(true);
                            AGiftDetailRow giftDetails;
                            ParseTransactionLine(gift, giftBatch, ref previousGift, numberOfElements, ref totalBatchAmount, ref ImportMessage,
                                RowNumber, AMessages, ValidationControlsDictGift, ValidationControlsDictGiftDetail, CostCentreTable, out giftDetails);

                            if (TaxDeductiblePercentageEnabled)
                            {
                                // Sets TaxDeductiblePct and uses it to calculate the tax deductibility amounts for a Gift Detail
                                TGift.SetDefaultTaxDeductibilityData(ref giftDetails, gift.DateEntered, Transaction);
                            }

                            if (TVerificationHelper.IsNullOrOnlyNonCritical(AMessages))
                            {
                                if ((FLedgerBaseCurrency != LedgerIntlCurrency) && (giftDetails.GiftAmountIntl != 0))
                                {
                                    // We should add a Daily Exchange Rate row pair
                                    // start with To Ledger currency
                                    decimal fromIntlToBase = GLRoutines.Divide(giftDetails.GiftAmount, giftDetails.GiftAmountIntl);
                                    if (UpdateDailyExchangeRateTable(DailyExchangeToLedgerTable, LedgerIntlCurrency, FLedgerBaseCurrency,
                                        fromIntlToBase, giftBatch.GlEffectiveDate))
                                    {
                                        ADailyExchangeRateAccess.SubmitChanges(DailyExchangeToLedgerTable, Transaction);
                                        DailyExchangeToLedgerTable.AcceptChanges();

                                        AMessages.Add(new TVerificationResult(String.Format(MCommonConstants.StrImportInformationForLine, RowNumber),
                                            String.Format(Catalog.GetString("Added exchange rate of {0} to Daily Exchange Rate table for ledger currency / international currency on {1}"),
                                                fromIntlToBase, StringHelper.DateToLocalizedString(giftBatch.GlEffectiveDate)),
                                            TResultSeverity.Resv_Info));
                                    }

                                    // Now the inverse for From Ledger currency
                                    decimal inverseRate = GLRoutines.Divide(giftDetails.GiftAmountIntl, giftDetails.GiftAmount);

                                    if (UpdateDailyExchangeRateTable(DailyExchangeToIntlTable, FLedgerBaseCurrency, LedgerIntlCurrency,
                                        inverseRate, giftBatch.GlEffectiveDate))
                                    {
                                        ADailyExchangeRateAccess.SubmitChanges(DailyExchangeToIntlTable, Transaction);
                                    }
                                }

                                ImportMessage = Catalog.GetString("Saving gift");
                                AGiftAccess.SubmitChanges(FMainDS.AGift, Transaction);
                                FMainDS.AGift.AcceptChanges();

                                ImportMessage = Catalog.GetString("Saving giftdetails");
                                AGiftDetailAccess.SubmitChanges(FMainDS.AGiftDetail, Transaction);
                                FMainDS.AGiftDetail.AcceptChanges();
                            }

                            // Update progress tracker every 50 records
                            if (++BatchDetailCounter % 50 == 0)
                            {
                                TProgressTracker.SetCurrentState(DomainManager.GClientID.ToString(),
                                    string.Format(Catalog.GetString("Batch {0} - Importing gift detail"), giftBatch.BatchNumber),
                                    (BatchDetailCounter / 50 + 2) * 10 > 90 ? 90 : (BatchDetailCounter / 50 + 2) * 10);
                            }
                        } // If known row analysisType
                        else
                        {
                            AMessages.Add(new TVerificationResult(String.Format(MCommonConstants.StrParsingErrorInLine, RowNumber),
                                Catalog.GetString("Invalid Row Type. Perhaps using wrong CSV separator?"), TResultSeverity.Resv_Critical));
                        }
                    }  // if the CSV line qualifies

                    if (AMessages.Count > 100)
                    {
                        // This probably means that it is a big file and the user has made the same mistake many times over
                        break;
                    }

                    // Read the next line
                    FImportLine = sr.ReadLine();
                }  // while CSV lines

                // Finished reading the file - did we have critical errors?
                if (!TVerificationHelper.IsNullOrOnlyNonCritical(AMessages))
                {
                    TProgressTracker.SetCurrentState(DomainManager.GClientID.ToString(),
                        Catalog.GetString("Batch has critical errors"),
                        0);

                    // Record error count
                    AMessages.Add(new TVerificationResult(MCommonConstants.StrImportInformation,
                        String.Format(Catalog.GetString("{0} messages reported."), AMessages.Count), TResultSeverity.Resv_Info));

                    if (FImportLine == null)
                    {
                        // We did reach the end of the file
                        AMessages.Add(new TVerificationResult(MCommonConstants.StrImportInformation,
                            Catalog.GetString(
                                "Reached the end of file but errors occurred. When these errors are fixed the batch will import successfully."),
                            TResultSeverity.Resv_Info));
                    }
                    else
                    {
                        // We gave up before the end
                        AMessages.Add(new TVerificationResult(MCommonConstants.StrImportInformation,
                            Catalog.GetString(
                                "Stopped reading the file after generating more than 100 messages.  The file may contian more errors beyond the ones listed here."),
                            TResultSeverity.Resv_Info));
                    }

                    TLogging.Log("Return from here!");

                    // Do the 'finally' actions and return false
                    return false;
                }

                // Everything is ok, so we can do our finish actions

                //Update batch total for the last batch entered.
                if (giftBatch != null)
                {
                    giftBatch.BatchTotal = totalBatchAmount;
                }

                ImportMessage = Catalog.GetString("Saving all data into the database");

                //Finally save pending changes (the last number is updated !)
                AGiftBatchAccess.SubmitChanges(FMainDS.AGiftBatch, Transaction);
                ALedgerAccess.SubmitChanges(LedgerTable, Transaction);
                FMainDS.AGiftBatch.AcceptChanges();
                FMainDS.ALedger.AcceptChanges();

                // Commit the transaction (we know that we got a new one and can control it)
                DBAccess.GDBAccessObj.CommitTransaction();
                ok = true;
            }
            catch (Exception ex)
            {
                // Parse the exception text for possible references to database foreign keys
                // Make the message more friendly in that case
                string friendlyExceptionText = MakeFriendlyFKExceptions(ex);

                if (AMessages == null)
                {
                    AMessages = new TVerificationResultCollection();
                }

                if (RowNumber > 0)
                {
                    // At least we made a start
                    string msg = ImportMessage;

                    if (friendlyExceptionText.Length > 0)
                    {
                        msg += FNewLine + friendlyExceptionText;
                    }

                    AMessages.Add(new TVerificationResult(String.Format(MCommonConstants.StrExceptionWhileParsingLine, RowNumber),
                        msg, TResultSeverity.Resv_Critical));
                }
                else
                {
                    // We got an exception before we even started parsing the rows (getting a transaction?)
                    AMessages.Add(new TVerificationResult(String.Format(MCommonConstants.StrExceptionWhileParsingLine, RowNumber),
                        friendlyExceptionText, TResultSeverity.Resv_Critical));
                }

                TProgressTracker.SetCurrentState(DomainManager.GClientID.ToString(),
                    Catalog.GetString("Exception Occurred"),
                    0);

                ok = false;
            }
            finally
            {
                try
                {
                    sr.Close();
                }
                catch (Exception Exc)
                {
                    TLogging.Log("An Exception occured while closing the Import File:" + Environment.NewLine + Exc.ToString());

                    if (AMessages == null)
                    {
                        AMessages = new TVerificationResultCollection();
                    }

                    AMessages.Add(new TVerificationResult(Catalog.GetString("Import exception"),
                            Catalog.GetString("A problem was encountered while closing the Import File:"),
                            TResultSeverity.Resv_Critical));

                    TProgressTracker.SetCurrentState(DomainManager.GClientID.ToString(),
                        Catalog.GetString("Exception Occurred"),
                        0);

                    TProgressTracker.FinishJob(DomainManager.GClientID.ToString());

                    throw;
                }

                if (ok)
                {
                    TProgressTracker.SetCurrentState(DomainManager.GClientID.ToString(),
                        Catalog.GetString("Gift batch import successful"),
                        100);
                }
                else
                {
                    DBAccess.GDBAccessObj.RollbackTransaction();

                    if (AMessages == null)
                    {
                        AMessages = new TVerificationResultCollection();
                    }

                    AMessages.Add(new TVerificationResult(MCommonConstants.StrImportInformation,
                        Catalog.GetString("None of the data from the import was saved."),
                        TResultSeverity.Resv_Critical));

                    TProgressTracker.SetCurrentState(DomainManager.GClientID.ToString(),
                        Catalog.GetString("Data could not be saved."),
                        0);
                }

                TProgressTracker.FinishJob(DomainManager.GClientID.ToString());
            } // end of 'finally'

            return ok;
        }

        private void ParseBatchLine(ref AGiftBatchRow AGiftBatch, ref TDBTransaction ATransaction, ref ALedgerTable ALedgerTable,
            ref string AImportMessage, int ARowNumber, TVerificationResultCollection AMessages, TValidationControlsDict AValidationControlsDictBatch,
            AAccountTable AValidationAccountTable, ACostCentreTable AValidationCostCentreTable, ACorporateExchangeRateTable AValidationCorporateExchTable)
        {
            // There are 8 elements to import
            string BatchDescription = ImportString(Catalog.GetString("Batch description"),
                FMainDS.AGiftBatch.ColumnBatchDescription, AValidationControlsDictBatch);
            string BankAccountCode = ImportString(Catalog.GetString("Bank account code"),
                FMainDS.AGiftBatch.ColumnBankAccountCode, AValidationControlsDictBatch);
            decimal HashTotal = ImportDecimal(Catalog.GetString("Hash total"),
                FMainDS.AGiftBatch.ColumnHashTotal, ARowNumber, AMessages, AValidationControlsDictBatch);
            DateTime GlEffectiveDate = ImportDate(Catalog.GetString("Effective Date"),
                FMainDS.AGiftBatch.ColumnGlEffectiveDate, ARowNumber, AMessages, AValidationControlsDictBatch);

            AImportMessage = "Creating new batch";

            // This call sets: BatchNumber, BatchYear, BatchPeriod, GlEffectiveDate, ExchangeRateToBase, BatchDescription, BankAccountCode
            //  BankCostCentre and CurrencyCode.  The effective date will NOT be modified.
            //  The first three are not validated because they should be ok by default
            AGiftBatch = TGiftBatchFunctions.CreateANewGiftBatchRow(ref FMainDS,
                ref ATransaction,
                ref ALedgerTable,
                FLedgerNumber,
                GlEffectiveDate,
                false);

            // Now we modify some of these in the light of the imported data
            AGiftBatch.BatchDescription = BatchDescription;
            AGiftBatch.BankAccountCode = BankAccountCode;
            AGiftBatch.HashTotal = HashTotal;
            AGiftBatch.CurrencyCode = ImportString(Catalog.GetString("Currency code"),
                FMainDS.AGiftBatch.ColumnCurrencyCode, AValidationControlsDictBatch);
            AGiftBatch.ExchangeRateToBase = ImportDecimal(Catalog.GetString("Exchange rate to base"),
                FMainDS.AGiftBatch.ColumnExchangeRateToBase, ARowNumber, AMessages, AValidationControlsDictBatch);

            AGiftBatch.BankCostCentre = ImportString(Catalog.GetString("Bank cost centre"),
                FMainDS.AGiftBatch.ColumnBankCostCentre, AValidationControlsDictBatch);
            AGiftBatch.GiftType = ImportString(Catalog.GetString("Gift type"),
                FMainDS.AGiftBatch.ColumnGiftType, AValidationControlsDictBatch, false);     // can be empty, will default to GIFT

            int messageCountBeforeValidate = AMessages.Count;

            // Do our standard gift batch validation checks on this row
            AImportMessage = Catalog.GetString("Validating the gift batch data");
            AGiftBatchValidation.Validate(this, AGiftBatch, ref AMessages, AValidationControlsDictBatch);

            // And do the additional manual ones
            AImportMessage = Catalog.GetString("Additional validation of the gift batch data");
            TSharedFinanceValidation_Gift.ValidateGiftBatchManual(this, AGiftBatch, ref AMessages,
                AValidationControlsDictBatch, AValidationAccountTable, AValidationCostCentreTable, AValidationCorporateExchTable, FLedgerBaseCurrency);

            for (int i = messageCountBeforeValidate; i < AMessages.Count; i++)
            {
                ((TVerificationResult)AMessages[i]).OverrideResultContext(String.Format(MCommonConstants.StrValidationErrorInLine, ARowNumber));

                if (AMessages[i] is TScreenVerificationResult)
                {
                    TVerificationResult downgrade = new TVerificationResult((TScreenVerificationResult)AMessages[i]);
                    AMessages.RemoveAt(i);
                    AMessages.Insert(i, downgrade);
                }
            }

            if (AGiftBatch.ExchangeRateToBase > 10000000)  // Huge numbers here indicate that the decimal comma/point is incorrect.
            {
                AMessages.Add(new TVerificationResult(String.Format(MCommonConstants.StrImportValidationErrorInLine, ARowNumber),
                    String.Format(Catalog.GetString("A huge exchange rate of {0} suggests a decimal point format problem."),
                        AGiftBatch.ExchangeRateToBase),
                    TResultSeverity.Resv_Noncritical));
            }

            // If this batch is in my base currency, the ExchangeRateToBase must be 1:
            if ((AGiftBatch.CurrencyCode == FLedgerBaseCurrency)
                && (AGiftBatch.ExchangeRateToBase != 1.0m))
            {
                AMessages.Add(new TVerificationResult(String.Format(MCommonConstants.StrImportValidationErrorInLine, ARowNumber),
                    Catalog.GetString("A batch in the ledger base currency must have exchange rate of 1.00."), TResultSeverity.Resv_Critical));
            }
        }

        private void ParseTransactionLine(AGiftRow AGift, AGiftBatchRow AGiftBatch, ref AGiftRow APreviousGift, int ANumberOfColumns,
            ref decimal ATotalBatchAmount, ref string AImportMessage, int ARowNumber, TVerificationResultCollection AMessages,
            TValidationControlsDict AValidationControlsDictGift, TValidationControlsDict AValidationControlsDictGiftDetail,
            ACostCentreTable AValidationCostCentreTable, out AGiftDetailRow AGiftDetails)
        {
            //this is the format with extra columns
            bool HasExtraColumns = (ANumberOfColumns >= 27);

            AImportMessage = Catalog.GetString("Importing the gift data");

            AGift.DonorKey = ImportInt64(Catalog.GetString("Donor key"),
                FMainDS.AGift.ColumnDonorKey, ARowNumber, AMessages, AValidationControlsDictGift);

            ImportString(Catalog.GetString("short name of donor (unused)"), null, null); // unused

            AGift.MethodOfGivingCode = ImportString(Catalog.GetString("Method of giving Code"),
                FMainDS.AGift.ColumnMethodOfGivingCode, AValidationControlsDictGift, false);
            AGift.MethodOfPaymentCode = ImportString(Catalog.GetString("Method Of Payment Code"),
                FMainDS.AGift.ColumnMethodOfPaymentCode, AValidationControlsDictGift, false);
            AGift.Reference = ImportString(Catalog.GetString("Reference"),
                FMainDS.AGift.ColumnReference, AValidationControlsDictGift, false);
            AGift.ReceiptLetterCode = ImportString(Catalog.GetString("Receipt letter code"),
                FMainDS.AGift.ColumnReceiptLetterCode, AValidationControlsDictGift, false);

            if (HasExtraColumns)
            {
                AGift.ReceiptNumber = ImportInt32(Catalog.GetString("Receipt number"),
                    FMainDS.AGift.ColumnReceiptNumber, ARowNumber, AMessages, AValidationControlsDictGift);
                AGift.FirstTimeGift = ImportBoolean(Catalog.GetString("First time gift"),
                    FMainDS.AGift.ColumnFirstTimeGift, AValidationControlsDictGift);
                AGift.ReceiptPrinted = ImportBoolean(Catalog.GetString("Receipt printed"),
                    FMainDS.AGift.ColumnReceiptPrinted, AValidationControlsDictGift);
            }

            AImportMessage = Catalog.GetString("Importing the gift details");

            AGiftDetails = FMainDS.AGiftDetail.NewRowTyped(true);

            if ((APreviousGift != null) && (AGift.DonorKey == APreviousGift.DonorKey)
                && (AGift.MethodOfGivingCode == APreviousGift.MethodOfGivingCode)
                && (AGift.MethodOfPaymentCode == APreviousGift.MethodOfPaymentCode)
                && (AGift.Reference == APreviousGift.Reference)
                && (AGift.ReceiptLetterCode == APreviousGift.ReceiptLetterCode)
                && (AGift.ReceiptNumber == APreviousGift.ReceiptNumber)
                && (AGift.FirstTimeGift == APreviousGift.FirstTimeGift)
                && (AGift.ReceiptPrinted == APreviousGift.ReceiptPrinted))
            {
                // this row is a new detail for the previousGift
                AGift = APreviousGift;
                AGift.LastDetailNumber++;
                AGiftDetails.DetailNumber = AGift.LastDetailNumber;
            }
            else
            {
                APreviousGift = AGift;
                AGift.LedgerNumber = AGiftBatch.LedgerNumber;
                AGift.BatchNumber = AGiftBatch.BatchNumber;
                AGift.GiftTransactionNumber = AGiftBatch.LastGiftNumber + 1;
                AGiftBatch.LastGiftNumber++;
                AGift.LastDetailNumber = 1;
                FMainDS.AGift.Rows.Add(AGift);
                AGiftDetails.DetailNumber = 1;
            }

            AGiftDetails.LedgerNumber = AGift.LedgerNumber;
            AGiftDetails.BatchNumber = AGiftBatch.BatchNumber;
            AGiftDetails.GiftTransactionNumber = AGift.GiftTransactionNumber;
            FMainDS.AGiftDetail.Rows.Add(AGiftDetails);

            AGiftDetails.RecipientKey = ImportInt64(Catalog.GetString("Recipient key"),
                FMainDS.AGiftDetail.ColumnRecipientKey, ARowNumber, AMessages, AValidationControlsDictGiftDetail);

            ImportString(Catalog.GetString("short name of recipient (unused)"), null, null); // unused

            if (HasExtraColumns)
            {
                AGiftDetails.RecipientLedgerNumber = ImportInt32(Catalog.GetString("Recipient ledger number"),
                    FMainDS.AGiftDetail.ColumnRecipientLedgerNumber, ARowNumber, AMessages, AValidationControlsDictGiftDetail);
            }

            decimal currentGiftAmount = ImportDecimal(Catalog.GetString("Gift amount"),
                FMainDS.AGiftDetail.ColumnGiftTransactionAmount, ARowNumber, AMessages, AValidationControlsDictGiftDetail);
            AGiftDetails.GiftTransactionAmount = currentGiftAmount;     // amount in batch currency
            ATotalBatchAmount += currentGiftAmount;

            AGiftDetails.GiftAmount = GLRoutines.Divide(currentGiftAmount, AGiftBatch.ExchangeRateToBase);      // amount in ledger currency

            if (HasExtraColumns)
            {
                // amount in international currency
                AGiftDetails.GiftAmountIntl = ImportDecimal(Catalog.GetString("Gift amount intl"),
                    FMainDS.AGiftDetail.ColumnGiftAmountIntl, ARowNumber, AMessages, AValidationControlsDictGiftDetail);
            }

            AGiftDetails.ConfidentialGiftFlag = ImportBoolean(Catalog.GetString("Confidential gift"),
                FMainDS.AGiftDetail.ColumnConfidentialGiftFlag, AValidationControlsDictGiftDetail);
            AGiftDetails.MotivationGroupCode = ImportString(Catalog.GetString("Motivation group code"),
                FMainDS.AGiftDetail.ColumnMotivationGroupCode, AValidationControlsDictGiftDetail);
            AGiftDetails.MotivationDetailCode = ImportString(Catalog.GetString("Motivation detail"),
                FMainDS.AGiftDetail.ColumnMotivationDetailCode, AValidationControlsDictGiftDetail);

            if (HasExtraColumns)
            {
                AGiftDetails.CostCentreCode = ImportString(Catalog.GetString("Cost centre code"),
                    FMainDS.AGiftDetail.ColumnCostCentreCode, AValidationControlsDictGiftDetail);
            }
            else
            {
                // "In Petra Cost Centre is always inferred from recipient field and motivation detail so is not needed in the import."
                AGiftDetails.CostCentreCode = InferCostCentre(AGiftDetails);
            }

            AGiftDetails.GiftCommentOne = ImportString(Catalog.GetString("Gift comment one"),
                FMainDS.AGiftDetail.ColumnGiftCommentOne, AValidationControlsDictGiftDetail, false);
            AGiftDetails.CommentOneType = ImportString(Catalog.GetString("Comment one type"),
                FMainDS.AGiftDetail.ColumnCommentOneType, AValidationControlsDictGiftDetail, false);

            AGiftDetails.MailingCode = ImportString(Catalog.GetString("Mailing code"),
                FMainDS.AGiftDetail.ColumnMailingCode, AValidationControlsDictGiftDetail);

            AGiftDetails.GiftCommentTwo = ImportString(Catalog.GetString("Gift comment two"),
                FMainDS.AGiftDetail.ColumnGiftCommentTwo, AValidationControlsDictGiftDetail, false);
            AGiftDetails.CommentTwoType = ImportString(Catalog.GetString("Comment two type"),
                FMainDS.AGiftDetail.ColumnCommentTwoType, AValidationControlsDictGiftDetail, false);
            AGiftDetails.GiftCommentThree = ImportString(Catalog.GetString("Gift comment three"),
                FMainDS.AGiftDetail.ColumnGiftCommentThree, AValidationControlsDictGiftDetail, false);
            AGiftDetails.CommentThreeType = ImportString(Catalog.GetString("Comment three type"),
                FMainDS.AGiftDetail.ColumnCommentThreeType, AValidationControlsDictGiftDetail, false);
            AGiftDetails.TaxDeductible = ImportBoolean(Catalog.GetString("Tax deductible"),
                FMainDS.AGiftDetail.ColumnTaxDeductible, AValidationControlsDictGiftDetail);

            AGift.DateEntered = AGiftBatch.GlEffectiveDate;

            AImportMessage = Catalog.GetString("Validating the gift data");

            int messageCountBeforeValidate = AMessages.Count;

            // Do our standard validation on this gift
            AGiftValidation.Validate(this, AGift, ref AMessages, AValidationControlsDictGift);
            TSharedFinanceValidation_Gift.ValidateGiftManual(this, AGift, AGiftBatch.BatchYear, AGiftBatch.BatchPeriod,
                null, ref AMessages, AValidationControlsDictGift);

            AImportMessage = Catalog.GetString("Validating the gift details data");

            AGiftDetailValidation.Validate(this, AGiftDetails, ref AMessages, AValidationControlsDictGiftDetail);
            TSharedFinanceValidation_Gift.ValidateGiftDetailManual(this, (GiftBatchTDSAGiftDetailRow)AGiftDetails,
                ref AMessages, AValidationControlsDictGiftDetail, AValidationCostCentreTable, AGiftDetails.RecipientKey);

            for (int i = messageCountBeforeValidate; i < AMessages.Count; i++)
            {
                ((TVerificationResult)AMessages[i]).OverrideResultContext(String.Format(MCommonConstants.StrValidationErrorInLine, ARowNumber));

                if (AMessages[i] is TScreenVerificationResult)
                {
                    TVerificationResult downgrade = new TVerificationResult((TScreenVerificationResult)AMessages[i]);
                    AMessages.RemoveAt(i);
                    AMessages.Insert(i, downgrade);
                }
            }
        }

        /// <summary>
        /// returns the most recently imported gift batch
        /// </summary>
        public Int32 GetLastGiftBatchNumber()
        {
            if ((FMainDS != null) && (FMainDS.AGiftBatch != null) && (FMainDS.AGiftBatch.Count > 0))
            {
                return FMainDS.AGiftBatch[FMainDS.AGiftBatch.Count - 1].BatchNumber;
            }

            return -1;
        }

        private String MakeFriendlyFKExceptions(Exception ex)
        {
            //note that this is only done for "user errors" not for program errors!
            String innerMessage = ex.InnerException.ToString();

            string formatStr = Catalog.GetString("  Do you need to add this code to the '{0}' main setup screen?");
            string createNewPartner = Catalog.GetString("  Do you need to add this partner using the 'Create Partner' main setup screen?");

            if (innerMessage.Contains("a_gift_batch_fk2"))
            {
                return Catalog.GetString("Unknown account code.") + String.Format(formatStr, "Manage Accounts");
            }

            if (innerMessage.Contains("a_gift_batch_fk3"))
            {
                return Catalog.GetString("Unknown cost centre.") + String.Format(formatStr, "Manage Cost Centres");
            }

            if (innerMessage.Contains("a_gift_batch_fk4"))
            {
                return Catalog.GetString("Unknown currency code.") + String.Format(formatStr, "Currencies");
            }

            if (innerMessage.Contains("a_gift_fk2"))
            {
                return Catalog.GetString("Unknown method of giving.") + String.Format(formatStr, "Methods Of Giving");
            }

            if (innerMessage.Contains("a_gift_fk3"))
            {
                return Catalog.GetString("Unknown method of payment.") + String.Format(formatStr, "Methods Of Payment");
            }

            if (innerMessage.Contains("a_gift_fk4"))
            {
                return Catalog.GetString("Unknown donor partner key.") + createNewPartner;
            }

            if (innerMessage.Contains("a_gift_detail_fk2"))
            {
                return Catalog.GetString("Unknown motivation detail.") + String.Format(formatStr, "Motivation Details");
            }

            if (innerMessage.Contains("a_gift_detail_fk3"))
            {
                return Catalog.GetString("Unknown recipient partner key.") + createNewPartner;
            }

            if (innerMessage.Contains("a_gift_detail_fk4"))
            {
                return Catalog.GetString("Unknown mailing code.") + String.Format(formatStr, "Mailings");
            }

            if (innerMessage.Contains("a_gift_detail_fk5"))
            {
                return Catalog.GetString("Unknown recipient ledger number.") + createNewPartner;
            }

            if (innerMessage.Contains("a_gift_detail_fk6"))
            {
                return Catalog.GetString("Unknown cost centre.") + String.Format(formatStr, "Manage Cost Centres");
            }

            TLogging.Log("Importing Gift batch: " + ex.ToString());

            return ex.Message;
        }

        private String ImportString(String AColumnTitle,
            DataColumn ADataColumn,
            TValidationControlsDict AValidationColumnsDict,
            bool ATreatEmptyStringAsText = true)
        {
            if ((ADataColumn != null) && (AValidationColumnsDict != null) && !AValidationColumnsDict.ContainsKey(ADataColumn))
            {
                AValidationColumnsDict.Add(ADataColumn, new TValidationControlsData(null, AColumnTitle));
            }

            String sReturn = StringHelper.GetNextCSV(ref FImportLine, FDelimiter);

            if ((sReturn.Length == 0) && !ATreatEmptyStringAsText)
            {
                return null;
            }

            return sReturn;
        }

        private Boolean ImportBoolean(String AColumnTitle, DataColumn ADataColumn, TValidationControlsDict AValidationColumnsDict)
        {
            if ((ADataColumn != null) && (AValidationColumnsDict != null) && !AValidationColumnsDict.ContainsKey(ADataColumn))
            {
                AValidationColumnsDict.Add(ADataColumn, new TValidationControlsData(null, AColumnTitle));
            }

            String sReturn = StringHelper.GetNextCSV(ref FImportLine, FDelimiter);
            return sReturn.ToLower().Equals("yes");
        }

        private Int64 ImportInt64(String AColumnTitle,
            DataColumn ADataColumn,
            int ARowNumber,
            TVerificationResultCollection AMessages,
            TValidationControlsDict AValidationColumnsDict)
        {
            if ((ADataColumn != null) && (AValidationColumnsDict != null) && !AValidationColumnsDict.ContainsKey(ADataColumn))
            {
                AValidationColumnsDict.Add(ADataColumn, new TValidationControlsData(null, AColumnTitle));
            }

            String sReturn = StringHelper.GetNextCSV(ref FImportLine, FDelimiter);
            Int64 retVal;

            if (Int64.TryParse(sReturn, out retVal))
            {
                return retVal;
            }

            AMessages.Add(new TVerificationResult(String.Format(MCommonConstants.StrParsingErrorInLineColumn, ARowNumber, AColumnTitle),
                String.Format(Catalog.GetString("Cannot convert '{0}' to a number. Will assume a value of -1."), sReturn),
                TResultSeverity.Resv_Critical));
            return -1;
        }

        private Int32 ImportInt32(String AColumnTitle,
            DataColumn ADataColumn,
            int ARowNumber,
            TVerificationResultCollection AMessages,
            TValidationControlsDict AValidationColumnsDict)
        {
            if ((ADataColumn != null) && (AValidationColumnsDict != null) && !AValidationColumnsDict.ContainsKey(ADataColumn))
            {
                AValidationColumnsDict.Add(ADataColumn, new TValidationControlsData(null, AColumnTitle));
            }

            String sReturn = StringHelper.GetNextCSV(ref FImportLine, FDelimiter);
            Int32 retVal;

            if (Int32.TryParse(sReturn, out retVal))
            {
                return retVal;
            }

            AMessages.Add(new TVerificationResult(String.Format(MCommonConstants.StrParsingErrorInLineColumn, ARowNumber, AColumnTitle),
                String.Format(Catalog.GetString("Cannot convert '{0}' to a number. Will assume a value of -1."), sReturn),
                TResultSeverity.Resv_Critical));
            return -1;
        }

        private decimal ImportDecimal(String AColumnTitle,
            DataColumn ADataColumn,
            int ARowNumber,
            TVerificationResultCollection AMessages,
            TValidationControlsDict AValidationColumnsDict)
        {
            if ((ADataColumn != null) && (AValidationColumnsDict != null) && !AValidationColumnsDict.ContainsKey(ADataColumn))
            {
                AValidationColumnsDict.Add(ADataColumn, new TValidationControlsData(null, AColumnTitle));
            }

            String sReturn = StringHelper.GetNextCSV(ref FImportLine, FDelimiter);
            try
            {
                decimal dec = Convert.ToDecimal(sReturn, FCultureInfoNumberFormat);
                return dec;
            }
            catch
            {
                AMessages.Add(new TVerificationResult(String.Format(MCommonConstants.StrParsingErrorInLineColumn, ARowNumber, AColumnTitle),
                    String.Format(Catalog.GetString("Cannot convert '{0}' to a decimal number. Will assume a value of 1.00."), sReturn),
                    TResultSeverity.Resv_Critical));
                return 1.0m;
            }
        }

        private DateTime ImportDate(String AColumnTitle,
            DataColumn ADataColumn,
            int ARowNumber,
            TVerificationResultCollection AMessages,
            TValidationControlsDict AValidationColumnsDict)
        {
            if ((ADataColumn != null) && (AValidationColumnsDict != null) && !AValidationColumnsDict.ContainsKey(ADataColumn))
            {
                AValidationColumnsDict.Add(ADataColumn, new TValidationControlsData(null, AColumnTitle));
            }

            String sDate = StringHelper.GetNextCSV(ref FImportLine, FDelimiter);
            DateTime dtReturn;

            try
            {
                dtReturn = Convert.ToDateTime(sDate, FCultureInfoDate);
            }
            catch (Exception)
            {
                AMessages.Add(new TVerificationResult(String.Format(MCommonConstants.StrParsingErrorInLineColumn, ARowNumber, AColumnTitle),
                    String.Format(Catalog.GetString("Cannot convert '{0}' to a date. Will assume a value of 'Today'."), sDate),
                    TResultSeverity.Resv_Critical));
                TLogging.Log("Problem parsing " + sDate + " with format " + FCultureInfoDate.DateTimeFormat.ShortDatePattern);
                return DateTime.Today;
            }

            return dtReturn;
        }
    }
}