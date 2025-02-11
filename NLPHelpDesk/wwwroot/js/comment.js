/**
 * Handles the submission of a new comment.
 *
 * @param {Event} event The form submit event.
 */
function addComment(event) {
    event.preventDefault();

    const form = document.getElementById('add-comment-form');
    const formData = new FormData(form);
    const url = form.getAttribute('action');

    // Make a POST request to the server to submit the comment.
    fetch(url, {
        method: 'POST',
        body: formData,
    })
        .then(response => {
            if (!response.ok) {
                // If there's an error, parse the error message from the response body and throw an error.
                return response.text().then(text => {
                    throw new Error(text);
                });
            }
            return response.json();
        })
        .then(newComment => {
            const commentList = document.querySelector('.comment-list');
            const countBadge = document.querySelector('.card-header .badge');

            // Remove the "No comments yet" message if it exists.
            const noCommentsMessage = commentList.querySelector('p');
            if (noCommentsMessage.textContent === 'No comments yet.') {
                noCommentsMessage.remove();
            }

            // Create a new comment element (article) and populate it with the new comment data.
            const newCommentElement = document.createElement('article');
            newCommentElement.className = 'card m-3 shadow-sm border-0';
            newCommentElement.innerHTML = `
                <header class="card-header bg-info text-white d-flex align-items-center">
                    <div>
                        <h6 class="mb-0 fw-semibold">${newComment.userName}</h6>
                        <small class="text-light">${newComment.createDate}</small>
                    </div>
                </header>
                <div class="card-body bg-light text-dark p-3">
                    <p class="mb-0">${newComment.commentText}</p>
                    ${newComment.fileUrl ? `
                        <div class="mt-2">
                            ${(newComment.fileType == ".jpg" || newComment.fileType == ".jpeg" || newComment.fileType == ".png") ? `
                            <img src="${newComment.fileUrl}" alt="Attached image" class="img-thumbnail" style="max-width: 200px; cursor: pointer;" data-bs-toggle="modal" data-bs-target="#imageModal" />
                            <a href="${newComment.fileUrl}" target="_blank" download>${newComment.fileName}</a>
                            <div class="modal fade" id="imageModal" tabindex="-1" aria-labelledby="imageModalLabel" aria-hidden="true">
                                <div class="modal-dialog modal-xl modal-dialog-centered">
                                    <div class="modal-content">
                                        <div class="modal-header">
                                            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                                        </div>
                                        <div class="modal-body text-center">
                                            <img src="${newComment.fileUrl}" alt="Attached image" class="img-fluid" style="max-height: 90vh;" />
                                        </div>
                                    </div>
                                </div>
                            </div>` : `<a href="${newComment.fileUrl}" target="_blank" download>${newComment.fileName}</a>`}
                        </div>` : ''}
                </div>`;

            // Add the new comment to the list.
            commentList.appendChild(newCommentElement);
            // Scroll to the bottom to show the new comment.
            commentList.scrollTop = commentList.scrollHeight;

            // Update the comment count badge.
            let currentCount = parseInt(countBadge.textContent);
            countBadge.textContent = (currentCount + 1).toString();
            
            form.reset();
        })
        .catch(error => {
            console.error('Error:', error.message);
            alert(error.message || 'An error occurred while submitting your comment.');
        })
}