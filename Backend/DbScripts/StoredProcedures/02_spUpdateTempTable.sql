DROP PROCEDURE IF EXISTS `spUpdateTempTable`;
DELIMITER $$
CREATE DEFINER=`a927ee_comlian`@`%` PROCEDURE `spUpdateTempTable`( 
 OrdSeq_ToBeUpdated BIGINT,
 OrdSeq_list VARCHAR(255)
)
BEGIN

    DROP TEMPORARY TABLE IF EXISTS TempLoanAdvances_Intermediate;
    CREATE TEMPORARY TABLE TempLoanAdvances_Intermediate LIKE TempLoanAdvances;

	insert into TempLoanAdvances_Intermediate select * from TempLoanAdvances;

	SET SQL_SAFE_UPDATES=0;
    
    UPDATE TempLoanAdvances t
	JOIN (
		SELECT 
			SUM(AprToPrvMonth_AcctCount) AS TotalAcctCount,
			SUM(AprToPrvMonth_GrossAmount) AS TotalGrossAmount
		FROM TempLoanAdvances_Intermediate
        WHERE FIND_IN_SET(OrdSeq, OrdSeq_list)
	) AS aggregated_data
	SET 
		t.AprToPrvMonth_AcctCount = aggregated_data.TotalAcctCount,
		t.AprToPrvMonth_GrossAmount = aggregated_data.TotalGrossAmount
	WHERE t.OrdSeq = OrdSeq_ToBeUpdated;  
   
end$$
DELIMITER ;
