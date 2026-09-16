// Editor.js block plugin: an image picked from the media library (replaces
// the stock @editorjs/image tool, which uploaded its own files instead of
// reusing the shared library). Renders as block type "image" — the shape
// { url, alt } is what EditorJsRenderer.RenderBlock's "image" case expects.
class MediaImageTool {
    static get toolbox() {
        return {
            title: "Image",
            icon: "<svg width=\"17\" height=\"17\" viewBox=\"0 0 24 24\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect x=\"3\" y=\"3\" width=\"18\" height=\"18\" rx=\"2\" stroke=\"currentColor\" stroke-width=\"2\"/><circle cx=\"8.5\" cy=\"8.5\" r=\"1.5\" stroke=\"currentColor\" stroke-width=\"2\"/><path d=\"M21 15l-5-5-9 9\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/></svg>"
        };
    }

    static get isReadOnlySupported() {
        return true;
    }

    constructor({ data }) {
        data = data || {};
        this.data = {
            mediaId: data.mediaId || "",
            url: data.url || "",
            alt: data.alt || ""
        };
        this.wrapper = null;
    }

    render() {
        const wrapper = document.createElement("div");
        wrapper.className = "editorjs-component";

        const imageRow = document.createElement("div");
        imageRow.className = "editorjs-row";
        const preview = document.createElement("img");
        preview.className = "media-picker-preview";
        preview.alt = this.data.alt;
        preview.src = this.data.url;
        preview.style.cssText = "display:" + (this.data.url ? "block" : "none") + ";";
        const chooseBtn = document.createElement("button");
        chooseBtn.type = "button";
        chooseBtn.className = "btn btn-sm";
        chooseBtn.textContent = this.data.url ? "Change image" : "Choose image";
        chooseBtn.addEventListener("click", async () => {
            if (typeof window.openMediaPicker !== "function") return;
            const media = await window.openMediaPicker();
            if (!media) return;
            this.data.mediaId = media.id;
            this.data.url = media.url;
            this.data.alt = media.alt || "";
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
            this.data.mediaId = "";
            this.data.url = "";
            this.data.alt = "";
            preview.src = "";
            preview.style.display = "none";
            chooseBtn.textContent = "Choose image";
        });
        imageRow.append(preview, chooseBtn, clearBtn);

        wrapper.append(imageRow);
        this.wrapper = wrapper;
        return wrapper;
    }

    save() {
        return this.data;
    }

    validate() {
        // No image is a valid (empty) block — nothing to reject.
        return true;
    }
}

window.MediaImageTool = MediaImageTool;