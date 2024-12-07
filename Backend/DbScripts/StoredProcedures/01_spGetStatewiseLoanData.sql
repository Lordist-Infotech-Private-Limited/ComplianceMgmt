DROP PROCEDURE IF EXISTS `spGetStatewiseLoanData`;
DELIMITER $$
CREATE DEFINER=`a927ee_comlian`@`%` PROCEDURE `spGetStatewiseLoanData`( 
 p_FromDate	DATE,
 p_ToDate	DATE,
 OUT p_ErrNo INT ,
 OUT p_ErrMsg VARCHAR(4000)
)
BEGIN
	DECLARE v_Id, v_AcctCountLoan, v_SanctAmount, v_AcctCountOut, v_LoanOutAmount BIGINT DEFAULT 0;
    DECLARE v_AcctCountLoanTotal, v_SanctAmountTotal, v_AcctCountOutTotal, v_LoanOutAmountTotal BIGINT DEFAULT 0;
    DECLARE v_AcctCountLoan_LP1_EWS, v_SanctAmount_LP1_EWS, v_AcctCountOut_LP1_EWS, v_LoanOutAmount_LP1_EWS BIGINT DEFAULT 0;
    DECLARE v_AcctCountLoan_LP1_LIG, v_SanctAmount_LP1_LIG, v_AcctCountOut_LP1_LIG, v_LoanOutAmount_LP1_LIG BIGINT DEFAULT 0;
    DECLARE v_AcctCountLoan_LP1_MIG, v_SanctAmount_LP1_MIG, v_AcctCountOut_LP1_MIG, v_LoanOutAmount_LP1_MIG BIGINT DEFAULT 0;
    DECLARE v_AcctCountLoan_LP1_HIG, v_SanctAmount_LP1_HIG, v_AcctCountOut_LP1_HIG, v_LoanOutAmount_LP1_HIG BIGINT DEFAULT 0;
    DECLARE v_AcctCountLoan_LP1_IncomeSlabTotal, v_SanctAmount_LP1_IncomeSlabTotal, v_AcctCountOut_LP1_IncomeSlabTotal, v_LoanOutAmount_LP1_IncomeSlabTotal BIGINT DEFAULT 0;
    DECLARE v_Counter INT DEFAULT 1;
    DECLARE v_LoanPurpose VARCHAR(50) DEFAULT 'POL-01';
	DECLARE v_StateId, v_OrdSeq INT;
	DECLARE v_StateName VARCHAR(255);
    DECLARE done INT DEFAULT 0;
    
    DECLARE state_cursor CURSOR FOR
    SELECT StateId, StateName, 0 AS OrdSeq
    FROM statemaster
    WHERE StateId NOT IN (35,4,25,26,7,37,38,1,31,34)
    UNION
    SELECT StateId, StateName, 1 AS OrdSeq
    FROM statemaster
    WHERE StateId IN (35,4,25,26,7,37,38,1,31,34)
    ORDER BY OrdSeq, StateName;
    
    DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = 1;

	DECLARE EXIT HANDLER FOR SQLEXCEPTION 
    BEGIN
        GET DIAGNOSTICS CONDITION 1
        @errorcode      = MYSQL_ERRNO,
        @errormessage   = MESSAGE_TEXT,
        @p3             = RETURNED_SQLSTATE;
        
		SET p_ErrNo  = IFNULL(p_ErrNo,@errorcode);
        SET p_ErrMsg = CONCAT('ERROR ', p_ErrNo, ' (', @p3, '): ', @errormessage);
	END;

    DROP TEMPORARY TABLE IF EXISTS TempResults;
    CREATE TEMPORARY TABLE TempResults (
		Id				BIGINT PRIMARY KEY,
        StateId 		INT,
        StateName 		VARCHAR(255),
        OrdSeq 			INT,
        NoOfBranches	INT,
        /*LoanPurpose 	VARCHAR(50),*/
        AcctCountLoan_LP1	INT,
        SanctAmount_LP1		BIGINT,
        AcctCountOut_LP1	INT,
        LoanOutAmount_LP1	BIGINT,
        AcctCountLoan_LP2	INT,
        SanctAmount_LP2		BIGINT,
        AcctCountOut_LP2	INT,
        LoanOutAmount_LP2	BIGINT,
        AcctCountLoan_LP6	INT,
        SanctAmount_LP6		BIGINT,
        AcctCountOut_LP6	INT,
        LoanOutAmount_LP6	BIGINT,
        AcctCountLoan_Total	INT,
        SanctAmount_Total		BIGINT,
        AcctCountOut_Total	INT,
        LoanOutAmount_Total	BIGINT,
        AcctCountLoan_LP1_EWS	INT,
        SanctAmount_LP1_EWS		BIGINT,
        AcctCountOut_LP1_EWS	INT,
        LoanOutAmount_LP1_EWS	BIGINT,
        AcctCountLoan_LP1_LIG	INT,
        SanctAmount_LP1_LIG		BIGINT,
        AcctCountOut_LP1_LIG	INT,
        LoanOutAmount_LP1_LIG	BIGINT,
        AcctCountLoan_LP1_MIG	INT,
        SanctAmount_LP1_MIG		BIGINT,
        AcctCountOut_LP1_MIG	INT,
        LoanOutAmount_LP1_MIG	BIGINT,
        AcctCountLoan_LP1_HIG	INT,
        SanctAmount_LP1_HIG		BIGINT,
        AcctCountOut_LP1_HIG	INT,
        LoanOutAmount_LP1_HIG	BIGINT,
        AcctCountLoan_LP1_IncomeSlabTotal	INT,
        SanctAmount_LP1_IncomeSlabTotal		BIGINT,
        AcctCountOut_LP1_IncomeSlabTotal	INT,
        LoanOutAmount_LP1_IncomeSlabTotal	BIGINT
    );    
    
    OPEN state_cursor;
    
    read_loop: LOOP
        FETCH state_cursor INTO v_StateId, v_StateName, v_OrdSeq;

        IF done = 1 THEN
            LEAVE read_loop;
        END IF;
        
        SET v_Counter = 1;
        SET v_AcctCountLoanTotal = 0;
		SET v_SanctAmountTotal = 0;
        SET v_AcctCountOutTotal = 0;
        SET v_LoanOutAmountTotal = 0;
        
        WHILE v_Counter < 4 DO
			SET v_AcctCountLoan = 0;
            SET v_SanctAmount = 0;
            SET v_AcctCountOut = 0;
            SET v_LoanOutAmount = 0;
            SET v_AcctCountLoan_LP1_IncomeSlabTotal = 0;
            SET v_SanctAmount_LP1_IncomeSlabTotal = 0;
            SET v_AcctCountOut_LP1_IncomeSlabTotal = 0;
            SET v_LoanOutAmount_LP1_IncomeSlabTotal = 0;
            
            
            select count(1), count(a.SanctAmount), count(a.TotalLoanOut) into v_AcctCountLoan,  v_SanctAmount, v_LoanOutAmount
            from stgborrowerloan a, stgborrowermortgage b
            where a.SanctDate between p_FromDate and p_ToDate
            and a.LoanPurpose = v_LoanPurpose
            and b.Cin = a.Cin
            and b.BLoanNo = a.BLoanNo
            and b.State = v_StateId;
        
            /*select count(1), count(a.TotalLoanOut) into v_AcctCountOut,  v_LoanOutAmount 
            from stgborrowerloan a, stgborrowermortgage b
            where a.SanctDate between p_FromDate and p_ToDate
            and a.LoanPurpose = v_LoanPurpose
            and coalesce(a.TotalLoanOut,0) > 0
            and b.Cin = a.Cin
            and b.BLoanNo = a.BLoanNo
            and b.State = v_StateId;*/

			SET v_AcctCountLoanTotal = v_AcctCountLoanTotal + v_AcctCountLoan;
			SET v_SanctAmountTotal = v_SanctAmountTotal + v_SanctAmount;
            SET v_AcctCountOutTotal = v_AcctCountOutTotal + v_AcctCountOut;
            SET v_LoanOutAmountTotal = v_LoanOutAmountTotal + v_LoanOutAmount;

            select count(1), count(a.SanctAmount), count(a.TotalLoanOut) into v_AcctCountLoan_LP1_EWS,  v_SanctAmount_LP1_EWS, v_LoanOutAmount_LP1_EWS
            from stgborrowerloan a, stgborrowermortgage b, stgborrowerdetail c
            where a.SanctDate between p_FromDate and p_ToDate
            and a.LoanPurpose = v_LoanPurpose
            and b.Cin = a.Cin
            and b.BLoanNo = a.BLoanNo
            and b.State = v_StateId
            and c.Cin = a.Cin
            and c.BMonthlyIncome < 25000;

            SET v_AcctCountLoan_LP1_IncomeSlabTotal = v_AcctCountLoan_LP1_IncomeSlabTotal + v_AcctCountLoan_LP1_EWS;
            SET v_SanctAmount_LP1_IncomeSlabTotal = v_SanctAmount_LP1_IncomeSlabTotal + v_SanctAmount_LP1_EWS;
            SET v_AcctCountOut_LP1_IncomeSlabTotal = v_AcctCountOut_LP1_IncomeSlabTotal + v_AcctCountLoan_LP1_EWS;
            SET v_LoanOutAmount_LP1_IncomeSlabTotal = v_LoanOutAmount_LP1_IncomeSlabTotal + v_LoanOutAmount_LP1_EWS;
            
            select count(1), count(a.SanctAmount), count(a.TotalLoanOut) into v_AcctCountLoan_LP1_LIG,  v_SanctAmount_LP1_LIG, v_LoanOutAmount_LP1_LIG
            from stgborrowerloan a, stgborrowermortgage b, stgborrowerdetail c
            where a.SanctDate between p_FromDate and p_ToDate
            and a.LoanPurpose = v_LoanPurpose
            and b.Cin = a.Cin
            and b.BLoanNo = a.BLoanNo
            and b.State = v_StateId
            and c.Cin = a.Cin
            and c.BMonthlyIncome >= 25000 and c.BMonthlyIncome < 50000;

            SET v_AcctCountLoan_LP1_IncomeSlabTotal = v_AcctCountLoan_LP1_IncomeSlabTotal + v_AcctCountLoan_LP1_LIG;
            SET v_SanctAmount_LP1_IncomeSlabTotal = v_SanctAmount_LP1_IncomeSlabTotal + v_SanctAmount_LP1_LIG;
            SET v_AcctCountOut_LP1_IncomeSlabTotal = v_AcctCountOut_LP1_IncomeSlabTotal + v_AcctCountLoan_LP1_LIG;
            SET v_LoanOutAmount_LP1_IncomeSlabTotal = v_LoanOutAmount_LP1_IncomeSlabTotal + v_LoanOutAmount_LP1_LIG;

            select count(1), count(a.SanctAmount), count(a.TotalLoanOut) into v_AcctCountLoan_LP1_MIG,  v_SanctAmount_LP1_MIG, v_LoanOutAmount_LP1_MIG
            from stgborrowerloan a, stgborrowermortgage b, stgborrowerdetail c
            where a.SanctDate between p_FromDate and p_ToDate
            and a.LoanPurpose = v_LoanPurpose
            and b.Cin = a.Cin
            and b.BLoanNo = a.BLoanNo
            and b.State = v_StateId
            and c.Cin = a.Cin
            and c.BMonthlyIncome >= 50000 and c.BMonthlyIncome < 150000;

            SET v_AcctCountLoan_LP1_IncomeSlabTotal = v_AcctCountLoan_LP1_IncomeSlabTotal + v_AcctCountLoan_LP1_MIG;
            SET v_SanctAmount_LP1_IncomeSlabTotal = v_SanctAmount_LP1_IncomeSlabTotal + v_SanctAmount_LP1_MIG;
            SET v_AcctCountOut_LP1_IncomeSlabTotal = v_AcctCountOut_LP1_IncomeSlabTotal + v_AcctCountLoan_LP1_MIG;
            SET v_LoanOutAmount_LP1_IncomeSlabTotal = v_LoanOutAmount_LP1_IncomeSlabTotal + v_LoanOutAmount_LP1_MIG;
            
            select count(1), count(a.SanctAmount), count(a.TotalLoanOut) into v_AcctCountLoan_LP1_HIG,  v_SanctAmount_LP1_HIG, v_LoanOutAmount_LP1_HIG
            from stgborrowerloan a, stgborrowermortgage b, stgborrowerdetail c
            where a.SanctDate between p_FromDate and p_ToDate
            and a.LoanPurpose = v_LoanPurpose
            and b.Cin = a.Cin
            and b.BLoanNo = a.BLoanNo
            and b.State = v_StateId
            and c.Cin = a.Cin
            and c.BMonthlyIncome >= 150000;

            SET v_AcctCountLoan_LP1_IncomeSlabTotal = v_AcctCountLoan_LP1_IncomeSlabTotal + v_AcctCountLoan_LP1_HIG;
            SET v_SanctAmount_LP1_IncomeSlabTotal = v_SanctAmount_LP1_IncomeSlabTotal + v_SanctAmount_LP1_HIG;
            SET v_AcctCountOut_LP1_IncomeSlabTotal = v_AcctCountOut_LP1_IncomeSlabTotal + v_AcctCountLoan_LP1_HIG;
            SET v_LoanOutAmount_LP1_IncomeSlabTotal = v_LoanOutAmount_LP1_IncomeSlabTotal + v_LoanOutAmount_LP1_HIG;

			IF v_Counter = 1 THEN
				SET v_Id = v_Id + 1;
                
				INSERT INTO TempResults (Id, StateId, StateName, OrdSeq,
                    AcctCountLoan_LP1, SanctAmount_LP1, AcctCountOut_LP1, LoanOutAmount_LP1,
                    AcctCountLoan_LP1_EWS, SanctAmount_LP1_EWS, AcctCountOut_LP1_EWS, LoanOutAmount_LP1_EWS,
                    AcctCountLoan_LP1_LIG, SanctAmount_LP1_LIG, AcctCountOut_LP1_LIG, LoanOutAmount_LP1_LIG,
                    AcctCountLoan_LP1_MIG, SanctAmount_LP1_MIG, AcctCountOut_LP1_MIG, LoanOutAmount_LP1_MIG,
                    AcctCountLoan_LP1_HIG, SanctAmount_LP1_HIG, AcctCountOut_LP1_HIG, LoanOutAmount_LP1_HIG,
                    AcctCountLoan_LP1_IncomeSlabTotal, SanctAmount_LP1_IncomeSlabTotal, AcctCountOut_LP1_IncomeSlabTotal, LoanOutAmount_LP1_IncomeSlabTotal
                    )
				VALUES (v_Id, v_StateId, v_StateName, v_OrdSeq,
                v_AcctCountLoan, v_SanctAmount, v_AcctCountLoan, v_LoanOutAmount,
                v_AcctCountLoan_LP1_EWS, v_SanctAmount_LP1_EWS, v_AcctCountLoan_LP1_EWS, v_LoanOutAmount_LP1_EWS,
                v_AcctCountLoan_LP1_LIG, v_SanctAmount_LP1_LIG, v_AcctCountLoan_LP1_LIG, v_LoanOutAmount_LP1_LIG,
                v_AcctCountLoan_LP1_MIG, v_SanctAmount_LP1_MIG, v_AcctCountLoan_LP1_MIG, v_LoanOutAmount_LP1_MIG,
                v_AcctCountLoan_LP1_HIG, v_SanctAmount_LP1_HIG, v_AcctCountLoan_LP1_HIG, v_LoanOutAmount_LP1_HIG,
                v_AcctCountLoan_LP1_IncomeSlabTotal, v_SanctAmount_LP1_IncomeSlabTotal, v_AcctCountLoan_LP1_IncomeSlabTotal, v_LoanOutAmount_LP1_IncomeSlabTotal);
			ELSEIF v_Counter = 2 THEN
				UPDATE TempResults
                SET AcctCountLoan_LP2 = v_AcctCountLoan, SanctAmount_LP2 = v_SanctAmount, 
                    AcctCountOut_LP2 = v_AcctCountLoan, LoanOutAmount_LP2 = v_LoanOutAmount
				WHERE Id = v_Id;
			ELSEIF v_Counter = 3 THEN
				UPDATE TempResults
                SET AcctCountLoan_LP6 = v_AcctCountLoan, SanctAmount_LP6 = v_SanctAmount, 
                    AcctCountOut_LP6 = v_AcctCountLoan, LoanOutAmount_LP6 = v_LoanOutAmount,
                    AcctCountLoan_Total = v_AcctCountLoanTotal, SanctAmount_Total = v_SanctAmountTotal,
                    AcctCountOut_Total = AcctCountLoan_Total, LoanOutAmount_Total = v_LoanOutAmountTotal
				WHERE Id = v_Id;
            END IF;
            SET v_Counter = v_Counter + 1;
            
            IF v_Counter = 2 THEN
				SET v_LoanPurpose = 'POL-02';
            ELSEIF v_Counter = 3 THEN
				SET v_LoanPurpose = 'POL-06';
            END IF;
		END WHILE;
    END LOOP;
    
    select * from TempResults;
    DROP TEMPORARY TABLE IF EXISTS TempResults;
    
end$$
DELIMITER ;
