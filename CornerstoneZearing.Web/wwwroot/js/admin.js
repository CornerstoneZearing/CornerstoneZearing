// Editor.js bootstrap + shared admin helpers.
(function () {
    const EDITOR_CDN = "https://cdn.jsdelivr.net/npm/@editorjs/editorjs@2.30.7/dist/editorjs.umd.min.js";
    const TOOLS = [
        ["Header", "https://cdn.jsdelivr.net/npm/@editorjs/header@2.8.7/dist/header.umd.min.js"],
        ["List", "https://cdn.jsdelivr.net/npm/@editorjs/list@1.10.0/dist/list.umd.min.js"],
        ["Quote", "https://cdn.jsdelivr.net/npm/@editorjs/quote@2.6.0/dist/quote.umd.min.js"],
        ["Table", "https://cdn.jsdelivr.net/npm/@editorjs/table@2.3.0/dist/table.umd.min.js"],
        ["Delimiter", "https://cdn.jsdelivr.net/npm/@editorjs/delimiter@1.4.0/dist/delimiter.umd.min.js"],
        ["CodeTool", "https://cdn.jsdelivr.net/npm/@editorjs/code@2.9.0/dist/code.umd.min.js"],
        ["Embed", "https://cdn.jsdelivr.net/npm/@editorjs/embed@2.7.4/dist/embed.umd.min.js"],
        ["Marker", "https://cdn.jsdelivr.net/npm/@editorjs/marker@1.4.0/dist/marker.umd.min.js"],
        ["InlineCode", "https://cdn.jsdelivr.net/npm/@editorjs/inline-code@1.5.1/dist/inline-code.umd.min.js"],
        ["ImageTool", "https://cdn.jsdelivr.net/npm/@editorjs/image@2.9.3/dist/image.umd.min.js"]
    ];

    function loadScript(src) {
        return new Promise((resolve, reject) => {
            const s = document.createElement("script");
            s.src = src; s.onload = resolve; s.onerror = () => reject(new Error("Failed to load " + src));
            document.head.appendChild(s);
        });
    }

    async function ensureEditorLibs() {
        if (window.__editorLibsLoaded) return;
        await loadScript(EDITOR_CDN);
        await Promise.all(TOOLS.map(t => loadScript(t[1])));
        window.__editorLibsLoaded = true;
    }

    window.initEditor = async function (opts) {
        const holder = document.getElementById(opts.holderId);
        const input = document.getElementById(opts.inputId);
        const form = holder.closest("form");
        await ensureEditorLibs();

        let initialData = {};
        if (input.value) { try { initialData = JSON.parse(input.value); } catch (e) { initialData = {}; } }

        const editor = new EditorJS({
            holder: opts.holderId,
            data: initialData,
            placeholder: opts.placeholder || "Write content…",
            tools: {
                header: { class: Header, inlineToolbar: true },
                list: { class: List, inlineToolbar: true },
                quote: { class: Quote, inlineToolbar: true },
                table: { class: Table, inlineToolbar: true },
                delimiter: Delimiter,
                code: CodeTool,
                embed: Embed,
                marker: Marker,
                inlineCode: InlineCode,
                image: {
                    class: ImageTool,
                    config: {
                        endpoints: { byFile: opts.uploadUrl, byUrl: opts.uploadByUrl },
                        additionalRequestHeaders: opts.antiForgery ? { "RequestVerificationToken": opts.antiForgery } : {}
                    }
                }
            }
        });

        form.addEventListener("submit", async function (e) {
            e.preventDefault();
            const saved = await editor.save();
            input.value = JSON.stringify(saved);
            form.submit();
        });
    };

    // --- Media picker modal ---
    async function openMediaModal() {
        let modal = document.getElementById("media-modal");
        if (!modal) {
            modal = document.createElement("div");
            modal.id = "media-modal";
            modal.style.cssText = "position:fixed;inset:0;background:rgba(0,0,0,.4);display:flex;align-items:center;justify-content:center;z-index:1000";
            modal.innerHTML = "<div style='background:#fff;border-radius:10px;max-width:820px;width:92%;max-height:80vh;overflow:auto;padding:20px'>" +
                "<div style='display:flex;justify-content:space-between;align-items:center;margin-bottom:12px'>" +
                "<strong>Media library</strong><button type='button' class='btn btn-sm btn-ghost' id='media-modal-close'>Close</button></div>" +
                "<div id='media-modal-body' class='muted'>Loading…</div></div>";
            document.body.appendChild(modal);
            modal.addEventListener("click", e => { if (e.target === modal) modal.remove(); });
            modal.querySelector("#media-modal-close").addEventListener("click", () => modal.remove());
        }
        const body = modal.querySelector("#media-modal-body");
        const res = await fetch("/Admin/Media/Picker");
        body.innerHTML = await res.text();
        return modal;
    }

    window.pickMedia = async function (fieldId) {
        const modal = await openMediaModal();
        modal.querySelectorAll("[data-media-id]").forEach(el => {
            el.style.cursor = "pointer";
            el.onclick = () => {
                document.getElementById(fieldId).value = el.getAttribute("data-media-id");
                const preview = document.querySelector(".media-picker[data-field='" + fieldId + "'] .media-picker-preview");
                if (preview) { preview.src = el.getAttribute("data-media-url"); preview.style.display = "block"; }
                modal.remove();
            };
        });
    };

    window.clearMedia = function (fieldId) {
        document.getElementById(fieldId).value = "";
        const preview = document.querySelector(".media-picker[data-field='" + fieldId + "'] .media-picker-preview");
        if (preview) { preview.src = ""; preview.style.display = "none"; }
    };
})();
