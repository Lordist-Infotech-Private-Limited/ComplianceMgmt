import React from "react";

const DataTable = ({
  data,
  openEditModal,
  openViewAllModal,
  handleValidate,
}) => {
  return (
    <div className="overflow-x-auto bg-white rounded-xl shadow-sm mt-2">
      <table className="w-full border-separate border-spacing-0">
        <thead>
          <tr className="bg-gray-100">
            <th className="bg-blue-700 p-4 text-left font-semibold text-white border-b border-gray-200">
              Table Name
            </th>
            <th className="bg-blue-700 p-4 text-left font-semibold text-white border-b border-gray-200">
              Total Records
            </th>
            <th className="bg-blue-700 p-4 text-left font-semibold text-white border-b border-gray-200">
              Success
            </th>
            <th className="bg-blue-700 p-4 text-left font-semibold text-white border-b border-gray-200">
              Constraint Rejection
            </th>
            <th className="bg-blue-700 p-4 text-left font-semibold text-white border-b border-gray-200">
              Business Rejection
            </th>
            <th className="bg-blue-700 p-4 text-left font-semibold text-white border-b border-gray-200">
              Action
            </th>
          </tr>
        </thead>
        <tbody>
          {data.map((item, index) => (
            <tr
              key={index}
              className={`hover:bg-gray-200 ${
                index % 2 === 0 ? "bg-white" : "bg-[#F2F2F2]"
              }`}
            >
              <td className="p-2 border-b border-gray-300">
                {item.MsgStructure}
              </td>
              <td className="p-2 border-b border-gray-300">
                {item.TotalRecords}
              </td>
              <td className="p-2 border-b border-gray-300">
                <button
                  className="text-blue-500 hover:underline mr-2"
                  onClick={() =>
                    openEditModal(index, "Success", "borrowerTable")
                  }
                >
                  +
                </button>
                <button
                  className="text-blue-500 hover:underline"
                  onClick={() => openViewAllModal(index, "Success")}
                >
                  {item.SuccessRecords}
                </button>
              </td>
              <td className="p-2 border-b border-gray-300">
                <button
                  className="text-blue-500 hover:underline mr-2"
                  onClick={() =>
                    openEditModal(index, "Constraint", "borrowerTable")
                  }
                >
                  +
                </button>
                <button
                  className="text-blue-500 hover:underline"
                  onClick={() => openViewAllModal(index, "Constraint")}
                >
                  {item.ConstraintRejection}
                </button>
              </td>
              <td className="p-2 border-b border-gray-300">
                <button
                  className="text-blue-500 hover:underline mr-2"
                  onClick={() =>
                    openEditModal(index, "Business", "borrowerTable")
                  }
                >
                  +
                </button>
                <button
                  className="text-blue-500 hover:underline"
                  onClick={() => openViewAllModal(index, "Business")}
                >
                  {item.BusinessRejection}
                </button>
              </td>
              <td className="p-2 border-b border-gray-300">
                <button
                  className="bg-green-500 text-white px-3 py-1 rounded-md hover:bg-green-600"
                  onClick={() => handleValidate(index)}
                >
                  Validate
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default DataTable;
