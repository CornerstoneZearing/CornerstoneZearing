// Editor.js block plugin: a Bootstrap 5 card (image, title, text, link).
// All fields are optional; the button is only emitted (server-side, see
// EditorJsRenderer.RenderBlock's "bootstrapCard" case) when both a link URL
// and link text are present. Image alt text always mirrors the selected
// media item's alt text and isn't independently editable here.
class BootstrapCardTool {
    static get toolbox() {
        return {
            title: "Bootstrap Card",
            icon: "<svg width=\"17\" height=\"17\" viewBox=\"0 0 24 24\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect x=\"3\" y=\"4\" width=\"18\" height=\"16\" rx=\"2\" stroke=\"currentColor\" stroke-width=\"2\"/><path d=\"M3 10h18\" stroke=\"currentColor\" stroke-width=\"2\"/><path d=\"M7 14h6M7 17h4\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\"/></svg>"
        };
    }

    static get isReadOnlySupported() {
        return true;
    }

    constructor({ data }) {
        data = data || {};
        this.data = {
            imageId: data.imageId || "",
            imageUrl: data.imageUrl || "",
            imageAlt: data.imageAlt || "",
            title: data.title || "",
            text: data.text || "",
            linkUrl: data.linkUrl || "",
            linkText: data.linkText || ""
        };
        this.wrapper = null;
    }

    render() {
        const wrapper = document.createElement("div");
        wrapper.className = "editorjs-component";

        // Image
        const imageRow = document.createElement("div");
        imageRow.className = "editorjs-row";
        const preview = document.createElement("img");
        preview.className = "media-picker-preview";
        preview.alt = this.data.imageAlt;
        preview.src = this.data.imageUrl;
        preview.style.cssText = "display:" + (this.data.imageUrl ? "block" : "none") + ";";
        const chooseBtn = document.createElement("button");
        chooseBtn.type = "button";
        chooseBtn.className = "btn btn-sm";
        chooseBtn.textContent = this.data.imageUrl ? "Change image" : "Choose image";
        chooseBtn.addEventListener("click", async () => {
            if (typeof window.openMediaPicker !== "function") return;
            const media = await window.openMediaPicker();
            if (!media) return;
            this.data.imageId = media.id;
            this.data.imageUrl = media.url;
            this.data.imageAlt = media.alt || "";
            preview.src = media.url;
            preview.alt = media.alt || "";
            preview.style.display = "block";
            chooseBtn.textContent = "Change image";
        });
        const clearBtn = document.createElement("button");
        clearBtn.type = "button";
        clearBtn.className = "btn btn-sm";
        clearBtn.textContent = "Remove image";
        clearBtn.addEventListener("click", () => {
            this.data.imageId = "";
            this.data.imageUrl = "";
            this.data.imageAlt = "";
            preview.src = "";
            preview.style.display = "none";
            chooseBtn.textContent = "Choose image";
        });
        imageRow.append(preview, chooseBtn, clearBtn);

        // Title
        const titleRow = document.createElement("div");
        titleRow.className = "editorjs-row";
        const titleInput = document.createElement("input");
        titleInput.type = "text";
        titleInput.placeholder = "Title";
        titleInput.value = this.data.title;
        titleInput.addEventListener("input", () => { this.data.title = titleInput.value; });
        titleRow.append(titleInput);

        // Text
        const textRow = document.createElement("div");
        textRow.className = "editorjs-row";
        textRow.style.cssText = "margin-bottom: 0;";
        const textArea = document.createElement("textarea");
        textArea.placeholder = "Card text";
        textArea.value = this.data.text;
        textArea.addEventListener("input", () => { this.data.text = textArea.value; });
        textRow.append(textArea);

        // Link (URL + text) — button only renders when both are filled in
        const linkRow = document.createElement("div");
        linkRow.className = "editorjs-row";
        const linkFields = document.createElement("div");
        linkFields.style.cssText = "display:flex;gap:5px;";
        const linkUrlInput = document.createElement("input");
        linkUrlInput.type = "text";
        linkUrlInput.placeholder = "Button URL";
        linkUrlInput.value = this.data.linkUrl;
        linkUrlInput.style.flex = "1 1 50%";
        linkUrlInput.addEventListener("input", () => { this.data.linkUrl = linkUrlInput.value; });
        const linkTextInput = document.createElement("input");
        linkTextInput.type = "text";
        linkTextInput.placeholder = "Button Text";
        linkTextInput.value = this.data.linkText;
        linkTextInput.style.flex = "1 1 50%";
        linkTextInput.addEventListener("input", () => { this.data.linkText = linkTextInput.value; });
        linkFields.append(linkUrlInput, linkTextInput);
        linkRow.append(linkFields);
        wrapper.append(imageRow, titleRow, textRow, linkRow);
        this.wrapper = wrapper;
        return wrapper;
    }

    save() {
        return this.data;
    }

    validate() {
        // Every field is optional — nothing to reject.
        return true;
    }
}

window.BootstrapCardTool = BootstrapCardTool;