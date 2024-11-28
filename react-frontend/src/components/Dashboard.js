import React, { useState } from "react";
import Actions from "./Actions";
import DataTable from "./DataTable";
import Loader from "./Loader";
import EditRecordModal from "./EditRecordModal";
import ViewAllRecordsModal from "./ViewAllRecordsModal";
import {
  fetchData,
  fetchBorrowerDetails,
  fetchCoBorrowerDetails,
  validateBorrowerDetail,
  fetchBorrowerLoanDetails,
  fetchBorrowerMortgageDetails,
  fetchBorrowerMortgageOtherDetails,
} from "../utils/service";

function Dashboard() {
  const [data, setData] = useState([]);
  const [loading, setLoading] = useState(false);
  const [currentPage, setCurrentPage] = useState(0);
  const recordsPerPage = 10;
  const [selectedDate, setSelectedDate] = useState("");
  const [editRecordIndex, setEditRecordIndex] = useState(-1);
  const [viewAllRecords, setViewAllRecords] = useState([]);
  const [currentModal, setCurrentModal] = useState(null);
  const [currentTableName, setCurrentTableName] = useState("");
  const [editRecord, setEditRecord] = useState(null);

  const handleFetchData = async (date) => {
    setLoading(true);
    try {
      const fetchedData = await fetchData(date);
      // Initialize data with only TotalRecords
      const initialData = fetchedData.map((item) => ({
        MsgStructure: item.MsgStructure,
        TotalRecords: item.TotalRecords,
        SuccessRecords: 0,
        ConstraintRejection: 0,
        BusinessRejection: 0,
      }));
      setData(initialData);
    } catch (error) {
      console.error("Error fetching data:", error);
      alert("An error occurred while fetching data.");
    } finally {
      setLoading(false);
    }
  };

  const fetchRecord = async (index, tableName, referenceDate) => {
    showLoader();
    try {
      let data;
      if (tableName === "borrowerDetail") {
        data = await fetchBorrowerDetails(referenceDate);
      } else if (tableName === "coBorrowerDetail") {
        data = await fetchCoBorrowerDetails(referenceDate);
      } else if (tableName === "borrowerLoan") {
        data = await fetchBorrowerLoanDetails(referenceDate);
      } else if (tableName === "borrowerMortgage") {
        data = await fetchBorrowerMortgageDetails(referenceDate);
      } else if (tableName === "borrowerMortgageOther") {
        data = await fetchBorrowerMortgageOtherDetails(referenceDate);
      } else {
        throw new Error(`Unsupported table name: ${tableName}`);
      }

      const fetchedRecord = data[index];
      setEditRecord(fetchedRecord);
    } catch (error) {
      console.error("Error fetching record:", error);
      alert("Failed to fetch record.");
    }
    hideLoader();
  };

  const handleOpenEditModal = async (index, type) => {
    setEditRecordIndex(index);
    setCurrentModal("edit");
    // Pass selectedDate as a prop
    // Determine the current table name based on the index
    let tableName = "";
    if (index === 0) {
      setCurrentTableName("borrowerDetail");
      tableName = "borrowerDetail";
    } else if (index === 1) {
      setCurrentTableName("borrowerLoan");
      tableName = "borrowerLoan";
    } else if (index === 2) {
      setCurrentTableName("borrowerMortgage");
      tableName = "borrowerMortgage";
    } else if (index === 3) {
      setCurrentTableName("borrowerMortgageOther");
      tableName = "borrowerMortgageOther";
    } else if (index === 4) {
      setCurrentTableName("coBorrowerDetail");
      tableName = "coBorrowerDetail";
    } else {
      setCurrentTableName("");
    }

    // Fetch the record before opening the modal
    await fetchRecord(index, tableName, selectedDate);
  };

  const handleOpenViewAllModal = async (index, status) => {
    showLoader();
    const referenceDate = selectedDate;
    if (index === 0) {
      const borrowerData = await fetchBorrowerDetails(referenceDate);
      setViewAllRecords(borrowerData);
    } else if (index === 1) {
      const borrowerLoanData = await fetchBorrowerLoanDetails(referenceDate);
      setViewAllRecords(borrowerLoanData);
    } else if (index === 2) {
      const borrowerMortgageData = await fetchBorrowerMortgageDetails(
        referenceDate
      );
      setViewAllRecords(borrowerMortgageData);
    } else if (index === 3) {
      const borrowerMortgageOtherData = await fetchBorrowerMortgageOtherDetails(
        referenceDate
      );
      setViewAllRecords(borrowerMortgageOtherData);
    } else if (index === 4) {
      const coBorrowerData = await fetchCoBorrowerDetails(referenceDate);
      setViewAllRecords(coBorrowerData);
    }
    setCurrentModal("viewAll");
    // Determine the current table name based on the index
    if (index === 0) {
      setCurrentTableName("borrowerDetail");
    } else if (index === 1) {
      setCurrentTableName("borrowerLoan");
    } else if (index === 2) {
      setCurrentTableName("borrowerMortgage");
    } else if (index === 3) {
      setCurrentTableName("borrowerMortgageOther");
    } else if (index === 4) {
      setCurrentTableName("coBorrowerDetail");
    } else {
      // Default to the current table name if index is neither 0 nor 4
      setCurrentTableName("");
    }
    hideLoader();
  };

  const handleCloseModal = () => {
    setCurrentModal(null);
    setEditRecord(null); // Clear the edit record when closing the modal
  };

  const handleValidate = async (index) => {
    showLoader();
    try {
      const referenceDate = selectedDate;
      const bankId = 103;
      const response = await validateBorrowerDetail(referenceDate, bankId);
      if (response.status === 204 || response.ok) {
        alert("Validation successful!");
        // Fetch the full data after successful validation
        const fetchedData = await fetchData(selectedDate);
        setData(fetchedData);
      } else {
        throw new Error(`Failed to update record. Status: ${response.status}`);
      }
    } catch (error) {
      console.error("Error validating borrower details:", error);
      alert("Validation failed.");
    } finally {
      hideLoader();
    }
  };

  const showLoader = () => {
    setLoading(true);
  };

  const hideLoader = () => {
    setLoading(false);
  };

  const paginatedData = data.slice(
    currentPage * recordsPerPage,
    (currentPage + 1) * recordsPerPage
  );

  return (
    <div className="container mx-auto p-4">
      <Actions
        onFetchData={handleFetchData}
        selectedDate={selectedDate}
        setSelectedDate={setSelectedDate}
      />
      {loading && <Loader />}
      <DataTable
        data={paginatedData}
        openEditModal={handleOpenEditModal}
        openViewAllModal={handleOpenViewAllModal}
        handleValidate={handleValidate}
      />
      {currentModal === "edit" && (
        <EditRecordModal
          record={editRecord}
          tableName={currentTableName}
          referenceDate={selectedDate}
          onClose={handleCloseModal}
          showLoader={showLoader}
          hideLoader={hideLoader}
        />
      )}
      {currentModal === "viewAll" && (
        <ViewAllRecordsModal
          records={viewAllRecords}
          tableName={currentTableName}
          onClose={handleCloseModal}
          showLoader={showLoader}
          hideLoader={hideLoader}
        />
      )}
    </div>
  );
}

export default Dashboard;
