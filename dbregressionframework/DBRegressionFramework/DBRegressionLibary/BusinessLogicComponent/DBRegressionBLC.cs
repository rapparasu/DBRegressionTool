using System;
using System.Collections.Generic;
using System.Text;

using DBRegressionLibrary.DataAccessComponent;
using DBRegressionLibrary.BusinessEntities;
using DBRegressionLibrary.Common;
using System.Configuration;
using System.Data;
using System.Collections.Specialized;
using System.Linq;
using System.IO;
namespace DBRegressionLibrary.BusinessLogicComponent
{

    /// <summary>
    /// DBRegression Framework BusinessLogicComponent
    /// </summary>
    public class DBRegressionBLC
    {

        private DBRegressionDAC dbRegressionDAC = new DBRegressionDAC();

        /// <summary>
        /// Run test cases for a given Test Tag, if the tests expect RunTimeParams then it must be supplied if not leave empty.
        /// The user triggering the framework can opt to receive email notifications for any Rec Errors if RecSendAndEmail attribute is configured for test case.
        /// </summary>
        /// <param name="TestTag"></param>
        /// <param name="RunTimeParams"></param>
        /// <param name="NTUserEmail"></param>
        /// <returns></returns>
        public bool RunTests(string TestTag, string RunTimeParams, string NTUserEmail)
        {
            //get all enabled tests from DB
            IEnumerable<TestCaseEntity> testCaseEntities = GetDBTestsConfig();

            //check and run only the tagged tests
            if (!string.IsNullOrEmpty(TestTag))
            {

                testCaseEntities = testCaseEntities.Where(x => (x.Tag != null || !string.IsNullOrEmpty(x.Tag)) && x.Tag.Equals(TestTag, StringComparison.InvariantCultureIgnoreCase));
                if (testCaseEntities.Count() == 0)
                    throw new Exception($"Invalid Test Tag: {TestTag}");
            }

            return ExecuteTests(testCaseEntities, RunTimeParams, NTUserEmail);
        }

        /// <summary>
        /// Get all the enabled tests from database
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TestCaseEntity> GetDBTestsConfig()
        {
            return dbRegressionDAC.GetDBTestsConfig();
        }


        /// <summary>
        /// Execute Tests for the Tag 
        /// </summary>
        /// <param name="TestCaseEntities"></param>
        /// <param name="RunTimeParams"></param>
        /// <param name="NTUserEmail"></param>
        /// <returns></returns>
        public bool ExecuteTests(IEnumerable<TestCaseEntity> TestCaseEntities, string RunTimeParams, string NTUserEmail)
        {

            //A Tag can have multiple tests underneath so track over all execution status for caller. 
            bool overallExecutionResult = true;

            foreach (TestCaseEntity TestCaseEntity in TestCaseEntities)
            {

                TestCaseResultEntity testCaseResultEntity = new TestCaseResultEntity();
                try
                {
                    Console.WriteLine($"Processing TestCase: {TestCaseEntity.Id}");

                    string sourceDBConnectionString = ConfigurationManager.ConnectionStrings["SourceDBConnectionString"].ConnectionString;
                    string targetDBConnectionString = ConfigurationManager.ConnectionStrings["TargetDBConnectionString"].ConnectionString;

                    //if the test case use ValuationDate or EffectiveDate and SnapshotType as filter then use these values while creating the dbObject table for detailed results
                    string paramEffectiveDate = string.Empty;
                    string paramSnapshotType = string.Empty;

                    long executionTimeInSeconds = -1;
                    string runtimeQueryString = string.Empty;

                    testCaseResultEntity.TestConfigId = TestCaseEntity.Id;


                    TestCaseEntity.SourceDBConnectionString = sourceDBConnectionString.Replace("{Server}", TestCaseEntity.SourceDBServer).Replace("{Database}", TestCaseEntity.SourceDB);
                    TestCaseEntity.TargetDBConnectionString = targetDBConnectionString.Replace("{Server}", TestCaseEntity.TargetDBServer).Replace("{Database}", TestCaseEntity.TargetDB);

                    TestCaseEntity.Params = GetParamValues(TestCaseEntity.Params, RunTimeParams);

                    DataSet sourceDS = GetDataSet(TestCaseEntity.SourceDBObjectName, TestCaseEntity.SourceDBObjectType, TestCaseEntity.Params, TestCaseEntity.SourceDBConnectionString, ref executionTimeInSeconds, ref runtimeQueryString);
                    //ValidateDataSet(sourceDS, TestCaseEntity.SourceDBObjectName, TestCaseEntity.Params, TestCaseEntity.SourceDBConnectionString);

                    testCaseResultEntity.SourceExecutionTimeInSeconds = executionTimeInSeconds;
                    testCaseResultEntity.SourceQuery = runtimeQueryString;

                    DataTable convertedSourceDT = ConvertDataTableSQLTypesToDotNetType(sourceDS.Tables[0]);

                    DataTable sourceDT = convertedSourceDT;
                    sourceDT.TableName = "SourceTable";

                    DataSet targetDS = GetDataSet(TestCaseEntity.TargetDBObjectName, TestCaseEntity.TargetDBObjectType, TestCaseEntity.Params, TestCaseEntity.TargetDBConnectionString, ref executionTimeInSeconds, ref runtimeQueryString);
                    //ValidateDataSet(targetDS, TestCaseEntity.TargetDBObjectName, TestCaseEntity.Params, TestCaseEntity.TargetDBConnectionString);
                    testCaseResultEntity.TargetExecutionTimeInSeconds = executionTimeInSeconds;
                    testCaseResultEntity.TargetQuery = runtimeQueryString;

                    DataTable convertedtargetDT = ConvertDataTableSQLTypesToDotNetType(targetDS.Tables[0]);

                    DataTable targetDT = convertedtargetDT;
                    targetDT.TableName = "TargetTable";

                    List<string> primaryKeyColumnNames = null;
                    List<string> sortColumnsWhenNoPrimaryKey = null;

                    ValidateKeyColumns(TestCaseEntity, ref primaryKeyColumnNames, ref sortColumnsWhenNoPrimaryKey);

                    if (!string.IsNullOrEmpty(TestCaseEntity.ColumnsToIncludeForChecks) && TestCaseEntity.ColumnsToIncludeForChecks.Trim() != "*")
                    {
                        List<string> columnsToIncludeForChecks = GetColumnInclusionList(TestCaseEntity, primaryKeyColumnNames, sortColumnsWhenNoPrimaryKey);
                        PreserveIncludedColumnsInDataTables(ref sourceDT, ref targetDT, columnsToIncludeForChecks);
                    }

                    if (!string.IsNullOrEmpty(TestCaseEntity.ColumnsToExcludeForChecks))
                    {
                        List<string> columnsToExcludeForChecks = TestCaseEntity.ColumnsToExcludeForChecks.Split(',').Select(x => x.Trim()).ToList();
                        RemoveExcludedColumnsFromDataTables(ref sourceDT, ref targetDT, columnsToExcludeForChecks);
                    }



                    testCaseResultEntity.SourceRowCount = sourceDT.Rows.Count;
                    testCaseResultEntity.TargetRowCount = targetDT.Rows.Count;

                    testCaseResultEntity.SourceColumnCount = sourceDT.Columns.Count;
                    testCaseResultEntity.TargetColumnCount = targetDT.Columns.Count;


                    if (primaryKeyColumnNames != null && primaryKeyColumnNames.Count > 0)
                    {
                        //primary columns configured/specified in the config table should come in the same order as they appear in the dataset otherwise the results will be different. 
                        //so sort the primary key columns basd on the order of their appearance in dataset
                        primaryKeyColumnNames = primaryKeyColumnNames.OrderBy(x => { return sourceDT.Columns.IndexOf(x); }).ToList();
                        sourceDT.PrimaryKey = primaryKeyColumnNames.Select(x => sourceDT.Columns[x]).ToArray();
                        targetDT.PrimaryKey = primaryKeyColumnNames.Select(x => targetDT.Columns[x]).ToArray();
                    }
                    else if (sortColumnsWhenNoPrimaryKey != null && sortColumnsWhenNoPrimaryKey.Count > 0)
                        sortColumnsWhenNoPrimaryKey = sortColumnsWhenNoPrimaryKey.OrderBy(x => { return sourceDT.Columns.IndexOf(x); }).ToList();


                    List<string> columnsToAlwaysShowDespiteMatching = null;
                    List<string> diffColumnsToInject = null;

                    if (!string.IsNullOrEmpty(TestCaseEntity.ColumnsToAlwaysShowDespiteMatching))
                        columnsToAlwaysShowDespiteMatching = TestCaseEntity.ColumnsToAlwaysShowDespiteMatching.Split(',').Select(x => x.Trim()).ToList();

                    if (!string.IsNullOrEmpty(TestCaseEntity.DiffColumnsToInject))
                        diffColumnsToInject = TestCaseEntity.DiffColumnsToInject.Split(',').Select(x => x.Trim()).ToList();


                    DataTable dtResult = CompareDataTables(sourceDT, targetDT, ref testCaseResultEntity, primaryKeyColumnNames, sortColumnsWhenNoPrimaryKey, columnsToAlwaysShowDespiteMatching, diffColumnsToInject, TestCaseEntity.DiffColumnsTolerance, TestCaseEntity.ShowMatchingColumnValues);

                    CheckAndRemoveDiffColumnsFromDataTableResults(ref dtResult, diffColumnsToInject, TestCaseEntity.DiffColumnsTolerance);

                    List<string> columnOrderSequenceInOutput = null;
                    if (!string.IsNullOrEmpty(TestCaseEntity.ColumnOrderSequenceInOutput))
                        columnOrderSequenceInOutput = TestCaseEntity.ColumnOrderSequenceInOutput.Split(',').Select(x => x.Trim()).ToList();

                    CheckAndSetColumnsOrder(ref dtResult, columnOrderSequenceInOutput);

                    //don't format diff column by introducing comma's in the values, it would make sql queries harder for filtering
                    //FormatDiffColumns(ref dtResult, diffColumnsToInject);


                    if (!string.IsNullOrEmpty(TestCaseEntity.Params))
                    {
                        TestCaseEntity.NVParams = GetParamNameValueCollection(TestCaseEntity.Params);

                        var effectiveDate = GetIndividualParamFromNVCollection("EffectiveDate", TestCaseEntity.NVParams);
                        if (effectiveDate != null)
                        {
                            if (DateTime.TryParse(effectiveDate.ToString(), out DateTime dateTimeResult))
                                // paramEffectiveDate = dateTimeResult.ToString("yyyy-MM-dd");
                                paramEffectiveDate = dateTimeResult.ToString("yyyyMMdd");
                        }

                        var snapshotType = GetIndividualParamFromNVCollection("SnapshotType", TestCaseEntity.NVParams);
                        if (snapshotType != null)
                        {
                            paramSnapshotType = snapshotType.ToString();

                        }
                    }


                    List<string> columnsToCheckForBusinessAlerting = null;
                    if (!string.IsNullOrEmpty(TestCaseEntity.ColumnsToCheckForBusinessAlerting))
                        columnsToCheckForBusinessAlerting = TestCaseEntity.ColumnsToCheckForBusinessAlerting.Split(',').Select(x => x.Trim()).ToList();


                    string dbTableName = GetLogTableName(TestCaseEntity, paramEffectiveDate, paramSnapshotType);

                    if (testCaseResultEntity.Result)
                    {
                        //Check and delete the log table from previous run as the current run is successful. 
                        CheckAndDropTable(dbTableName);
                        SaveTestMetricsToDB(testCaseResultEntity);
                        Console.WriteLine($"data sets are matching");

                    }
                    else
                    {
                        overallExecutionResult = false;
                        //only log the tablename when the result is fail
                        testCaseResultEntity.LogTableName = dbTableName;
                        SaveTestMetricsToDB(testCaseResultEntity);
                        StoreDataTableResultsToDB(dtResult, dbTableName);

                        if (TestCaseEntity.ToRecAndSendEmail)
                            CheckAndEmailRegressionTestResults(dtResult, dbTableName, testCaseResultEntity, paramEffectiveDate, TestCaseEntity.TestName, TestCaseEntity.ITRecipients, TestCaseEntity.BusinessRecipients, columnsToCheckForBusinessAlerting, NTUserEmail);

                        Console.WriteLine($"Data sets are not matching, please review the result in the table: {dbTableName}");
                    }
                }
                catch (Exception ex)
                {
                    overallExecutionResult = false;
                    //log exception for the test and continue
                    testCaseResultEntity.LogMessage = ex.ToString();
                    SaveTestMetricsToDB(testCaseResultEntity);
                    // Console.WriteLine($"Exception occured while processing TestCase {TestCaseEntity.Id}, details: {ex.ToString()}");
                }
            }

            return overallExecutionResult;
        }

        /// <summary>
        /// Returns list of columns to be checked for Rec
        /// </summary>
        /// <param name="TestCaseEntity"></param>
        /// <param name="PrimaryKeyColumnNames"></param>
        /// <param name="SortColumnsWhenNoPrimaryKey"></param>
        /// <returns></returns>
        private List<string> GetColumnInclusionList(TestCaseEntity TestCaseEntity, List<string> PrimaryKeyColumnNames, List<string> SortColumnsWhenNoPrimaryKey)
        {

            List<string> columnsToIncludeForChecks = TestCaseEntity.ColumnsToIncludeForChecks.Split(',').Select(x => x.Trim()).ToList();

            //always include primary key/sort key columns in the dataset if PrimaryKeyColumns and ColumnToIncldueForChecks values are mutually exclusive
            if (PrimaryKeyColumnNames != null && PrimaryKeyColumnNames.Count > 0)
                columnsToIncludeForChecks = columnsToIncludeForChecks.Concat(PrimaryKeyColumnNames).ToList();

            if (SortColumnsWhenNoPrimaryKey != null && SortColumnsWhenNoPrimaryKey.Count > 0)
                columnsToIncludeForChecks = columnsToIncludeForChecks.Concat(SortColumnsWhenNoPrimaryKey).ToList();

            return columnsToIncludeForChecks;
        }


        /// <summary>
        /// Converts configured parameters for a test case to NameValue Collection
        /// </summary>
        /// <param name="Params"></param>
        /// <returns></returns>
        private NameValueCollection GetParamNameValueCollection(string Params)
        {

            NameValueCollection nvParams = new NameValueCollection();

            string[] paramsSplit = Params.Split(',');

            foreach (string param in paramsSplit)
            {
                string[] paramSplit = param.Split('=');

                if (paramSplit != null && paramSplit.Length == 2)
                {
                    nvParams.Add(paramSplit[0].Replace("@", "").Trim(), paramSplit[1].Replace("'", "").Trim());

                }

            }

            return nvParams;

        }

        /// <summary>
        /// Get the parameters for tests.
        /// </summary>
        /// <param name="ParamsFromConfig"></param>
        /// <param name="RunTimeParams"></param>
        /// <returns></returns>
        private string GetParamValues(string ParamsFromConfig, string RunTimeParams)
        {
            string runTimeparams = string.IsNullOrEmpty(ParamsFromConfig) ? RunTimeParams : ParamsFromConfig.Contains("@DynamicParameters") ? ParamsFromConfig.Replace("@DynamicParameters", RunTimeParams) : ParamsFromConfig;

            if (!string.IsNullOrEmpty(runTimeparams) && ParamsFromConfig.ToUpper().Contains("{T-1}"))
            {

                string effectiveDate = GetTMinusOneDateForParams(runTimeparams);
                runTimeparams = ParamsFromConfig.Replace("{T-1}", effectiveDate);

            }

            return runTimeparams;

        }

        /// <summary>
        /// Get T-1 params for Tests
        /// </summary>
        /// <param name="RunTimeparams"></param>
        /// <returns></returns>
        public string GetTMinusOneDateForParams(string RunTimeparams)
        {

            string snapshotType = string.Empty;
            if (RunTimeparams.ToUpper().Contains("MONTH-END"))
                snapshotType = "MONTH-END";
            else if (RunTimeparams.ToUpper().Contains("DAILY"))
                snapshotType = "DAILY";
            else//sometimes you have daily batches without snapshot Type, example NonDimensionalHoldings data in IDH
                snapshotType = "DAILY";

            return GetTMinusOneDateForSnapshotType(snapshotType);
        }


        /// <summary>
        /// Returns the corresponding effective date for T-1 based on the snapshotType
        /// </summary>
        /// <param name="SnapshotType"></param>
        /// <returns></returns>
        public string GetTMinusOneDateForSnapshotType(string SnapshotType)
        {
            string effectiveDate = string.Empty;
            if (SnapshotType.ToUpper().Equals("MONTH-END"))
            {
                var now = DateTime.Now;
                var firstDayCurrentMonth = new DateTime(now.Year, now.Month, 1);
                var lastDayLastMonth = firstDayCurrentMonth.AddDays(-1);
                effectiveDate = lastDayLastMonth.ToString("yyyy-MM-dd");
            }
            if (SnapshotType.ToUpper().Equals("DAILY"))
            {
                if (DateTime.Now.DayOfWeek.ToString().ToUpper().Equals("MONDAY"))
                    effectiveDate = DateTime.Now.AddDays(-3).ToString("yyyy-MM-dd");
                else
                    effectiveDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            }

            return effectiveDate;

        }

        /// <summary>
        /// Executes a given DB object and returns the dataset
        /// </summary>
        /// <param name="DBObjectName"></param>
        /// <param name="DBObjectType"></param>
        /// <param name="Params"></param>
        /// <param name="DBConnectionString"></param>
        /// <param name="ExecutionTimeInSeconds"></param>
        /// <param name="RuntimeQueryString"></param>
        /// <returns></returns>
        public DataSet GetDataSet(string DBObjectName, string DBObjectType, string Params, string DBConnectionString, ref long ExecutionTimeInSeconds, ref string RuntimeQueryString)
        {
            return dbRegressionDAC.GetDataSet(DBObjectName, DBObjectType, Params, DBConnectionString, ref ExecutionTimeInSeconds, ref RuntimeQueryString);
        }


        /// <summary>
        /// validates the dataset (currently not being used)
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="DBObjectName"></param>
        /// <param name="Params"></param>
        /// <param name="DBConnectionString"></param>
        private void ValidateDataSet(DataSet ds, string DBObjectName, string Params, string DBConnectionString)
        {

            string dbObject = DBObjectName + " " + (string.IsNullOrEmpty(Params) ? "" : Params);

            if (ds.Tables.Count == 0)
                throw new Exception($"Error populating the DataSet for DBObjectName:{dbObject} from DBConnectionString:{DBConnectionString}");
            else if (ds.Tables[0].Rows.Count == 0)
                throw new Exception($"No rows returned for DBObjectName:{dbObject} from DBConnectionString:{DBConnectionString}");

        }

        /// <summary>
        /// Validates configured key columns. A test cannot have both Primary Key and Sort Key columns configured
        /// </summary>
        /// <param name="TestCaseEntity"></param>
        /// <param name="PrimaryKeyColumns"></param>
        /// <param name="SortColumnsWhenNoPrimaryKey"></param>
        private void ValidateKeyColumns(TestCaseEntity TestCaseEntity, ref List<string> PrimaryKeyColumns, ref List<string> SortColumnsWhenNoPrimaryKey)
        {
            if (string.IsNullOrEmpty(TestCaseEntity.PrimaryKeyColumns) && string.IsNullOrEmpty(TestCaseEntity.SortColumnsWhenNoPrimaryKey))
                throw new Exception("PrimaryKey/SortKey Columns are not configured. If the datasets have a primary key configure PrimaryKeyColumns else configure SortColumns");

            if (!string.IsNullOrEmpty(TestCaseEntity.PrimaryKeyColumns))
                PrimaryKeyColumns = TestCaseEntity.PrimaryKeyColumns.Split(',').Select(x => x.Trim()).ToList();

            if (!string.IsNullOrEmpty(TestCaseEntity.SortColumnsWhenNoPrimaryKey))
                SortColumnsWhenNoPrimaryKey = TestCaseEntity.SortColumnsWhenNoPrimaryKey.Split(',').Select(x => x.Trim()).ToList();

            if ((PrimaryKeyColumns != null && PrimaryKeyColumns.Count > 0) && (SortColumnsWhenNoPrimaryKey != null && SortColumnsWhenNoPrimaryKey.Count > 0))
                throw new Exception($"Invalid configuration, cannot have both PrimaryKeyColumns and SortColumns configured. SortColumns to be only configured when data sets don't have a primary key");

        }

        /// <summary>
        /// Converts C# DataTable SQL Types to native .NET Types
        /// </summary>
        /// <param name="DataTable"></param>
        /// <returns></returns>
        private DataTable ConvertDataTableSQLTypesToDotNetType(DataTable DataTable)
        {
            DataTable convertedDT = new DataTable();

            foreach (DataColumn dc in DataTable.Columns)
            {
                DataColumn newColumn = new DataColumn(dc.ColumnName);

                newColumn.DataType = System.Type.GetType("System.String");

                convertedDT.Columns.Add(newColumn);
            }

            foreach (DataRow dr in DataTable.Rows)
            {
                convertedDT.LoadDataRow(dr.ItemArray, true);
            }

            return convertedDT;

        }

        /// <summary>
        /// Any included columns configured will be preserved in DataTable for Rec comparison
        /// </summary>
        /// <param name="SourceTable"></param>
        /// <param name="TargetTable"></param>
        /// <param name="ColumnsToIncludeForChecks"></param>
        public void PreserveIncludedColumnsInDataTables(ref DataTable SourceTable, ref DataTable TargetTable, List<String> ColumnsToIncludeForChecks)
        {


            List<string> sourceDTColumnList = (from DataColumn column in SourceTable.Columns
                                               select column.ColumnName).ToList<String>();

            //only include the columns from "ColumnsToIncludeForChecks" in DataTable and delete the rest
            IEnumerable<string> columnsToRemove = sourceDTColumnList.Except(ColumnsToIncludeForChecks);


            //include only those columns which are are configured in ColumnsToInclude list and remove the others
            foreach (string columnToRemove in columnsToRemove)
            {
                if (SourceTable.Columns.Contains(columnToRemove))
                    SourceTable.Columns.Remove(columnToRemove);

                if (TargetTable.Columns.Contains(columnToRemove))
                    TargetTable.Columns.Remove(columnToRemove);
            }

        }

        /// <summary>
        /// Any exclusion columns configured will be removed from DataTable for Rec comparison
        /// </summary>
        /// <param name="SourceTable"></param>
        /// <param name="TargetTable"></param>
        /// <param name="ColumnsToExcludeForChecks"></param>
        public void RemoveExcludedColumnsFromDataTables(ref DataTable SourceTable, ref DataTable TargetTable, List<String> ColumnsToExcludeForChecks)
        {

            foreach (string columnToExclude in ColumnsToExcludeForChecks)
            {
                if (SourceTable.Columns.Contains(columnToExclude))
                    SourceTable.Columns.Remove(columnToExclude);

                if (TargetTable.Columns.Contains(columnToExclude))
                    TargetTable.Columns.Remove(columnToExclude);
            }


        }

        /// <summary>
        /// Routes DataTables Comparison based on PrimaryKey/SortKey columns configured
        /// </summary>
        /// <param name="SourceDT"></param>
        /// <param name="TargetDT"></param>
        /// <param name="TestCaseResultEntity"></param>
        /// <param name="PrimaryKeyColumns"></param>
        /// <param name="SortColumnsWhenNoPrimaryKey"></param>
        /// <param name="ColumnsToAlwaysShowDespiteMatching"></param>
        /// <param name="DiffColumnsToInject"></param>
        /// <param name="DiffColumnsTolerance"></param>
        /// <param name="ShowMatchingColumnValues"></param>
        /// <returns></returns>
        private DataTable CompareDataTables(DataTable SourceDT, DataTable TargetDT, ref TestCaseResultEntity TestCaseResultEntity, List<string> PrimaryKeyColumns, List<string> SortColumnsWhenNoPrimaryKey, List<string> ColumnsToAlwaysShowDespiteMatching, List<string> DiffColumnsToInject, double DiffColumnsTolerance, Boolean ShowMatchingColumnValues)
        {
            DataTable dtResult = null;

            if (PrimaryKeyColumns != null && PrimaryKeyColumns.Count > 0)
                dtResult = CompareDataTablesWithPrimaryKeyColumns(SourceDT, TargetDT, ref TestCaseResultEntity, PrimaryKeyColumns, ColumnsToAlwaysShowDespiteMatching, DiffColumnsToInject, DiffColumnsTolerance, ShowMatchingColumnValues);
            else if (SortColumnsWhenNoPrimaryKey != null && SortColumnsWhenNoPrimaryKey.Count > 0)
                dtResult = CompareDataTablesWithSortKeyColumns(SourceDT, TargetDT, ref TestCaseResultEntity, SortColumnsWhenNoPrimaryKey, ColumnsToAlwaysShowDespiteMatching, DiffColumnsToInject, DiffColumnsTolerance, ShowMatchingColumnValues);

            return dtResult;
        }

        /// <summary>
        /// Compares 2 datatables based on configured list of Primay Key Columns
        /// </summary>
        /// <param name="SourceDT"></param>
        /// <param name="TargetDT"></param>
        /// <param name="TestCaseResultEntity"></param>
        /// <param name="PrimaryKeyColumns"></param>
        /// <param name="ColumnsToAlwaysShowDespiteMatching"></param>
        /// <param name="DiffColumnsToInject"></param>
        /// <param name="DiffColumnsTolerance"></param>
        /// <param name="ShowMatchingColumnValues"></param>
        /// <returns></returns>
        private DataTable CompareDataTablesWithPrimaryKeyColumns(DataTable SourceDT, DataTable TargetDT, ref TestCaseResultEntity TestCaseResultEntity, List<string> PrimaryKeyColumns, List<string> ColumnsToAlwaysShowDespiteMatching, List<string> DiffColumnsToInject, double DiffColumnsTolerance, Boolean ShowMatchingColumnValues)
        {

            TestCaseResultEntity.Result = true; //default to pass unless there are unmatched rows or a new row found in source

            DataTable sourceDTCopy = SourceDT.Copy(); //sourceDT is already part of a dataset so it won't let you to to be added to another dataset
            DataTable targetDTCopy = TargetDT.Copy();

            //Create empty DataTable
            DataTable resultsDT = new DataTable("ResultsDataTable");


            //use a DataSet to make use of a DataRelation object

            using (DataSet ds = new DataSet())
            {
                //add data tables
                ds.Tables.AddRange(new DataTable[] { sourceDTCopy, targetDTCopy });

                ds.Tables[0].Columns.Add("Reason", typeof(String));
                ds.Tables[1].Columns.Add("Reason", typeof(String));

                ds.Tables[0].Columns.Add("UnMatchedCount", typeof(String));
                ds.Tables[1].Columns.Add("UnMatchedCount", typeof(String));


                foreach (DataColumn dataColumn in ds.Tables[0].Columns)
                    resultsDT.Columns.Add(dataColumn.ColumnName.ToUpper(), typeof(String));


                //if the SourceTableRow not in TargetTable then add it to ResultsDataTable
                resultsDT.BeginLoadData();

                //finds the matching records between source and target based on primary key columns + the records in source which
                // don't exist in target
                //primary columns specified should come in the same order as they appear in the dataset
                var rowsMatchingOrInSource = sourceDTCopy.Rows.Cast<DataRow>().Select(sourceRow => new
                {
                    sourceRow,
                    targetRow = targetDTCopy.Rows.Find(sourceRow.ItemArray.Where((x, y) =>
                        PrimaryKeyColumns.Contains(sourceDTCopy.Columns[y].ColumnName,
                               StringComparer.InvariantCultureIgnoreCase)).ToArray())
                })
                           .Where(x => (x.targetRow != null || x.targetRow == null));

                var rowOnlyInTarget = targetDTCopy.Rows.Cast<DataRow>().Select(targetRow => new
                {
                    targetRow,
                    sourceRow = sourceDTCopy.Rows.Find(targetRow.ItemArray.Where((x, y) =>
                        PrimaryKeyColumns.Contains(targetDTCopy.Columns[y].ColumnName,
                               StringComparer.InvariantCultureIgnoreCase)).ToArray())
                })
                       .Where(x => (x.sourceRow == null));



                int unMatchedCountForTable = 0;
                bool foundNewRowInSource = false;
                bool foundNewRowInTarget = false;


                //process all the rows which are either matching  or only in source
                foreach (var dataRow in rowsMatchingOrInSource)
                {
                    DataRow rowToAdd = resultsDT.NewRow();
                    int unmatchedCountForRow = 0;

                    //row in source which doesn't have a matching row in Target
                    if (dataRow.targetRow == null)
                    {

                        ProcessNewRow(ref rowToAdd, ref resultsDT, ds, dataRow.sourceRow, "NewRowInSource");

                        foundNewRowInSource = true;
                        continue;
                    }

                    //for matching rows between source and target
                    foreach (DataColumn column in ds.Tables[0].Columns)
                    {
                        ProcessColumnValueForMatchingRow(column, dataRow.sourceRow, dataRow.targetRow, ref unmatchedCountForRow, ref unMatchedCountForTable, ref resultsDT, ref rowToAdd, DiffColumnsToInject, DiffColumnsTolerance);

                    }


                    if (unmatchedCountForRow == 0)
                    {
                        rowToAdd["Reason"] = "Match";
                        rowToAdd["UnMatchedCount"] = 0;
                    }

                    else
                    {
                        //rowToAdd["Reason"] = "NoMatch" + (unmatchedCountForRow.ToString() == "0" ? "" : "(" + unmatchedCountForRow.ToString() + ")");
                        rowToAdd["Reason"] = "NoMatch";
                        rowToAdd["UnMatchedCount"] = unmatchedCountForRow.ToString();

                    }

                    resultsDT.LoadDataRow(rowToAdd.ItemArray, true);


                }


                //process all the rows which are either matching  or only in target
                foreach (var dataRow in rowOnlyInTarget)
                {
                    DataRow rowToAdd = resultsDT.NewRow();

                    //row is source which doesn't have a matching row in Target
                    if (dataRow.sourceRow == null)
                    {

                        ProcessNewRow(ref rowToAdd, ref resultsDT, ds, dataRow.targetRow, "NewRowInTarget");

                        foundNewRowInTarget = true;
                        continue;
                    }

                }


                TestCaseResultEntity.UnMatchedCellCount = unMatchedCountForTable;

                TestCaseResultEntity.Result = (unMatchedCountForTable == 0 && !foundNewRowInSource && !foundNewRowInTarget) ? true : false;

                resultsDT.EndLoadData();
            }


            //if the flag is off then remove all the columns with blank values to make any analysis easier for Devs)
            if (!ShowMatchingColumnValues)
                RemoveMatchingColumnsFromDataTable(resultsDT, PrimaryKeyColumns, ColumnsToAlwaysShowDespiteMatching);

            return resultsDT;

        }

        /// <summary>
        /// Compares 2 datatables based on configured list of Sort Key Columns
        /// </summary>
        /// <param name="SourceDT"></param>
        /// <param name="TargetDT"></param>
        /// <param name="TestCaseResultEntity"></param>
        /// <param name="SortColumnsWhenNoPrimaryKey"></param>
        /// <param name="ColumnsToAlwaysShowDespiteMatching"></param>
        /// <param name="DiffColumnsToInject"></param>
        /// <param name="DiffColumnsTolerance"></param>
        /// <param name="ShowMatchingColumnValues"></param>
        /// <returns></returns>
        private DataTable CompareDataTablesWithSortKeyColumns(DataTable SourceDT, DataTable TargetDT, ref TestCaseResultEntity TestCaseResultEntity, List<string> SortColumnsWhenNoPrimaryKey, List<string> ColumnsToAlwaysShowDespiteMatching, List<string> DiffColumnsToInject, double DiffColumnsTolerance, Boolean ShowMatchingColumnValues)
        {
            DataTable sourceDTCopy = SourceDT.Copy(); //sourceDT is already part of a dataset so it won't let you to to be added to another dataset
            DataTable targetDTCopy = TargetDT.Copy();

            //Create empty DataTable
            DataTable resultsDT = new DataTable("ResultsDataTable");


            //use a DataSet to make use of a DataRelation object

            using (DataSet ds = new DataSet())
            {
                //add data tables
                ds.Tables.AddRange(new DataTable[] { sourceDTCopy, targetDTCopy });

                ds.Tables[0].Columns.Add("Reason", typeof(String));
                ds.Tables[1].Columns.Add("Reason", typeof(String));

                ds.Tables[0].Columns.Add("UnMatchedCount", typeof(String));
                ds.Tables[1].Columns.Add("UnMatchedCount", typeof(String));


                DataColumn[] sourceColumns = SortColumnsWhenNoPrimaryKey.Select(x => sourceDTCopy.Columns[x]).ToArray();
                DataColumn[] targetColumns = SortColumnsWhenNoPrimaryKey.Select(x => targetDTCopy.Columns[x]).ToArray();


                //add the relationship between the key columns in the data set

                DataRelation dRelation1 = new DataRelation(string.Empty, sourceColumns, targetColumns, false);
                ds.Relations.Add(dRelation1);

                DataRelation dRelation2 = new DataRelation(string.Empty, targetColumns, sourceColumns, false);
                ds.Relations.Add(dRelation2);


                foreach (DataColumn dataColumn in ds.Tables[0].Columns)
                    resultsDT.Columns.Add(dataColumn.ColumnName.ToUpper(), typeof(String));


                //if the SourceTableRow not in TargetTable then add it to ResultsDataTable
                resultsDT.BeginLoadData();

                int unMatchedCountForTable = 0;
                bool foundNewRowInSource = false;
                bool foundNewRowInTarget = false;

                //TODO: redundant code shared between both comparision - to be refactored
                foreach (DataRow parentRow in ds.Tables[0].Rows)
                {

                    DataRow rowToAdd = resultsDT.NewRow();
                    int unmatchedCountForRow = 0;

                    DataRow[] childRows = parentRow.GetChildRows(dRelation1);


                    //TODO: needs refactoring here
                    if (childRows is null || childRows.Count() == 0)
                    {

                        ProcessNewRow(ref rowToAdd, ref resultsDT, ds, parentRow, "NewRowInSource");

                        foundNewRowInSource = true;
                        continue;

                    }



                    DataRow childRow = childRows[0];


                    foreach (DataColumn column in ds.Tables[0].Columns)
                    {
                        ProcessColumnValueForMatchingRow(column, parentRow, childRow, ref unmatchedCountForRow, ref unMatchedCountForTable, ref resultsDT, ref rowToAdd, DiffColumnsToInject, DiffColumnsTolerance);
                    }

                    if (unmatchedCountForRow == 0)
                    {
                        rowToAdd["Reason"] = "Match";
                        rowToAdd["UnMatchedCount"] = 0;
                    }

                    else
                    {
                        //rowToAdd["Reason"] = "NoMatch" + (unmatchedCountForRow.ToString() == "0" ? "" : "(" + unmatchedCountForRow.ToString() + ")");
                        rowToAdd["Reason"] = "NoMatch";
                        rowToAdd["UnMatchedCount"] = unmatchedCountForRow.ToString();

                    }

                    resultsDT.LoadDataRow(rowToAdd.ItemArray, true);

                }

                foreach (DataRow parentRow in ds.Tables[1].Rows)
                {

                    DataRow rowToAdd = resultsDT.NewRow();

                    DataRow[] childRows = parentRow.GetChildRows(dRelation2);


                    //TODO: needs refactoring here
                    if (childRows is null || childRows.Count() == 0)
                    {

                        ProcessNewRow(ref rowToAdd, ref resultsDT, ds, parentRow, "NewRowInTarget");

                        foundNewRowInSource = true;
                        continue;

                    }


                }


                TestCaseResultEntity.UnMatchedCellCount = unMatchedCountForTable;

                TestCaseResultEntity.Result = (unMatchedCountForTable == 0 && !foundNewRowInSource && !foundNewRowInTarget) ? true : false;
                resultsDT.EndLoadData();
            }

            //if the flag is off then remove all the columns with blank values to make any analysis easier for Devs)
            if (!ShowMatchingColumnValues)
                RemoveMatchingColumnsFromDataTable(resultsDT, SortColumnsWhenNoPrimaryKey, ColumnsToAlwaysShowDespiteMatching);

            return resultsDT;
        }


        /// <summary>
        /// Processes a new row if found in source or target
        /// </summary>
        /// <param name="RowToAdd"></param>
        /// <param name="ResultsDT"></param>
        /// <param name="ds"></param>
        /// <param name="DataRow"></param>
        /// <param name="Reason"></param>
        private void ProcessNewRow(ref DataRow RowToAdd, ref DataTable ResultsDT, DataSet ds, DataRow DataRow, string Reason)
        {
            foreach (DataColumn column in ds.Tables[0].Columns)
            {
                string columnName = column.ColumnName;
                string sourceRowColumnValue = GetRowColumValue(DataRow[columnName]);
                RowToAdd[columnName] = sourceRowColumnValue;
            }
            RowToAdd["Reason"] = Reason;
            ResultsDT.LoadDataRow(RowToAdd.ItemArray, true);
        }

        /// <summary>
        /// Processes column values for matching rows based on Primary/Sort Keys 
        /// </summary>
        /// <param name="Column"></param>
        /// <param name="SourceRow"></param>
        /// <param name="TargetRow"></param>
        /// <param name="RowUnMatchedCount"></param>
        /// <param name="TableUnMatchedCount"></param>
        /// <param name="ResultsDT"></param>
        /// <param name="RowToAdd"></param>
        /// <param name="DiffColumnsToInject"></param>
        /// <param name="DiffColumnsTolerance"></param>
        private void ProcessColumnValueForMatchingRow(DataColumn Column, DataRow SourceRow, DataRow TargetRow, ref int RowUnMatchedCount, ref int TableUnMatchedCount, ref DataTable ResultsDT, ref DataRow RowToAdd, List<string> DiffColumnsToInject, double DiffColumnsTolerance)
        {
            string columnName = Column.ColumnName;
            string sourceRowColumnValue = GetRowColumValue(SourceRow[columnName]);
            string targetRowColumnValue = GetRowColumValue(TargetRow[columnName]);
            

         
            //handle cases for NULL<>"" or ""<>NULL and ignore any of those differences
            if (sourceRowColumnValue.Trim().ToUpper().Equals("NULL") && targetRowColumnValue.Trim().ToUpper().Equals("") ||
                sourceRowColumnValue.Trim().ToUpper().Equals("") && targetRowColumnValue.Trim().ToUpper().Equals("NULL")
                )
            {
                sourceRowColumnValue = "";
                targetRowColumnValue = "";
            }
                

            if (!sourceRowColumnValue.Trim().ToUpper().Equals(targetRowColumnValue.Trim().ToUpper()))
            {

                RowUnMatchedCount += 1;
                TableUnMatchedCount += 1;

                double diffValue = 9999999999999999999;
                //for any diff column injected if the diff value is less than certain threshhold then treat that as match
                CheckAndAddDiffColumn(ref ResultsDT, ref RowToAdd, columnName, sourceRowColumnValue, targetRowColumnValue, DiffColumnsToInject, ref diffValue);
                if (diffValue <= DiffColumnsTolerance)
                {
                    RowUnMatchedCount -= 1;
                    TableUnMatchedCount -= 1;
                }

                RowToAdd[Column.ColumnName] = sourceRowColumnValue + "<>" + targetRowColumnValue;
            }
            else
                RowToAdd[columnName] = sourceRowColumnValue;
        }

        /// <summary>
        /// Process column value for a given column in a row
        /// </summary>
        /// <param name="RowColumn"></param>
        /// <returns></returns>
        private string GetRowColumValue(Object RowColumn)
        {
            string rowColumnValue = DBNull.Value.Equals(RowColumn) ? "NULL" : RowColumn.ToString();
            //rowColumnValue = string.IsNullOrEmpty(rowColumnValue.Trim()) ? "''" : rowColumnValue;
            rowColumnValue = string.IsNullOrEmpty(rowColumnValue.Trim()) ? "" : rowColumnValue;

            int intResult;
            DateTime dtResult;
            Double doubleResult;

            if (int.TryParse(rowColumnValue, out intResult))
                rowColumnValue = intResult.ToString();
            //sometimes a double value 11.02 can be converted to DateTime hence do the Double checking first
            else if (Double.TryParse(rowColumnValue, out doubleResult))
            {
                //sometimes a certain value in LM_Common for instance DIRTY_VALUE_QC is 1400 and the same in IDH is 1400.00 we don't want different formattings to be 
                //applied for these values that will report them as difference, hence the below logig
                if (doubleResult == 0 || Math.Floor(doubleResult) == doubleResult)
                    rowColumnValue = doubleResult.ToString();
                else//if the incoming value is 0.00 then the below string format function will result in "" hence check if doubleResult == 0 in the if statement above
                    rowColumnValue = String.Format("{0:#,###,###.##########}", doubleResult);
            }
            else if (DateTime.TryParse(rowColumnValue, out dtResult))
                rowColumnValue = dtResult.ToString("yyyy-MM-dd");


            return rowColumnValue;

        }

        /// <summary>
        /// Injects Diff columns into the results datatable for any metric colums configured in the test
        /// </summary>
        /// <param name="ResultsDT"></param>
        /// <param name="DR"></param>
        /// <param name="ColumName"></param>
        /// <param name="SourceColumnValue"></param>
        /// <param name="TargetColumnValue"></param>
        /// <param name="DiffColumnsToInject"></param>
        /// <param name="DiffValue"></param>
        private void CheckAndAddDiffColumn(ref DataTable ResultsDT, ref DataRow DR, string ColumName, string SourceColumnValue, string TargetColumnValue, List<string> DiffColumnsToInject, ref double DiffValue)
        {

            if (DiffColumnsToInject != null && DiffColumnsToInject.Where(x => x.ToUpper().Contains(ColumName.ToUpper())).Count() > 0)
            {
                Double sourceDoubleResult;
                Double targetDoubleResult;
                string diffColumNameToAdd;

                if (Double.TryParse(SourceColumnValue, out sourceDoubleResult) && Double.TryParse(TargetColumnValue, out targetDoubleResult))
                {

                    DiffValue = Math.Abs(Math.Abs(Math.Round(targetDoubleResult, 2)) - Math.Abs(Math.Round(sourceDoubleResult, 2)));

                    diffColumNameToAdd = ColumName + "_DIFF";
                    if (!ResultsDT.Columns.Contains(diffColumNameToAdd))
                        ResultsDT.Columns.Add(diffColumNameToAdd);

                    DR[diffColumNameToAdd] = DiffValue;

                }
            }

        }

        /// <summary>
        /// removes any columns from data table where the values are perfectly matching between source and destination. Set any matching column values (which doesnt have <>) to blank
        /// to make analysis easier for developer
        /// </summary>
        /// <param name="DTResult"></param>
        private void RemoveMatchingColumnsFromDataTable(DataTable DTResult, List<String> PrimaryKeyColumns, List<string> ColumnsToAlwaysShowDespiteMatching)
        {

            //remove those columns where there the values are perfectly matching
            var columnsToRemove = (from DataColumn column in DTResult.Columns.Cast<DataColumn>().AsQueryable()
                                   where DTResult.AsEnumerable().All(row => !row[column].ToString().Contains("<>"))
                                   select column).ToList();


            foreach (DataColumn dc in columnsToRemove)
            {
                if (
                   !PrimaryKeyColumns.Contains(dc.ColumnName, StringComparer.InvariantCultureIgnoreCase)
                   && (ColumnsToAlwaysShowDespiteMatching == null || !ColumnsToAlwaysShowDespiteMatching.Contains(dc.ColumnName, StringComparer.InvariantCultureIgnoreCase))
                   && !dc.ColumnName.ToUpper().Equals("REASON")
                   && !dc.ColumnName.ToUpper().Equals("UNMATCHEDCOUNT")
                   && !dc.ColumnName.Contains("_DIFF")
                    )
                    DTResult.Columns.Remove(dc);
            }


            //check all the row values in each column and where the cell value doesn't contain <>, just replace it with blank value.
            //make it easy for developers to anlyse results

            foreach (DataColumn dc in DTResult.Columns)
            {
                if (
                   !PrimaryKeyColumns.Contains(dc.ColumnName, StringComparer.InvariantCultureIgnoreCase)//ignore PrimaryKeyColumns (always show values for PrimaryKey columns)
                   && (ColumnsToAlwaysShowDespiteMatching == null || !ColumnsToAlwaysShowDespiteMatching.Contains(dc.ColumnName, StringComparer.InvariantCultureIgnoreCase))//ignore any always show columns
                   && !dc.ColumnName.ToUpper().Equals("REASON")  //ignore framework inject columns
                   && !dc.ColumnName.ToUpper().Equals("UNMATCHEDCOUNT")
                   && !dc.ColumnName.Contains("_DIFF") //ignore DIFF columns
                    )
                    DTResult.Rows.Cast<DataRow>().Where(r => !r[dc].ToString().Contains("<>")).ToList().ForEach(r => r.SetField(dc, ""));

            }



        }

        /// <summary>
        /// If diff column differences are within acceptable threshhold and the source column used for diff doesn't have
        /// any other differences (ex 0<>NULL etc) then delete both the diff and source column for the diff. 
        /// </summary>
        /// <param name="DTResult"></param>
        /// <param name="DiffColumnsToInject"></param>
        private void CheckAndRemoveDiffColumnsFromDataTableResults(ref DataTable DTResult, List<string> DiffColumnsToInject, double DiffColumnsTolerance)
        {

            if (DiffColumnsToInject != null)
            {
                foreach (string columnToDiff in DiffColumnsToInject)
                {
                    string diffColumnName = $"{columnToDiff}_DIFF";

                    if (DTResult.Columns.Contains(diffColumnName))
                    {

                        //SELECT * FROM dbo.SCD_Pfcholdings_Select_20200229_Month_End_1613 WHERE DIRTY_VALUE_AUD_DIFF > 0.10 OR(DIRTY_VALUE_AUD_DIFF IS NULL AND DIRTY_VALUE_AUD <> '')
                        //string query = $"{diffColumnName} > 0.10 OR ({diffColumnName} IS NULL AND Reason <> 'Match')";
                        //string query = $"{diffColumnName} > 0.10 OR ({diffColumnName} IS NULL AND {columnToDiff} <>'')";
                        string query = $"{diffColumnName} > {DiffColumnsTolerance.ToString("0.00")} OR ({diffColumnName} IS NULL AND {columnToDiff} <>'')";

                        //remove the source column for diff(Ex.DIRTY_VALUE_AUD) only when the above query criteria matches.
                        //diff column (DIRT_VALUE_AUD_DIFF) can have null value if DIRTY_VALUE_AUD is stored in log table as 0<>NULL or the value is blank/null from both systems
                        if (DTResult.Select(query).Count() == 0)
                            DTResult.Columns.Remove(columnToDiff);

                        //remove any Diff columns (ex.DIRTY_VALUE_AUD_DIFF) where difference > 0.10
                        query = $"{diffColumnName} > {DiffColumnsTolerance.ToString("0.00")}";
                        if (DTResult.Select(query).Count() == 0)
                            DTResult.Columns.Remove(diffColumnName);

                        /*
                        IEnumerable<DataRow> rows = DTResult.Rows.Cast<DataRow>().Where(r => r[diffColumnName].ToString().Equals("0") && r["REASON"].ToString().Equals("NoMatch") && r[columnToDiff].ToString().Contains("<>"));
                        foreach (DataRow dr in rows)
                            dr.SetField(columnToDiff, "");
                        */

                        //set any source columns for DIFF (ex. DIRTY_VALUE_AUD 9965775.215<>9965775.21519969) where the values are not matching because of rounding issues but DIFF value is null and the reason is NoMatch because there are other non matching columns.
                        if (DTResult.Columns.Contains(diffColumnName) && DTResult.Columns.Contains(columnToDiff))
                            DTResult.Rows.Cast<DataRow>().Where(r => r[diffColumnName].ToString().Equals("0") && r["Reason"].Equals("NoMatch") && r[columnToDiff].ToString().Contains("<>")).ToList().ForEach(r => r.SetField(columnToDiff, ""));

                    }
                }
            }
        }


        /// <summary>
        /// Return the value for a given key from NameValue collection
        /// </summary>
        /// <param name="ParamName"></param>
        /// <param name="NVParams"></param>
        /// <returns></returns>
        private Object GetIndividualParamFromNVCollection(string ParamName, NameValueCollection NVParams)
        {

            Object paramValue = null;

            //TODO: Make these aliases configurable EffectiveDate/ValuationDate/AsAtDate etc
            //same for SnapshotType
            if (ParamName.ToUpper().Equals("EFFECTIVEDATE"))
            {
                if (NVParams.AllKeys.Contains(ParamName, StringComparer.OrdinalIgnoreCase))
                    paramValue = NVParams[ParamName];
                else if (NVParams.AllKeys.Contains("ValuationDate", StringComparer.OrdinalIgnoreCase))
                    paramValue = NVParams["ValuationDate"];
                else if (NVParams.AllKeys.Contains("AsAtDate", StringComparer.OrdinalIgnoreCase))
                    paramValue = NVParams["AsAtDate"];
            }

            if (ParamName.ToUpper().Equals("SNAPSHOTTYPE"))
            {
                if (NVParams.AllKeys.Contains(ParamName, StringComparer.OrdinalIgnoreCase))
                    paramValue = NVParams[ParamName];

            }

            return paramValue;

        }

        /// <summary>
        /// Orders the results datatable based on the configured column order sequence
        /// </summary>
        /// <param name="DTResult"></param>
        /// <param name="columnOrderSequenceInOutput"></param>
        public void CheckAndSetColumnsOrder(ref DataTable DTResult, List<string> columnOrderSequenceInOutput)
        {

            if (columnOrderSequenceInOutput != null)
            {
                int colIndex = 0;
                foreach (string column in columnOrderSequenceInOutput)
                {
                    if (DTResult.Columns.Contains(column))
                    {
                        DTResult.Columns[column].SetOrdinal(colIndex);
                        colIndex += 1;
                    }

                }
            }

        }


        /// <summary>
        /// Formats Diff Columns by adding commas in the values for readability purposes
        /// currently not being used as this would make any filtering harder in sql 
        /// </summary>
        /// <param name="DTResult"></param>
        /// <param name="DiffColumnsToInject"></param>
        public void FormatDiffColumns(ref DataTable DTResult, List<string> DiffColumnsToInject)
        {
            if (DiffColumnsToInject != null)
            {
                foreach (string columnToDiff in DiffColumnsToInject)
                {
                    string diffColumnName = $"{columnToDiff}_DIFF";

                    if (DTResult.Columns.Contains(diffColumnName))
                    {
                        double doubleResult;
                        if (DTResult.Columns.Contains(diffColumnName) && DTResult.Columns.Contains(columnToDiff))
                        {
                            /*
                             IEnumerable<DataRow> dRows = DTResult.Rows.Cast<DataRow>().Where(r => double.TryParse(r[diffColumnName].ToString(), out doubleResult)).ToList();
                             foreach (DataRow dr in dRows)
                             {
                                 double diffColumnValue = double.Parse(dr[diffColumnName].ToString()); ;
                                 //dr[diffColumnName] = String.Format("{0:#,###,###.##########}", diffColumnValue);
                                 dr[diffColumnName] = String.Format("{0:#,###,###.##########}", double.Parse(dr[diffColumnName].ToString()));
                             }
                             */

                            DTResult.Rows.Cast<DataRow>().Where(r => double.TryParse(r[diffColumnName].ToString(), out doubleResult)).ToList().ForEach(r => r.SetField(diffColumnName, String.Format("{0:#,###,###.##########}", double.Parse(r[diffColumnName].ToString()))));

                        }

                    }
                }
            }
        }

        /// <summary>
        /// Gets the LogTable name for a given test
        /// </summary>
        /// <param name="TestCaseEntity"></param>
        /// <param name="ParamEffectiveDate"></param>
        /// <param name="ParamSnapshotType"></param>
        /// <returns></returns>
        public string GetLogTableName(TestCaseEntity TestCaseEntity, string ParamEffectiveDate, string ParamSnapshotType)
        {
            string logTableName = string.Empty;

            if (TestCaseEntity.SourceDBObjectType.ToUpper().Equals("QUERY"))
                //default the schema to dbo
                logTableName = "dbo." + TestCaseEntity.TestName.Replace(" ", "_").Replace("'", "") + "_" + DateTime.Now.ToString("yyyyMMdd") + "_" + TestCaseEntity.Id.ToString();
            else
            {
                logTableName = TestCaseEntity.SourceDBObjectName.Replace("[", "").Replace("]", "");
                if (string.IsNullOrEmpty(ParamEffectiveDate) && string.IsNullOrEmpty(ParamSnapshotType))
                    logTableName += "_" + DateTime.Now.ToString("yyyyMMdd");

                if (!string.IsNullOrEmpty(ParamEffectiveDate))
                    logTableName += "_" + ParamEffectiveDate;

                if (!string.IsNullOrEmpty(ParamSnapshotType))
                    logTableName += "_" + ParamSnapshotType.Replace("-", "_");//repalce Month-End has Month_End has hyphen causes issues with SQL

                logTableName += "_" + TestCaseEntity.Id.ToString();
            }

            return logTableName;


        }

        /// <summary>
        /// Checks and Drops given LogTable
        /// </summary>
        /// <param name="TableName"></param>
        private void CheckAndDropTable(string TableName)
        {
            dbRegressionDAC.CheckAndDropTable(TableName);
        }

        /// <summary>
        /// Saves TestMetrics (Summary Results) for a test to DB
        /// </summary>
        /// <param name="TestCaseResultEntity"></param>
        public void SaveTestMetricsToDB(TestCaseResultEntity TestCaseResultEntity)
        {
            dbRegressionDAC.SaveTestMetricsToDB(TestCaseResultEntity);
        }

        /// <summary>
        /// Saves detailed Rec Results for a test to DB
        /// </summary>
        /// <param name="DT"></param>
        /// <param name="TableName"></param>
        private void StoreDataTableResultsToDB(DataTable DT, string TableName)
        {
            //creates log table and grants insert permission for the SaveDataTableToDB() to write results
            dbRegressionDAC.CheckAndCreateTable(DT, TableName);

            //writes rec errors to the log table using bulk copy
            dbRegressionDAC.SaveDataTableToDB(DT, TableName);

            //revokes insert permissions for the log table
            dbRegressionDAC.RevokePermssionsForLogTable(TableName);

        }

        /// <summary>
        /// Checks and emails Regression Test Results (csv attachment) to configured recipients
        /// </summary>
        /// <param name="DTResult"></param>
        /// <param name="LogTableName"></param>
        /// <param name="TestCaseResultEntity"></param>
        /// <param name="EffectiveDate"></param>
        /// <param name="TestName"></param>
        /// <param name="ITRecipients"></param>
        /// <param name="BusinessRecipients"></param>
        /// <param name="ColumnsToCheckForBusinessAlerting"></param>
        /// <param name="NTUserEmail"></param>
        private void CheckAndEmailRegressionTestResults(DataTable DTResult, String LogTableName, TestCaseResultEntity TestCaseResultEntity, string EffectiveDate, string TestName, string ITRecipients, string BusinessRecipients, List<string> ColumnsToCheckForBusinessAlerting, string NTUserEmail)
        {

            DataTable dtRecErrors = DTResult.Select("Reason <> 'Match'").CopyToDataTable();

            if (dtRecErrors.Rows.Count > 0)
            {

                string recErrorsCSVFilePath = AppConfiguration.RecErrorsCSVFilePath.Replace("{FileName}", LogTableName);

                Common.Common.CheckAndDeleteFile(recErrorsCSVFilePath);

                CheckAndCreateRegressionErrorsCSV(LogTableName, recErrorsCSVFilePath, dtRecErrors);


                List<string> fileAttachments = new List<string>();
                fileAttachments.Add(recErrorsCSVFilePath);

                string recResult = TestCaseResultEntity.Result ? "SUCCESS" : "FAILED";
                if (!string.IsNullOrEmpty(EffectiveDate))
                {
                    EffectiveDate = DateTime.ParseExact(EffectiveDate, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                }

                string subject = GetEmailSubject(recResult, TestName, EffectiveDate);

                string body = GetEmailBody(TestCaseResultEntity, recResult);

                bool toIncludeBusinessRecipients = ToIncludeBusinessRecipients(DTResult, ColumnsToCheckForBusinessAlerting);

                string emailRecipients = GetEmailRecipients(ITRecipients, BusinessRecipients, toIncludeBusinessRecipients, NTUserEmail);

                if (string.IsNullOrEmpty(emailRecipients))
                    throw new Exception("EmailRecipients are not configured for the test case");

                Common.Common.SendEmail(subject, body, true, emailRecipients, fileAttachments);


            }

        }

        /// <summary>
        /// Constructs the email subject for a test
        /// </summary>
        /// <param name="RecResult"></param>
        /// <param name="TestName"></param>
        /// <param name="EffectiveDate"></param>
        /// <returns></returns>
        private string GetEmailSubject(string RecResult, string TestName, string EffectiveDate)
        {

            string emailSubject = string.Empty;

            emailSubject = $"<{RecResult}> {TestName}";

            if (!string.IsNullOrEmpty(EffectiveDate))
            {
                emailSubject += $" for { EffectiveDate}";
            }

            return emailSubject;
        }


        /// <summary>
        /// constructs the email body for a test
        /// </summary>
        /// <param name="TestCaseResultEntity"></param>
        /// <param name="RecResult"></param>
        /// <returns></returns>
        private string GetEmailBody(TestCaseResultEntity TestCaseResultEntity, string RecResult)
        {

            StringBuilder sbEmailBody = new StringBuilder();

            sbEmailBody.AppendLine("Summary: ");
            sbEmailBody.AppendLine("<br/>");
            sbEmailBody.AppendLine("<br/>");

            sbEmailBody.AppendLine("<table border = \"1\" cellpadding= \"1\" cellspacing= \"2\">");
            sbEmailBody.AppendLine("<tr>");
            sbEmailBody.AppendLine("<td><b>TestConfigId</b></td>");
            sbEmailBody.AppendLine("<td><b>LM_CommonRowCount</b></td>");
            sbEmailBody.AppendLine("<td><b>IDHRowCount</b></td>");
            sbEmailBody.AppendLine("<td><b>TotalDifferences</b></td>");
            sbEmailBody.AppendLine("<td><b>Result</b></td>");
            sbEmailBody.AppendLine("<td><b>LogTableName</b></td>");
            sbEmailBody.AppendLine("</tr>");

            sbEmailBody.AppendLine("<tr>");
            sbEmailBody.AppendLine($"<td>{TestCaseResultEntity.TestConfigId}</td>");
            sbEmailBody.AppendLine($"<td>{TestCaseResultEntity.SourceRowCount}</td>");
            sbEmailBody.AppendLine($"<td>{TestCaseResultEntity.TargetRowCount}</td>");
            sbEmailBody.AppendLine($"<td>{TestCaseResultEntity.UnMatchedCellCount}</td>");
            sbEmailBody.AppendLine($"<td>{RecResult}</td>");
            sbEmailBody.AppendLine($"<td>{TestCaseResultEntity.LogTableName}</td>");
            sbEmailBody.AppendLine("</tr>");
            sbEmailBody.AppendLine("</table>");
            sbEmailBody.AppendLine("<br/>");

            sbEmailBody.AppendLine("Please refer to the attachment for more details.");

            return sbEmailBody.ToString();


        }


        /// <summary>
        /// Checks and includes business recipients only if columns configured in the test attribute "ColumnsToCheckForBusinessAlerting" has rec issues
        /// </summary>
        /// <param name="DTResult"></param>
        /// <param name="ColumnsToCheckForBusinessAlerting"></param>
        /// <returns></returns>
        public bool ToIncludeBusinessRecipients(DataTable DTResult, List<string> ColumnsToCheckForBusinessAlerting)
        {

            bool result = false;

            //for some test cases the business might want to receive the Rec email only when certain configured columns have issues ex DIRTY_VALUE_QC, DIRTY_VALUE_AUD etc.
            if (ColumnsToCheckForBusinessAlerting is null)
                result = true;
            else
            {
                foreach (string column in ColumnsToCheckForBusinessAlerting)
                {
                    if (DTResult.Columns.Contains(column))
                        result = true;

                }
            }

            return result;
        }

        /// <summary>
        /// Constructs email recipients for the test
        /// </summary>
        /// <param name="ITRecipients"></param>
        /// <param name="BusinessRecipients"></param>
        /// <param name="ToIncludeBusinessRecipients"></param>
        /// <param name="NTUserEmail"></param>
        /// <returns></returns>
        public string GetEmailRecipients(string ITRecipients, string BusinessRecipients, bool ToIncludeBusinessRecipients, string NTUserEmail)
        {
            string emailRecipients = string.Empty;

            emailRecipients += !string.IsNullOrEmpty(ITRecipients) ? ITRecipients : "";

            if (ToIncludeBusinessRecipients)
            {

                if (emailRecipients.EndsWith(",") || emailRecipients.EndsWith(";"))
                    emailRecipients = emailRecipients.Substring(0, emailRecipients.Length - 1);

                emailRecipients += !string.IsNullOrEmpty(BusinessRecipients) ? "," + BusinessRecipients : "";

                if (emailRecipients.EndsWith(",") || emailRecipients.EndsWith(";"))
                    emailRecipients = emailRecipients.Substring(0, emailRecipients.Length - 1);

                //if the user is triggering the Rec framework through UI (LifeDWPortal) nd the test has been configured to receive rec email then include the NTUser in the Rec email.
                emailRecipients += !string.IsNullOrEmpty(NTUserEmail) ? "," + NTUserEmail : "";
            }

            return emailRecipients;

        }

        /// <summary>
        /// Checks and creates csv file with Rec errors for a test
        /// </summary>
        /// <param name="LogTableName"></param>
        /// <param name="RecErrorsCSVFilePath"></param>
        /// <param name="DTRecErrors"></param>
        private void CheckAndCreateRegressionErrorsCSV(string LogTableName, string RecErrorsCSVFilePath, DataTable DTRecErrors)
        {
            try
            {
                IEnumerable<string> columnNames = DTRecErrors.Columns.Cast<DataColumn>().Select(x => x.ColumnName);

                StringBuilder sb = new StringBuilder();
                var delimiter = ",";
                sb.AppendLine(string.Join(delimiter, columnNames));
                foreach (DataRow row in DTRecErrors.Rows)
                {
                    string[] fields = row.ItemArray.Select(field => EscapeSpecialCharactersInField(field.ToString())).
                                                    ToArray();
                    sb.AppendLine(string.Join(delimiter, fields));
                }

                File.WriteAllText(RecErrorsCSVFilePath, sb.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating CSV for LogTableName: {LogTableName}, details {ex.ToString()}");
            }

        }

        /// <summary>
        /// Escapes any special characters from a given field value
        /// </summary>
        /// <param name="FieldValue"></param>
        /// <returns></returns>
        private string EscapeSpecialCharactersInField(string FieldValue)
        {

            //for any other special characters, handle as per here: https://www.codingvila.com/2018/12/export-dataset-datatable-to-csv-file-using-csharp-and-vb-dot-net.html
            if (FieldValue.Contains(","))
                return "\"" + FieldValue + "\"";
            else if (FieldValue.Contains("<>"))
                return FieldValue.Replace("<>", "< >");//csv treats "<>" as conditional check and evaluates the expression to true or false for field values 32.1234<>32.12345, hence put a space.
            else
                return FieldValue;

        }
    }
}

