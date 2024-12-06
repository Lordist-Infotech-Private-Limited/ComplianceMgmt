import React from "react";
import { Pie } from "react-chartjs-2";
import { Chart, ArcElement, Tooltip, Legend } from "chart.js";
import Header from "./Header";

// Register the necessary elements and plugins
Chart.register(ArcElement, Tooltip, Legend);

const ManagementDashboard = ({ user, onLogout }) => {
  const data = {
    labels: [
      "Sum of ntotalloanout",
      "Count of sbloanno",
      "Sum of nsanctamount",
      "Sum of ntotalloanout",
    ],
    datasets: [
      {
        data: [439732298.4, 6, 8670000, 8507749.84],
        backgroundColor: ["#FF6384", "#36A2EB", "#FFCE56", "#4BC0C0"],
        hoverBackgroundColor: ["#FF6384", "#36A2EB", "#FFCE56", "#4BC0C0"],
      },
    ],
  };

  return (
    <>
      <Header
        componentName={"Management Dashboard"}
        user={user}
        onLogout={onLogout}
      />
      <div className="p-4">
        <div className="w-1/2 mx-auto">
          <Pie data={data} />
        </div>
      </div>
    </>
  );
};

export default ManagementDashboard;
