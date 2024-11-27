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
      setData(fetchedData);
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
      const data =
        tableName === "borrowerDetail"
          ? await fetchBorrowerDetails(referenceDate)
          : await fetchCoBorrowerDetails(referenceDate);

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
    } else if (index === 4) {
      const coBorrowerData = await fetchCoBorrowerDetails(referenceDate);
      setViewAllRecords(coBorrowerData);
    }
    setCurrentModal("viewAll");
    // Determine the current table name based on the index
    if (index === 0) {
      setCurrentTableName("borrowerDetail");
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
