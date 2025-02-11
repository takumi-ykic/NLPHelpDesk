/**
 * Handles opening the modal for assigning a user to a ticket.
 * Fetches the list of assignable users from the server.
 *
 * @param {HTMLElement} element The element that triggered the modal (e.g., a button).
 */
function handleAssignableUserModal(element) {
    // Get the URL for fetching users from the data attribute of the triggering element.
    const url = element.getAttribute('data-assign-user-url');

    // Fetch the user data.
    fetch(url)
        .then(response => response.json())
        .then(data => {
            let userList = document.getElementById('assignable-userList');
            userList.innerHTML = '';

            // Check if data exists and is not empty
            if (data && data.length > 0) {
                // Iterate through the received user data and create list items for each user.
                data.forEach(function (user) {
                    const fullName = user.firstName + " " + user.lastName;
                    // Create the HTML for each user list item, including a radio button and category badge.
                    userList.innerHTML += `<div class="list-group-item d-flex justify-content-between align-items-center">
                                                <div class="d-flex align-items-center">
                                                    <input type="radio" name="assignableUserSelect" id="user-${user.id}" class="user-select-radio" data-user-id="${user.id}" />
                                                    <span class="ms-2 font-weight-bold">${fullName}</span>
                                                </div>
                                                <span class="badge bg-theme text-white rounded-pill">${user.helpDeskCategory.categoryName}</span>
                                            </div>`;
                });
            } else {
                // Display a message if no users are available.
                userList.innerHTML = '<p>No users available for assignment.</p>';
            }

            document.body.insertAdjacentHTML('beforeend', data);

            // Show the modal.
            const modal = new bootstrap.Modal(document.getElementById('assignableUserModal'));
            modal.show();

            // Set up the event listener for the assign user button.
            setupAssignUserButton();
        })
        .catch(error => {
            console.error('Error fetching modal:', error);
            alert('Error loading user modal.');
        });
}

/**
 * Sets up the event listener for the "Assign User" button in the modal.
 * Enables the button only when a user is selected.
 */
function setupAssignUserButton() {
    const radios = document.querySelectorAll('.user-select-radio');
    let assignButton = document.getElementById('assignUserButton');

    // Add event listeners to the radio buttons to enable/disable the assign button.
    radios.forEach(function (radio) {
        radio.addEventListener('change', function() {
            // Enable the button when a radio button is selected.
            assignButton.disabled = false;
        });
    });

    // Add event listener to the assign button to handle the assignment process.
    assignButton.addEventListener('click', function () {
        const selectedUserId = document.querySelector('input[name="assignableUserSelect"]:checked')?.getAttribute('data-user-id');
        const ticketId = document.getElementById('assign-ticket-id').value;

        // Check if both user ID and ticket ID are available before making the request.
        if (selectedUserId && ticketId) {
            addUser(selectedUserId, ticketId);
        }
    });
}

/**
 * Sends a request to the server to assign the selected user to the ticket.
 *
 * @param {number} userId The ID of the user to assign.
 * @param {number} ticketId The ID of the ticket to assign the user to.
 */
function addUser(userId, ticketId) {
    const url = document.getElementById('assign-user-url').value;

    // Make a POST request to the server.
    fetch(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            userId: userId,
            ticketId: ticketId
        })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                alert('User assigned successfully!');
                const modal = new bootstrap.Modal(document.getElementById('assignableUserModal'));
                modal.hide();

                // Reload the page to reflect the changes.
                const reload = document.getElementById('reload-detail-url').value;
                window.location.href = reload;
            } else {
                alert('Failed to assign user.');
            }
        })
        .catch(error => {
            console.error('Error assigning user:', error);
            alert('Error assigning user.');
        });
}

/**
 * Sets up event listeners for the "Delete Assigned User" buttons.
 */
function setupDeleteAssignedUserListener() {
    const deleteButtons = document.querySelectorAll('.delete-assign-user-btn');
    
    // If no delete buttons are found, exit the function.
    if (deleteButtons.length === 0) return;

    deleteButtons.forEach(button => {
        button.addEventListener('click', function () {
            const deleteConfirm = confirm(`Are you sure you want to delete this user from this ticket ?`);

            if (deleteConfirm) {
                const deleteUrl = this.getAttribute('data-delete-url');
                const reloadUrl = this.getAttribute('data-reload-url');
                deleteUser(deleteUrl, reloadUrl);
            }
        });
    });
}

/**
 * Sends a DELETE request to the server to unassign a user from a ticket.
 *
 * @param {string} deleteUrl The URL to send the DELETE request to.
 * @param {string} reloadUrl The URL to redirect to after successful deletion.
 */
function deleteUser(deleteUrl, reloadUrl) {
    fetch(deleteUrl, {
        method: 'DELETE',
        headers: {
            'Content-Type': 'application/json',
        }
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                alert('User was unassigned successfully!');
                window.location.href = reloadUrl;
            } else {
                alert('Failed to delete user.');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('An error occurred while deleting the user.');
        });
}

// Add event listener for DOMContentLoaded to ensure that the script runs after the DOM is fully loaded.
document.addEventListener('DOMContentLoaded', () => {
    setupDeleteAssignedUserListener();
})