USE [RegressionTests] 

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Change History
-- Description:  
-- Returns configurations for enabled test cases
-- Date 		Who				JIRA			Change				
-- --------------------------------------------------------------
-- 26/05/2020  Ravi Apparasu	FO-387		initial creation
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[uspGetDBTestsConfig] 

As
Begin	
		SELECT * FROM dbo.DBTestsConfig WHERE IsEnabled = 1
End

/*Testing

EXECUTE [dbo].[uspGetDBTestsConfig] 
 
 
*/






