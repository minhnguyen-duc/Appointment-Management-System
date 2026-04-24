// Scroll chat to bottom
window.scrollToBottom = (el) => {
    if (el) el.scrollTop = el.scrollHeight;
};

// OTP input auto-focus next box
document.addEventListener('input', (e) => {
    if (e.target.classList.contains('otp-box')) {
        const val = e.target.value;
        if (val.length === 1) {
            const next = e.target.nextElementSibling;
            if (next && next.classList.contains('otp-box')) next.focus();
        }
    }
});
