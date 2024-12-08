const apiBaseUrl = "https://adfapi.lordist.in";

export async function fetchData(date) {
  const url = `${apiBaseUrl}/Dashboard/GetRecordCount?date=${encodeURIComponent(
    date
  )}`;
  const response = await fetch(url, {
    method: "GET",
    headers: { "Content-Type": "application/json" },
  });
  if (!response.ok) {
    throw new Error(`API call failed with status: ${response.status}`);
  }
  return await response.json();
}

export async function fetchBorrowerDetails(date) {
  const url = `${apiBaseUrl}/BorrowerDetail/byDate/${encodeURIComponent(date)}`;
  const response = await fetch(url, {
    method: "GET",
    headers: { "Content-Type": "application/json" },
  });
  if (!response.ok) {
    throw new Error(`Error fetching borrower details: ${response.status}`);
  }
  return await response.json();
}

export async function fetchCoBorrowerDetails(date) {
  const url = `${apiBaseUrl}/CoBorrowerDetails/byDate/${encodeURIComponent(
    date
  )}`;
  const response = await fetch(url, {
    method: "GET",
    headers: { "Content-Type": "application/json" },
  });
  if (!response.ok) {
    throw new Error(`Error fetching co-borrower details: ${response.status}`);
  }
  return await response.json();
}

export async function fetchBorrowerLoanDetails(date) {
  const url = `${apiBaseUrl}/BorrowerLoan/byDate/${encodeURIComponent(date)}`;
  const response = await fetch(url, {
    method: "GET",
    headers: { "Content-Type": "application/json" },
  });
  if (!response.ok) {
    throw new Error(`Error fetching borrower loan details: ${response.status}`);
  }
  return await response.json();
}

export async function fetchBorrowerMortgageDetails(date) {
  const url = `${apiBaseUrl}/BorrowerMortgage/byDate/${encodeURIComponent(
    date
  )}`;
  const response = await fetch(url, {
    method: "GET",
    headers: { "Content-Type": "application/json" },
  });
  if (!response.ok) {
    throw new Error(
      `Error fetching borrower mortgage details: ${response.status}`
    );
  }
  return await response.json();
}

export async function fetchBorrowerMortgageOtherDetails(date) {
  const url = `${apiBaseUrl}/BorrowerMortgageOther/byDate/${encodeURIComponent(
    date
  )}`;
  const response = await fetch(url, {
    method: "GET",
    headers: { "Content-Type": "application/json" },
  });
  if (!response.ok) {
    throw new Error(
      `Error fetching borrower mortgage other details: ${response.status}`
    );
  }
  return await response.json();
}

export async function saveSingleRecord(record, tableName) {
  let endpoint = "";
  if (tableName === "borrowerDetail") {
    endpoint = "/BorrowerDetail";
  } else if (tableName === "coBorrowerDetail") {
    endpoint = "/CoBorrowerDetails";
  } else if (tableName === "borrowerLoan") {
    endpoint = "/BorrowerLoan";
  } else if (tableName === "borrowerMortgage") {
    endpoint = "/BorrowerMortgage";
  } else if (tableName === "borrowerMortgageOther") {
    endpoint = "/BorrowerMortgageOther";
  }

  const url = `${apiBaseUrl}${endpoint}`;
  const response = await fetch(url, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(record),
  });

  if (!response.ok || response.status !== 204) {
    throw new Error(`Failed to save record: ${response.status}`);
  }
  return await response;
}

export async function uploadFile(file, endpoint) {
  const formData = new FormData();
  formData.append("file", file);
  const url = `${apiBaseUrl}${endpoint}`;
  const response = await fetch(url, {
    method: "POST",
    body: formData,
  });
  if (!response.ok) {
    throw new Error(`File upload failed: ${response.status}`);
  }
  return await response.json();
}

export async function loginUser(credentials) {
  const url = `${apiBaseUrl}/Auth/Login`;
  const response = await fetch(url, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(credentials),
  });

  if (!response.ok) {
    throw new Error(`Login failed with status: ${response.status}`);
  }

  return await response.json();
}

export async function validateBorrowerDetail(date, bankId) {
  const url = `${apiBaseUrl}/BorrowerDetail/Validate`;
  const response = await fetch(url, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ Date: date, BankId: bankId }),
  });

  if (!response.ok) {
    throw new Error(`Validation failed with status: ${response.status}`);
  }

  return await response;
}
