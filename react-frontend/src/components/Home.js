import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import Sidebar from "./Sidebar";
import Header from "./Header";
import Dashboard from "./Dashboard";
import Report from "./Report";

function Home({ user, onLogout }) {
  const [selectedComponent, setSelectedComponent] = useState("Dashboard");
  const navigate = useNavigate();

  useEffect(() => {
    if (!user) {
      navigate("/");
    }
  }, [user, navigate]);

  const handleComponentChange = (component) => {
    setSelectedComponent(component);
  };

  return (
    <div className="flex flex-col md:flex-row">
      <Sidebar
        onComponentChange={handleComponentChange}
        activeComponent={selectedComponent}
      />
      <div className="flex-1 flex flex-col">
        <Header
          componentName={selectedComponent}
          user={user}
          onLogout={onLogout}
        />
        {selectedComponent === "Dashboard" ? <Dashboard /> : <Report />}
      </div>
    </div>
  );
}

export default Home;
