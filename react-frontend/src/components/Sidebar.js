import React, { useState } from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faBars,
  faChartBar,
  faFileAlt,
  faCogs,
} from "@fortawesome/free-solid-svg-icons";
import logo from "../assets/logo.png";
import fullLogo from "../assets/fulllogo.png";

const Sidebar = ({ onComponentChange, activeComponent }) => {
  const [isCollapsed, setIsCollapsed] = useState(false);

  const toggleSidebar = () => {
    setIsCollapsed(!isCollapsed);
  };

  return (
    <div
      className={`bg-gradient-to-b from-gray-100 via-white to-gray-300 text-black min-h-screen flex flex-col transition-all duration-300 ${
        isCollapsed ? "w-20" : "w-56"
      } shadow-lg`}
    >
      <div className="flex justify-between items-center p-4">
        <div className={`${isCollapsed ? "hidden" : "block"}`}>
          <img src={fullLogo} alt="Large Logo" className="w-32" />
        </div>
        <div className={`${!isCollapsed ? "hidden" : "block"}`}>
          <img src={logo} alt="Small Logo" className="w-8" />
        </div>
        <button onClick={toggleSidebar} className="text-xl">
          <FontAwesomeIcon icon={faBars} />
        </button>
      </div>
      <nav className="mt-8 flex-grow">
        <ul>
          <li
            className={`p-4 hover:bg-gray-200 transition-all duration-200 rounded-md mx-2 my-2 ${
              isCollapsed ? "text-center" : ""
            } ${activeComponent === "Dashboard" ? "bg-gray-200" : ""}`}
            onClick={() => onComponentChange("Dashboard")}
          >
            <a href="#" className="flex items-center">
              <FontAwesomeIcon icon={faChartBar} className="mr-2" />
              {isCollapsed ? "" : "Dashboard"}
            </a>
          </li>
          <li
            className={`p-4 hover:bg-gray-200 transition-all duration-200 rounded-md mx-2 my-2 ${
              isCollapsed ? "text-center" : ""
            } ${activeComponent === "Report" ? "bg-gray-200" : ""}`}
            onClick={() => onComponentChange("Report")}
          >
            <a href="#" className="flex items-center">
              <FontAwesomeIcon icon={faFileAlt} className="mr-2" />
              {isCollapsed ? "" : "Report"}
            </a>
          </li>
          <li
            className={`p-4 hover:bg-gray-200 transition-all duration-200 rounded-md mx-2 my-2 ${
              isCollapsed ? "text-center" : ""
            } ${activeComponent === "Configuration" ? "bg-gray-200" : ""}`}
            onClick={() => onComponentChange("Configuration")}
          >
            <a href="#" className="flex items-center">
              <FontAwesomeIcon icon={faCogs} className="mr-2" />
              {isCollapsed ? "" : "Configuration"}
            </a>
          </li>
        </ul>
      </nav>
    </div>
  );
};

export default Sidebar;
