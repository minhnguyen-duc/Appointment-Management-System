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

// ── Toast notification (login/logout success) ─────────────────────────────
// Called from Homepage.razor OnAfterRenderAsync via JS interop.
// Uses pure DOM manipulation — reliable regardless of Blazor render cycle.
window.amsShowToast = function(msg, type) {
    if (!msg) return;
    type = type || 'info';
    var bg = type === 'error' ? '#DC2626' : '#1A9CE0';

    // Inject keyframe animation once
    if (!document.getElementById('ams-toast-kf')) {
        var s = document.createElement('style');
        s.id = 'ams-toast-kf';
        s.textContent =
            '@keyframes amsIn{from{opacity:0;transform:translateX(20px) scale(.97)}' +
            'to{opacity:1;transform:translateX(0) scale(1)}}';
        document.head.appendChild(s);
    }

    var el = document.createElement('div');
    el.style.cssText =
        'position:fixed;top:24px;right:24px;z-index:9999;' +
        'display:flex;align-items:center;gap:10px;' +
        'padding:13px 20px;border-radius:12px;' +
        'background:' + bg + ';color:#fff;' +
        'font-family:"Be Vietnam Pro",-apple-system,sans-serif;' +
        'font-size:14px;font-weight:500;' +
        'box-shadow:0 8px 28px rgba(0,0,0,.18);' +
        'cursor:pointer;max-width:360px;' +
        'animation:amsIn .3s cubic-bezier(.16,1,.3,1);';

    el.innerHTML =
        '<svg width="18" height="18" viewBox="0 0 24 24" fill="none" ' +
        'stroke="currentColor" stroke-width="2.5" stroke-linecap="round">' +
        '<circle cx="12" cy="12" r="10"/><path d="m9 12 2 2 4-4"/></svg>' +
        '<span>' + msg + '</span>';

    el.addEventListener('click', function() { dismiss(); });
    document.body.appendChild(el);

    var timer = setTimeout(dismiss, 3500);

    function dismiss() {
        clearTimeout(timer);
        el.style.transition = 'opacity .3s,transform .3s';
        el.style.opacity = '0';
        el.style.transform = 'translateX(20px)';
        setTimeout(function() { el.remove(); }, 320);
    }
};

// Read ?t= from current browser URL (called from OnAfterRenderAsync)
window.amsConsumeToastParam = function() {
    try {
        var params = new URLSearchParams(window.location.search);
        var msg = params.get('t');
        if (msg) {
            // Clean URL immediately so it's not visible/bookmarkable
            var clean = window.location.pathname;
            history.replaceState(null, document.title, clean);
            return decodeURIComponent(msg);
        }
    } catch(e) {}
    return null;
};

// Force browser navigation — bypasses Blazor enhanced navigation interception.
// Use this for server-side endpoints (logout, do-login) where Blazor's router
// would silently swallow the navigation instead of doing a real HTTP request.
window.amsNavigate = function(url) {
    window.location.href = url;
};
