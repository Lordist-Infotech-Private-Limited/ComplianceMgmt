document.getElementById('loginForm').addEventListener('submit', function(event) {
    event.preventDefault();

    const username = document.getElementById('username').value;
    const password = document.getElementById('password').value;

    // Replace with your API endpoint
    const apiUrl = 'https://api.example.com/login';

    fetch(apiUrl, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({
            username: username,
            password: password,
        }),
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            // Redirect to a dashboard or home page
            window.location.href = 'dashboard.html';
        } else {
            document.getElementById('error-message').textContent = data.message || 'Login failed';
        }
    })
    .catch(error => {
        console.error('Error:', error);
        document.getElementById('error-message').textContent = 'An error occurred during login';
    });
});