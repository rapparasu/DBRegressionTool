using System;
using System.Collections.Generic;
using System.Text;

namespace DBRegressionLibrary.BusinessEntities
{

    /// <summary>
    /// Business Entity class for TestResultsSummary
    /// </summary>
    public class TestCaseResultEntity
    {
        //output properties

        public int TestConfigId { get; set; }
        public long SourceExecutionTimeInSeconds { get; set; }
        public long TargetExecutionTimeInSeconds { get; set; }

        public int SourceRowCount { get; set; }
        public int TargetRowCount { get; set; }
        public int SourceColumnCount { get; set; }
        public int TargetColumnCount { get; set; }
        public int UnMatchedCellCount { get; set; }
        //public string Summary { get; set; }

        public string SourceQuery { get; set; }
        public string TargetQuery { get; set; }

        public bool Result { get; set; }


        public string LogTableName { get; set; }
        public string LogMessage { get; set; }
    }
}
