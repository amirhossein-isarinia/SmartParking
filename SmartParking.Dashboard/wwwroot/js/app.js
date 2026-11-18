// توابع کمکی مرورگر برای داشبورد SmartParking (چاپ، دانلود فایل، تنظیمات ظاهری)
(function () {
    'use strict';
    var root = document.documentElement;
    var KEYS = ['theme', 'anim', 'density', 'pageSize'];

    function applyAttr(key, value) {
        if (key === 'theme') root.setAttribute('data-theme', value === 'light' ? 'light' : 'dark');
        else if (key === 'anim') { if (value === 'off') root.setAttribute('data-anim', 'off'); else root.removeAttribute('data-anim'); }
        else if (key === 'density') { if (value === 'compact') root.setAttribute('data-density', 'compact'); else root.removeAttribute('data-density'); }
    }

    window.smartParking = {
        getPrefs: function () {
            var result = {};
            try { KEYS.forEach(function (k) { var v = localStorage.getItem('sp.' + k); if (v !== null) result[k] = v; }); } catch (e) { }
            return result;
        },
        setPref: function (key, value) {
            try { localStorage.setItem('sp.' + key, value); } catch (e) { }
            applyAttr(key, value);
        },
        print: function (mode) {
            document.body.classList.toggle('print-receipt', mode === 'receipt');
            window.requestAnimationFrame(function () { window.print(); });
        },
        download: function (fileName, content, mime) {
            var blob = new Blob([content], { type: mime || 'text/plain;charset=utf-8' });
            var url = URL.createObjectURL(blob);
            var a = document.createElement('a');
            a.href = url; a.download = fileName; a.style.display = 'none';
            document.body.appendChild(a); a.click(); document.body.removeChild(a);
            setTimeout(function () { URL.revokeObjectURL(url); }, 2000);
        },
        focus: function (id) {
            var el = document.getElementById(id);
            if (el) el.focus();
        }
    };

    window.addEventListener('afterprint', function () { document.body.classList.remove('print-receipt'); });

    // پیام اتصال مجدد (Blazor کلاس‌های components-reconnect-* را روی این عنصر می‌گذارد)
    document.addEventListener('components-reconnect-state-changed', function (e) {
        var modal = document.getElementById('components-reconnect-modal');
        if (!modal) return;
        var state = e.detail && e.detail.state;
        modal.classList.remove('components-reconnect-show', 'components-reconnect-failed', 'components-reconnect-rejected');
        if (state === 'show' || state === 'retrying') modal.classList.add('components-reconnect-show');
        else if (state === 'failed') modal.classList.add('components-reconnect-failed');
        else if (state === 'rejected') modal.classList.add('components-reconnect-rejected');
    });
})();
