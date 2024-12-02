import React, { useState, useEffect } from "react";
import { BrowserRouter as Router, Route, Routes } from "react-router-dom";
import Login from "./components/Login";
import Home from "./components/Home";
import "./globals";
import "@boldreports/javascript-reporting-controls/Content/v2.0/tailwind-light/bold.report-viewer.min.css";
import "@boldreports/javascript-reporting-controls/Scripts/v2.0/common/bold.reports.common.min";
import "@boldreports/javascript-reporting-controls/Scripts/v2.0/common/bold.reports.widgets.min";
import "@boldreports/javascript-reporting-controls/Scripts/v2.0/bold.report-viewer.min";
//Reports react base
import "@boldreports/react-reporting-components/Scripts/bold.reports.react.min";

function App() {
  const [user, setUser] = useState(null);

  useEffect(() => {
    const storedUser = localStorage.getItem("user");
    const storedExpiration = localStorage.getItem("sessionExpiration");

    if (storedUser && storedExpiration) {
      const expirationDate = new Date(storedExpiration);
      const currentDate = new Date();

      if (currentDate < expirationDate) {
        setUser(JSON.parse(storedUser));
      } else {
        localStorage.removeItem("user");
        localStorage.removeItem("sessionExpiration");
      }
    }
  }, []);

  const handleLogin = (userData) => {
    const expirationDate = new Date();
    expirationDate.setDate(expirationDate.getDate() + 7);

    localStorage.setItem("user", JSON.stringify(userData));
    localStorage.setItem("sessionExpiration", expirationDate.toISOString());

    setUser(userData);
  };

  const handleLogout = () => {
    localStorage.removeItem("user");
    localStorage.removeItem("sessionExpiration");
    setUser(null);
  };

  return (
    <Router>
      <Routes>
        <Route
          path="/"
          element={
            user ? (
              <Home user={user} onLogout={handleLogout} />
            ) : (
              <Login onLogin={handleLogin} />
            )
          }
        />
      </Routes>
    </Router>
  );
}

export default App;
