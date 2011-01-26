﻿//
// DO NOT REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
//
// @Authors:
//       matthiash
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
using Ict.Petra.Server.MFinance.Account.Data.Access;
using Ict.Petra.Server.MPartner.Partner.Data.Access;
using Ict.Petra.Server.MSysMan.Data.Access;
using Ict.Petra.Shared;
using Ict.Petra.Shared.MFinance.Account.Data;
using Ict.Petra.Shared.MFinance.GL.Data;
using Ict.Petra.Shared.MPartner.Partner.Data;
using Ict.Petra.Shared.MSysMan.Data;


namespace Ict.Petra.Server.MFinance.GL
{
    /// <summary>
    /// Import a GL Batch
    /// </summary>
    public class TGLImporting
    {
        String FDelimiter;
        Int32 FLedgerNumber;
        String FDateFormatString;
        TDBTransaction FTransaction;
        GLSetupTDS FSetupDS;
        GLBatchTDS FMainDS;
        CultureInfo FCultureInfoNumberFormat;
        CultureInfo FCultureInfoDate;


        private String FImportMessage;
        private String FImportLine;
        private String FNewLine;


        /// <summary>
        /// Import GL Batches data
        /// The data file contents from the client is sent as a string, imported in the database
        /// and committed immediately
        /// </summary>
        /// <param name="requestParams">Hashtable containing the given params </param>
        /// <param name="importString">Big parts of the export file as a simple String</param>
        /// <param name="AMessages">Additional messages to display in a messagebox</param>
        /// <returns>false if error</returns>
        public bool ImportGLBatches(
            Hashtable requestParams,
            String importString,
            out TVerificationResultCollection AMessages
            )
        {
            AMessages = new TVerificationResultCollection();
            FMainDS = new GLBatchTDS();
            FSetupDS = new GLSetupTDS();
            StringReader sr = new StringReader(importString);

            FDelimiter = (String)requestParams["Delimiter"];
            FLedgerNumber = (Int32)requestParams["ALedgerNumber"];
            FDateFormatString = (String)requestParams["DateFormatString"];
            String NumberFormat = (String)requestParams["NumberFormat"];
            FNewLine = (String)requestParams["NewLine"];

            FCultureInfoNumberFormat = new CultureInfo(NumberFormat.Equals("American") ? "en-US" : "de-DE");
            FCultureInfoDate = new CultureInfo("en-GB");
            FCultureInfoDate.DateTimeFormat.ShortDatePattern = FDateFormatString;

            FTransaction = DBAccess.GDBAccessObj.BeginTransaction(IsolationLevel.ReadCommitted);

            AAnalysisTypeAccess.LoadAll(FSetupDS, FTransaction);
            AFreeformAnalysisAccess.LoadViaALedger(FSetupDS, FLedgerNumber, FTransaction);
            AAnalysisAttributeAccess.LoadViaALedger(FSetupDS, FLedgerNumber, FTransaction);

            ABatchRow NewBatch = null;
            AJournalRow NewJournal = null;

            //AGiftRow gift = null;
            FImportMessage = Catalog.GetString("Parsing first line");
            Int32 RowNumber = 0;
            bool ok = false;
            try
            {
                ALedgerAccess.LoadByPrimaryKey(FMainDS, FLedgerNumber, FTransaction);

                while ((FImportLine = sr.ReadLine()) != null)
                {
                    RowNumber++;

                    // skip empty lines and commented lines
                    if ((FImportLine.Trim().Length > 0) && !FImportLine.StartsWith("/*") && !FImportLine.StartsWith("#"))
                    {
                        string RowType = ImportString(Catalog.GetString("row type"));

                        if (RowType == "B")
                        {
                            NewBatch = FMainDS.ABatch.NewRowTyped(true);
                            NewBatch.LedgerNumber = FLedgerNumber;
                            FMainDS.ALedger[0].LastBatchNumber++;
                            NewBatch.BatchNumber = FMainDS.ALedger[0].LastBatchNumber;
                            NewBatch.BatchPeriod = FMainDS.ALedger[0].CurrentPeriod;
                            FMainDS.ABatch.Rows.Add(NewBatch);
                            NewJournal = null;


                            NewBatch.BatchDescription = ImportString(Catalog.GetString("batch description"));
                            NewBatch.BatchControlTotal = ImportDecimal(Catalog.GetString("batch hash value"));
                            NewBatch.DateEffective = ImportDate(Catalog.GetString("batch  effective date"));
                            FImportMessage = Catalog.GetString("Saving GL batch:");

                            if (!ABatchAccess.SubmitChanges(FMainDS.ABatch, FTransaction, out AMessages))
                            {
                                return false;
                            }

                            FMainDS.ABatch.AcceptChanges();
                        }
                        else if (RowType == "J")
                        {
                            if (NewBatch == null)
                            {
                                FImportMessage = Catalog.GetString("Expected a Batch line, but found a Journal");
                                throw new Exception();
                            }

                            NewJournal = FMainDS.AJournal.NewRowTyped(true);
                            NewJournal.LedgerNumber = NewBatch.LedgerNumber;
                            NewJournal.BatchNumber = NewBatch.BatchNumber;
                            NewJournal.JournalNumber = NewBatch.LastJournal + 1;
                            NewJournal.SubSystemCode = "GL";
                            NewJournal.TransactionTypeCode = "STD";
                            NewJournal.TransactionCurrency = "EUR";
                            NewJournal.ExchangeRateToBase = 1;
                            NewJournal.DateEffective = NewBatch.DateEffective;
                            NewJournal.JournalPeriod = NewBatch.BatchPeriod;
                            NewBatch.LastJournal++;

                            FMainDS.AJournal.Rows.Add(NewJournal);

                            NewJournal.JournalDescription = ImportString(Catalog.GetString("journal") + " - " + Catalog.GetString("description"));
                            NewJournal.SubSystemCode = ImportString(Catalog.GetString("journal") + " - " + Catalog.GetString("sub system code"));
                            NewJournal.TransactionTypeCode = ImportString(Catalog.GetString("journal") + " - " + Catalog.GetString("transaction type"));
                            NewJournal.TransactionCurrency =
                                ImportString(Catalog.GetString("journal") + " - " + Catalog.GetString("transaction currency"));
                            NewJournal.ExchangeRateToBase = ImportDecimal(Catalog.GetString("journal") + " - " + Catalog.GetString("exchange rate"));
                            NewJournal.DateEffective = ImportDate(Catalog.GetString("journal") + " - " + Catalog.GetString("effective date"));

                            FImportMessage = Catalog.GetString("Saving the journal:");

                            if (!AJournalAccess.SubmitChanges(FMainDS.AJournal, FTransaction, out AMessages))
                            {
                                return false;
                            }

                            FMainDS.AJournal.AcceptChanges();
                        }
                        else if (RowType == "T")
                        {
                            if (NewJournal == null)
                            {
                                FImportMessage = Catalog.GetString("Expected a Journal or Batch line, but found a Transaction");
                                throw new Exception();
                            }

                            GLBatchTDSATransactionRow NewTransaction = FMainDS.ATransaction.NewRowTyped(true);
                            NewTransaction.LedgerNumber = NewJournal.LedgerNumber;
                            NewTransaction.BatchNumber = NewJournal.BatchNumber;
                            NewTransaction.JournalNumber = NewJournal.JournalNumber;
                            NewTransaction.TransactionNumber = NewJournal.LastTransactionNumber + 1;
                            NewJournal.LastTransactionNumber++;
                            FMainDS.ATransaction.Rows.Add(NewTransaction);


                            NewTransaction.CostCentreCode = ImportString(Catalog.GetString("transaction") + " - " + Catalog.GetString("cost centre"));
                            // TODO check if cost centre exists, and is a posting costcentre.
                            // TODO check if cost centre is active. ask user if he wants to use an inactive cost centre

                            NewTransaction.AccountCode = ImportString(Catalog.GetString("transaction") + " - " + Catalog.GetString("account code"));
                            // TODO check if account exists, and is a posting account.
                            // TODO check if account is active. warning when using an inactive account

                            NewTransaction.Narrative = ImportString(Catalog.GetString("transaction") + " - " + Catalog.GetString("narrative"));

                            NewTransaction.Reference = ImportString(Catalog.GetString("transaction") + " - " + Catalog.GetString("reference"));

                            NewTransaction.TransactionDate = ImportDate(Catalog.GetString("transaction") + " - " + Catalog.GetString("date"));


                            decimal DebitAmount = ImportDecimal(Catalog.GetString("transaction") + " - " + Catalog.GetString("debit amount"));
                            decimal CreditAmount = ImportDecimal(Catalog.GetString("transaction") + " - " + Catalog.GetString("credit amount"));

                            if ((DebitAmount == 0) && (CreditAmount == 0))
                            {
                                FImportMessage = Catalog.GetString("Either the debit amount or the debit amount must be greater than 0.");
                            }

                            if ((DebitAmount != 0) && (CreditAmount != 0))
                            {
                                FImportMessage = Catalog.GetString("You can not have a value for both debit and credit amount");
                            }

                            if (DebitAmount != 0)
                            {
                                NewTransaction.DebitCreditIndicator = true;
                                NewTransaction.TransactionAmount = DebitAmount;
                                NewJournal.JournalDebitTotal += DebitAmount;
                                NewBatch.BatchDebitTotal += DebitAmount;
                                //NewBatch.BatchControlTotal += DebitAmount;
                                NewBatch.BatchRunningTotal += DebitAmount;
                            }
                            else
                            {
                                NewTransaction.DebitCreditIndicator = false;
                                NewTransaction.TransactionAmount = CreditAmount;
                                NewJournal.JournalCreditTotal += CreditAmount;
                                NewBatch.BatchCreditTotal += CreditAmount;
                            }

                            for (int i = 0; i < 10; i++)
                            {
                                String type = ImportString(Catalog.GetString("Transaction") + " - " + Catalog.GetString("Analysis Type") + "#" + i);
                                String val = ImportString(Catalog.GetString("Transaction") + " - " + Catalog.GetString("Analysis Value") + "#" + i);

                                //these data is only be imported if all corresponding values are there
                                if ((type != null) && (type.Length > 0) && (val != null) && (val.Length > 0))
                                {
                                    DataRow atrow = FSetupDS.AAnalysisType.Rows.Find(new Object[] { type });
                                    DataRow afrow = FSetupDS.AFreeformAnalysis.Rows.Find(new Object[] { NewTransaction.LedgerNumber, type, val });
                                    DataRow anrow =
                                        FSetupDS.AAnalysisAttribute.Rows.Find(new Object[] { NewTransaction.LedgerNumber,
                                                                                             NewTransaction.AccountCode,
                                                                                             type });

                                    if ((atrow != null) && (afrow != null) && (anrow != null))
                                    {
                                        ATransAnalAttribRow NewTransAnalAttrib = FMainDS.ATransAnalAttrib.NewRowTyped(true);
                                        NewTransAnalAttrib.LedgerNumber = NewTransaction.LedgerNumber;
                                        NewTransAnalAttrib.BatchNumber = NewTransaction.BatchNumber;
                                        NewTransAnalAttrib.JournalNumber = NewTransaction.JournalNumber;
                                        NewTransAnalAttrib.TransactionNumber = NewTransaction.TransactionNumber;
                                        NewTransAnalAttrib.AnalysisTypeCode = type;
                                        NewTransAnalAttrib.AnalysisAttributeValue = val;
                                        NewTransAnalAttrib.AccountCode = NewTransaction.AccountCode;
                                        FMainDS.ATransAnalAttrib.Rows.Add(NewTransAnalAttrib);
                                    }
                                }
                            }

                            FImportMessage = Catalog.GetString("Saving the transaction:");

                            // TODO If this is a fund transfer to a foreign cost centre, check whether there are Key Ministries available for it.
                            if (!ATransactionAccess.SubmitChanges(FMainDS.ATransaction, FTransaction, out AMessages))
                            {
                                return false;
                            }

                            FMainDS.ATransaction.AcceptChanges();
                            FImportMessage = Catalog.GetString("Saving the attributes:");

                            if (!ATransAnalAttribAccess.SubmitChanges(FMainDS.ATransAnalAttrib, FTransaction, out AMessages))
                            {
                                return false;
                            }

                            FMainDS.ATransAnalAttrib.AcceptChanges();
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                }

                FImportMessage = Catalog.GetString("Saving counter fields:");

                //Finally save all pending changes (last xxx number is updated)
                if (ABatchAccess.SubmitChanges(FMainDS.ABatch, FTransaction, out AMessages))
                {
                    if (ALedgerAccess.SubmitChanges(FMainDS.ALedger, FTransaction, out AMessages))
                    {
                        if (AJournalAccess.SubmitChanges(FMainDS.AJournal, FTransaction, out AMessages))
                        {
                            ok = true;
                        }
                    }
                }

                FMainDS.AcceptChanges();
            }
            catch (Exception ex)
            {
                String speakingExceptionText = SpeakingExceptionMessage(ex);
                AMessages.Add(new TVerificationResult(Catalog.GetString("Import"),

                        String.Format(Catalog.GetString("There is a problem parsing the file in row {0}."), RowNumber) +
                        FNewLine +
                        Catalog.GetString(FImportMessage) + FNewLine + speakingExceptionText,
                        TResultSeverity.Resv_Critical));
                DBAccess.GDBAccessObj.RollbackTransaction();
                return false;
            }
            finally
            {
                try
                {
                    sr.Close();
                }
                catch
                {
                };
            }

            if (ok)
            {
                DBAccess.GDBAccessObj.CommitTransaction();
            }
            else
            {
                DBAccess.GDBAccessObj.RollbackTransaction();
                AMessages.Add(new TVerificationResult("Import",
                        Catalog.GetString("Data could not be saved."),
                        TResultSeverity.Resv_Critical));
            }

            return true;
        }

        private String SpeakingExceptionMessage(Exception ex)
        {
            //note that this is only done for "user errors" not for program errors!
            String theExMessage = ex.Message;

            if (theExMessage.Contains("a_journal_fk3"))
            {
                return Catalog.GetString("Invalid sub system code or transaction type code");
            }

            if (theExMessage.Contains("a_journal_fk4"))
            {
                return Catalog.GetString("Invalid transaction currency");
            }

            if (theExMessage.Contains("a_transaction_fk3"))
            {
                return Catalog.GetString("Invalid cost centre");
            }

            if (theExMessage.Contains("a_transaction_fk2"))
            {
                return Catalog.GetString("Invalid account code");
            }

            return ex.ToString();
        }

        private String ImportString(String message)
        {
            FImportMessage = String.Format(Catalog.GetString("Parsing the {0}:"), message);
            String sReturn = StringHelper.GetNextCSV(ref FImportLine, FDelimiter);

            if (sReturn.Length == 0)
            {
                return null;
            }

            return sReturn;
        }

        private Boolean ImportBoolean(String message)
        {
            FImportMessage = String.Format(Catalog.GetString("Parsing the {0}:"), message);
            String sReturn = StringHelper.GetNextCSV(ref FImportLine, FDelimiter);
            return sReturn.ToLower().Equals("yes");
        }

        private Int64 ImportInt64(String message)
        {
            FImportMessage = String.Format(Catalog.GetString("Parsing the {0}:"), message);
            String sReturn = StringHelper.GetNextCSV(ref FImportLine, FDelimiter);
            return Convert.ToInt64(sReturn);
        }

        private Int32 ImportInt32(String message)
        {
            FImportMessage = String.Format(Catalog.GetString("Parsing the {0}:"), message);
            String sReturn = StringHelper.GetNextCSV(ref FImportLine, FDelimiter);
            return Convert.ToInt32(sReturn);
        }

        private decimal ImportDecimal(String message)
        {
            FImportMessage = String.Format(Catalog.GetString("Parsing the {0}:"), message);
            String sReturn = StringHelper.GetNextCSV(ref FImportLine, FDelimiter);
            decimal dec = sReturn.Trim().Length == 0 ? 0.0M : Convert.ToDecimal(sReturn, FCultureInfoNumberFormat);
            return dec;
        }

        private DateTime ImportDate(String message)
        {
            FImportMessage = String.Format(Catalog.GetString("Parsing the {0}:"), message);
            String sDate = StringHelper.GetNextCSV(ref FImportLine, FDelimiter);
            DateTime dtReturn = Convert.ToDateTime(sDate, FCultureInfoDate);
            return dtReturn;
        }
    }
}