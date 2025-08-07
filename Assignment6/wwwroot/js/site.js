// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function getHomeName(homeId,idSelector) {
    const homeSelector = document.getElementById(idSelector);
    for (let option of homeSelector.options) {
        if (parseInt(option.value) === homeId) {
            return option.text;
        }
    }
    return `Property ${homeId}`;
}
function showNotification(message, type = 'info', duration = 5000) {
    const alertClass = type === 'success' ? 'alert-success' :
        type === 'error' ? 'alert-danger' : 'alert-info';
    const iconClass = type === 'success' ? 'fa-check-circle' :
        type === 'error' ? 'fa-exclamation-circle' : 'fa-info-circle';

    const notification = $(`
                    <div class="alert ${alertClass} alert-dismissible fade show notification" role="alert">
                        <i class="fas ${iconClass} me-2"></i>
                        ${message}
                        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
                    </div>
                `);

    // Remove existing notifications
    $('.notification').remove();
    $('body').append(notification);

    if (duration > 0) {
        setTimeout(() => {
            notification.alert('close');
        }, duration);
    }
}