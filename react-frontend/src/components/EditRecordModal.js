import React, { useState, useEffect } from "react";
import { saveSingleRecord } from "../utils/service";
import {
  borrowerFields,
  coBorrowerFields,
  borrowerLoanFields,
  borrowerMortgageFields,
  borrowerMortgageOtherFields,
} from "../utils/tables";

const EditRecordModal = ({ record, tableName, onClose }) => {
  const [editedRecord, setEditedRecord] = useState(record);

  useEffect(() => {
    console.log(record);

    setEditedRecord(record);
  }, [record]);

  const handleInputChange = (e) => {
    const { id, value, type, checked } = e.target;
    let fieldValue = value;

    if (type === "number") {
      fieldValue = parseInt(value, 10);
    } else if (type === "date") {
      fieldValue = new Date(value).toISOString();
    } else if (type === "checkbox") {
      fieldValue = checked;
    }

    setEditedRecord({
      ...editedRecord,
      [id]: fieldValue,
    });
  };

  const handleSave = async () => {
    const recordToSave = {};

    let fields = [];
    if (tableName === "borrowerDetail") {
      fields = borrowerFields;
    } else if (tableName === "coBorrowerDetail") {
      fields = coBorrowerFields;
    } else if (tableName === "borrowerLoan") {
      fields = borrowerLoanFields;
    } else if (tableName === "borrowerMortgage") {
      fields = borrowerMortgageFields;
    } else if (tableName === "borrowerMortgageOther") {
      fields = borrowerMortgageOtherFields;
    }

    fields.forEach((field) => {
      if (field.type === "number") {
        recordToSave[field.name] = parseInt(editedRecord[field.name], 10);
      } else if (field.type === "date") {
        const date = new Date(editedRecord[field.name]);
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, "0");
        const day = String(date.getDate()).padStart(2, "0");
        recordToSave[field.name] = `${year}-${month}-${day}T00:00:00`;
      } else if (field.type === "checkbox") {
        recordToSave[field.name] = editedRecord[field.name] || false;
      } else {
        recordToSave[field.name] = editedRecord[field.name];
      }
    });

    try {
      const response = await saveSingleRecord(recordToSave, tableName);
      if (response.status === 204 || response.ok) {
        alert("Record updated successfully!");
        onClose();
      } else {
        throw new Error(`Failed to update record. Status: ${response.status}`);
      }
    } catch (error) {
      console.error("Error updating record:", error);
      alert("Failed to update record.");
    }
  };

  let fields = [];
  if (tableName === "borrowerDetail") {
    fields = borrowerFields;
  } else if (tableName === "coBorrowerDetail") {
    fields = coBorrowerFields;
  } else if (tableName === "borrowerLoan") {
    fields = borrowerLoanFields;
  } else if (tableName === "borrowerMortgage") {
    fields = borrowerMortgageFields;
  } else if (tableName === "borrowerMortgageOther") {
    fields = borrowerMortgageOtherFields;
  }

  const formatDateForInput = (dateString) => {
    if (!dateString) return "";
    const date = new Date(dateString);
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const day = String(date.getDate()).padStart(2, "0");
    return `${year}-${month}-${day}`;
  };

  if (editedRecord) {
    return (
      <div
        className="fixed top-0 left-0 w-full h-full bg-black bg-opacity-50 flex justify-center items-center overflow-y-auto backdrop-blur-md"
        onClick={(e) => e.stopPropagation()}
      >
        <div
          className="bg-white p-3 rounded-xl shadow-lg w-11/12 max-w-4xl text-left mx-auto max-h-[90vh] overflow-y-auto"
          onClick={(e) => e.stopPropagation()}
        >
          <div className="flex justify-between items-center">
            <h2 className="text-xl font-bold">Edit Record</h2>
            <button
              className="border-none text-sm text-white cursor-pointer rounded-md px-4 py-2 bg-blue-700 font-medium hover:bg-blue-800"
              onClick={onClose}
            >
              X
            </button>
          </div>
          <div className="p-2">
            {fields.map((field) => (
              <div key={field.name} className="flex items-center mb-1">
                <label
                  htmlFor={field.name}
                  className="max-w-[20%] w-[500px] pr-2 font-medium"
                >
                  {field.name}:
                </label>
                {field.type === "checkbox" ? (
                  <input
                    type="checkbox"
                    id={field.name}
                    checked={editedRecord[field.name] || false}
                    onChange={handleInputChange}
                    disabled={field.disabled}
                    className="ml-2 flex-1"
                  />
                ) : (
                  <input
                    type={field.type}
                    id={field.name}
                    value={
                      field.type === "date"
                        ? formatDateForInput(editedRecord[field.name])
                        : editedRecord[field.name] || ""
                    }
                    onChange={handleInputChange}
                    disabled={field.disabled}
                    className="flex-1 m-1 text-base p-2 px-4 border border-gray-400 rounded-lg outline-none transition-colors duration-200 bg-white text-gray-800 shadow-sm disabled:bg-gray-200 disabled:text-gray-500 disabled:border-gray-300 disabled:cursor-not-allowed disabled:shadow-none"
                  />
                )}
              </div>
            ))}
            <div className="flex items-end flex-col">
              <button
                className="px-4 py-2 bg-blue-700 text-white border-none rounded-md cursor-pointer text-sm hover:bg-blue-800"
                onClick={handleSave}
              >
                Save
              </button>
            </div>
          </div>
        </div>
      </div>
    );
  }
};

export default EditRecordModal;
