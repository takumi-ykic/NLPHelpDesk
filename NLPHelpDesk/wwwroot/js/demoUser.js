/**
 * Initializes the demo user selection feature.
 * This function adds an event listener to the demo user list,
 * allowing users to quickly populate the email and password fields
 * by clicking on a demo user.
 */
function initializeDemoUserSelection() {
    const demoUserList = document.getElementById('demo-users');
    if (!demoUserList) return;
    const emailInput = document.getElementById('email');
    const passwordInput = document.getElementById('password');

    // Add a click event listener to the demo user list.
    demoUserList.addEventListener('click', (event) => {
        event.preventDefault();

        // Find the closest ancestor element with the class "demo-user".
        const clickedUser = event.target.closest('.demo-user');

        // Check if a demo user was clicked.
        if (clickedUser) {
            const email = clickedUser.dataset.email;
            const password = clickedUser.dataset.password;

            // Populate the email and password input fields with the demo user's credentials.
            emailInput.value = email;
            passwordInput.value = password;
        }
    });
}

// Add a DOMContentLoaded event listener to ensure that the function is executed.
document.addEventListener('DOMContentLoaded', initializeDemoUserSelection);