// Auto-dismiss success/error banners after a few seconds.
document.addEventListener('DOMContentLoaded', function () {
    var alerts = document.querySelectorAll('.alert-success-custom, .alert-error-custom');
    alerts.forEach(function (el) {
        setTimeout(function () {
            el.style.transition = 'opacity 0.4s ease';
            el.style.opacity = '0';
            setTimeout(function () { el.remove(); }, 400);
        }, 4000);
    });
});
