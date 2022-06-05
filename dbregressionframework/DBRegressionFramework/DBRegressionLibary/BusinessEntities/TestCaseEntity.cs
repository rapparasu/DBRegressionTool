using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace DBRegressionLibrary.BusinessEntities
{
    /// <summary>
    /// Business Entity class for Test Case Config
    /// </summary>
    public class TestCaseEntity
    {
        //Properties from DB for configured TestCase
        public int Id { get; set; }

        public string TestName { get; set; }

        public string Tag { get; set; }

        public string SourceDBObjectName { get; set; }
        public string SourceDBObjectType { get; set; }

        public string TargetDBObjectName { get; set; }
        public string TargetDBObjectType { get; set; }

        public string Params { get; set; }
        public string SourceDBServer { get; set; }
        public string SourceDB { get; set; }
        public string TargetDBServer { get; set; }
        public string TargetDB { get; set; }

        public string PrimaryKeyColumns { get; set; }

        public string SortColumnsWhenNoPrimaryKey { get; set; }

        public string ColumnsToIncludeForChecks { get; set; }

        public string ColumnsToAlwaysShowDespiteMatching { get; set; }

        public string ColumnsToExcludeForChecks { get; set; }

        public string DiffColumnsToInject { get; set; }

        public double DiffColumnsTolerance { get; set; }

        public string ColumnOrderSequenceInOutput { get; set; }

        public int ExecutionToleranceInSeconds { get; set; }

        public Boolean ShowMatchingColumnValues { get; set; }

        public Boolean IsEnabled { get; set; }

        public Boolean ToRecAndSendEmail { get; set; }

        public string ITRecipients { get; set; }

        public string BusinessRecipients { get; set; }

        public string ColumnsToCheckForBusinessAlerting { get; set; }


        //Derived properties

        public string SourceDBConnectionString { get; set; }
        public string TargetDBConnectionString { get; set; }
        public NameValueCollection NVParams { get; set; }



        //Test output metrics
        public TestCaseResultEntity  TestMetric { get; set; }

    }
}
