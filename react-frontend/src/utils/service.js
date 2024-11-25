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

export async function saveSingleRecord(record, tableName) {
  const endpoint =
    tableName === "borrowerDetail" ? "/BorrowerDetail" : "/CoBorrowerDetails";
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
