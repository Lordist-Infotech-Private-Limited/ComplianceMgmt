DROP PROCEDURE IF EXISTS `spGetIntRateStructure`;
DELIMITER $$
CREATE /*DEFINER=`a927ee_comlian`@`%`*/ PROCEDURE `spGetIntRateStructure`( 
 p_ReportDate	date,
 OUT p_ErrNo int ,
 OUT p_ErrMsg varchar(4000)
)
BEGIN
	declare v_Id bigint default 0;
    declare v_Particulars varchar(2000);
    declare v_FinYearStartDate, v_PrvMonthEndDate, v_CurrMonthStartDate date;
    declare v_Int1, v_Int2 bigint;
	declare v_Dec1, v_Dec2 decimal(15,2);
	
	declare EXIT HANDLER FOR SQLEXCEPTION 
    BEGIN
        GET DIAGNOSTICS CONDITION 1
        @errorcode      = MYSQL_ERRNO,
        @errormessage   = MESSAGE_TEXT,
        @p3             = RETURNED_SQLSTATE;
        
		SET p_ErrNo  = IFNULL(p_ErrNo,@errorcode);
        SET p_ErrMsg = CONCAT('ERROR ', p_ErrNo, ' (', @p3, '): ', @errormessage);
	END;

    DROP TEMPORARY TABLE IF EXISTS TempIntRateStructure;
    CREATE TEMPORARY TABLE TempIntRateStructure (
		Id				bigint PRIMARY KEY,
        SrNo			varchar(15),
		Particulars		varchar(2000),
		Fixed_AcctCount bigint,
		Fixed_OutstandingAmount decimal(15, 2),
		Floating_AcctCount bigint,
		Floating_OutstandingAmount decimal(15, 2),
		OrdSeq int
    ); 
    
    SET v_FinYearStartDate = 
        CASE 
            WHEN MONTH(p_ReportDate) >= 4 THEN
                MAKEDATE(YEAR(p_ReportDate), 1) + intERVAL 3 MONTH
            ELSE
                MAKEDATE(YEAR(p_ReportDate) - 1, 1) + intERVAL 3 MONTH
        END;	
	
	SET v_PrvMonthEndDate = LAST_DAY(p_ReportDate - intERVAL 1 MONTH);
	SET v_CurrMonthStartDate = v_PrvMonthEndDate + INTERVAL 1 DAY;
    
	-- select v_FinYearStartDate, v_PrvMonthEndDate;
    
	
	/*set v_Particulars = 'Retail Prime Lending Rate - Housing';
    SET v_Id = v_Id + 1;
	insert into TempIntRateStructure (Id, SrNo, Particulars, OrdSeq, Fixed_AcctCount, Fixed_OutstandingAmount, Floating_AcctCount, Floating_OutstandingAmount )
	values ( v_Id, '', v_Particulars, 1, null, null, null, null );
	
	set v_Particulars = 'Retail Prime Lending Rate - Non-Housing';
    SET v_Id = v_Id + 1;
	insert into TempIntRateStructure (Id, SrNo, Particulars, OrdSeq, Fixed_AcctCount, Fixed_OutstandingAmount, Floating_AcctCount, Floating_OutstandingAmount )
	values ( v_Id, '', v_Particulars, 2, null, null, null, null );

	set v_Particulars = 'ROI Slab';
    SET v_Id = v_Id + 1;
	insert into TempIntRateStructure (Id, SrNo, Particulars, OrdSeq, Fixed_AcctCount, Fixed_OutstandingAmount, Floating_AcctCount, Floating_OutstandingAmount )
	values ( v_Id, '', v_Particulars, 3, null, null, null, null );*/

	set v_Particulars = 'Housing Loan to Individuals';
    SET v_Id = v_Id + 1;
	insert into TempIntRateStructure (Id, SrNo, Particulars, OrdSeq )
	values ( v_Id, '1', v_Particulars, 11 );
    
	set v_Particulars = 'Upto 5%';
    
    select count(1), coalesce(sum(TotalLoanOut),0) into v_Int1, v_Dec1
	from stgborrowerloan a
	where a.SanctDate <= p_ReportDate
    and a.LoanPurpose in ('POL-01','POL-02','POL-03','POL-04','POL-05','POL-06')
	and a.IntType = 'TOI-01' -- Fixed Rate
    and a.Roi <= 5; 

    select count(1), coalesce(sum(TotalLoanOut),0) into v_Int2, v_Dec2
	from stgborrowerloan a
	where a.SanctDate <= p_ReportDate
    and a.LoanPurpose in ('POL-01','POL-02','POL-03','POL-04','POL-05','POL-06')
	and a.IntType = 'TOI-02' -- Floating Rate
    and a.Roi <= 5; 

    SET v_Id = v_Id + 1;
	insert into TempIntRateStructure (Id, SrNo, Particulars, OrdSeq, Fixed_AcctCount, Fixed_OutstandingAmount, Floating_AcctCount, Floating_OutstandingAmount )
	values ( v_Id, '1', v_Particulars, 12, v_Int1, v_Dec1, v_Int2, v_Dec2 );

	set v_Particulars = 'Above 5% to 10%';
    
    select count(1), coalesce(sum(TotalLoanOut),0) into v_Int1, v_Dec1
	from stgborrowerloan a
	where a.SanctDate <= p_ReportDate
    and a.LoanPurpose in ('POL-01','POL-02','POL-03','POL-04','POL-05','POL-06')
	and a.IntType = 'TOI-01' -- Fixed Rate
    and a.Roi > 5 and a.Roi <= 10; 

    select count(1), coalesce(sum(TotalLoanOut),0) into v_Int2, v_Dec2
	from stgborrowerloan a
	where a.SanctDate <= p_ReportDate
    and a.LoanPurpose in ('POL-01','POL-02','POL-03','POL-04','POL-05','POL-06')
	and a.IntType = 'TOI-02' -- Floating Rate
    and a.Roi > 5 and a.Roi <= 10; 

    SET v_Id = v_Id + 1;
	insert into TempIntRateStructure (Id, SrNo, Particulars, OrdSeq, Fixed_AcctCount, Fixed_OutstandingAmount, Floating_AcctCount, Floating_OutstandingAmount )
	values ( v_Id, '1', v_Particulars, 13, v_Int1, v_Dec1, v_Int2, v_Dec2 );

	set v_Particulars = 'Above 10% to 15%';
    
    select count(1), coalesce(sum(TotalLoanOut),0) into v_Int1, v_Dec1
	from stgborrowerloan a
	where a.SanctDate <= p_ReportDate
    and a.LoanPurpose in ('POL-01','POL-02','POL-03','POL-04','POL-05','POL-06')
	and a.IntType = 'TOI-01' -- Fixed Rate
    and a.Roi > 10 and a.Roi <= 15; 

    select count(1), coalesce(sum(TotalLoanOut),0) into v_Int2, v_Dec2
	from stgborrowerloan a
	where a.SanctDate <= p_ReportDate
    and a.LoanPurpose in ('POL-01','POL-02','POL-03','POL-04','POL-05','POL-06')
	and a.IntType = 'TOI-02' -- Floating Rate
    and a.Roi > 10 and a.Roi <= 15; 

    SET v_Id = v_Id + 1;
	insert into TempIntRateStructure (Id, SrNo, Particulars, OrdSeq, Fixed_AcctCount, Fixed_OutstandingAmount, Floating_AcctCount, Floating_OutstandingAmount )
	values ( v_Id, '1', v_Particulars, 14, v_Int1, v_Dec1, v_Int2, v_Dec2 );

	set v_Particulars = 'Above 15% to 20%';
    
    select count(1), coalesce(sum(TotalLoanOut),0) into v_Int1, v_Dec1
	from stgborrowerloan a
	where a.SanctDate <= p_ReportDate
    and a.LoanPurpose in ('POL-01','POL-02','POL-03','POL-04','POL-05','POL-06')
	and a.IntType = 'TOI-01' -- Fixed Rate
    and a.Roi > 15 and a.Roi <= 20; 

    select count(1), coalesce(sum(TotalLoanOut),0) into v_Int2, v_Dec2
	from stgborrowerloan a
	where a.SanctDate <= p_ReportDate
    and a.LoanPurpose in ('POL-01','POL-02','POL-03','POL-04','POL-05','POL-06')
	and a.IntType = 'TOI-02' -- Floating Rate
    and a.Roi > 15 and a.Roi <= 20; 

    SET v_Id = v_Id + 1;
	insert into TempIntRateStructure (Id, SrNo, Particulars, OrdSeq, Fixed_AcctCount, Fixed_OutstandingAmount, Floating_AcctCount, Floating_OutstandingAmount )
	values ( v_Id, '1', v_Particulars, 15, v_Int1, v_Dec1, v_Int2, v_Dec2 );

	set v_Particulars = 'Above 20% to 30%';
    
    select count(1), coalesce(sum(TotalLoanOut),0) into v_Int1, v_Dec1
	from stgborrowerloan a
	where a.SanctDate <= p_ReportDate
    and a.LoanPurpose in ('POL-01','POL-02','POL-03','POL-04','POL-05','POL-06')
	and a.IntType = 'TOI-01' -- Fixed Rate
    and a.Roi > 20 and a.Roi <= 30; 

    select count(1), coalesce(sum(TotalLoanOut),0) into v_Int2, v_Dec2
	from stgborrowerloan a
	where a.SanctDate <= p_ReportDate
    and a.LoanPurpose in ('POL-01','POL-02','POL-03','POL-04','POL-05','POL-06')
	and a.IntType = 'TOI-02' -- Floating Rate
    and a.Roi > 20 and a.Roi <= 30;  

    SET v_Id = v_Id + 1;
	insert into TempIntRateStructure (Id, SrNo, Particulars, OrdSeq, Fixed_AcctCount, Fixed_OutstandingAmount, Floating_AcctCount, Floating_OutstandingAmount )
	values ( v_Id, '1', v_Particulars, 16, v_Int1, v_Dec1, v_Int2, v_Dec2 );


	set v_Particulars = 'Above 30%';
    
    select count(1), coalesce(sum(TotalLoanOut),0) into v_Int1, v_Dec1
	from stgborrowerloan a
	where a.SanctDate <= p_ReportDate
    and a.LoanPurpose in ('POL-01','POL-02','POL-03','POL-04','POL-05','POL-06')
	and a.IntType = 'TOI-01' -- Fixed Rate
    and a.Roi > 30; 

    select count(1), coalesce(sum(TotalLoanOut),0) into v_Int2, v_Dec2
	from stgborrowerloan a
	where a.SanctDate <= p_ReportDate
    and a.LoanPurpose in ('POL-01','POL-02','POL-03','POL-04','POL-05','POL-06')
	and a.IntType = 'TOI-02' -- Floating Rate
    and a.Roi > 30;  

    SET v_Id = v_Id + 1;
	insert into TempIntRateStructure (Id, SrNo, Particulars, OrdSeq, Fixed_AcctCount, Fixed_OutstandingAmount, Floating_AcctCount, Floating_OutstandingAmount )
	values ( v_Id, '1', v_Particulars, 17, v_Int1, v_Dec1, v_Int2, v_Dec2 );


	set v_Particulars = 'Non-Housing Loan to Individuals';
    SET v_Id = v_Id + 1;
	insert into TempIntRateStructure (Id, SrNo, Particulars, OrdSeq )
	values ( v_Id, '1', v_Particulars, 18 );
    
	set v_Particulars = 'Upto 5%';
    
    select count(1), coalesce(sum(TotalLoanOut),0) into v_Int1, v_Dec1
	from stgborrowerloan a
	where a.SanctDate <= p_ReportDate
    and a.LoanPurpose in ('POL-07','POL-08','POL-09','POL-10','POL-11','POL-12','POL-14','POL-14')
	and a.IntType = 'TOI-01' -- Fixed Rate
    and a.Roi <= 5; 

    select count(1), coalesce(sum(TotalLoanOut),0) into v_Int2, v_Dec2
	from stgborrowerloan a
	where a.SanctDate <= p_ReportDate
    and a.LoanPurpose in ('POL-07','POL-08','POL-09','POL-10','POL-11','POL-12','POL-14','POL-14')
	and a.IntType = 'TOI-02' -- Floating Rate
    and a.Roi <= 5; 

    SET v_Id = v_Id + 1;
	insert into TempIntRateStructure (Id, SrNo, Particulars, OrdSeq, Fixed_AcctCount, Fixed_OutstandingAmount, Floating_AcctCount, Floating_OutstandingAmount )
	values ( v_Id, '1', v_Particulars, 19, v_Int1, v_Dec1, v_Int2, v_Dec2 );

	set v_Particulars = 'Above 5% to 10%';
    
    select count(1), coalesce(sum(TotalLoanOut),0) into v_Int1, v_Dec1
	from stgborrowerloan a
	where a.SanctDate <= p_ReportDate
    and a.LoanPurpose in ('POL-07','POL-08','POL-09','POL-10','POL-11','POL-12','POL-14','POL-14')
	and a.IntType = 'TOI-01' -- Fixed Rate
    and a.Roi > 5 and a.Roi <= 10; 

    select count(1), coalesce(sum(TotalLoanOut),0) into v_Int2, v_Dec2
	from stgborrowerloan a
	where a.SanctDate <= p_ReportDate
    and a.LoanPurpose in ('POL-07','POL-08','POL-09','POL-10','POL-11','POL-12','POL-14','POL-14')
	and a.IntType = 'TOI-02' -- Floating Rate
    and a.Roi > 5 and a.Roi <= 10; 

    SET v_Id = v_Id + 1;
	insert into TempIntRateStructure (Id, SrNo, Particulars, OrdSeq, Fixed_AcctCount, Fixed_OutstandingAmount, Floating_AcctCount, Floating_OutstandingAmount )
	values ( v_Id, '1', v_Particulars, 20, v_Int1, v_Dec1, v_Int2, v_Dec2 );

	set v_Particulars = 'Above 10% to 15%';
    
    select count(1), coalesce(sum(TotalLoanOut),0) into v_Int1, v_Dec1
	from stgborrowerloan a
	where a.SanctDate <= p_ReportDate
    and a.LoanPurpose in ('POL-07','POL-08','POL-09','POL-10','POL-11','POL-12','POL-14','POL-14')
	and a.IntType = 'TOI-01' -- Fixed Rate
    and a.Roi > 10 and a.Roi <= 15; 

    select count(1), coalesce(sum(TotalLoanOut),0) into v_Int2, v_Dec2
	from stgborrowerloan a
	where a.SanctDate <= p_ReportDate
    and a.LoanPurpose in ('POL-07','POL-08','POL-09','POL-10','POL-11','POL-12','POL-14','POL-14')
	and a.IntType = 'TOI-02' -- Floating Rate
    and a.Roi > 10 and a.Roi <= 15; 

    SET v_Id = v_Id + 1;
	insert into TempIntRateStructure (Id, SrNo, Particulars, OrdSeq, Fixed_AcctCount, Fixed_OutstandingAmount, Floating_AcctCount, Floating_OutstandingAmount )
	values ( v_Id, '1', v_Particulars, 21, v_Int1, v_Dec1, v_Int2, v_Dec2 );

	set v_Particulars = 'Above 15% to 20%';
    
    select count(1), coalesce(sum(TotalLoanOut),0) into v_Int1, v_Dec1
	from stgborrowerloan a
	where a.SanctDate <= p_ReportDate
    and a.LoanPurpose in ('POL-07','POL-08','POL-09','POL-10','POL-11','POL-12','POL-14','POL-14')
	and a.IntType = 'TOI-01' -- Fixed Rate
    and a.Roi > 15 and a.Roi <= 20; 

    select count(1), coalesce(sum(TotalLoanOut),0) into v_Int2, v_Dec2
	from stgborrowerloan a
	where a.SanctDate <= p_ReportDate
    and a.LoanPurpose in ('POL-07','POL-08','POL-09','POL-10','POL-11','POL-12','POL-14','POL-14')
	and a.IntType = 'TOI-02' -- Floating Rate
    and a.Roi > 15 and a.Roi <= 20; 

    SET v_Id = v_Id + 1;
	insert into TempIntRateStructure (Id, SrNo, Particulars, OrdSeq, Fixed_AcctCount, Fixed_OutstandingAmount, Floating_AcctCount, Floating_OutstandingAmount )
	values ( v_Id, '1', v_Particulars, 22, v_Int1, v_Dec1, v_Int2, v_Dec2 );

	set v_Particulars = 'Above 20% to 30%';
    
    select count(1), coalesce(sum(TotalLoanOut),0) into v_Int1, v_Dec1
	from stgborrowerloan a
	where a.SanctDate <= p_ReportDate
    and a.LoanPurpose in ('POL-07','POL-08','POL-09','POL-10','POL-11','POL-12','POL-14','POL-14')
	and a.IntType = 'TOI-01' -- Fixed Rate
    and a.Roi > 20 and a.Roi <= 30; 

    select count(1), coalesce(sum(TotalLoanOut),0) into v_Int2, v_Dec2
	from stgborrowerloan a
	where a.SanctDate <= p_ReportDate
    and a.LoanPurpose in ('POL-07','POL-08','POL-09','POL-10','POL-11','POL-12','POL-14','POL-14')
	and a.IntType = 'TOI-02' -- Floating Rate
    and a.Roi > 20 and a.Roi <= 30;  

    SET v_Id = v_Id + 1;
	insert into TempIntRateStructure (Id, SrNo, Particulars, OrdSeq, Fixed_AcctCount, Fixed_OutstandingAmount, Floating_AcctCount, Floating_OutstandingAmount )
	values ( v_Id, '1', v_Particulars, 23, v_Int1, v_Dec1, v_Int2, v_Dec2 );


	set v_Particulars = 'Above 30%';
    
    select count(1), coalesce(sum(TotalLoanOut),0) into v_Int1, v_Dec1
	from stgborrowerloan a
	where a.SanctDate <= p_ReportDate
    and a.LoanPurpose in ('POL-07','POL-08','POL-09','POL-10','POL-11','POL-12','POL-14','POL-14')
	and a.IntType = 'TOI-01' -- Fixed Rate
    and a.Roi > 30; 

    select count(1), coalesce(sum(TotalLoanOut),0) into v_Int2, v_Dec2
	from stgborrowerloan a
	where a.SanctDate <= p_ReportDate
    and a.LoanPurpose in ('POL-07','POL-08','POL-09','POL-10','POL-11','POL-12','POL-14','POL-14')
	and a.IntType = 'TOI-02' -- Floating Rate
    and a.Roi > 30;  

    SET v_Id = v_Id + 1;
	insert into TempIntRateStructure (Id, SrNo, Particulars, OrdSeq, Fixed_AcctCount, Fixed_OutstandingAmount, Floating_AcctCount, Floating_OutstandingAmount )
	values ( v_Id, '1', v_Particulars, 24, v_Int1, v_Dec1, v_Int2, v_Dec2 );


    select * from TempIntRateStructure order by OrdSeq;
    DROP TEMPORARY TABLE IF EXISTS TempIntRateStructure;
    
end$$
DELIMITER ;
