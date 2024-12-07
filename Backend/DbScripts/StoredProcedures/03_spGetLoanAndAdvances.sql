DROP PROCEDURE IF EXISTS `spGetLoanAndAdvances`;
DELIMITER $$
CREATE DEFINER=`a927ee_comlian`@`%` PROCEDURE `spGetLoanAndAdvances`( 
 p_ReportDate	date,
 OUT p_ErrNo int ,
 OUT p_ErrMsg varchar(4000)
)
BEGIN
    declare v_Counter int default 0;
	declare v_Id bigint default 0;
    declare v_PartSection, v_Particulars varchar(2000);
	declare v_FinYearStartDate, v_PrvMonthEndDate, v_CurrMonthStartDate date;
	declare v_Int1, v_Int2, v_Int3 bigint;
	declare v_Dec1, v_Dec2, v_Dec3 decimal(15,2);
	
	declare EXIT HANDLER FOR SQLEXCEPTION 
    BEGIN
        GET DIAGNOSTICS CONDITION 1
        @errorcode      = MYSQL_ERRNO,
        @errormessage   = MESSAGE_TEXT,
        @p3             = RETURNED_SQLSTATE;
        
		SET p_ErrNo  = IFNULL(p_ErrNo,@errorcode);
        SET p_ErrMsg = CONCAT('ERROR ', p_ErrNo, ' (', @p3, '): ', @errormessage);
	END;

    DROP TEMPORARY TABLE IF EXISTS TempLoanAdvances;
    CREATE TEMPORARY TABLE TempLoanAdvances (
		Id				bigint PRIMARY KEY,
		PartSection		varchar(2000),
        SrNo			varchar(15),
		Particulars		varchar(2000),
		AprToPrvMonth_AcctCount bigint,
		AprToPrvMonth_GrossAmount decimal(15, 2),
		DuringMonth_AcctCount bigint,
		DuringMonth_GrossAmount decimal(15, 2),
		Total_AcctCount bigint,
		Total_GrossAmount decimal(15, 2),
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
	
    set v_PartSection = 'PART-1A: LOAN SANCTIONS';
    
    -- Housing Loans
	set v_Particulars = 'Housing Loans';
    SET v_Id = v_Id + 1;
	insert into TempLoanAdvances (Id, PartSection, SrNo, Particulars, OrdSeq )
	values ( v_Id, v_PartSection, '1', v_Particulars, 1 );
	
	set v_Particulars = 'Individuals';
    SET v_Id = v_Id + 1;
	insert into TempLoanAdvances (Id, PartSection, SrNo, Particulars, OrdSeq )
	values ( v_Id, v_PartSection, '1.1', v_Particulars, 2 );

	set v_Particulars = '(i) for construction/purchase of new units';
    
    select count(1), coalesce(sum(SanctAmount),0) into v_Int1, v_Dec1
	from stgborrowerloan a
	where a.SanctDate between v_FinYearStartDate and v_PrvMonthEndDate
	and a.LoanPurpose in ('POL-01','POL-02','POL-05');

	select count(1), coalesce(sum(SanctAmount),0) into v_Int2, v_Dec2
	from stgborrowerloan a
	where a.SanctDate between v_CurrMonthStartDate and p_ReportDate
	and a.LoanPurpose in ('POL-01','POL-02','POL-05');


	SET v_Id = v_Id + 1;
	insert into TempLoanAdvances (Id, PartSection, SrNo, Particulars, OrdSeq, AprToPrvMonth_AcctCount, AprToPrvMonth_GrossAmount,
		DuringMonth_AcctCount, DuringMonth_GrossAmount, Total_AcctCount, Total_GrossAmount)
	values ( v_Id, v_PartSection, '', v_Particulars, 3, v_Int1, v_Dec1, v_Int2, v_Dec2, (v_Int1+v_Int2), (v_Dec1+v_Dec2) );

	
    set v_Particulars = '(a) Out of the 1.1(i) above, loans granted for purchase of units from builders (under construction + completed)';
    
    select count(1), coalesce(sum(SanctAmount),0) into v_Int1, v_Dec1
	from stgborrowerloan a
	where a.SanctDate between v_FinYearStartDate and v_PrvMonthEndDate
	and a.LoanPurpose = 'POL-02';

	select count(1), coalesce(sum(SanctAmount),0) into v_Int2, v_Dec2
	from stgborrowerloan a
	where a.SanctDate between v_CurrMonthStartDate and p_ReportDate
	and a.LoanPurpose = 'POL-02';
    

	SET v_Id = v_Id + 1;
    
	insert into TempLoanAdvances (Id, PartSection, SrNo, Particulars, OrdSeq, AprToPrvMonth_AcctCount, AprToPrvMonth_GrossAmount,
		DuringMonth_AcctCount, DuringMonth_GrossAmount, Total_AcctCount, Total_GrossAmount)
	values ( v_Id, v_PartSection, '', v_Particulars, 4, v_Int1, v_Dec1, v_Int2, v_Dec2, (v_Int1+v_Int2), (v_Dec1+v_Dec2) );

	
    set v_Particulars = '(b) Out of the 1.1(i) above, loans granted for Plot + Construction';
    
    select count(1), coalesce(sum(SanctAmount),0) into v_Int1, v_Dec1
	from stgborrowerloan a
	where a.SanctDate between v_FinYearStartDate and v_PrvMonthEndDate
	and a.LoanPurpose = 'POL-05';

	select count(1), coalesce(sum(SanctAmount),0) into v_Int2, v_Dec2
	from stgborrowerloan a
	where a.SanctDate between v_CurrMonthStartDate and p_ReportDate
	and a.LoanPurpose = 'POL-05';

	SET v_Id = v_Id + 1;
    
	insert into TempLoanAdvances (Id, PartSection, SrNo, Particulars, OrdSeq, AprToPrvMonth_AcctCount, AprToPrvMonth_GrossAmount,
		DuringMonth_AcctCount, DuringMonth_GrossAmount, Total_AcctCount, Total_GrossAmount)
	values ( v_Id, v_PartSection, '', v_Particulars, 5, v_Int1, v_Dec1, v_Int2, v_Dec2, (v_Int1+v_Int2), (v_Dec1+v_Dec2) );

    set v_Particulars = '(ii) for purchasing old units (Resale)';
    
    select count(1), coalesce(sum(SanctAmount),0) into v_Int1, v_Dec1
	from stgborrowerloan a
	where a.SanctDate between v_FinYearStartDate and v_PrvMonthEndDate
	and a.LoanPurpose = 'POL-03';

	select count(1), coalesce(sum(SanctAmount),0) into v_Int2, v_Dec2
	from stgborrowerloan a
	where a.SanctDate between v_CurrMonthStartDate and p_ReportDate
	and a.LoanPurpose = 'POL-03';

	SET v_Id = v_Id + 1;
    
	insert into TempLoanAdvances (Id, PartSection, SrNo, Particulars, OrdSeq, AprToPrvMonth_AcctCount, AprToPrvMonth_GrossAmount,
		DuringMonth_AcctCount, DuringMonth_GrossAmount, Total_AcctCount, Total_GrossAmount)
	values ( v_Id, v_PartSection, '', v_Particulars, 6, v_Int1, v_Dec1, v_Int2, v_Dec2, (v_Int1+v_Int2), (v_Dec1+v_Dec2) );
	
    set v_Particulars = '(iii) for repair & renovation of existing units';
    
    select count(1), coalesce(sum(SanctAmount),0) into v_Int1, v_Dec1
	from stgborrowerloan a
	where a.SanctDate between v_FinYearStartDate and v_PrvMonthEndDate
	and a.LoanPurpose = 'POL-06';

	select count(1), coalesce(sum(SanctAmount),0) into v_Int2, v_Dec2
	from stgborrowerloan a
	where a.SanctDate between v_CurrMonthStartDate and p_ReportDate
	and a.LoanPurpose = 'POL-06';

	SET v_Id = v_Id + 1;
    
	insert into TempLoanAdvances (Id, PartSection, SrNo, Particulars, OrdSeq, AprToPrvMonth_AcctCount, AprToPrvMonth_GrossAmount,
		DuringMonth_AcctCount, DuringMonth_GrossAmount, Total_AcctCount, Total_GrossAmount)
	values ( v_Id, v_PartSection, '', v_Particulars, 7, v_Int1, v_Dec1, v_Int2, v_Dec2, (v_Int1+v_Int2), (v_Dec1+v_Dec2) );
    
    
    -- Non-Housing Loans
	set v_Particulars = 'Non-Housing Loans';
    SET v_Id = v_Id + 1;
	insert into TempLoanAdvances (Id, PartSection, SrNo, Particulars, OrdSeq )
	values ( v_Id, v_PartSection, '2', v_Particulars, 12 );

	set v_Particulars = 'Individuals';
    SET v_Id = v_Id + 1;
	insert into TempLoanAdvances (Id, PartSection, SrNo, Particulars, OrdSeq )
	values ( v_Id, v_PartSection, '2.1', v_Particulars, 13 );
    
    set v_Particulars = '(i) for mortgage/property/home equity loans/LAP';
    
    select count(1), coalesce(sum(SanctAmount),0) into v_Int1, v_Dec1
	from stgborrowerloan a
	where a.SanctDate between v_FinYearStartDate and v_PrvMonthEndDate
	and a.LoanPurpose = 'POL-13';

	select count(1), coalesce(sum(SanctAmount),0) into v_Int2, v_Dec2
	from stgborrowerloan a
	where a.SanctDate between v_CurrMonthStartDate and p_ReportDate
	and a.LoanPurpose = 'POL-13';

	SET v_Id = v_Id + 1;
    
	insert into TempLoanAdvances (Id, PartSection, SrNo, Particulars, OrdSeq, AprToPrvMonth_AcctCount, AprToPrvMonth_GrossAmount,
		DuringMonth_AcctCount, DuringMonth_GrossAmount, Total_AcctCount, Total_GrossAmount)
	values ( v_Id, v_PartSection, '', v_Particulars, 14, v_Int1, v_Dec1, v_Int2, v_Dec2, (v_Int1+v_Int2), (v_Dec1+v_Dec2) );
    
	set v_Particulars = '(ii) Lease Rental Discounting';
    SET v_Id = v_Id + 1;
	insert into TempLoanAdvances (Id, PartSection, SrNo, Particulars, OrdSeq )
	values ( v_Id, v_PartSection, '2.1', v_Particulars, 15 );
    
    set v_Particulars = '(iii) Others';
    
    select count(1), coalesce(sum(SanctAmount),0) into v_Int1, v_Dec1
	from stgborrowerloan a
	where a.SanctDate between v_FinYearStartDate and v_PrvMonthEndDate
	and a.LoanPurpose in ('POL-07','POL-08','POL-09','POL-10','POL-11','POL-12','POL-14');

	select count(1), coalesce(sum(SanctAmount),0) into v_Int2, v_Dec2
	from stgborrowerloan a
	where a.SanctDate between v_CurrMonthStartDate and p_ReportDate
	and a.LoanPurpose in ('POL-07','POL-08','POL-09','POL-10','POL-11','POL-12','POL-14');

	SET v_Id = v_Id + 1;
    
	insert into TempLoanAdvances (Id, PartSection, SrNo, Particulars, OrdSeq, AprToPrvMonth_AcctCount, AprToPrvMonth_GrossAmount,
		DuringMonth_AcctCount, DuringMonth_GrossAmount, Total_AcctCount, Total_GrossAmount)
	values ( v_Id, v_PartSection, '', v_Particulars, 16, v_Int1, v_Dec1, v_Int2, v_Dec2, (v_Int1+v_Int2), (v_Dec1+v_Dec2) );
    
    set v_Particulars = '(a) Of (iii) above loan against the security of shares/ debentures / bonds';
    
    select count(1), coalesce(sum(SanctAmount),0) into v_Int1, v_Dec1
	from stgborrowerloan a
	where a.SanctDate between v_FinYearStartDate and v_PrvMonthEndDate
	and a.LoanPurpose = 'POL-08';

	select count(1), coalesce(sum(SanctAmount),0) into v_Int2, v_Dec2
	from stgborrowerloan a
	where a.SanctDate between v_CurrMonthStartDate and p_ReportDate
	and a.LoanPurpose = 'POL-08';

	SET v_Id = v_Id + 1;
    
	insert into TempLoanAdvances (Id, PartSection, SrNo, Particulars, OrdSeq, AprToPrvMonth_AcctCount, AprToPrvMonth_GrossAmount,
		DuringMonth_AcctCount, DuringMonth_GrossAmount, Total_AcctCount, Total_GrossAmount)
	values ( v_Id, v_PartSection, '', v_Particulars, 17, v_Int1, v_Dec1, v_Int2, v_Dec2, (v_Int1+v_Int2), (v_Dec1+v_Dec2) );

    set v_Particulars = '(b) Of (iii) above loans against security of gold jewellery';
    
    select count(1), coalesce(sum(SanctAmount),0) into v_Int1, v_Dec1
	from stgborrowerloan a
	where a.SanctDate between v_FinYearStartDate and v_PrvMonthEndDate
	and a.LoanPurpose = 'POL-09';

	select count(1), coalesce(sum(SanctAmount),0) into v_Int2, v_Dec2
	from stgborrowerloan a
	where a.SanctDate between v_CurrMonthStartDate and p_ReportDate
	and a.LoanPurpose = 'POL-09';

	SET v_Id = v_Id + 1;
    
	insert into TempLoanAdvances (Id, PartSection, SrNo, Particulars, OrdSeq, AprToPrvMonth_AcctCount, AprToPrvMonth_GrossAmount,
		DuringMonth_AcctCount, DuringMonth_GrossAmount, Total_AcctCount, Total_GrossAmount)
	values ( v_Id, v_PartSection, '', v_Particulars, 18, v_Int1, v_Dec1, v_Int2, v_Dec2, (v_Int1+v_Int2), (v_Dec1+v_Dec2) );
    

    
      
    set v_PartSection = 'PART-1B: LOAN DISBURSEMENTS';	
    
    -- Housing Loans
	set v_Particulars = 'Housing Loans';
    SET v_Id = v_Id + 1;
	insert into TempLoanAdvances (Id, PartSection, SrNo, Particulars, OrdSeq )
	values ( v_Id, v_PartSection, '1', v_Particulars, 31 );
	
	set v_Particulars = 'Individuals';
    SET v_Id = v_Id + 1;
	insert into TempLoanAdvances (Id, PartSection, SrNo, Particulars, OrdSeq )
	values ( v_Id, v_PartSection, '1.1', v_Particulars, 32 );

	set v_Particulars = '(i) for construction/purchase of new units';
    
    select count(1), coalesce(sum(LoanDisbDuringMonth),0) into v_Int1, v_Dec1
	from stgborrowerloan a
	where a.SanctDate between v_FinYearStartDate and v_PrvMonthEndDate
	and a.LoanPurpose in ('POL-01','POL-02','POL-05')
    and a.LoanDisbDuringMonth > 0;

	select count(1), coalesce(sum(LoanDisbDuringMonth),0) into v_Int2, v_Dec2
	from stgborrowerloan a
	where a.SanctDate between v_CurrMonthStartDate and p_ReportDate
	and a.LoanPurpose in ('POL-01','POL-02','POL-05')
	and a.LoanDisbDuringMonth > 0;

	SET v_Id = v_Id + 1;
	insert into TempLoanAdvances (Id, PartSection, SrNo, Particulars, OrdSeq, AprToPrvMonth_AcctCount, AprToPrvMonth_GrossAmount,
		DuringMonth_AcctCount, DuringMonth_GrossAmount, Total_AcctCount, Total_GrossAmount)
	values ( v_Id, v_PartSection, '', v_Particulars, 33, v_Int1, v_Dec1, v_Int2, v_Dec2, (v_Int1+v_Int2), (v_Dec1+v_Dec2) );

	
    set v_Particulars = '(a) Out of the 1.1(i) above, loans granted for purchase of units from builders (under construction + completed)';
    
    select count(1), coalesce(sum(LoanDisbDuringMonth),0) into v_Int1, v_Dec1
	from stgborrowerloan a
	where a.SanctDate between v_FinYearStartDate and v_PrvMonthEndDate
	and a.LoanPurpose = 'POL-02'
    and a.LoanDisbDuringMonth > 0;

	select count(1), coalesce(sum(LoanDisbDuringMonth),0) into v_Int2, v_Dec2
	from stgborrowerloan a
	where a.SanctDate between v_CurrMonthStartDate and p_ReportDate
	and a.LoanPurpose = 'POL-02'
    and a.LoanDisbDuringMonth > 0;
    

	SET v_Id = v_Id + 1;
    
	insert into TempLoanAdvances (Id, PartSection, SrNo, Particulars, OrdSeq, AprToPrvMonth_AcctCount, AprToPrvMonth_GrossAmount,
		DuringMonth_AcctCount, DuringMonth_GrossAmount, Total_AcctCount, Total_GrossAmount)
	values ( v_Id, v_PartSection, '', v_Particulars, 34, v_Int1, v_Dec1, v_Int2, v_Dec2, (v_Int1+v_Int2), (v_Dec1+v_Dec2) );

	
    set v_Particulars = '(b) Out of the 1.1(i) above, loans granted for Plot + Construction';
    
    select count(1), coalesce(sum(LoanDisbDuringMonth),0) into v_Int1, v_Dec1
	from stgborrowerloan a
	where a.SanctDate between v_FinYearStartDate and v_PrvMonthEndDate
	and a.LoanPurpose = 'POL-05'
    and a.LoanDisbDuringMonth > 0;

	select count(1), coalesce(sum(LoanDisbDuringMonth),0) into v_Int2, v_Dec2
	from stgborrowerloan a
	where a.SanctDate between v_CurrMonthStartDate and p_ReportDate
	and a.LoanPurpose = 'POL-05'
    and a.LoanDisbDuringMonth > 0;

	SET v_Id = v_Id + 1;
    
	insert into TempLoanAdvances (Id, PartSection, SrNo, Particulars, OrdSeq, AprToPrvMonth_AcctCount, AprToPrvMonth_GrossAmount,
		DuringMonth_AcctCount, DuringMonth_GrossAmount, Total_AcctCount, Total_GrossAmount)
	values ( v_Id, v_PartSection, '', v_Particulars, 35, v_Int1, v_Dec1, v_Int2, v_Dec2, (v_Int1+v_Int2), (v_Dec1+v_Dec2) );

    set v_Particulars = '(ii) for purchasing old units (Resale)';
    
    select count(1), coalesce(sum(LoanDisbDuringMonth),0) into v_Int1, v_Dec1
	from stgborrowerloan a
	where a.SanctDate between v_FinYearStartDate and v_PrvMonthEndDate
	and a.LoanPurpose = 'POL-03'
    and a.LoanDisbDuringMonth > 0;

	select count(1), coalesce(sum(LoanDisbDuringMonth),0) into v_Int2, v_Dec2
	from stgborrowerloan a
	where a.SanctDate between v_CurrMonthStartDate and p_ReportDate
	and a.LoanPurpose = 'POL-03'
    and a.LoanDisbDuringMonth > 0;

	SET v_Id = v_Id + 1;
    
	insert into TempLoanAdvances (Id, PartSection, SrNo, Particulars, OrdSeq, AprToPrvMonth_AcctCount, AprToPrvMonth_GrossAmount,
		DuringMonth_AcctCount, DuringMonth_GrossAmount, Total_AcctCount, Total_GrossAmount)
	values ( v_Id, v_PartSection, '', v_Particulars, 36, v_Int1, v_Dec1, v_Int2, v_Dec2, (v_Int1+v_Int2), (v_Dec1+v_Dec2) );
	
    set v_Particulars = '(iii) for repair & renovation of existing units';
    
    select count(1), coalesce(sum(LoanDisbDuringMonth),0) into v_Int1, v_Dec1
	from stgborrowerloan a
	where a.SanctDate between v_FinYearStartDate and v_PrvMonthEndDate
	and a.LoanPurpose = 'POL-06'
    and a.LoanDisbDuringMonth > 0;

	select count(1), coalesce(sum(LoanDisbDuringMonth),0) into v_Int2, v_Dec2
	from stgborrowerloan a
	where a.SanctDate between v_CurrMonthStartDate and p_ReportDate
	and a.LoanPurpose = 'POL-06'
    and a.LoanDisbDuringMonth > 0;

	SET v_Id = v_Id + 1;
    
	insert into TempLoanAdvances (Id, PartSection, SrNo, Particulars, OrdSeq, AprToPrvMonth_AcctCount, AprToPrvMonth_GrossAmount,
		DuringMonth_AcctCount, DuringMonth_GrossAmount, Total_AcctCount, Total_GrossAmount)
	values ( v_Id, v_PartSection, '', v_Particulars, 37, v_Int1, v_Dec1, v_Int2, v_Dec2, (v_Int1+v_Int2), (v_Dec1+v_Dec2) );
    
    
    -- Non-Housing Loans
	set v_Particulars = 'Non-Housing Loans';
    SET v_Id = v_Id + 1;
	insert into TempLoanAdvances (Id, PartSection, SrNo, Particulars, OrdSeq )
	values ( v_Id, v_PartSection, '2', v_Particulars, 42 );

	set v_Particulars = 'Individuals';
    SET v_Id = v_Id + 1;
	insert into TempLoanAdvances (Id, PartSection, SrNo, Particulars, OrdSeq )
	values ( v_Id, v_PartSection, '2.1', v_Particulars, 43 );
    
    set v_Particulars = '(i) for mortgage/property/home equity loans/LAP';
    
    select count(1), coalesce(sum(LoanDisbDuringMonth),0) into v_Int1, v_Dec1
	from stgborrowerloan a
	where a.SanctDate between v_FinYearStartDate and v_PrvMonthEndDate
	and a.LoanPurpose = 'POL-13'
    and a.LoanDisbDuringMonth > 0;

	select count(1), coalesce(sum(LoanDisbDuringMonth),0) into v_Int2, v_Dec2
	from stgborrowerloan a
	where a.SanctDate between v_CurrMonthStartDate and p_ReportDate
	and a.LoanPurpose = 'POL-13'
    and a.LoanDisbDuringMonth > 0;

	SET v_Id = v_Id + 1;
    
	insert into TempLoanAdvances (Id, PartSection, SrNo, Particulars, OrdSeq, AprToPrvMonth_AcctCount, AprToPrvMonth_GrossAmount,
		DuringMonth_AcctCount, DuringMonth_GrossAmount, Total_AcctCount, Total_GrossAmount)
	values ( v_Id, v_PartSection, '', v_Particulars, 44, v_Int1, v_Dec1, v_Int2, v_Dec2, (v_Int1+v_Int2), (v_Dec1+v_Dec2) );
    
	set v_Particulars = '(ii) Lease Rental Discounting';
    SET v_Id = v_Id + 1;
	insert into TempLoanAdvances (Id, PartSection, SrNo, Particulars, OrdSeq )
	values ( v_Id, v_PartSection, '2.1', v_Particulars, 45 );
    
    set v_Particulars = '(iii) Others';
    
    select count(1), coalesce(sum(LoanDisbDuringMonth),0) into v_Int1, v_Dec1
	from stgborrowerloan a
	where a.SanctDate between v_FinYearStartDate and v_PrvMonthEndDate
	and a.LoanPurpose in ('POL-07','POL-08','POL-09','POL-10','POL-11','POL-12','POL-14')
    and a.LoanDisbDuringMonth > 0;

	select count(1), coalesce(sum(LoanDisbDuringMonth),0) into v_Int2, v_Dec2
	from stgborrowerloan a
	where a.SanctDate between v_CurrMonthStartDate and p_ReportDate
	and a.LoanPurpose in ('POL-07','POL-08','POL-09','POL-10','POL-11','POL-12','POL-14')
    and a.LoanDisbDuringMonth > 0;

	SET v_Id = v_Id + 1;
    
	insert into TempLoanAdvances (Id, PartSection, SrNo, Particulars, OrdSeq, AprToPrvMonth_AcctCount, AprToPrvMonth_GrossAmount,
		DuringMonth_AcctCount, DuringMonth_GrossAmount, Total_AcctCount, Total_GrossAmount)
	values ( v_Id, v_PartSection, '', v_Particulars, 46, v_Int1, v_Dec1, v_Int2, v_Dec2, (v_Int1+v_Int2), (v_Dec1+v_Dec2) );
    
    set v_Particulars = '(a) Of (iii) above loan against the security of shares/ debentures / bonds';
    
    select count(1), coalesce(sum(LoanDisbDuringMonth),0) into v_Int1, v_Dec1
	from stgborrowerloan a
	where a.SanctDate between v_FinYearStartDate and v_PrvMonthEndDate
	and a.LoanPurpose = 'POL-08'
    and a.LoanDisbDuringMonth > 0;

	select count(1), coalesce(sum(LoanDisbDuringMonth),0) into v_Int2, v_Dec2
	from stgborrowerloan a
	where a.SanctDate between v_CurrMonthStartDate and p_ReportDate
	and a.LoanPurpose = 'POL-08'
    and a.LoanDisbDuringMonth > 0;

	SET v_Id = v_Id + 1;
    
	insert into TempLoanAdvances (Id, PartSection, SrNo, Particulars, OrdSeq, AprToPrvMonth_AcctCount, AprToPrvMonth_GrossAmount,
		DuringMonth_AcctCount, DuringMonth_GrossAmount, Total_AcctCount, Total_GrossAmount)
	values ( v_Id, v_PartSection, '', v_Particulars, 47, v_Int1, v_Dec1, v_Int2, v_Dec2, (v_Int1+v_Int2), (v_Dec1+v_Dec2) );

    set v_Particulars = '(b) Of (iii) above loans against security of gold jewellery';
    
    select count(1), coalesce(sum(LoanDisbDuringMonth),0) into v_Int1, v_Dec1
	from stgborrowerloan a
	where a.SanctDate between v_FinYearStartDate and v_PrvMonthEndDate
	and a.LoanPurpose = 'POL-09'
    and a.LoanDisbDuringMonth > 0;

	select count(1), coalesce(sum(LoanDisbDuringMonth),0) into v_Int2, v_Dec2
	from stgborrowerloan a
	where a.SanctDate between v_CurrMonthStartDate and p_ReportDate
	and a.LoanPurpose = 'POL-09'
    and a.LoanDisbDuringMonth > 0;

	SET v_Id = v_Id + 1;
    
	insert into TempLoanAdvances (Id, PartSection, SrNo, Particulars, OrdSeq, AprToPrvMonth_AcctCount, AprToPrvMonth_GrossAmount,
		DuringMonth_AcctCount, DuringMonth_GrossAmount, Total_AcctCount, Total_GrossAmount)
	values ( v_Id, v_PartSection, '', v_Particulars, 48, v_Int1, v_Dec1, v_Int2, v_Dec2, (v_Int1+v_Int2), (v_Dec1+v_Dec2) );


    set v_PartSection = 'PART-1C: LOAN OUTSTANDINGS';	
    
    -- Housing Loans
	set v_Particulars = 'Housing Loans';
    SET v_Id = v_Id + 1;
	insert into TempLoanAdvances (Id, PartSection, SrNo, Particulars, OrdSeq )
	values ( v_Id, v_PartSection, '1', v_Particulars, 71 );
	
	set v_Particulars = 'Individuals';
    SET v_Id = v_Id + 1;
	insert into TempLoanAdvances (Id, PartSection, SrNo, Particulars, OrdSeq )
	values ( v_Id, v_PartSection, '1.1', v_Particulars, 72 );

	set v_Particulars = '(i) for construction/purchase of new units';
    
    select count(1), coalesce(sum(TotalLoanOut),0) into v_Int1, v_Dec1
	from stgborrowerloan a
	where a.SanctDate <= v_PrvMonthEndDate
	and a.LoanPurpose in ('POL-01','POL-02','POL-05')
    and a.TotalLoanOut > 0;

	select count(1), coalesce(sum(LoanRepayDurMth),0) into v_Int2, v_Dec2
	from stgborrowerloan a
	where a.SanctDate between v_CurrMonthStartDate and p_ReportDate
	and a.LoanPurpose in ('POL-01','POL-02','POL-05')
	and a.LoanRepayDurMth > 0;

    select count(1), coalesce(sum(TotalLoanOut),0) into v_Int3, v_Dec3
	from stgborrowerloan a
	where a.SanctDate = p_ReportDate
	and a.LoanPurpose in ('POL-01','POL-02','POL-05')
    and a.TotalLoanOut > 0;

	SET v_Id = v_Id + 1;
	insert into TempLoanAdvances (Id, PartSection, SrNo, Particulars, OrdSeq, AprToPrvMonth_AcctCount, AprToPrvMonth_GrossAmount,
		DuringMonth_AcctCount, DuringMonth_GrossAmount, Total_AcctCount, Total_GrossAmount)
	values ( v_Id, v_PartSection, '', v_Particulars, 73	, v_Int1, v_Dec1, v_Int2, v_Dec2, v_Int3, v_Dec3 );

	
    set v_Particulars = '(a) Out of the 1.1(i) above, loans granted for purchase of units from builders (under construction + completed)';
    
    select count(1), coalesce(sum(TotalLoanOut),0) into v_Int1, v_Dec1
	from stgborrowerloan a
	where a.SanctDate <= v_PrvMonthEndDate
	and a.LoanPurpose = 'POL-02'
    and a.TotalLoanOut > 0;

	select count(1), coalesce(sum(LoanRepayDurMth),0) into v_Int2, v_Dec2
	from stgborrowerloan a
	where a.SanctDate between v_CurrMonthStartDate and p_ReportDate
	and a.LoanPurpose = 'POL-02'
	and a.LoanRepayDurMth > 0;

    select count(1), coalesce(sum(TotalLoanOut),0) into v_Int3, v_Dec3
	from stgborrowerloan a
	where a.SanctDate = p_ReportDate
	and a.LoanPurpose = 'POL-02'
    and a.TotalLoanOut > 0;    

	SET v_Id = v_Id + 1;
    
	insert into TempLoanAdvances (Id, PartSection, SrNo, Particulars, OrdSeq, AprToPrvMonth_AcctCount, AprToPrvMonth_GrossAmount,
		DuringMonth_AcctCount, DuringMonth_GrossAmount, Total_AcctCount, Total_GrossAmount)
	values ( v_Id, v_PartSection, '', v_Particulars, 74, v_Int1, v_Dec1, v_Int2, v_Dec2, v_Int3, v_Dec3 );

	
    set v_Particulars = '(b) Out of the 1.1(i) above, loans granted for Plot + Construction';
    
    select count(1), coalesce(sum(TotalLoanOut),0) into v_Int1, v_Dec1
	from stgborrowerloan a
	where a.SanctDate <= v_PrvMonthEndDate
	and a.LoanPurpose = 'POL-05'
    and a.TotalLoanOut > 0;

	select count(1), coalesce(sum(LoanRepayDurMth),0) into v_Int2, v_Dec2
	from stgborrowerloan a
	where a.SanctDate between v_CurrMonthStartDate and p_ReportDate
	and a.LoanPurpose = 'POL-05'
	and a.LoanRepayDurMth > 0;

    select count(1), coalesce(sum(TotalLoanOut),0) into v_Int3, v_Dec3
	from stgborrowerloan a
	where a.SanctDate = p_ReportDate
	and a.LoanPurpose = 'POL-05'
    and a.TotalLoanOut > 0;

	SET v_Id = v_Id + 1;
   
	insert into TempLoanAdvances (Id, PartSection, SrNo, Particulars, OrdSeq, AprToPrvMonth_AcctCount, AprToPrvMonth_GrossAmount,
		DuringMonth_AcctCount, DuringMonth_GrossAmount, Total_AcctCount, Total_GrossAmount)
	values ( v_Id, v_PartSection, '', v_Particulars, 75, v_Int1, v_Dec1, v_Int2, v_Dec2, v_Int3, v_Dec3 );

    set v_Particulars = '(ii) for purchasing old units (Resale)';
    
    select count(1), coalesce(sum(TotalLoanOut),0) into v_Int1, v_Dec1
	from stgborrowerloan a
	where a.SanctDate <= v_PrvMonthEndDate
	and a.LoanPurpose = 'POL-03'
    and a.TotalLoanOut > 0;

	select count(1), coalesce(sum(LoanRepayDurMth),0) into v_Int2, v_Dec2
	from stgborrowerloan a
	where a.SanctDate between v_CurrMonthStartDate and p_ReportDate
	and a.LoanPurpose = 'POL-03'
	and a.LoanRepayDurMth > 0;

    select count(1), coalesce(sum(TotalLoanOut),0) into v_Int3, v_Dec3
	from stgborrowerloan a
	where a.SanctDate = p_ReportDate
	and a.LoanPurpose = 'POL-03'
    and a.TotalLoanOut > 0;

	SET v_Id = v_Id + 1;
    
	insert into TempLoanAdvances (Id, PartSection, SrNo, Particulars, OrdSeq, AprToPrvMonth_AcctCount, AprToPrvMonth_GrossAmount,
		DuringMonth_AcctCount, DuringMonth_GrossAmount, Total_AcctCount, Total_GrossAmount)
	values ( v_Id, v_PartSection, '', v_Particulars, 76, v_Int1, v_Dec1, v_Int2, v_Dec2, v_Int3, v_Dec3 );
	
    set v_Particulars = '(iii) for repair & renovation of existing units';
    
    select count(1), coalesce(sum(TotalLoanOut),0) into v_Int1, v_Dec1
	from stgborrowerloan a
	where a.SanctDate <= v_PrvMonthEndDate
	and a.LoanPurpose = 'POL-06'
    and a.TotalLoanOut > 0;

	select count(1), coalesce(sum(LoanRepayDurMth),0) into v_Int2, v_Dec2
	from stgborrowerloan a
	where a.SanctDate between v_CurrMonthStartDate and p_ReportDate
	and a.LoanPurpose = 'POL-06'
	and a.LoanRepayDurMth > 0;

    select count(1), coalesce(sum(TotalLoanOut),0) into v_Int3, v_Dec3
	from stgborrowerloan a
	where a.SanctDate = p_ReportDate
	and a.LoanPurpose = 'POL-06'
    and a.TotalLoanOut > 0;
    
	SET v_Id = v_Id + 1;
    
	insert into TempLoanAdvances (Id, PartSection, SrNo, Particulars, OrdSeq, AprToPrvMonth_AcctCount, AprToPrvMonth_GrossAmount,
		DuringMonth_AcctCount, DuringMonth_GrossAmount, Total_AcctCount, Total_GrossAmount)
	values ( v_Id, v_PartSection, '', v_Particulars, 77, v_Int1, v_Dec1, v_Int2, v_Dec2, v_Int3, v_Dec3 );
    
    
    -- Non-Housing Loans
	set v_Particulars = 'Non-Housing Loans';
    SET v_Id = v_Id + 1;
	insert into TempLoanAdvances (Id, PartSection, SrNo, Particulars, OrdSeq )
	values ( v_Id, v_PartSection, '2', v_Particulars, 82 );

	set v_Particulars = 'Individuals';
    SET v_Id = v_Id + 1;
	insert into TempLoanAdvances (Id, PartSection, SrNo, Particulars, OrdSeq )
	values ( v_Id, v_PartSection, '2.1', v_Particulars, 83 );
    
    set v_Particulars = '(i) for mortgage/property/home equity loans/LAP';
    
    select count(1), coalesce(sum(TotalLoanOut),0) into v_Int1, v_Dec1
	from stgborrowerloan a
	where a.SanctDate <= v_PrvMonthEndDate
	and a.LoanPurpose = 'POL-13'
    and a.TotalLoanOut > 0;

	select count(1), coalesce(sum(LoanRepayDurMth),0) into v_Int2, v_Dec2
	from stgborrowerloan a
	where a.SanctDate between v_CurrMonthStartDate and p_ReportDate
	and a.LoanPurpose = 'POL-13'
	and a.LoanRepayDurMth > 0;

    select count(1), coalesce(sum(TotalLoanOut),0) into v_Int3, v_Dec3
	from stgborrowerloan a
	where a.SanctDate = p_ReportDate
	and a.LoanPurpose = 'POL-13'
    and a.TotalLoanOut > 0;

	SET v_Id = v_Id + 1;
    
	insert into TempLoanAdvances (Id, PartSection, SrNo, Particulars, OrdSeq, AprToPrvMonth_AcctCount, AprToPrvMonth_GrossAmount,
		DuringMonth_AcctCount, DuringMonth_GrossAmount, Total_AcctCount, Total_GrossAmount)
	values ( v_Id, v_PartSection, '', v_Particulars, 84, v_Int1, v_Dec1, v_Int2, v_Dec2, v_Int3, v_Dec3 );
    
	set v_Particulars = '(ii) Lease Rental Discounting';
    SET v_Id = v_Id + 1;
	insert into TempLoanAdvances (Id, PartSection, SrNo, Particulars, OrdSeq )
	values ( v_Id, v_PartSection, '', v_Particulars, 45 );
    
    set v_Particulars = '(iii) Others';
    
    select count(1), coalesce(sum(TotalLoanOut),0) into v_Int1, v_Dec1
	from stgborrowerloan a
	where a.SanctDate <= v_PrvMonthEndDate
	and a.LoanPurpose in ('POL-07','POL-08','POL-09','POL-10','POL-11','POL-12','POL-14')
    and a.TotalLoanOut > 0;

	select count(1), coalesce(sum(LoanRepayDurMth),0) into v_Int2, v_Dec2
	from stgborrowerloan a
	where a.SanctDate between v_CurrMonthStartDate and p_ReportDate
	and a.LoanPurpose in ('POL-07','POL-08','POL-09','POL-10','POL-11','POL-12','POL-14')
	and a.LoanRepayDurMth > 0;

    select count(1), coalesce(sum(TotalLoanOut),0) into v_Int3, v_Dec3
	from stgborrowerloan a
	where a.SanctDate = p_ReportDate
	and a.LoanPurpose in ('POL-07','POL-08','POL-09','POL-10','POL-11','POL-12','POL-14')
    and a.TotalLoanOut > 0;    
    
	SET v_Id = v_Id + 1;
    
	insert into TempLoanAdvances (Id, PartSection, SrNo, Particulars, OrdSeq, AprToPrvMonth_AcctCount, AprToPrvMonth_GrossAmount,
		DuringMonth_AcctCount, DuringMonth_GrossAmount, Total_AcctCount, Total_GrossAmount)
	values ( v_Id, v_PartSection, '', v_Particulars, 86, v_Int1, v_Dec1, v_Int2, v_Dec2, v_Int3, v_Dec3 );
    
    set v_Particulars = '(a) Of (iii) above loan against the security of shares/ debentures / bonds';
    
    select count(1), coalesce(sum(TotalLoanOut),0) into v_Int1, v_Dec1
	from stgborrowerloan a
	where a.SanctDate <= v_PrvMonthEndDate
	and a.LoanPurpose = 'POL-08'
    and a.TotalLoanOut > 0;

	select count(1), coalesce(sum(LoanRepayDurMth),0) into v_Int2, v_Dec2
	from stgborrowerloan a
	where a.SanctDate between v_CurrMonthStartDate and p_ReportDate
	and a.LoanPurpose = 'POL-08'
	and a.LoanRepayDurMth > 0;

    select count(1), coalesce(sum(TotalLoanOut),0) into v_Int3, v_Dec3
	from stgborrowerloan a
	where a.SanctDate = p_ReportDate
	and a.LoanPurpose = 'POL-08'
    and a.TotalLoanOut > 0;
    
	SET v_Id = v_Id + 1;
    
	insert into TempLoanAdvances (Id, PartSection, SrNo, Particulars, OrdSeq, AprToPrvMonth_AcctCount, AprToPrvMonth_GrossAmount,
		DuringMonth_AcctCount, DuringMonth_GrossAmount, Total_AcctCount, Total_GrossAmount)
	values ( v_Id, v_PartSection, '', v_Particulars, 87, v_Int1, v_Dec1, v_Int2, v_Dec2, v_Int3, v_Dec3 );

    set v_Particulars = '(b) Of (iii) above loans against security of gold jewellery';
    
    select count(1), coalesce(sum(TotalLoanOut),0) into v_Int1, v_Dec1
	from stgborrowerloan a
	where a.SanctDate <= v_PrvMonthEndDate
	and a.LoanPurpose = 'POL-09'
    and a.TotalLoanOut > 0;

	select count(1), coalesce(sum(LoanRepayDurMth),0) into v_Int2, v_Dec2
	from stgborrowerloan a
	where a.SanctDate between v_CurrMonthStartDate and p_ReportDate
	and a.LoanPurpose = 'POL-09'
	and a.LoanRepayDurMth > 0;

    select count(1), coalesce(sum(TotalLoanOut),0) into v_Int3, v_Dec3
	from stgborrowerloan a
	where a.SanctDate = p_ReportDate
	and a.LoanPurpose = 'POL-09'
    and a.TotalLoanOut > 0;
	SET v_Id = v_Id + 1;
    
	insert into TempLoanAdvances (Id, PartSection, SrNo, Particulars, OrdSeq, AprToPrvMonth_AcctCount, AprToPrvMonth_GrossAmount,
		DuringMonth_AcctCount, DuringMonth_GrossAmount, Total_AcctCount, Total_GrossAmount)
	values ( v_Id, v_PartSection, '', v_Particulars, 88, v_Int1, v_Dec1, v_Int2, v_Dec2, v_Int3, v_Dec3 );



    
   
	SET SQL_SAFE_UPDATES=0;
    -- Updating total for PART-1A: LOAN SANCTIONS
    call spUpdateTempTable (2,'3, 6, 7');
    call spUpdateTempTable (13,'14, 15, 16');
	call spUpdateTempTable (12,'13, 19, 24');
    call spUpdateTempTable (1,'2, 8, 11');
    
    -- Updating total for PART-1B: LOAN DISBURSEMENTS
    call spUpdateTempTable (32,'33, 36, 37');
    call spUpdateTempTable (43,'44, 45, 46');
	call spUpdateTempTable (42,'43, 49, 54');
    call spUpdateTempTable (31,'32, 38, 41');    
    
    -- Updating total for PART-1C: LOAN OUTSTANDINGS
    call spUpdateTempTable (72,'73, 76, 77');
    call spUpdateTempTable (83,'84, 85, 86');
	call spUpdateTempTable (82,'83, 89, 94');
    call spUpdateTempTable (71,'72, 78, 81');    

    SET SQL_SAFE_UPDATES=1;

    select * from TempLoanAdvances order by OrdSeq;
    DROP TEMPORARY TABLE IF EXISTS TempLoanAdvances_Intermediate;
    DROP TEMPORARY TABLE IF EXISTS TempLoanAdvances;
    
end$$
DELIMITER ;
