import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import Sidebar from "./Sidebar";
import Dashboard from "./Dashboard";
import Report from "./Report";
import ManagementDashboard from "./ManagementDashboard";
import ComplianceReports from "./ComplianceReports";
import MobileSidebar from "./MobileSidebar";

function Home({ user, onLogout }) {
  const [selectedComponent, setSelectedComponent] = useState("Dashboard");
  const navigate = useNavigate();

  useEffect(() => {
    if (!user) {
      navigate("/");
    }
  }, [user, navigate]);

  const handleComponentChange = (component) => {
    console.log("this is working", component);
    setSelectedComponent(component);
  };

  return (
    <div className="flex flex-col md:flex-row">
      <Sidebar
        onComponentChange={handleComponentChange}
        activeComponent={selectedComponent}
      />
      <MobileSidebar
        onComponentChange={handleComponentChange}
        activeComponent={selectedComponent}
        user={user}
        onLogout={onLogout}
      />
      <div className="flex-1 flex flex-col">
        {selectedComponent === "Dashboard" ? (
          <Dashboard user={user} onLogout={onLogout} />
        ) : selectedComponent === "Management Dashboard" ? (
          <ManagementDashboard user={user} onLogout={onLogout} />
        ) : selectedComponent === "MIS Reports" ? (
          <Report user={user} onLogout={onLogout} />
        ) : (
          <ComplianceReports user={user} onLogout={onLogout} />
        )}
      </div>
    </div>
  );
}

export default Home;
