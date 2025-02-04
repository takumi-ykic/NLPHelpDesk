/**
 * Initializes ticket filtering functionality.
 * Allows users to filter tickets based on keywords in ticket details.
 */
function ticketFiltering() {
    const btnFilter = document.getElementById('btnFilter');
    // Exit if the filter button is not found
    if (!btnFilter) return;
    
    const keywordFilter = document.getElementById('keywordFilter');
    const ticketList = document.getElementById('ticket-list');

    // Filtering
    // Add a click event listener to the filter button.
    btnFilter.addEventListener('click', () => {
        const keyword = keywordFilter.value.toLowerCase();

        const rows = ticketList.querySelectorAll('tr');
        rows.forEach(function(row) {
            // Get ticket details and date from data attributes, converting to lowercase for case-insensitive search.
            const detail = row.getAttribute('data-ticket-detail') ? row.getAttribute('data-ticket-detail').toLowerCase() : '';
            const date = row.getAttribute('data-date') ? row.getAttribute('data-date').toLowerCase() : '';

            // Check if the ticket detail or date contains the keyword.
            const matchKeyword = detail.indexOf(keyword) !== -1;
            
            if (matchKeyword) {
                // Show the row if it matches the keyword.
                row.style.display = '';
            } else {
                // Hide the row if it doesn't match.
                row.style.display = 'none';
            }
        });
    });
}

/**
 * Initializes ticket sorting functionality.
 * Allows users to sort tickets by clicking on table headers.
 */
function ticketSorting() {
    const ticketList = document.getElementById('ticket-list');
    const tableHeaders = document.querySelectorAll('th');

    let currentSortBy = '';
    let currentSortDir = 'asc';

    // Sorting
    // Iterate over each table header and add a click event listener.
    tableHeaders.forEach((header, index) => {
        header.addEventListener('click', () => {
            // Toggle sort direction if the same header is clicked.
            if (currentSortBy === index) {
                currentSortDir = currentSortDir === 'asc' ? 'desc' : 'asc';
            } else {
                currentSortBy = index;
                currentSortDir = 'asc';
            }
            
            // Get the ticket rows as an array.
            const rows = Array.from(ticketList.querySelectorAll('tr.ticket-item'));

            // Sort the rows based on the clicked column.
            rows.sort((a, b) => {
                const valueA = a.cells[index].textContent.trim().toLowerCase();
                const valueB = b.cells[index].textContent.trim().toLowerCase();

                if (index === 0) {
                    const priorityA = parseInt(a.getAttribute('data-priority'), 10);
                    const priorityB = parseInt(b.getAttribute('data-priority'), 10);

                    return currentSortDir === 'desc' ? priorityA - priorityB : priorityB - priorityA;
                }
                
                // TicketID
                if (index === 1) {
                    const extractIdParts = (value) => {
                        const match = value.match(/^([a-z]+)-(\d+)$/i);
                        if (match) {
                            return { prefix: match[1], number: parseInt(match[2], 10) };
                        }
                        return null;
                    };
                    
                    const partA = extractIdParts(valueA);
                    const partB = extractIdParts(valueB);
                    
                    if (partA && partB) {
                        if (partA.prefix !== partB.prefix) {
                            return currentSortDir === 'asc' ? partA.prefix.localeCompare(partB.prefix) : partB.prefix.localeCompare(partA.prefix);
                        } else if (partA.number !== partB.number) {
                            return currentSortDir === 'asc' ? partA.number - partB.number : partB.number - partA.number;
                        }
                        return 0;
                    } else if (!partA && partB) {
                        return currentSortDir === 'asc' ? -1 : 1;
                    } else if (partA && !partB) {
                        return currentSortDir === 'asc' ? 1 : -1;
                    } else {
                        return currentSortDir === 'asc' ? valueA.localeCompare(valueB) : valueB.localeCompare(valueA);
                    }
                }
                
                // Status
                if (index === 4) {
                    const statusA = parseInt(a.getAttribute('data-status'), 10);
                    const statusB = parseInt(b.getAttribute('data-status'), 10);

                    return currentSortDir === 'asc' ? statusA - statusB : statusB - statusA;
                }

                // Date
                const dateA = new Date(valueA);
                const dateB = new Date(valueB);
                if (!isNaN(dateA) && !isNaN(dateB)) {
                    return currentSortDir === 'asc' ? dateA - dateB : dateB - dateA;
                }

                // Number
                const intA = parseInt(valueA, 10);
                const intB = parseInt(valueB, 10);
                if (!isNaN(intA) && !isNaN(intB)) {
                    return currentSortDir === 'asc' ? intA - intB : intB - intA;
                }

                return currentSortDir === 'asc' ? valueA.localeCompare(valueB) : valueB.localeCompare(valueA);
            });

            // Re-append the sorted rows to the table.
            rows.forEach(row => ticketList.appendChild(row));
            updateSortIcons(currentSortBy, currentSortDir);
        });
    });
}

/**
 * Initializes the clear filtering functionality.
 * Resets the keyword input and shows all tickets.
 */
function clearFiltering() {
    const btnClear = document.getElementById('btnClear');
    // Exit if the clear button is not found.
    if (!btnClear) return;
    
    const keywordFilter = document.getElementById('keywordFilter');
    const ticketList = document.getElementById('ticket-list');

    // Clear search condition
    // Add a click event listener to the clear button.
    btnClear.addEventListener('click', () => {
        // Clear the keyword input field.
        keywordFilter.value = '';

        // Get all table rows.
        const rows = ticketList.querySelectorAll('tr');
        rows.forEach(function(row) {
            // Show all rows.
            row.style.display = '';
        });
    });
}

/**
 * Updates the sort icons in the table headers.
 * @param {number} currentSortBy The index of the currently sorted column.
 * @param {string} currentSortDir The current sort direction ('asc' or 'desc').
 */
function updateSortIcons(currentSortBy, currentSortDir) {
    const tableHeaders = document.querySelectorAll('th');
    
    tableHeaders.forEach((header, index) => {
        header.innerHTML = header.innerHTML.replace(/<i class="bi.*"><\/i>/, '');

        if (currentSortBy === index) {
            const icon = document.createElement('i');
            // Add the correct sort icon.
            icon.className = currentSortDir === 'asc' ? 'bi bi-arrow-up' : 'bi bi-arrow-down';
            header.appendChild(icon);
        }
    })
}

/**
 * Sets up click event listeners for ticket rows to navigate to ticket details page.
 */
function setTicketRowClick() {
    const rows = document.querySelectorAll('tr[data-ticket-url]');
    rows.forEach(row => {
        row.addEventListener('click', () => {
            // Get status
            const status = parseInt(row.getAttribute('data-status'));
            // Initialize URL string
            let url = '';
            
            // Check if status is complete
            if (status === 3) {
                // If status is complete, get ticket completion url
                url = row.getAttribute('data-ticket-completion-url');
            } else {
                // Get ticket details url
                url = row.getAttribute('data-ticket-url');
            }
            
            // Check if url exists and it's not empty
            if (url && url.trim() !== '') {
                window.location.href = url;
            }
        });
    });
}

/**
 * Initializes the ticket toggle functionality.
 * Allows users to toggle between viewing in-progress tickets and completed/cancelled tickets.
 */
function setTicketToggle() {
    const toggleButton = document.getElementById('toggle-tickets');
    // Exit if the toggle button is not found.
    if(!toggleButton) return;

    // Determine the initial state based on the button text.
    let isCompleted = toggleButton.textContent.trim() === 'In-progress';
    
    // Add clikc event on toggle button
    toggleButton.addEventListener('click', function() {
        // Toggle the state.
        isCompleted = !isCompleted;
        // Update the button text.
        toggleButton.textContent = isCompleted ? 'In-progress' : 'Completed and Cancelled';

        // Determine the URL based on the current state.
        const url = isCompleted ? toggleButton.getAttribute('data-completed-url')
            : toggleButton.getAttribute('data-progress-url');

        // Fetch the updated ticket list from the server.
        fetch(url)
            .then(response => response.text())
            .then(html => {
                // Replace the current document content with the new HTML.
                // Open the document stream.
                document.open();
                // Write the new HTML to the document.
                document.write(html);
                // Close the document stream.
                document.close();
            })
            .catch(error => {
                console.error('Error loading ticket list:', error);
                alert('Failed to load ticket list.');
            })
    });
}

// Add a DOMContentLoaded event listener to ensure that the initialization functions are executed // after the DOM is fully loaded.
document.addEventListener('DOMContentLoaded', () => {
    ticketFiltering();
    ticketSorting();
    clearFiltering();
    setTicketRowClick();
    setTicketToggle();
});