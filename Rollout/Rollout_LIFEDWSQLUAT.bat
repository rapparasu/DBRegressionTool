@echo off

SET "SERVER=LIFEDWSQLUAT"
SET "DATABASE=LifeDW"
SET "TRACEFILENAME=output_LIFEDWSQLUAT.txt"

echo You are about to rollout the Change to %SERVER% Database: %DATABASE%
pause 

echo Processing starts ...
ECHO %USERNAME% started the batch process on Server: %SERVER% Database: %DATABASE% on %DATE% at %TIME%  > %TRACEFILENAME%

echo "00.Create_Database_RegressionTests.sql"
echo "00.Create_Database_RegressionTests.sql" >> %TRACEFILENAME%
sqlcmd.exe  -S %SERVER% -E   -d %DATABASE% -i "00.Create_Database_RegressionTests.sql" >> %TRACEFILENAME%

echo "01. Create_Table_dbo.DBTestsConfig.sql"
echo "01. Create_Table_dbo.DBTestsConfig.sql" >> %TRACEFILENAME%
sqlcmd.exe  -S %SERVER% -E   -d %DATABASE% -i "01. Create_Table_dbo.DBTestsConfig.sql" >> %TRACEFILENAME%

echo "02. Create_Table_dbo.DBTestResultsSummary.sql"
echo "02. Create_Table_dbo.DBTestResultsSummary.sql" >> %TRACEFILENAME%
sqlcmd.exe  -S %SERVER% -E   -d %DATABASE% -i "02. Create_Table_dbo.DBTestResultsSummary.sql" >> %TRACEFILENAME%

echo "03. CREATE_SP_dbo.uspGetDBTestsConfig.sql"
echo "03. CREATE_SP_dbo.uspGetDBTestsConfig.sql" >> %TRACEFILENAME%
sqlcmd.exe  -S %SERVER% -E   -d %DATABASE% -i "03. CREATE_SP_dbo.uspGetDBTestsConfig.sql" >> %TRACEFILENAME%

echo "04. CREATE_SP_dbo.uspCheckAndDropLogTable.sql"
echo "04. CREATE_SP_dbo.uspCheckAndDropLogTable.sql" >> %TRACEFILENAME%
sqlcmd.exe  -S %SERVER% -E   -d %DATABASE% -i "04. CREATE_SP_dbo.uspCheckAndDropLogTable.sql" >> %TRACEFILENAME%

echo "05. CREATE_SP_dbo.uspSaveTestMetricsToDB.sql"
echo "05. CREATE_SP_dbo.uspSaveTestMetricsToDB.sql" >> %TRACEFILENAME%
sqlcmd.exe  -S %SERVER% -E   -d %DATABASE% -i "05. CREATE_SP_dbo.uspSaveTestMetricsToDB.sql" >> %TRACEFILENAME%

echo "06. CREATE_SP_dbo.uspCheckAndCreateLogTable.sql"
echo "06. CREATE_SP_dbo.uspCheckAndCreateLogTable.sql" >> %TRACEFILENAME%
sqlcmd.exe  -S %SERVER% -E   -d %DATABASE% -i "06. CREATE_SP_dbo.uspCheckAndCreateLogTable.sql" >> %TRACEFILENAME%

echo "07. CREATE_SP_dbo.uspRevokePermissionsForLogTable.sql"
echo "07. CREATE_SP_dbo.uspRevokePermissionsForLogTable.sql" >> %TRACEFILENAME%
sqlcmd.exe  -S %SERVER% -E   -d %DATABASE% -i "07. CREATE_SP_dbo.uspRevokePermissionsForLogTable.sql" >> %TRACEFILENAME%

echo "08. CREATE_SP_dbo.uspTestConfigInsert.sql"
echo "08. CREATE_SP_dbo.uspTestConfigInsert.sql" >> %TRACEFILENAME%
sqlcmd.exe  -S %SERVER% -E   -d %DATABASE% -i "08. CREATE_SP_dbo.uspTestConfigInsert.sql" >> %TRACEFILENAME%

echo "09. CREATE_Test.sql"
echo "09. CREATE_Test.sql" >> %TRACEFILENAME%
sqlcmd.exe  -S %SERVER% -E   -d %DATABASE% -i "09. CREATE_Test.sql" >> %TRACEFILENAME%

echo "IDHPermissions.sql"
echo "IDHPermissions.sql" >> %TRACEFILENAME%
sqlcmd.exe  -S %SERVER% -E   -d %DATABASE% -i "IDHPermissions.sql" >> %TRACEFILENAME%


echo Processing Complete. >> %TRACEFILENAME%
echo Processing complete. Close the window and check the logfile: %TRACEFILENAME% for results.
pause