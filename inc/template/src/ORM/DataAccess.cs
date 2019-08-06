// Auto generated by nant generateORM from a template at inc\template\src\ORM\DataAccess.cs
//
// Do not modify this file manually!
//
{#GPLFILEHEADER}

using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using Ict.Common;
using Ict.Common.DB;
using Ict.Common.Verification;
using Ict.Common.Verification.Exceptions;
using Ict.Common.Data;
using Ict.Petra.Shared;
using Ict.Petra.Server.MCommon.Data.Cascading;
{#USINGNAMESPACES}

namespace {#NAMESPACE}
{
    {#TABLEACCESSLOOP}
}

{##TABLEACCESS}

{#TABLE_DESCRIPTION}
public class {#TABLENAME}Access : TTypedDataAccess
{

    /// CamelCase version of table name
    public const string DATATABLENAME = "{#TABLENAME}";

    /// original table name in database
    public const string DBTABLENAME = "{#SQLTABLENAME}";

    /// this method is called by all overloads
    public static void LoadAll(DataSet ADataSet, StringCollection AFieldList, TDBTransaction ATransaction, StringCollection AOrderBy, int AStartRecord, int AMaxRecords)
    {
        ATransaction.DataBaseObj.Select(ADataSet, ((GenerateSelectClause(AFieldList, {#TABLENAME}Table.TableId) + " FROM PUB_{#SQLTABLENAME}") +
                        GenerateOrderByClause(AOrderBy)), TTypedDataTable.GetTableName({#TABLENAME}Table.TableId), ATransaction, AStartRecord, AMaxRecords);
    }

    /// auto generated
    public static void LoadAll(DataSet AData, TDBTransaction ATransaction)
    {
        LoadAll(AData, null, ATransaction, null, 0, 0);
    }
    
    /// auto generated
    public static void LoadAll(DataSet AData, StringCollection AFieldList, TDBTransaction ATransaction)
    {
        LoadAll(AData, AFieldList, ATransaction, null, 0, 0);
    }

    /// auto generated
    public static {#TABLENAME}Table LoadAll(StringCollection AFieldList, TDBTransaction ATransaction, StringCollection AOrderBy, int AStartRecord, int AMaxRecords)
    {
        {#TABLENAME}Table Data = new {#TABLENAME}Table();
        ATransaction.DataBaseObj.SelectDT(Data, GenerateSelectClause(AFieldList, {#TABLENAME}Table.TableId) + " FROM PUB_{#SQLTABLENAME}" + GenerateOrderByClause(AOrderBy), ATransaction, null, AStartRecord, AMaxRecords);
        return Data;
    }
    
    /// auto generated
    public static {#TABLENAME}Table LoadAll(TDBTransaction ATransaction)
    {
        return LoadAll(null, ATransaction, null, 0, 0);
    }
    
    /// auto generated
    public static {#TABLENAME}Table LoadAll(StringCollection AFieldList, TDBTransaction ATransaction)
    {
        return LoadAll(AFieldList, ATransaction, null, 0, 0);
    }
{#IFDEF FORMALPARAMETERSPRIMARYKEY}

    /// this method is called by all overloads
    public static {#TABLENAME}Row LoadByPrimaryKey(DataSet ADataSet, {#FORMALPARAMETERSPRIMARYKEY}, StringCollection AFieldList, TDBTransaction ATransaction, StringCollection AOrderBy, int AStartRecord, int AMaxRecords)
    {
        return ({#TABLENAME}Row)LoadByPrimaryKey({#TABLENAME}Table.TableId, ADataSet, new System.Object[{#PRIMARYKEYNUMBERCOLUMNS}]{{#ACTUALPARAMETERSPRIMARYKEY}}, AFieldList, ATransaction, AOrderBy, AStartRecord, AMaxRecords);
    }

    /// auto generated
    public static {#TABLENAME}Row LoadByPrimaryKey(DataSet AData, {#FORMALPARAMETERSPRIMARYKEY}, TDBTransaction ATransaction)
    {
        return ({#TABLENAME}Row)LoadByPrimaryKey(AData, {#ACTUALPARAMETERSPRIMARYKEY}, null, ATransaction, null, 0, 0);
    }
    
    /// auto generated
    public static {#TABLENAME}Row LoadByPrimaryKey(DataSet AData, {#FORMALPARAMETERSPRIMARYKEY}, StringCollection AFieldList, TDBTransaction ATransaction)
    {
        return ({#TABLENAME}Row)LoadByPrimaryKey(AData, {#ACTUALPARAMETERSPRIMARYKEY}, AFieldList, ATransaction, null, 0, 0);
    }

    /// auto generated
    public static {#TABLENAME}Table LoadByPrimaryKey({#FORMALPARAMETERSPRIMARYKEY}, StringCollection AFieldList, TDBTransaction ATransaction, StringCollection AOrderBy, int AStartRecord, int AMaxRecords)
    {
        {#TABLENAME}Table Data = new {#TABLENAME}Table();
        LoadByPrimaryKey({#TABLENAME}Table.TableId, Data, new System.Object[{#PRIMARYKEYNUMBERCOLUMNS}]{{#ACTUALPARAMETERSPRIMARYKEY}}, AFieldList, ATransaction, AOrderBy, AStartRecord, AMaxRecords);
        return Data;
    }
    
    /// auto generated
    public static {#TABLENAME}Table LoadByPrimaryKey({#FORMALPARAMETERSPRIMARYKEY}, TDBTransaction ATransaction)
    {
        return LoadByPrimaryKey({#ACTUALPARAMETERSPRIMARYKEY}, null, ATransaction, null, 0, 0);
    }
    
    /// auto generated
    public static {#TABLENAME}Table LoadByPrimaryKey({#FORMALPARAMETERSPRIMARYKEY}, StringCollection AFieldList, TDBTransaction ATransaction)
    {
        return LoadByPrimaryKey({#ACTUALPARAMETERSPRIMARYKEY}, AFieldList, ATransaction, null, 0, 0);
    }
{#ENDIF FORMALPARAMETERSPRIMARYKEY}
{#IFDEF FORMALPARAMETERSUNIQUEKEY}
    /// this method is called by all overloads
    public static void LoadByUniqueKey(DataSet ADataSet, {#FORMALPARAMETERSUNIQUEKEY}, StringCollection AFieldList, TDBTransaction ATransaction, StringCollection AOrderBy, int AStartRecord, int AMaxRecords)
    {
        LoadByUniqueKey({#TABLENAME}Table.TableId, ADataSet, new System.Object[{#UNIQUEKEYNUMBERCOLUMNS}]{{#ACTUALPARAMETERSUNIQUEKEY}}, AFieldList, ATransaction, AOrderBy, AStartRecord, AMaxRecords);
    }

    /// auto generated
    public static void LoadByUniqueKey(DataSet AData, {#FORMALPARAMETERSUNIQUEKEY}, TDBTransaction ATransaction)
    {
        LoadByUniqueKey(AData, {#ACTUALPARAMETERSUNIQUEKEY}, null, ATransaction, null, 0, 0);
    }
    
    /// auto generated
    public static void LoadByUniqueKey(DataSet AData, {#FORMALPARAMETERSUNIQUEKEY}, StringCollection AFieldList, TDBTransaction ATransaction)
    {
        LoadByUniqueKey(AData, {#ACTUALPARAMETERSUNIQUEKEY}, AFieldList, ATransaction, null, 0, 0);
    }

    /// auto generated
    public static {#TABLENAME}Table LoadByUniqueKey({#FORMALPARAMETERSUNIQUEKEY}, StringCollection AFieldList, TDBTransaction ATransaction, StringCollection AOrderBy, int AStartRecord, int AMaxRecords)
    {
        {#TABLENAME}Table Data = new {#TABLENAME}Table();
        LoadByUniqueKey({#TABLENAME}Table.TableId, Data, new System.Object[{#UNIQUEKEYNUMBERCOLUMNS}]{{#ACTUALPARAMETERSUNIQUEKEY}}, AFieldList, ATransaction, AOrderBy, AStartRecord, AMaxRecords);
        return Data;
    }
    
    /// auto generated
    public static {#TABLENAME}Table LoadByUniqueKey({#FORMALPARAMETERSUNIQUEKEY}, TDBTransaction ATransaction)
    {
        return LoadByUniqueKey({#ACTUALPARAMETERSUNIQUEKEY}, null, ATransaction, null, 0, 0);
    }
    
    /// auto generated
    public static {#TABLENAME}Table LoadByUniqueKey({#FORMALPARAMETERSUNIQUEKEY}, StringCollection AFieldList, TDBTransaction ATransaction)
    {
        return LoadByUniqueKey({#ACTUALPARAMETERSUNIQUEKEY}, AFieldList, ATransaction, null, 0, 0);
    }
{#ENDIF FORMALPARAMETERSUNIQUEKEY}

    /// this method is called by all overloads
    public static void LoadUsingTemplate(DataSet ADataSet, {#TABLENAME}Row ATemplateRow, StringCollection ATemplateOperators, StringCollection AFieldList, TDBTransaction ATransaction, StringCollection AOrderBy, int AStartRecord, int AMaxRecords)
    {
        LoadUsingTemplate({#TABLENAME}Table.TableId, ADataSet, ATemplateRow, ATemplateOperators, AFieldList, ATransaction, AOrderBy, AStartRecord, AMaxRecords);
    }
    
    /// auto generated
    public static void LoadUsingTemplate(DataSet AData, {#TABLENAME}Row ATemplateRow, TDBTransaction ATransaction)
    {
        LoadUsingTemplate(AData, ATemplateRow, null, null, ATransaction, null, 0, 0);
    }
    
    /// auto generated
    public static void LoadUsingTemplate(DataSet AData, {#TABLENAME}Row ATemplateRow, StringCollection AFieldList, TDBTransaction ATransaction)
    {
        LoadUsingTemplate(AData, ATemplateRow, null, AFieldList, ATransaction, null, 0, 0);
    }
    
    /// auto generated
    public static {#TABLENAME}Table LoadUsingTemplate({#TABLENAME}Row ATemplateRow, StringCollection ATemplateOperators, StringCollection AFieldList, TDBTransaction ATransaction, StringCollection AOrderBy, int AStartRecord, int AMaxRecords)
    {
        {#TABLENAME}Table Data = new {#TABLENAME}Table();
        LoadUsingTemplate({#TABLENAME}Table.TableId, Data, ATemplateRow, ATemplateOperators, AFieldList, ATransaction, AOrderBy, AStartRecord, AMaxRecords);
        return Data;
    }
    
    /// auto generated
    public static {#TABLENAME}Table LoadUsingTemplate({#TABLENAME}Row ATemplateRow, TDBTransaction ATransaction)
    {
        return LoadUsingTemplate(ATemplateRow, null, null, ATransaction, null, 0, 0);
    }
    
    /// auto generated
    public static {#TABLENAME}Table LoadUsingTemplate({#TABLENAME}Row ATemplateRow, StringCollection AFieldList, TDBTransaction ATransaction)
    {
        return LoadUsingTemplate(ATemplateRow, null, AFieldList, ATransaction, null, 0, 0);
    }
    
    /// auto generated
    public static {#TABLENAME}Table LoadUsingTemplate({#TABLENAME}Row ATemplateRow, StringCollection ATemplateOperators, StringCollection AFieldList, TDBTransaction ATransaction)
    {
        return LoadUsingTemplate(ATemplateRow, ATemplateOperators, AFieldList, ATransaction, null, 0, 0);
    }

    /// this method is called by all overloads
    public static void LoadUsingTemplate(DataSet ADataSet, TSearchCriteria[] ASearchCriteria, StringCollection AFieldList, TDBTransaction ATransaction, StringCollection AOrderBy, int AStartRecord, int AMaxRecords)
    {
        LoadUsingTemplate({#TABLENAME}Table.TableId, ADataSet, ASearchCriteria, AFieldList, ATransaction, AOrderBy, AStartRecord, AMaxRecords);
    }
    
    /// auto generated
    public static void LoadUsingTemplate(DataSet AData, TSearchCriteria[] ASearchCriteria, TDBTransaction ATransaction)
    {
        LoadUsingTemplate(AData, ASearchCriteria, null, ATransaction, null, 0, 0);
    }
    
    /// auto generated
    public static void LoadUsingTemplate(DataSet AData, TSearchCriteria[] ASearchCriteria, StringCollection AFieldList, TDBTransaction ATransaction)
    {
        LoadUsingTemplate(AData, ASearchCriteria, AFieldList, ATransaction, null, 0, 0);
    }
    
    /// auto generated
    public static {#TABLENAME}Table LoadUsingTemplate(TSearchCriteria[] ASearchCriteria, StringCollection AFieldList, TDBTransaction ATransaction, StringCollection AOrderBy, int AStartRecord, int AMaxRecords)
    {
        {#TABLENAME}Table Data = new {#TABLENAME}Table();
        LoadUsingTemplate({#TABLENAME}Table.TableId, Data, ASearchCriteria, AFieldList, ATransaction, AOrderBy, AStartRecord, AMaxRecords);
        return Data;
    }
    
    /// auto generated
    public static {#TABLENAME}Table LoadUsingTemplate(TSearchCriteria[] ASearchCriteria, TDBTransaction ATransaction)
    {
        return LoadUsingTemplate(ASearchCriteria, null, ATransaction, null, 0, 0);
    }

    /// auto generated
    public static {#TABLENAME}Table LoadUsingTemplate(TSearchCriteria[] ASearchCriteria, StringCollection AFieldList, TDBTransaction ATransaction)
    {
        return LoadUsingTemplate(ASearchCriteria, AFieldList, ATransaction, null, 0, 0);
    }

    /// this method is called by all overloads
    public static int CountAll(TDBTransaction ATransaction)
    {
        return Convert.ToInt32(ATransaction.DataBaseObj.ExecuteScalar("SELECT COUNT(*) FROM PUB_{#SQLTABLENAME}", ATransaction));
    }
{#IFDEF FORMALPARAMETERSPRIMARYKEY}

    /// check if a row exists by using the primary key
    public static bool Exists({#FORMALPARAMETERSPRIMARYKEY}, TDBTransaction ATransaction)
    {
        return Exists({#TABLENAME}Table.TableId, new System.Object[{#PRIMARYKEYNUMBERCOLUMNS}]{{#ACTUALPARAMETERSPRIMARYKEY}}, ATransaction);
    }
{#ENDIF FORMALPARAMETERSPRIMARYKEY}
{#IFDEF FORMALPARAMETERSUNIQUEKEY}

    /// check if a row exists by using the unique key
    public static bool Exists({#FORMALPARAMETERSUNIQUEKEY}, TDBTransaction ATransaction)
    {
        return ExistsUniqueKey({#TABLENAME}Table.TableId, new System.Object[{#UNIQUEKEYNUMBERCOLUMNS}]{{#ACTUALPARAMETERSUNIQUEKEY}}, ATransaction);
    }
{#ENDIF FORMALPARAMETERSUNIQUEKEY}

    /// this method is called by all overloads
    public static int CountUsingTemplate({#TABLENAME}Row ATemplateRow, StringCollection ATemplateOperators, TDBTransaction ATransaction)
    {
        return Convert.ToInt32(ATransaction.DataBaseObj.ExecuteScalar(("SELECT COUNT(*) FROM PUB_{#SQLTABLENAME}" + GenerateWhereClause(TTypedDataTable.GetColumnStringList({#TABLENAME}Table.TableId), ATemplateRow, ATemplateOperators)), ATransaction, 
               GetParametersForWhereClause({#TABLENAME}Table.TableId, ATemplateRow)));
    }

    /// this method is called by all overloads
    public static int CountUsingTemplate(TSearchCriteria[] ASearchCriteria, TDBTransaction ATransaction)
    {
        return Convert.ToInt32(ATransaction.DataBaseObj.ExecuteScalar(("SELECT COUNT(*) FROM PUB_{#SQLTABLENAME}" + GenerateWhereClause(ASearchCriteria)), ATransaction, 
        GetParametersForWhereClause({#TABLENAME}Table.TableId, ASearchCriteria)));
    }
    {#VIAOTHERTABLE}
    {#VIALINKTABLE}

{#IFDEF FORMALPARAMETERSPRIMARYKEY}

    /// auto generated
    public static void DeleteByPrimaryKey({#FORMALPARAMETERSPRIMARYKEY}, TDBTransaction ATransaction)
    {
        DeleteByPrimaryKey({#TABLENAME}Table.TableId, new System.Object[{#PRIMARYKEYNUMBERCOLUMNS}]{{#ACTUALPARAMETERSPRIMARYKEY}}, ATransaction);
    }
{#ENDIF FORMALPARAMETERSPRIMARYKEY}
    
    /// auto generated
    public static void DeleteUsingTemplate({#TABLENAME}Row ATemplateRow, StringCollection ATemplateOperators, TDBTransaction ATransaction)
    {
        DeleteUsingTemplate({#TABLENAME}Table.TableId, ATemplateRow, ATemplateOperators, ATransaction);
    }

    /// auto generated
    public static void DeleteUsingTemplate(TSearchCriteria[] ASearchCriteria, TDBTransaction ATransaction)
    {
        DeleteUsingTemplate({#TABLENAME}Table.TableId, ASearchCriteria, ATransaction);
    }
    
    /// <summary>
    /// Auto generated code ...
    /// </summary>
    /// <param name="ATable"></param>
    /// <param name="ATransaction"></param>
    public static void SubmitChanges({#TABLENAME}Table ATable, TDBTransaction ATransaction)
    {
        TVerificationResultCollection VerificationResults = null;
        TVerificationResultCollection SingleVerificationResultCollection;
        DataView DeletedRows = new DataView(ATable, string.Empty, String.Empty, DataViewRowState.Deleted);
        
        for (int Counter = 0; Counter < DeletedRows.Count; Counter++) 
        {
            if({#TABLENAME}Cascading.CountByPrimaryKey(DataUtilities.GetPKValuesFromDataRow(DeletedRows[Counter].Row), 50, ATransaction, true, 
                out SingleVerificationResultCollection) > 0)
            {
                if (VerificationResults == null)
                {
                    VerificationResults = new TVerificationResultCollection();
                }
                    
                VerificationResults.AddCollection(SingleVerificationResultCollection);
            }
        }

        if (VerificationResults != null) 
        {
            throw new EVerificationResultsException(String.Format(
                "SubmitChanges for {#TABLENAME}Table: cannot delete {0} row(s) because they are referenced by at least one record in at least one other DB Table." +
                " Check the TVerificationResultCollection stored in this Exceptions' 'VerificationResults' Property for details! The count has been limited to the first 50.",
                VerificationResults.Count), VerificationResults);
        }
        else
        {
            SubmitChanges(ATable, ATransaction, UserInfo.GetUserInfo().UserID{#SEQUENCENAMEANDFIELD});
        }        
    }

{#IFDEF FORMALPARAMETERSPRIMARYKEY}

    /// If a record with this primary key already exists in the database, 
    /// that record will be updated. otherwise a new record will be added.
    /// 
    public static void AddOrModifyRecord({#FORMALPARAMETERSPRIMARYKEY}, {#TABLENAME}Table ATable, {#TABLENAME}Row ANewRow, bool ADoNotOverwrite, TDBTransaction ATransaction)
    {
        if (!{#TABLENAME}Access.Exists({#ACTUALPARAMETERSPRIMARYKEY}, ATransaction))
        {
            ATable.Rows.Add(ANewRow);
        }
        else
        {
            {#TABLENAME}Table ExistingRecord = {#TABLENAME}Access.LoadByPrimaryKey({#ACTUALPARAMETERSPRIMARYKEY}, ATransaction);
            
            List<DataUtilities.TColumnDifference> Differences;
            DataUtilities.CompareAllColumnValues(ANewRow, ExistingRecord[0], out Differences);
            
            if (ADoNotOverwrite)
            {
                TLogging.LogAtLevel(5, "Ignoring differences for {#TABLENAME} " + {#ACTUALPARAMETERSPRIMARYKEYTOSTRING});
                foreach (DataUtilities.TColumnDifference diff in Differences)
                {
                    TLogging.LogAtLevel(5, "  " + diff.FColumnName + " value: " + diff.FDestinationValue + " ignored new value: " + diff.FSourceValue);
                } 
            }
            else
            {
                if (Differences.Count > 0)
                {
                    TLogging.LogAtLevel(5, "Overwriting {#TABLENAME} " + {#ACTUALPARAMETERSPRIMARYKEYTOSTRING});
                    foreach (DataUtilities.TColumnDifference diff in Differences)
                    {
                        TLogging.LogAtLevel(5, "  " + diff.FColumnName + ": " + diff.FDestinationValue + " -> " + diff.FSourceValue);
                    }
                }
                DataUtilities.CopyAllColumnValues(ANewRow, ExistingRecord[0]);
                ATable.Merge(ExistingRecord);
            }
        }
    }
{#ENDIF FORMALPARAMETERSPRIMARYKEY}

}

{##VIAOTHERTABLE}

/// auto generated
public static void Load{#VIAPROCEDURENAME}(DataSet ADataSet, {#FORMALPARAMETERSOTHERPRIMARYKEY}, StringCollection AFieldList, TDBTransaction ATransaction, StringCollection AOrderBy, int AStartRecord, int AMaxRecords)
{
    LoadViaForeignKey({#TABLENAME}Table.TableId, {#OTHERTABLENAME}Table.TableId, ADataSet, new string[{#NUMBERFIELDS}]{{#THISTABLEFIELDS}},
        new System.Object[{#NUMBERFIELDSOTHER}]{{#ACTUALPARAMETERSOTHERPRIMARYKEY}}, AFieldList, ATransaction, AOrderBy, AStartRecord, AMaxRecords);
}

/// auto generated
public static void Load{#VIAPROCEDURENAME}(DataSet AData, {#FORMALPARAMETERSOTHERPRIMARYKEY}, TDBTransaction ATransaction)
{
    Load{#VIAPROCEDURENAME}(AData, {#ACTUALPARAMETERSOTHERPRIMARYKEY}, null, ATransaction, null, 0, 0);
}

/// auto generated
public static void Load{#VIAPROCEDURENAME}(DataSet AData, {#FORMALPARAMETERSOTHERPRIMARYKEY}, StringCollection AFieldList, TDBTransaction ATransaction)
{
    Load{#VIAPROCEDURENAME}(AData, {#ACTUALPARAMETERSOTHERPRIMARYKEY}, AFieldList, ATransaction, null, 0, 0);
}

/// auto generated
public static {#TABLENAME}Table Load{#VIAPROCEDURENAME}({#FORMALPARAMETERSOTHERPRIMARYKEY}, StringCollection AFieldList, TDBTransaction ATransaction, StringCollection AOrderBy, int AStartRecord, int AMaxRecords)
{
    {#TABLENAME}Table Data = new {#TABLENAME}Table();
    LoadViaForeignKey({#TABLENAME}Table.TableId, {#OTHERTABLENAME}Table.TableId, Data, new string[{#NUMBERFIELDS}]{{#THISTABLEFIELDS}},
        new System.Object[{#NUMBERFIELDSOTHER}]{{#ACTUALPARAMETERSOTHERPRIMARYKEY}}, AFieldList, ATransaction, AOrderBy, AStartRecord, AMaxRecords);
    return Data;
}

/// auto generated
public static {#TABLENAME}Table Load{#VIAPROCEDURENAME}({#FORMALPARAMETERSOTHERPRIMARYKEY}, TDBTransaction ATransaction)
{
    return Load{#VIAPROCEDURENAME}({#ACTUALPARAMETERSOTHERPRIMARYKEY}, null, ATransaction, null, 0, 0);
}

/// auto generated
public static {#TABLENAME}Table Load{#VIAPROCEDURENAME}({#FORMALPARAMETERSOTHERPRIMARYKEY}, StringCollection AFieldList, TDBTransaction ATransaction)
{
    return Load{#VIAPROCEDURENAME}({#ACTUALPARAMETERSOTHERPRIMARYKEY}, AFieldList, ATransaction, null, 0, 0);
}

/// auto generated
public static void Load{#VIAPROCEDURENAME}Template(DataSet ADataSet, {#OTHERTABLENAME}Row ATemplateRow, StringCollection ATemplateOperators, StringCollection AFieldList, TDBTransaction ATransaction, StringCollection AOrderBy, int AStartRecord, int AMaxRecords)
{
    LoadViaForeignKey({#TABLENAME}Table.TableId, {#OTHERTABLENAME}Table.TableId, ADataSet, new string[{#NUMBERFIELDS}]{{#THISTABLEFIELDS}},
        ATemplateRow, ATemplateOperators, AFieldList, ATransaction, AOrderBy, AStartRecord, AMaxRecords);
}

/// auto generated
public static void Load{#VIAPROCEDURENAME}Template(DataSet AData, {#OTHERTABLENAME}Row ATemplateRow, TDBTransaction ATransaction)
{
    Load{#VIAPROCEDURENAME}Template(AData, ATemplateRow, null, null, ATransaction, null, 0, 0);
}

/// auto generated
public static void Load{#VIAPROCEDURENAME}Template(DataSet AData, {#OTHERTABLENAME}Row ATemplateRow, StringCollection AFieldList, TDBTransaction ATransaction)
{
    Load{#VIAPROCEDURENAME}Template(AData, ATemplateRow, null, AFieldList, ATransaction, null, 0, 0);
}

/// auto generated
public static {#TABLENAME}Table Load{#VIAPROCEDURENAME}Template({#OTHERTABLENAME}Row ATemplateRow, StringCollection ATemplateOperators, StringCollection AFieldList, TDBTransaction ATransaction, StringCollection AOrderBy, int AStartRecord, int AMaxRecords)
{
    {#TABLENAME}Table Data = new {#TABLENAME}Table();
    LoadViaForeignKey({#TABLENAME}Table.TableId, {#OTHERTABLENAME}Table.TableId, Data, new string[{#NUMBERFIELDS}]{{#THISTABLEFIELDS}},
        ATemplateRow, ATemplateOperators, AFieldList, ATransaction, AOrderBy, AStartRecord, AMaxRecords);
    return Data;
}

/// auto generated
public static {#TABLENAME}Table Load{#VIAPROCEDURENAME}Template({#OTHERTABLENAME}Row ATemplateRow, TDBTransaction ATransaction)
{
    return Load{#VIAPROCEDURENAME}Template(ATemplateRow, null, null, ATransaction, null, 0, 0);
}

/// auto generated
public static {#TABLENAME}Table Load{#VIAPROCEDURENAME}Template({#OTHERTABLENAME}Row ATemplateRow, StringCollection AFieldList, TDBTransaction ATransaction)
{
    return Load{#VIAPROCEDURENAME}Template(ATemplateRow, null, AFieldList, ATransaction, null, 0, 0);
}

/// auto generated
public static {#TABLENAME}Table Load{#VIAPROCEDURENAME}Template({#OTHERTABLENAME}Row ATemplateRow, StringCollection ATemplateOperators, StringCollection AFieldList, TDBTransaction ATransaction)
{
    return Load{#VIAPROCEDURENAME}Template(ATemplateRow, ATemplateOperators, AFieldList, ATransaction, null, 0, 0);
}

/// auto generated
public static void Load{#VIAPROCEDURENAME}Template(DataSet ADataSet, TSearchCriteria[] ASearchCriteria, StringCollection AFieldList, TDBTransaction ATransaction, StringCollection AOrderBy, int AStartRecord, int AMaxRecords)
{
    LoadViaForeignKey({#TABLENAME}Table.TableId, {#OTHERTABLENAME}Table.TableId, ADataSet, new string[{#NUMBERFIELDS}]{{#THISTABLEFIELDS}},
        ASearchCriteria, AFieldList, ATransaction, AOrderBy, AStartRecord, AMaxRecords);
}

/// auto generated
public static void Load{#VIAPROCEDURENAME}Template(DataSet AData, TSearchCriteria[] ASearchCriteria, TDBTransaction ATransaction)
{
    Load{#VIAPROCEDURENAME}Template(AData, ASearchCriteria, null, ATransaction, null, 0, 0);
}

/// auto generated
public static void Load{#VIAPROCEDURENAME}Template(DataSet AData, TSearchCriteria[] ASearchCriteria, StringCollection AFieldList, TDBTransaction ATransaction)
{
    Load{#VIAPROCEDURENAME}Template(AData, ASearchCriteria, AFieldList, ATransaction, null, 0, 0);
}

/// auto generated
public static {#TABLENAME}Table Load{#VIAPROCEDURENAME}Template(TSearchCriteria[] ASearchCriteria, StringCollection AFieldList, TDBTransaction ATransaction, StringCollection AOrderBy, int AStartRecord, int AMaxRecords)
{
    {#TABLENAME}Table Data = new {#TABLENAME}Table();
    LoadViaForeignKey({#TABLENAME}Table.TableId, {#OTHERTABLENAME}Table.TableId, Data, new string[{#NUMBERFIELDS}]{{#THISTABLEFIELDS}},
        ASearchCriteria, AFieldList, ATransaction, AOrderBy, AStartRecord, AMaxRecords);
    return Data;
}

/// auto generated
public static {#TABLENAME}Table Load{#VIAPROCEDURENAME}Template(TSearchCriteria[] ASearchCriteria, TDBTransaction ATransaction)
{
    return Load{#VIAPROCEDURENAME}Template(ASearchCriteria, null, ATransaction, null, 0, 0);
}

/// auto generated
public static {#TABLENAME}Table Load{#VIAPROCEDURENAME}Template(TSearchCriteria[] ASearchCriteria, StringCollection AFieldList, TDBTransaction ATransaction)
{
    return Load{#VIAPROCEDURENAME}Template(ASearchCriteria, AFieldList, ATransaction, null, 0, 0);
}

/// auto generated
public static int Count{#VIAPROCEDURENAME}({#FORMALPARAMETERSOTHERPRIMARYKEY}, TDBTransaction ATransaction)
{
    return CountViaForeignKey({#TABLENAME}Table.TableId, {#OTHERTABLENAME}Table.TableId, new string[{#NUMBERFIELDS}]{{#THISTABLEFIELDS}}, 
        new System.Object[{#NUMBERFIELDSOTHER}]{{#ACTUALPARAMETERSOTHERPRIMARYKEY}}, ATransaction);
}

/// auto generated
public static int Count{#VIAPROCEDURENAME}Template({#OTHERTABLENAME}Row ATemplateRow, StringCollection ATemplateOperators, TDBTransaction ATransaction)
{
    return CountViaForeignKey({#TABLENAME}Table.TableId, {#OTHERTABLENAME}Table.TableId, new string[{#NUMBERFIELDS}]{{#THISTABLEFIELDS}}, 
        ATemplateRow, ATemplateOperators, ATransaction);
}

/// auto generated
public static int Count{#VIAPROCEDURENAME}Template(TSearchCriteria[] ASearchCriteria, TDBTransaction ATransaction)
{
    return CountViaForeignKey({#TABLENAME}Table.TableId, {#OTHERTABLENAME}Table.TableId, new string[{#NUMBERFIELDS}]{{#THISTABLEFIELDS}}, 
        ASearchCriteria, ATransaction);
}

{##VIALINKTABLE}

/// auto generated LoadViaLinkTable
public static void Load{#VIAPROCEDURENAME}(DataSet ADataSet, {#FORMALPARAMETERSOTHERPRIMARYKEY}, StringCollection AFieldList, TDBTransaction ATransaction, StringCollection AOrderBy, int AStartRecord, int AMaxRecords)
{
    {#ODBCPARAMETERSFOREIGNKEY}
    ATransaction.DataBaseObj.Select(ADataSet, ((GenerateSelectClauseLong("PUB_{#SQLTABLENAME}", AFieldList, {#TABLENAME}Table.TableId) + 
                    " FROM PUB_{#SQLTABLENAME}, PUB_{#SQLLINKTABLENAME} WHERE " + 
                    "{#WHERECLAUSEVIALINKTABLE}") +
                    GenerateOrderByClause(AOrderBy)), TTypedDataTable.GetTableName({#TABLENAME}Table.TableId), ATransaction, ParametersArray, AStartRecord, AMaxRecords);
}

/// auto generated
public static void Load{#VIAPROCEDURENAME}(DataSet AData, {#FORMALPARAMETERSOTHERPRIMARYKEY}, TDBTransaction ATransaction)
{
    Load{#VIAPROCEDURENAME}(AData, {#ACTUALPARAMETERSOTHERPRIMARYKEY}, null, ATransaction, null, 0, 0);
}

/// auto generated
public static void Load{#VIAPROCEDURENAME}(DataSet AData, {#FORMALPARAMETERSOTHERPRIMARYKEY}, StringCollection AFieldList, TDBTransaction ATransaction)
{
    Load{#VIAPROCEDURENAME}(AData, {#ACTUALPARAMETERSOTHERPRIMARYKEY}, AFieldList, ATransaction, null, 0, 0);
}

/// auto generated
public static {#TABLENAME}Table Load{#VIAPROCEDURENAME}({#FORMALPARAMETERSOTHERPRIMARYKEY}, StringCollection AFieldList, TDBTransaction ATransaction, StringCollection AOrderBy, int AStartRecord, int AMaxRecords)
{
    DataSet FillDataSet = new DataSet();
    {#TABLENAME}Table Data = new {#TABLENAME}Table();
    FillDataSet.Tables.Add(Data);
    Load{#VIAPROCEDURENAME}(FillDataSet, {#ACTUALPARAMETERSOTHERPRIMARYKEY}, AFieldList, ATransaction, AOrderBy, AStartRecord, AMaxRecords);
    FillDataSet.Tables.Remove(Data);
    return Data;
}

/// auto generated
public static {#TABLENAME}Table Load{#VIAPROCEDURENAME}({#FORMALPARAMETERSOTHERPRIMARYKEY}, TDBTransaction ATransaction)
{
    return Load{#VIAPROCEDURENAME}({#ACTUALPARAMETERSOTHERPRIMARYKEY}, null, ATransaction, null, 0, 0);
}

/// auto generated
public static {#TABLENAME}Table Load{#VIAPROCEDURENAME}({#FORMALPARAMETERSOTHERPRIMARYKEY}, StringCollection AFieldList, TDBTransaction ATransaction)
{
    return Load{#VIAPROCEDURENAME}({#ACTUALPARAMETERSOTHERPRIMARYKEY}, AFieldList, ATransaction, null, 0, 0);
}

/// auto generated
public static void Load{#VIAPROCEDURENAME}Template(DataSet ADataSet, {#OTHERTABLENAME}Row ATemplateRow, StringCollection ATemplateOperators, StringCollection AFieldList, TDBTransaction ATransaction, StringCollection AOrderBy, int AStartRecord, int AMaxRecords)
{
    ATransaction.DataBaseObj.Select(ADataSet, (((GenerateSelectClauseLong("PUB_{#SQLTABLENAME}", AFieldList, {#TABLENAME}Table.TableId) + 
                    " FROM PUB_{#SQLTABLENAME}, PUB_{#SQLLINKTABLENAME}, PUB_{#SQLOTHERTABLENAME} WHERE " +
                    "{#WHERECLAUSEALLVIATABLES}") +
                    GenerateWhereClauseLong("PUB_{#SQLOTHERTABLENAME}", {#OTHERTABLENAME}Table.TableId, ATemplateRow, ATemplateOperators)) +
                    GenerateOrderByClause(AOrderBy)), TTypedDataTable.GetTableName({#TABLENAME}Table.TableId), ATransaction, 
                    GetParametersForWhereClause({#OTHERTABLENAME}Table.TableId, ATemplateRow), AStartRecord, AMaxRecords);
}

/// auto generated
public static void Load{#VIAPROCEDURENAME}Template(DataSet AData, {#OTHERTABLENAME}Row ATemplateRow, TDBTransaction ATransaction)
{
    Load{#VIAPROCEDURENAME}Template(AData, ATemplateRow, null, null, ATransaction, null, 0, 0);
}

/// auto generated
public static void Load{#VIAPROCEDURENAME}Template(DataSet AData, {#OTHERTABLENAME}Row ATemplateRow, StringCollection AFieldList, TDBTransaction ATransaction)
{
    Load{#VIAPROCEDURENAME}Template(AData, ATemplateRow, null, AFieldList, ATransaction, null, 0, 0);
}

/// auto generated
public static {#TABLENAME}Table Load{#VIAPROCEDURENAME}Template({#OTHERTABLENAME}Row ATemplateRow, StringCollection ATemplateOperators, StringCollection AFieldList, TDBTransaction ATransaction, StringCollection AOrderBy, int AStartRecord, int AMaxRecords)
{
    DataSet FillDataSet = new DataSet();
    {#TABLENAME}Table Data = new {#TABLENAME}Table();
    FillDataSet.Tables.Add(Data);
    Load{#VIAPROCEDURENAME}Template(FillDataSet, ATemplateRow, ATemplateOperators, AFieldList, ATransaction, AOrderBy, AStartRecord, AMaxRecords);
    FillDataSet.Tables.Remove(Data);
    return Data;
}

/// auto generated
public static {#TABLENAME}Table Load{#VIAPROCEDURENAME}Template({#OTHERTABLENAME}Row ATemplateRow, TDBTransaction ATransaction)
{
    return Load{#VIAPROCEDURENAME}Template(ATemplateRow, null, null, ATransaction, null, 0, 0);
}

/// auto generated
public static {#TABLENAME}Table Load{#VIAPROCEDURENAME}Template({#OTHERTABLENAME}Row ATemplateRow, StringCollection AFieldList, TDBTransaction ATransaction)
{
    return Load{#VIAPROCEDURENAME}Template(ATemplateRow, null, AFieldList, ATransaction, null, 0, 0);
}

/// auto generated
public static {#TABLENAME}Table Load{#VIAPROCEDURENAME}Template({#OTHERTABLENAME}Row ATemplateRow, StringCollection ATemplateOperators, StringCollection AFieldList, TDBTransaction ATransaction)
{
    return Load{#VIAPROCEDURENAME}Template(ATemplateRow, ATemplateOperators, AFieldList, ATransaction, null, 0, 0);
}

/// auto generated
public static void Load{#VIAPROCEDURENAME}Template(DataSet ADataSet, TSearchCriteria[] ASearchCriteria, StringCollection AFieldList, TDBTransaction ATransaction, StringCollection AOrderBy, int AStartRecord, int AMaxRecords)
{
    ATransaction.DataBaseObj.Select(ADataSet, (((GenerateSelectClauseLong("PUB_{#SQLTABLENAME}", AFieldList, {#TABLENAME}Table.TableId) + 
                    " FROM PUB_{#SQLTABLENAME}, PUB_{#SQLLINKTABLENAME}, PUB_{#SQLOTHERTABLENAME} WHERE " +
                    "{#WHERECLAUSEALLVIATABLES}") +
                    GenerateWhereClauseLong("PUB_{#SQLOTHERTABLENAME}", ASearchCriteria)) +
                    GenerateOrderByClause(AOrderBy)), TTypedDataTable.GetTableName({#TABLENAME}Table.TableId), ATransaction, 
                    GetParametersForWhereClause({#TABLENAME}Table.TableId, ASearchCriteria), AStartRecord, AMaxRecords);
}

/// auto generated
public static void Load{#VIAPROCEDURENAME}Template(DataSet AData, TSearchCriteria[] ASearchCriteria, TDBTransaction ATransaction)
{
    Load{#VIAPROCEDURENAME}Template(AData, ASearchCriteria, null, ATransaction, null, 0, 0);
}

/// auto generated
public static void Load{#VIAPROCEDURENAME}Template(DataSet AData, TSearchCriteria[] ASearchCriteria, StringCollection AFieldList, TDBTransaction ATransaction)
{
    Load{#VIAPROCEDURENAME}Template(AData, ASearchCriteria, AFieldList, ATransaction, null, 0, 0);
}

/// auto generated
public static {#TABLENAME}Table Load{#VIAPROCEDURENAME}Template(TSearchCriteria[] ASearchCriteria, StringCollection AFieldList, TDBTransaction ATransaction, StringCollection AOrderBy, int AStartRecord, int AMaxRecords)
{
    DataSet FillDataSet = new DataSet();
    {#TABLENAME}Table Data = new {#TABLENAME}Table();
    FillDataSet.Tables.Add(Data);
    Load{#VIAPROCEDURENAME}Template(FillDataSet, ASearchCriteria, AFieldList, ATransaction, AOrderBy, AStartRecord, AMaxRecords);
    FillDataSet.Tables.Remove(Data);
    return Data;
}

/// auto generated
public static {#TABLENAME}Table Load{#VIAPROCEDURENAME}Template(TSearchCriteria[] ASearchCriteria, TDBTransaction ATransaction)
{
    return Load{#VIAPROCEDURENAME}Template(ASearchCriteria, null, ATransaction, null, 0, 0);
}

/// auto generated
public static {#TABLENAME}Table Load{#VIAPROCEDURENAME}Template(TSearchCriteria[] ASearchCriteria, StringCollection AFieldList, TDBTransaction ATransaction)
{
    return Load{#VIAPROCEDURENAME}Template(ASearchCriteria, AFieldList, ATransaction, null, 0, 0);
}

/// auto generated CountViaLinkTable
public static int Count{#VIAPROCEDURENAME}({#FORMALPARAMETERSOTHERPRIMARYKEY}, TDBTransaction ATransaction)
{
    {#ODBCPARAMETERSFOREIGNKEY}
    return Convert.ToInt32(ATransaction.DataBaseObj.ExecuteScalar("SELECT COUNT(*) FROM PUB_{#SQLTABLENAME}, PUB_{#SQLLINKTABLENAME} WHERE " +
                "{#WHERECLAUSEVIALINKTABLE}",
                ATransaction, ParametersArray));
}

/// auto generated
public static int Count{#VIAPROCEDURENAME}Template({#OTHERTABLENAME}Row ATemplateRow, StringCollection ATemplateOperators, TDBTransaction ATransaction)
{
    return Convert.ToInt32(ATransaction.DataBaseObj.ExecuteScalar(("SELECT COUNT(*) FROM PUB_{#SQLTABLENAME}, PUB_{#SQLLINKTABLENAME}, PUB_{#SQLOTHERTABLENAME} WHERE " +
                "{#WHERECLAUSEALLVIATABLES}" +
                GenerateWhereClauseLong("PUB_{#SQLLINKTABLENAME}", {#TABLENAME}Table.TableId, ATemplateRow, ATemplateOperators)), ATransaction, 
                GetParametersForWhereClauseWithPrimaryKey({#OTHERTABLENAME}Table.TableId, ATemplateRow)));
}

/// auto generated
public static int Count{#VIAPROCEDURENAME}Template(TSearchCriteria[] ASearchCriteria, TDBTransaction ATransaction)
{
    return Convert.ToInt32(ATransaction.DataBaseObj.ExecuteScalar(("SELECT COUNT(*) FROM PUB_{#SQLTABLENAME}, PUB_{#SQLLINKTABLENAME}, PUB_{#SQLOTHERTABLENAME} WHERE " +
                "{#WHERECLAUSEALLVIATABLES}" +
                GenerateWhereClauseLong("PUB_{#SQLLINKTABLENAME}", ASearchCriteria)), ATransaction, 
                GetParametersForWhereClause({#TABLENAME}Table.TableId, ASearchCriteria)));
}
