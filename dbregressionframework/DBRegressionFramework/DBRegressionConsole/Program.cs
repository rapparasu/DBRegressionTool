using System;
using System.Collections.Generic;
using System.Linq;
using DBRegressionLibrary.BusinessEntities;
using DBRegressionLibrary.BusinessLogicComponent;

using System.DirectoryServices.AccountManagement;

using System.Web;
using System.Web.Security;

using System.DirectoryServices;

namespace DBRegressionConsole
{

    /// <summary>
    /// Console class for debugging Regression framework
    /// </summary>

    class Program
    {
        private static DBRegressionBLC dbRegressionBLC = new DBRegressionBLC();
        static void Main(string[] args)
        {

            
            string testTag = string.Empty;
            string runTimeParams = string.Empty;

            string NTUserEmail = UserPrincipal.Current.EmailAddress;


            try
            {

               
                //if passing command line argument to executable (can be used while triggering exe through a scheduled task)
                if (args.Length > 0)
                {
                    testTag = args[0];
                    if (args.Length == 2)
                        runTimeParams = args[1];
                }

                else//someone clicking on executable manually
                {
                    Console.WriteLine("Please enter tag name of your tests and press key or leave it blank and press enter key to run all enabled tests");
                    testTag = Console.ReadLine();
                }

                DateTime startTime = DateTime.Now;
                DateTime endTime = DateTime.Now;
                double runTimeInMinutes = 0;


                /*Debug tests expecting RunTime parameters*/
                //bool taggedExecutionResult = dbRegressionBLC.RunTests(testTag, "@EffectiveDate='2019-05-31',@SnapshotType='Month-End'", NTUserEmail);
                //bool taggedExecutionResult = dbRegressionBLC.RunTests(testTag, "@EffectiveDate='2020-02-29',@SnapshotType='Month-End'", NTUserEmail);
                //bool taggedExecutionResult = dbRegressionBLC.RunTests(testTag, "@EffectiveDate='2020-01-31',@SnapshotType='Month-End'", NTUserEmail);
                //bool taggedExecutionResult = dbRegressionBLC.RunTests(testTag, "@EffectiveDate='2020-07-31',@SnapshotType='Month-End'", NTUserEmail);

                /*Debug static tests where the parameters are configured in the test case*/
                dbRegressionBLC.RunTests(testTag, runTimeParams, NTUserEmail);

                endTime = DateTime.Now;
                runTimeInMinutes = (endTime - startTime).TotalMinutes;
                Console.WriteLine($"TestCase Executions completed, RunTime: {runTimeInMinutes.ToString()} Minutes !");


            }

            catch (Exception ex)
            {
                Console.WriteLine($"Exception occured, details: {ex.ToString()}");
            }

            Console.ReadLine();
        }

        
    }

}
