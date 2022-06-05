using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Linq;



using DBRegressionLibrary.BusinessEntities;
using DBRegressionLibrary.Common;
using Dapper;




namespace DBRegressionLibrary.DataAccessComponent
{

    /// <summary>
    /// Data Access Component
    /// </summary>
    public class DBRegressionDAC
    {
        private string testConfigConnectionString = AppConfiguration.TestConfigConnectionString;


        /// <summary>
        /// Returns enabled tests from DB
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TestCaseEntity> GetDBTestsConfig()
        {
            IEnumerable<TestCaseEntity> objList = null;

            //string sql = "SELECT * FROM dbo.DBTestsConfig WHERE IsEnabled = 1";
            string sql = "[dbo].[uspGetDBTestsConfig]";

            try
            {
                using (IDbConnection dbCon = new SqlConnection(testConfigConnectionString))
                {

                    //CommandDefinition cmd = new CommandDefinition(sql, commandType: CommandType.Text);
                    CommandDefinition cmd = new CommandDefinition(sql, commandType: CommandType.StoredProcedure, commandTimeout: 300);
                    objList = dbCon.Query<TestCaseEntity>(cmd);

                }

                if (objList.Count() == 0)
                    throw new Exception("No Enabled tests are found for execution, please check");

            }
            catch (Exception ex)
            {

                throw new Exception($"Exception in GetDBTestsConfig(), details: {ex.ToString()}");
            }

            return objList;
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
            SqlConnection sqlCon = null;
            SqlCommand sqlCmd = null;
            SqlDataAdapter sqlDA = null;

            string sql = string.Empty;

            try
            {
                sqlCon = new SqlConnection(DBConnectionString);
                sqlCmd = new SqlCommand();
                sqlCmd.Connection = sqlCon;
                sqlCon.Open();

                sqlCon.StatisticsEnabled = true;

                sql = BuildSqlString(DBObjectName, DBObjectType, Params);
                RuntimeQueryString = sql;

                //sqlCmd.CommandText = $"SELECT * FROM {DBObjectName} WAITFOR DELAY '00:00:02' ";
                sqlCmd.CommandText = sql;
                sqlCmd.CommandTimeout = 0; //default timeout is 30 seconds, 0 means no time limit

                sqlDA = new SqlDataAdapter(sqlCmd);

                sqlDA.FillError += new FillErrorEventHandler(FillError);

                DataSet ds = new DataSet();

                //columns in database has type decimal(38,10), decimal(38, 0) etc. Their values range is larger than decimal's type in C#, that's why I get an OverflowException.
                sqlDA.ReturnProviderSpecificTypes = true;

                sqlDA.Fill(ds);

                var stats = sqlCon.RetrieveStatistics();
                ExecutionTimeInSeconds = (long)(stats["ExecutionTime"]) / 1000;

                return ds;


            }
            catch (Exception ex)
            {
                throw new Exception($"Exception in GetDataSet(), unable to fetch dataset for DBObjectName:{DBObjectName} from DBConnectionString:{DBConnectionString}, details: {ex.ToString()} ");
            }
            finally
            {

                if (sqlCon != null)
                {
                    sqlCon.Close();
                    sqlCon.Dispose();
                }

                if (sqlCmd != null)
                    sqlCmd.Dispose();

                if (sqlDA != null)
                    sqlDA.Dispose();


            }

        }

        /// <summary>
        /// Builds sql string to be executed for a given test
        /// </summary>
        /// <param name="DBObjectName"></param>
        /// <param name="DBObjectType"></param>
        /// <param name="Params"></param>
        /// <returns></returns>
        private string BuildSqlString(string DBObjectName, string DBObjectType, string Params)
        {
            string sql = string.Empty;

            switch (DBObjectType.ToUpper())
            {
                case "TABLE":
                case "VIEW":

                    sql = "SELECT * FROM " + DBObjectName;
                    if (!string.IsNullOrEmpty(Params))
                    {

                        //Params = GetParamValues(Params);
                        List<string> paramList = Params.Split(',').Select(x => x.Trim()).ToList();

                        if (paramList.Count > 0)
                            sql += " WHERE ";

                        foreach (string param in paramList)
                        {
                            sql += param;
                            sql += " AND ";

                        }

                        if (sql.EndsWith(" AND "))
                            sql = sql.Substring(0, sql.LastIndexOf(" AND "));
                    }
                    break;

                case "STOREDPROCEDURE":

                    sql = "EXECUTE " + DBObjectName;
                    if (!string.IsNullOrEmpty(Params))
                    {
                        //Params = GetParamValues(Params);
                        List<string> paramList = Params.Split(',').Select(x => x.Trim()).ToList();

                        if (paramList.Count > 0)
                            sql += " ";
                        foreach (string param in paramList)
                        {
                            sql += param;
                            sql += ",";

                        }

                        if (sql.EndsWith(","))
                            sql = sql.Substring(0, sql.LastIndexOf(","));
                    }
                    break;

                case "QUERY":

                    sql = DBObjectName;
                    if (!string.IsNullOrEmpty(Params))
                    {

                        //Params = GetParamValues(Params);
                        List<string> paramList = Params.Split(',').Select(x => x.Trim()).ToList();

                        if (paramList.Count > 0)
                            sql += " WHERE ";

                        foreach (string param in paramList)
                        {
                            sql += param;
                            sql += " AND ";

                        }

                        if (sql.EndsWith(" AND "))
                            sql = sql.Substring(0, sql.LastIndexOf(" AND "));
                    }
                    break;

                default:
                    throw new Exception($"Unknown DBObjectType: {DBObjectType}");
            }

            return sql;

        }

        /// <summary>
        /// Method to handle any errors during DataAdapter.FillError event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void FillError(object sender, FillErrorEventArgs args)
        {
            if (args.Errors.GetType() == typeof(System.OverflowException))
            {
                // Code to handle precision loss.  
                //Add a row to table using the values from the first two columns.  
                DataRow myRow = args.DataTable.Rows.Add(new object[]
                   {args.Values[0], args.Values[1], DBNull.Value});
                //Set the RowError containing the value for the third column.  
                myRow.RowError =
                   "OverflowException Encountered. Value from data source: " +
                   args.Values[2];
                args.Continue = true;
            }
        }

        /*
        public void SaveTestMetricsToDB(TestCaseResultEntity TestCaseResultEntity)
        {

            //probably delete and repopulate or attach TestConfigId with ValuationDate as key or convert the table to system versioned

            string sql = "DELETE FROM dbo.DBTestResultsSummary WHERE TestConfigId = " + TestCaseResultEntity.TestConfigId.ToString();
            
            ExecuteSQLQuery(sql);

            sql = "INSERT INTO dbo.DBTestResultsSummary( TestConfigId,SourceExecutionTimeInSeconds,TargetExecutionTimeInSeconds,SourceRowCount,TargetRowCount,SourceColumCount,TargetColumnCount,UnMatchedCellCount,SourceQuery,TargetQuery,Result,LogTableName,LogMessage) VALUES " +
                "(@TestConfigId,@SourceExecutionTimeInSeconds,@TargetExecutionTimeInSeconds,@SourceRowCount,@TargetRowCount,@SourceColumnCount,@TargetColumnCount,@UnMatchedCellCount,@SourceQuery,@TargetQuery,@Result,@LogTableName,@LogMessage)";

            try
            {
                using (IDbConnection dbCon = new SqlConnection(testConfigConnectionString))
                {

                    dbCon.Execute(sql, TestCaseResultEntity);

                }

            }
            catch (Exception ex)
            {

                throw new Exception($"Exception in SaveTestMetricsToDB(), details: {ex.ToString()}");
            }


        }
        */

        /// <summary>
        /// Saves TestMetrics (Summary results) to DB
        /// </summary>
        /// <param name="TestCaseResultEntity"></param>
        public void SaveTestMetricsToDB(TestCaseResultEntity TestCaseResultEntity)
        {

            //probably delete and repopulate or attach TestConfigId with ValuationDate as key or convert the table to system versioned

            string sql = "[dbo].[uspSaveTestMetricsToDB]";

            var procParams = new DynamicParameters();
            procParams.Add("@TestConfigId", TestCaseResultEntity.TestConfigId, DbType.Int32);
            procParams.Add("@SourceExecutionTimeInSeconds", TestCaseResultEntity.SourceExecutionTimeInSeconds, DbType.Int32);
            procParams.Add("@TargetExecutionTimeInSeconds", TestCaseResultEntity.TargetExecutionTimeInSeconds, DbType.Int32);
            procParams.Add("@SourceRowCount", TestCaseResultEntity.SourceRowCount, DbType.Int32);
            procParams.Add("@TargetRowCount", TestCaseResultEntity.TargetRowCount, DbType.Int32);
            procParams.Add("@SourceColumnCount", TestCaseResultEntity.SourceColumnCount, DbType.Int32);
            procParams.Add("@TargetColumnCount ", TestCaseResultEntity.TargetColumnCount, DbType.Int32);
            procParams.Add("@UnMatchedCellCount", TestCaseResultEntity.UnMatchedCellCount, DbType.Int32);
            procParams.Add("@SourceQuery", TestCaseResultEntity.SourceQuery, DbType.String);
            procParams.Add("@TargetQuery", TestCaseResultEntity.TargetQuery, DbType.String);
            procParams.Add("@Result", TestCaseResultEntity.Result, DbType.Boolean);
            procParams.Add("@LogTableName ", TestCaseResultEntity.LogTableName, DbType.String);
            procParams.Add("@LogMessage", TestCaseResultEntity.LogMessage, DbType.String);


            try
            {
                using (IDbConnection dbCon = new SqlConnection(testConfigConnectionString))
                {

                    dbCon.Execute(sql, procParams, commandType: CommandType.StoredProcedure, commandTimeout: 300);

                }

            }
            catch (Exception ex)
            {

                throw new Exception($"Exception in SaveTestMetricsToDB(), details: {ex.ToString()}");
            }


        }

        /// <summary>
        /// Checks and creates Log Table for storing detailed Rec results for test
        /// </summary>
        /// <param name="DT"></param>
        /// <param name="TableName"></param>
        public void CheckAndCreateTable(DataTable DT, String TableName)
        {
            string sql = "[dbo].[uspCheckAndCreateLogTable]";
            string schemaName = GetSchemaNameFromTableName(TableName);
            string tableColumnList = GetTableColumDefinition(DT);

            string createTableSQL = "CREATE TABLE " + TableName + Environment.NewLine +
                                                        " (" + Environment.NewLine +
                                                         tableColumnList + Environment.NewLine +
                                                        ")";

            try
            {
                using (IDbConnection dbCon = new SqlConnection(testConfigConnectionString))
                {

                    //variable names should match Storedproc parameter names otherwise an exception will be thrown. 
                    //alternative way is to use DynamicParameters as in SaveTestMetricsToDB()
                    dbCon.Execute(sql, new { TableName, schemaName, createTableSQL }, commandType: CommandType.StoredProcedure, commandTimeout: 300);

                }

            }
            catch (Exception ex)
            {

                throw new Exception($"Exception in CheckAndCreateTable(), details: {ex.ToString()}");
            }
        }

        /// <summary>
        /// Checks and drops LogTable
        /// </summary>
        /// <param name="TableName"></param>
        public void CheckAndDropTable(String TableName)
        {
            string sql = "[dbo].[uspCheckAndDropLogTable]";

            try
            {
                using (IDbConnection dbCon = new SqlConnection(testConfigConnectionString))
                {

                    dbCon.Execute(sql, new { TableName }, commandType: CommandType.StoredProcedure, commandTimeout: 300);

                }

            }
            catch (Exception ex)
            {

                throw new Exception($"Exception in CheckAndDropTable(), details: {ex.ToString()}");
            }
        }



        /*
        public void ExecuteSQLQuery(string SqlQuery)
        {


            SqlConnection sqlCon = null;
            SqlCommand sqlCmd = null;


            try
            {
                sqlCon = new SqlConnection(testConfigConnectionString);
                sqlCmd = new SqlCommand();
                sqlCmd.Connection = sqlCon;
                sqlCon.Open();

                sqlCmd.CommandText = SqlQuery;

                sqlCmd.ExecuteNonQuery();



            }
            catch (Exception ex)
            {
                throw new Exception($"Exception in ExecuteSQLQuery(), unable to process sql query :{SqlQuery} on DBConnectionString:{testConfigConnectionString}, details: {ex.ToString()} ");
            }
            finally
            {

                if (sqlCon != null)
                {
                    sqlCon.Close();
                    sqlCon.Dispose();
                }

                if (sqlCmd != null)
                    sqlCmd.Dispose();



            }


        }


        public void CheckAndCreateTable(DataTable DT, String TableName)
        {

            //if the table already exists then drop the table
            CheckAndDropTable(TableName);

            CheckAndCreateSchema(TableName);


            //Create table
            string tableColumnList = GetTableColumDefinition(DT);

            string createTableString = "CREATE TABLE " + TableName + Environment.NewLine +
                                                        " (" + Environment.NewLine +
                                                         tableColumnList + Environment.NewLine +
                                                        ")";
            ExecuteSQLQuery(createTableString);


        }


        public void CheckAndDropTable(String TableName)
        {
            string dropTableString = "IF OBJECT_ID('" + TableName + "') IS NOT NULL " + Environment.NewLine +
                                                       " DROP TABLE " + TableName + Environment.NewLine;

            ExecuteSQLQuery(dropTableString);
        }



        private void CheckAndCreateSchema(String TableName)
        {
            if (TableName.IndexOf(".") > 0)
            {
                string tableSchema = GetSchemaNameFromTableName(TableName);
                string checkAndCreateSchemaString = "IF SCHEMA_ID('" + tableSchema + "') IS NULL EXEC('CREATE SCHEMA " + tableSchema + "')";
                ExecuteSQLQuery(checkAndCreateSchemaString);
            }
        }

    */

        /// <summary>
        /// Returns schema name from a given table name
        /// </summary>
        /// <param name="TableName"></param>
        /// <returns></returns>
        private string GetSchemaNameFromTableName(string TableName)
        {
            return TableName.Substring(0, TableName.IndexOf("."));
        }

        /// <summary>
        /// Gets Table definition for log table to be created
        /// </summary>
        /// <param name="DT"></param>
        /// <returns></returns>
        private string GetTableColumDefinition(DataTable DT)
        {
            string colList = string.Empty;

            foreach (DataColumn dc in DT.Columns)
            {
                //if the column name has space for example "Security Name" then wrap the column name within parentesis like [Security Name] other Create Table statement will fail
                if (dc.ColumnName.Contains(" "))
                    colList += "[" + dc.ColumnName + "]" + " VARCHAR(1000)" + ", ";
                else
                    colList += dc.ColumnName + " VARCHAR(1000)" + ", ";
            }

            colList += "LoadDateTime" + " DATETIME DEFAULT GETDATE()" + ", ";
            colList += "LoadedBy" + " VARCHAR(200) DEFAULT SYSTEM_USER";

            if (colList.EndsWith(", "))
            {
                colList = colList.Substring(0, colList.LastIndexOf(","));

            }
            return colList;
        }

        /// <summary>
        /// Saves log table with detailed Rec errors for a test to DB
        /// </summary>
        /// <param name="DT"></param>
        /// <param name="TableName"></param>
        public void SaveDataTableToDB(DataTable DT, string TableName)
        {


            //TODO: needs refactoring here, proper exception handling

            using (var bulkCopy = new SqlBulkCopy(testConfigConnectionString, SqlBulkCopyOptions.KeepNulls))
            {
                foreach (DataColumn column in DT.Columns)
                {
                    bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);

                }

                bulkCopy.BulkCopyTimeout = 600; //in seconds

                bulkCopy.DestinationTableName = TableName;
                bulkCopy.WriteToServer(DT);

            }

        }

        /// <summary>
        /// Revokes insert permission on the log table once the data has been written successfully
        /// </summary>
        /// <param name="TableName"></param>
        public void RevokePermssionsForLogTable(String TableName)
        {
            string sql = "[dbo].[uspRevokePermissionsForLogTable]";

            try
            {
                using (IDbConnection dbCon = new SqlConnection(testConfigConnectionString))
                {

                    //variable names should match Storedproc parameter names otherwise an exception will be thrown. 
                    //alternative way is to use DynamicParameters as in SaveTestMetricsToDB()
                    dbCon.Execute(sql, new { TableName }, commandType: CommandType.StoredProcedure, commandTimeout: 300);

                }

            }
            catch (Exception ex)
            {

                throw new Exception($"Exception in RevokeInsertPermssionOnLogTable(), details: {ex.ToString()}");
            }
        }

    }
}
