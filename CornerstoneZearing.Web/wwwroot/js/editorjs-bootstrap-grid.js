// Editor.js block plugin: a Bootstrap 5.3 grid (row of columns). Each column
// picks a single breakpoint + size (e.g. "md-6", rendered as col-md-6) from
// one dropdown, and its content is edited by its own nested Editor.js
// instance using the same Text/Header/List/Image tools as the page editor
// (see admin.js's TOOLS/tools config — Header, List, MediaImageTool are
// already loaded globally by the time this tool is used). Each column's
// nested editor output (a standard { blocks: [...] } shape) is stored as
// column.blocks and rendered server-side by looping RenderBlock over it —
// see EditorJsRenderer.RenderBlock's "bootstrapGrid" case.
class BootstrapGridTool {
    static get toolbox() {
        return {
            title: "Grid",
            icon: "<svg width=\"17\" height=\"17\" viewBox=\"0 0 24 24\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect x=\"3\" y=\"4\" width=\"18\" height=\"16\" rx=\"2\" stroke=\"currentColor\" stroke-width=\"2\"/><path d=\"M9 4v16M15 4v16\" stroke=\"currentColor\" stroke-width=\"2\"/></svg>"
        };
    }

    static get isReadOnlySupported() {
        return true;
    }

    constructor({ data }) {
        data = data || {};
        this.data = {
            columns: Array.isArray(data.columns) && data.columns.length > 0
                ? data.columns.map(c => this.normalizeColumn(c))
                : [this.newColumn(), this.newColumn()]
        };
        this.wrapper = null;
        this.columnEditors = {}; // colIndex -> EditorJS instance
    }

    normalizeColumn(c) {
        c = c || {};
        return {
            colSize: c.colSize || "",
            blocks: Array.isArray(c.blocks) ? c.blocks : []
        };
    }

    newColumn() {
        return { colSize: "md-6", blocks: [] };
    }

    render() {
        const wrapper = document.createElement("div");
        wrapper.className = "editorjs-component editorjs-grid";
        this.wrapper = wrapper;

        this.columnsContainer = document.createElement("div");
        this.columnsContainer.className = "editorjs-grid-columns";
        wrapper.append(this.columnsContainer);

        const addColumnBtn = document.createElement("button");
        addColumnBtn.type = "button";
        addColumnBtn.className = "btn btn-sm";
        addColumnBtn.style.marginTop = "5px";
        addColumnBtn.textContent = "Add column";
        addColumnBtn.addEventListener("click", async () => {
            await this.syncFromEditors();
            this.data.columns.push(this.newColumn());
            this.rebuild();
        });

        const controlsRow = document.createElement("div");
        controlsRow.className = "editorjs-row";
        controlsRow.append(addColumnBtn);
        wrapper.append(controlsRow);

        this.buildColumnsDom();
        // In case the "rendered()" lifecycle hook isn't invoked (older host),
        // fall back to mounting on the next tick once the node is attached.
        setTimeout(() => this.mountAllEditors(), 0);
        return wrapper;
    }

    // Editor.js Tool API lifecycle hook: called once this tool's node has
    // actually been inserted into the DOM, which nested Editor.js instances
    // require before they can mount.
    rendered() {
        this.mountAllEditors();
    }

    // Rebuilds column DOM from this.data.columns (e.g. after add/remove/move)
    // and remounts nested editors. Callers must call syncFromEditors() first
    // to capture in-progress edits before mutating this.data.columns.
    rebuild() {
        this.destroyAllEditors();
        this.buildColumnsDom();
        this.mountAllEditors();
    }

    buildColumnsDom() {
        this.columnsContainer.innerHTML = "";
        this.columnHolders = {};
        this.data.columns.forEach((column, colIndex) => {
            this.columnsContainer.append(this.renderColumn(column, colIndex));
        });
    }

    async syncFromEditors() {
        const entries = Object.entries(this.columnEditors);
        for (const [colIndex, editor] of entries) {
            try {
                await editor.isReady;
                const saved = await editor.save();
                if (this.data.columns[colIndex]) {
                    this.data.columns[colIndex].blocks = saved.blocks || [];
                }
            } catch (e) { /* leave existing blocks as-is on failure */ }
        }
    }

    mountAllEditors() {
        if (!this.wrapper || !this.wrapper.isConnected) return;
        this.data.columns.forEach((column, colIndex) => {
            if (this.columnEditors[colIndex]) return;
            const holder = this.columnHolders[colIndex];
            if (!holder) return;
            this.columnEditors[colIndex] = new EditorJS({
                holder: holder,
                data: { blocks: column.blocks || [] },
                placeholder: "Column content…",
                minHeight: 60,
                tools: {
                    header: { class: Header, inlineToolbar: true },
                    list: { class: List, inlineToolbar: true },
                    image: MediaImageTool
                }
            });
        });
    }

    destroyAllEditors() {
        Object.values(this.columnEditors).forEach(editor => {
            try { editor.destroy(); } catch (e) { /* already gone */ }
        });
        this.columnEditors = {};
    }

    renderColumn(column, colIndex) {
        const colEl = document.createElement("div");
        colEl.className = "editorjs-grid-column";

        // Header: size dropdown + move/remove controls, all on one line
        const header = document.createElement("div");
        header.className = "editorjs-grid-column-header";

        // Column size: a single dropdown picking one breakpoint + width
        // (e.g. "md-6" -> col-md-6), grouped by breakpoint.
        const select = document.createElement("select");
        select.className = "editorjs-grid-sizing";
        const noneOpt = document.createElement("option");
        noneOpt.value = "";
        noneOpt.textContent = "(auto)";
        select.append(noneOpt);
        [["sm", "Small"], ["md", "Medium"], ["lg", "Large"]].forEach(([bp, bpLabel]) => {
            const group = document.createElement("optgroup");
            group.label = bpLabel;
            for (let n = 1; n <= 12; n++) {
                const opt = document.createElement("option");
                opt.value = bp + "-" + n;
                opt.textContent = "col-" + bp + "-" + n;
                group.append(opt);
            }
            select.append(group);
        });
        select.value = column.colSize || "";
        select.addEventListener("change", () => { column.colSize = select.value; });
        header.append(select);

        const headerBtns = document.createElement("span");
        if (colIndex > 0) {
            const left = document.createElement("button");
            left.type = "button";
            left.className = "btn btn-sm";
            left.textContent = "←";
            left.title = "Move left";
            left.addEventListener("click", async () => {
                await this.syncFromEditors();
                const cols = this.data.columns;
                [cols[colIndex - 1], cols[colIndex]] = [cols[colIndex], cols[colIndex - 1]];
                this.rebuild();
            });
            headerBtns.append(left);
        }
        if (colIndex < this.data.columns.length - 1) {
            const right = document.createElement("button");
            right.type = "button";
            right.className = "btn btn-sm";
            right.textContent = "→";
            right.title = "Move right";
            right.addEventListener("click", async () => {
                await this.syncFromEditors();
                const cols = this.data.columns;
                [cols[colIndex], cols[colIndex + 1]] = [cols[colIndex + 1], cols[colIndex]];
                this.rebuild();
            });
            headerBtns.append(right);
        }
        const removeCol = document.createElement("button");
        removeCol.type = "button";
        removeCol.className = "btn btn-sm";
        removeCol.textContent = "Remove column";
        removeCol.addEventListener("click", async () => {
            await this.syncFromEditors();
            this.data.columns.splice(colIndex, 1);
            this.rebuild();
        });
        headerBtns.append(removeCol);
        header.append(headerBtns);
        colEl.append(header);

        // Nested Editor.js holder for this column's content
        const holder = document.createElement("div");
        holder.className = "editorjs-holder editorjs-grid-column-holder";
        this.columnHolders[colIndex] = holder;
        colEl.append(holder);

        return colEl;
    }

    async save() {
        await this.syncFromEditors();
        return this.data;
    }

    validate() {
        // Empty columns are valid — nothing to reject.
        return true;
    }
}

window.BootstrapGridTool = BootstrapGridTool;